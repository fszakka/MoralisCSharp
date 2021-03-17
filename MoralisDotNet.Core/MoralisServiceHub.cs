namespace MoralisDotNet.Core
{
    using System;
    using System.Net.Http;
    using Parse.Abstractions.Infrastructure;
    using Parse.Abstractions.Infrastructure.Data;
    using Parse.Abstractions.Infrastructure.Execution;
    using Parse.Abstractions.Internal;
    using Parse.Abstractions.Platform.Analytics;
    using Parse.Abstractions.Platform.Cloud;
    using Parse.Abstractions.Platform.Configuration;
    using Parse.Abstractions.Platform.Files;
    using Parse.Abstractions.Platform.Installations;
    using Parse.Abstractions.Platform.Objects;
    using Parse.Abstractions.Platform.Push;
    using Parse.Abstractions.Platform.Queries;
    using Parse.Abstractions.Platform.Sessions;
    using Parse.Abstractions.Platform.Users;
    using Parse.Infrastructure;
    using Parse.Infrastructure.Data;
    using Parse.Infrastructure.Execution;
    using Parse.Infrastructure.Utilities;
    using Parse.Platform.Analytics;
    using Parse.Platform.Cloud;
    using Parse.Platform.Files;
    using Parse.Platform.Installations;
    using Parse.Platform.Objects;
    using Parse.Platform.Sessions;
    using Parse.Platform.Users;

    public class MoralisServiceHub : IServiceHub
    {
        public MoralisServiceHub(HttpClient httpClient, IServerConnectionData connectionData)
        {
            LateInitializer = new LateInitializer();
            
            webClient = new MoralisWebClient(httpClient);
            httpClient.DefaultRequestHeaders.Remove("IfModifiedSince");
            ServerConnectionData ??= connectionData;
            parseCommandRunner = new ParseCommandRunner(WebClient, InstallationController, MetadataController,
                ServerConnectionData, new Lazy<IParseUserController>(() => userController));
            userController = new ParseUserController(parseCommandRunner, Decoder);
            
        }

        private MoralisWebClient webClient;
        private LateInitializer LateInitializer { get; }
        private ParseCommandRunner parseCommandRunner;
        private ParseUserController userController;
        public IServerConnectionData ServerConnectionData { get; set; }
        public IMetadataController MetadataController => LateInitializer.GetValue(() => new MetadataController { HostManifestData = new HostManifestData(){Identifier = "", Name = "",ShortVersion = "",Version = ""}, EnvironmentData = new EnvironmentData()
        {
            OSVersion = "Microsoft Windows 10.0.19041",
            Platform = "Windows"
        } });

        public IServiceHubCloner Cloner => LateInitializer.GetValue(() => new { } as object as IServiceHubCloner);

        public IWebClient WebClient => LateInitializer.GetValue(() => webClient);
        public ICacheController CacheController => LateInitializer.GetValue(() => new MoralisCacheController { });
        public IParseObjectClassController ClassController => LateInitializer.GetValue(() => new ParseObjectClassController { });

        public IParseDataDecoder Decoder => LateInitializer.GetValue(() => new ParseDataDecoder(ClassController));

        public IParseInstallationController InstallationController => LateInitializer.GetValue(() => new ParseInstallationController(CacheController));
        public IParseCommandRunner CommandRunner => LateInitializer.GetValue(() => parseCommandRunner);

        public IParseCloudCodeController CloudCodeController => LateInitializer.GetValue(() => new ParseCloudCodeController(CommandRunner, Decoder));
        public IParseConfigurationController ConfigurationController => LateInitializer.GetValue(() => new ParseConfigurationController(CommandRunner, CacheController, Decoder));
        public IParseFileController FileController => LateInitializer.GetValue(() => new ParseFileController(CommandRunner));
        public IParseObjectController ObjectController => LateInitializer.GetValue(() => new ParseObjectController(CommandRunner, Decoder, ServerConnectionData));
        public IParseQueryController QueryController => LateInitializer.GetValue(() => new ParseQueryController(CommandRunner, Decoder));
        public IParseSessionController SessionController => LateInitializer.GetValue(() => new ParseSessionController(CommandRunner, Decoder));
        public IParseUserController UserController => LateInitializer.GetValue(() => new ParseUserController(CommandRunner, Decoder));
        public IParseCurrentUserController CurrentUserController => LateInitializer.GetValue(() => new ParseCurrentUserController(CacheController, ClassController, Decoder));

        public IParseAnalyticsController AnalyticsController => LateInitializer.GetValue(() => new ParseAnalyticsController(CommandRunner));

        public IParseInstallationCoder InstallationCoder => LateInitializer.GetValue(() => new ParseInstallationCoder(Decoder, ClassController));

        public IParsePushChannelsController PushChannelsController => LateInitializer.GetValue(() => new ParsePushChannelsController(CurrentInstallationController));
        public IParsePushController PushController => LateInitializer.GetValue(() => new ParsePushController(CommandRunner, CurrentUserController));
        public IParseCurrentInstallationController CurrentInstallationController => LateInitializer.GetValue(() => new ParseCurrentInstallationController(InstallationController, CacheController, InstallationCoder, ClassController));
        public IParseInstallationDataFinalizer InstallationDataFinalizer => LateInitializer.GetValue(() => new ParseInstallationDataFinalizer { });

        public bool Reset() => LateInitializer.Used && LateInitializer.Reset();
    }
}
