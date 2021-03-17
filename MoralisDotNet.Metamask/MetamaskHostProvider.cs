using System;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;

namespace MoralisDotNet.Metamask
{
    public class MetamaskHostProvider : IEthereumHostProvider
    {
        public readonly IMetamaskInterop _metamaskInterop;
        public static MetamaskHostProvider Current { get; private set; }
        public string Name { get; } = "Metamask";
        public bool Available { get; private set; }
        public string SelectedAccount { get; private set; }
        public int SelectedNetwork { get; private set; }
        public bool Enabled { get; private set; }

        public string ConnectedChain { get; set; }

        private MetamaskInterceptor _metamaskInterceptor;

        public event Func<string, Task> SelectedAccountChanged;
        public event Func<int, Task> NetworkChanged;

        /// <summary>
        /// Returns chain id
        /// </summary>
        public event Func<string, Task> Connect;

        /// <summary>
        /// Error message
        /// </summary>
        public event Func<string, Task> Disconnect;

        public event Func<bool, Task> AvailabilityChanged;
        public event Func<bool, Task> EnabledChanged;

        /// <summary>
        /// Chain id
        /// </summary>
        public event Func<string, Task> ChainChanged;


        public event Func<ProviderMessage, Task> Message;

        public async Task<bool> CheckProviderAvailabilityAsync()
        {
            var result = await _metamaskInterop.CheckMetamaskAvailability();
            await ChangeMetamaskAvailableAsync(result);
            return result;
        }



        public Task<Web3> GetWeb3Async()
        {
            var web3 = new Web3 { Client = { OverridingRequestInterceptor = _metamaskInterceptor } };
            return Task.FromResult(web3);
        }

        public async Task<string> EnableProviderAsync()
        {
            var selectedAccount = await _metamaskInterop.EnableEthereumAsync();
            Enabled = !string.IsNullOrEmpty(selectedAccount);

            if (Enabled)
            {
                SelectedAccount = selectedAccount;
                if (SelectedAccountChanged != null)
                {
                    await SelectedAccountChanged.Invoke(selectedAccount);
                }
                return selectedAccount;
            }

            return null;
        }

        public async Task<string> GetProviderSelectedAccountAsync()
        {
            var result = await _metamaskInterop.GetSelectedAddress();
            await ChangeSelectedAccountAsync(result);
            return result;
        }

        public Task<int> GetProviderSelectedNetworkAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<string> SignMessageAsync(string message)
        {
            return await _metamaskInterop.SignAsync(message.ToHexUTF8());
        }

        public MetamaskHostProvider(IMetamaskInterop metamaskInterop)
        {
            _metamaskInterop = metamaskInterop;
            _metamaskInterceptor = new MetamaskInterceptor(_metamaskInterop, this);
            Current = this;
        }

        public async Task ChangeSelectedAccountAsync(string selectedAccount)
        {
            if (SelectedAccount != selectedAccount)
            {
                SelectedAccount = selectedAccount;
                if (SelectedAccountChanged != null)
                {
                    await SelectedAccountChanged.Invoke(selectedAccount);
                }
            }
        }

        public async Task ChangeSelectedNetworkAsync(int networkId)
        {
            if (SelectedNetwork != networkId)
            {
                SelectedNetwork = networkId;
                if (NetworkChanged != null)
                {
                    await NetworkChanged.Invoke(networkId);
                }
            }
        }

        public async Task OnConnectAsync(string chainId)
        {
            if (ConnectedChain != chainId)
            {
                ConnectedChain = chainId;
                if (Connect != null)
                {
                    await Connect.Invoke(chainId);
                }
            }
        }

        public async Task OnDisconnectAsync(string message)
        {
            if (Disconnect != null)
            {
                await Disconnect.Invoke(message);
            }
        }

        public async Task ChainChangedAsync(string chainId)
        {
            if (ChainChanged != null)
            {
                await ChainChanged.Invoke(chainId);
            }
        }


        public async Task OnMessageReceived(ProviderMessage message)
        {
            if (Message != null)
            {
                await Message.Invoke(message);
            }
        }

        public async Task ChangeMetamaskAvailableAsync(bool available)
        {
            Available = available;
            if (AvailabilityChanged != null)
            {
                await AvailabilityChanged.Invoke(available);
            }
        }

    }

    public class ProviderMessage
    {
        public string type { get; set; }
        public object data { get; set; }
    }
}
