namespace MoralisDotNet.BlazorUtils
{
    using System.Threading.Tasks;
    using Microsoft.JSInterop;
    using Metamask;
    using Nethereum.JsonRpc.Client.RpcMessages;
    using Newtonsoft.Json;

    public class  MetamaskInterop : IMetamaskInterop
    {
        private readonly IJSRuntime _jsRuntime;

        public MetamaskInterop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async ValueTask<string> EnableEthereumAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("MetamaskInterop.EnableEthereum");
        }

        public async ValueTask<bool> CheckMetamaskAvailability()
        {
            return await _jsRuntime.InvokeAsync<bool>("MetamaskInterop.IsMetamaskAvailable");
        }

        public async ValueTask<RpcResponseMessage> SendAsync(RpcRequestMessage rpcRequestMessage)
        {
            var response = await _jsRuntime.InvokeAsync<string>("MetamaskInterop.Request", JsonConvert.SerializeObject(rpcRequestMessage));
            return JsonConvert.DeserializeObject<RpcResponseMessage>(response);
        }

        public async ValueTask<RpcResponseMessage> SendTransactionAsync(MetamaskRpcRequestMessage rpcRequestMessage)
        {
            var response = await _jsRuntime.InvokeAsync<string>("MetamaskInterop.Request", JsonConvert.SerializeObject(rpcRequestMessage));
            return JsonConvert.DeserializeObject<RpcResponseMessage>(response);
        }

        public async ValueTask<string> SignAsync(string utf8Hex)
        {
            var result = await _jsRuntime.InvokeAsync<string>("MetamaskInterop.Sign", utf8Hex);
            return result.Trim('"');
        }

        public async ValueTask<string> GetSelectedAddress()
        {
            return await _jsRuntime.InvokeAsync<string>("MetamaskInterop.GetSelectedAddress");
        }


        [JSInvokable()]
        public static async Task MetamaskAvailableChanged(bool available)
        {
            await MetamaskHostProvider.Current.ChangeMetamaskAvailableAsync(available);
        }

        [JSInvokable()]
        public static async Task SelectedAccountChanged(string selectedAccount)
        {
            await MetamaskHostProvider.Current.ChangeSelectedAccountAsync(selectedAccount);
        }

        [JSInvokable()]
        public static async Task NetworkChanged(int networkId)
        {
            await MetamaskHostProvider.Current.ChangeSelectedNetworkAsync(networkId);
        }

        [JSInvokable()]
        public static async Task OnConnect(string chainId)
        {
            await MetamaskHostProvider.Current.OnConnectAsync(chainId);
        }

        [JSInvokable()]
        public static async Task OnDisconnect(string message)
        {
            await MetamaskHostProvider.Current.OnDisconnectAsync(message);
        }

        [JSInvokable()]
        public static async Task ChainChanged(string chainId)
        {
            await MetamaskHostProvider.Current.ChainChangedAsync(chainId);
        }

        [JSInvokable()]
        public static async Task OnMessageReceived(ProviderMessage message)
        {
            await MetamaskHostProvider.Current.OnMessageReceived(message);
        }
    }
}