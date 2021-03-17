namespace MoralisDotNet.Core
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Parse;
    using Parse.Abstractions.Infrastructure.Data;
    using Parse.Abstractions.Infrastructure;
    using Parse.Abstractions.Infrastructure.Execution;
    using Parse.Abstractions.Platform.Configuration;
    using Parse.Infrastructure.Execution;
    using Parse.Infrastructure.Utilities;
    using Parse.Platform.Configuration;

    /// <summary>
    /// Config controller.
    /// </summary>
    internal class ParseConfigurationController : IParseConfigurationController
    {
        IParseCommandRunner CommandRunner { get; }

        IParseDataDecoder Decoder { get; }

        public IParseCurrentConfigurationController CurrentConfigurationController { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseConfigurationController"/> class.
        /// </summary>
        public ParseConfigurationController(IParseCommandRunner commandRunner, ICacheController storageController, IParseDataDecoder decoder)
        {
            CommandRunner = commandRunner;
            CurrentConfigurationController = new ParseCurrentConfigurationController(storageController, decoder);
            Decoder = decoder;
        }

        public Task<ParseConfiguration> FetchConfigAsync(string sessionToken, IServiceHub serviceHub, CancellationToken cancellationToken = default) => CommandRunner.RunCommandAsync(new ParseCommand("config", method: "GET", sessionToken: sessionToken, data: default), cancellationToken: cancellationToken).OnSuccess(task =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Decoder.BuildConfiguration(task.Result.Item2, serviceHub);
        }).OnSuccess(task =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            CurrentConfigurationController.SetCurrentConfigAsync(task.Result);
            return task;
        }).Unwrap();
    }
    /// <summary>
    /// Parse current config controller.
    /// </summary>
    internal class ParseCurrentConfigurationController : IParseCurrentConfigurationController
    {
        static string CurrentConfigurationKey { get; } = "CurrentConfig";

        TaskQueue TaskQueue { get; }

        ParseConfiguration CurrentConfiguration { get; set; }

        ICacheController StorageController { get; }

        IParseDataDecoder Decoder { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseCurrentConfigurationController"/> class.
        /// </summary>
        public ParseCurrentConfigurationController(ICacheController storageController, IParseDataDecoder decoder)
        {
            StorageController = storageController;
            Decoder = decoder;
            TaskQueue = new TaskQueue { };
        }

        public Task<ParseConfiguration> GetCurrentConfigAsync(IServiceHub serviceHub) => TaskQueue.Enqueue(toAwait => toAwait.ContinueWith(_ => CurrentConfiguration is { } ? Task.FromResult(CurrentConfiguration) : StorageController.LoadAsync().OnSuccess(task =>
        {
            task.Result.TryGetValue(CurrentConfigurationKey, out object data);
            return CurrentConfiguration = data is string { } configuration ? Decoder.BuildConfiguration(DeserializeJsonString(configuration), serviceHub) :serviceHub.BuildConfiguration(new Dictionary<string, object>());
        })), CancellationToken.None).Unwrap();

        public Task SetCurrentConfigAsync(ParseConfiguration target) => TaskQueue.Enqueue(toAwait => toAwait.ContinueWith(_ =>
        {
            CurrentConfiguration = target;
            return StorageController.LoadAsync().OnSuccess(task => task.Result.AddAsync(CurrentConfigurationKey, SerializeJsonString(((IJsonConvertible)target).ConvertToJSON())));
        }).Unwrap().Unwrap(), CancellationToken.None);

        public Task ClearCurrentConfigAsync() => TaskQueue.Enqueue(toAwait => toAwait.ContinueWith(_ =>
        {
            CurrentConfiguration = null;
            return StorageController.LoadAsync().OnSuccess(task => task.Result.RemoveAsync(CurrentConfigurationKey));
        }).Unwrap().Unwrap(), CancellationToken.None);

        public Task ClearCurrentConfigInMemoryAsync() => TaskQueue.Enqueue(toAwait => toAwait.ContinueWith(_ => CurrentConfiguration = null), CancellationToken.None);

        static IDictionary<string, object> DeserializeJsonString(string jsonData) => JsonUtilities.Parse(jsonData) as IDictionary<string, object>;

        static string SerializeJsonString(IDictionary<string, object> jsonData) => JsonUtilities.Encode(jsonData);
    }

}
