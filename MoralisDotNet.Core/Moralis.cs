namespace MoralisDotNet.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using Parse;
    using Parse.Abstractions.Infrastructure;
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

    public class Moralis
    {
        public Moralis(string applicationId, string serverUri, string key, string ethAddress, HttpClient httpClient = null, bool wasm = false)
        {
            var serverConnectionData = new ServerConnectionData()
            {
                Key = "",
                MasterKey = "",
                ApplicationID = applicationId,
                Headers = new Dictionary<string, string>(),
                ServerURI = serverUri
            };
            client = new ParseClient(applicationId, serverUri, key,wasm? new MoralisServiceHub(httpClient ?? new HttpClient(), serverConnectionData) : new ServiceHub(),null);
            client.ServerConnectionData.Key = key;
            client.ServerConnectionData.ServerURI = serverUri;
            client.ServerConnectionData.ApplicationID = applicationId;
            Storage = new LocalDatastore<ParseObject>(client);
            EthAddress = ethAddress;
        }

        public string EthAddress { get; }
        public IServiceHub ServiceHub => client.Services;
        ParseClient client;

        public void SetLocalDatastoreController()
        {
            throw new NotImplementedException();
        }

        public string ApplicationId
        {
            get => client.ServerConnectionData.ApplicationID;
            set
            {
                client.ServerConnectionData.ApplicationID = value;
            }
        }

        public string Key
        {
            get => client.ServerConnectionData.Key;
            set
            {
                client.ServerConnectionData.Key = value;
            }
        }


        public string MasterKey
        {
            get => client.ServerConnectionData.MasterKey;
            set
            {
                client.ServerConnectionData.MasterKey = value;
            }
        }

        public string ServerUrl
        {
            get => client.ServerConnectionData.ServerURI;
            set
            {
                client.ServerConnectionData.ServerURI = value;
            }
        }

        string serverAuthToken = "";
        string serverAuthType = "";

        public void SetServerAuthToken(string value)
        {
            serverAuthToken = value;
        }

        public string GetServerAuthToken()
        {
            return serverAuthToken;
        }

        public void SetServerAuthType(string value)
        {
            serverAuthToken = value;
        }

        public string GetServerAuthType()
        {
            return serverAuthToken;
        }

        public void SetLiveQueryServerURL(string value)
        {
            throw new NotImplementedException();
        }

        public string GetLiveQueryServerURL()
        {
            throw new NotImplementedException();
        }

        public void SetEncryptedUser(string value)
        {
            throw new NotImplementedException();
        }

        public string GetEncryptedUser()
        {
            throw new NotImplementedException();
        }

        public void SetSecret(string value)
        {
            throw new NotImplementedException();
        }

        public string GetSecret()
        {
            throw new NotImplementedException();
        }

        public void SetIdempotency(string value)
        {
            throw new NotImplementedException();
        }

        public string GetIdempotency()
        {
            throw new NotImplementedException();
        }

        public IParseAnalyticsController Analytics => client.AnalyticsController;
        public IParseCloudCodeController Cloud => client.CloudCodeController;
        public IParseConfigurationController Config => client.ConfigurationController;
        public IParseFileController File => client.FileController;
        public IParseInstallationController Installation => client.InstallationController;
        public IParseObjectController Object => client.ObjectController;
        public IParsePushController Push => client.PushController;
        public IParseQueryController Query => client.QueryController;
        public IParseSessionController Session => client.SessionController;
        public LocalDatastore<ParseObject> Storage { get; private set; }
        public IParseUserController User => client.UserController;

        public void EnableLocalDatastore()
        {
            Storage.IsEnabled = true;
        }

        public bool IsLocalDataStoreEnabled => Storage.IsEnabled;

        public IEnumerable<ParseObject> DumpLocalDatabase()
        {
            if (!Storage.IsEnabled)
            {
                Console.WriteLine("Moralis.EnableLocalDatastore() must be called first");
                return new List<ParseObject>();
            }

            return Storage.GetAllContents();
        }

        public async Task<Guid?> GetInstallationIdAsync() => await Installation.GetAsync();
    }
}
