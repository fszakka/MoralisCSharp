namespace MoralisDotNet.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Parse.Abstractions.Infrastructure;
    using Parse.Abstractions.Platform.Installations;
    using Parse.Abstractions.Platform.Push;
    using Parse.Infrastructure.Utilities;

    internal class ParsePushChannelsController : IParsePushChannelsController
    {
        IParseCurrentInstallationController CurrentInstallationController { get; }

        public ParsePushChannelsController(IParseCurrentInstallationController currentInstallationController) => CurrentInstallationController = currentInstallationController;

        public Task SubscribeAsync(IEnumerable<string> channels, IServiceHub serviceHub, CancellationToken cancellationToken = default) => CurrentInstallationController.GetAsync(serviceHub, cancellationToken).OnSuccess(task =>
        {
            task.Result.AddRangeUniqueToList(nameof(channels), channels);
            return task.Result.SaveAsync(cancellationToken);
        }).Unwrap();

        public Task UnsubscribeAsync(IEnumerable<string> channels, IServiceHub serviceHub, CancellationToken cancellationToken = default) => CurrentInstallationController.GetAsync(serviceHub, cancellationToken).OnSuccess(task =>
        {
            task.Result.RemoveAllFromList(nameof(channels), channels);
            return task.Result.SaveAsync(cancellationToken);
        }).Unwrap();
    }
}
