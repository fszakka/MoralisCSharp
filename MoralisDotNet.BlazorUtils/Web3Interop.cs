namespace MoralisDotNet.BlazorUtils
{
    using System.Threading.Tasks;
    using Microsoft.JSInterop;

    public class Web3Interop
    {
        private readonly IJSRuntime _jsRuntime;

        public Web3Interop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async ValueTask<string> EthPersonalSign(object data, string address, string? password = null)
        {
            return await _jsRuntime.InvokeAsync<string>("web3.eth.personal.sign", data, address, password);
        }
    }
}