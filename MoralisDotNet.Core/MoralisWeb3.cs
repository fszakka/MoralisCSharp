namespace MoralisDotNet.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Parse;
    using Parse.Abstractions.Internal;
    using Parse.Infrastructure;

    public class MoralisWeb3
    {
        public Moralis Moralis { get; set; }
        public MoralisWeb3(Moralis moralis)
        {
            Moralis = moralis;
        }

        public async Task<ParseUser> AuthenticateMoralisAsync(Func<string,Task<string>> getSignatureTask, string selectedAccount, Func<object, Task<string[]>> getWalletAccountsTask)
        {
            var signature = await getSignatureTask(GetSigningData());
            if (string.IsNullOrWhiteSpace(signature))
            {
                throw new Exception("Data not signed");
            }
 
            var authData = new Dictionary<string, object> { { "id", selectedAccount }, { "signature", signature }, { "data", GetSigningData() } };

            var user = await Moralis.ServiceHub.LogInWithAsync("moralisEth", authData, CancellationToken.None);
            if (user is null)
            {
                throw new Exception("Could not get User");
            }

            user.ACL = new ParseACL(user);

            var accounts = (await getWalletAccountsTask(null)).Select(a => a.ToLower()).ToList();
 
            if (user.Keys.Contains("accounts"))
            {
                accounts = accounts.Concat(user.Get<List<object>>("accounts").Select(s => s.ToString())).Distinct().ToList();
            }
            user.Set("accounts", accounts);
            user.Set("ethAddress", Moralis.EthAddress);
            await user.SaveAsync();
            return user;
        }

        public async Task<ParseUser> LinkAsync(string selectedAccount, Func<object, Task<string[]>> getWalletAccountsTask, Func<string, Task<string>> getSignatureTask)
        {
            var user = await Moralis.ServiceHub.CurrentUserController.GetAsync(Moralis.ServiceHub);
            var query = new ParseQuery<ParseObject>(Moralis.ServiceHub, "_EthAddress");
            ParseObject ethAddressRecord = null;
            try
            {
                ethAddressRecord = await query.GetAsync(selectedAccount);
            }
            catch (ParseFailureException)
            {
                user = await Moralis.ServiceHub.CurrentUserController.GetAsync(Moralis.ServiceHub);
            }

            if (ethAddressRecord != null)
            {
                var signature = await getSignatureTask(GetSigningData());

                if (string.IsNullOrWhiteSpace(signature))
                {
                    throw new Exception("Data not signed");
                }
                var authData = new Dictionary<string, object> { { "id", selectedAccount }, { "signature", signature }, { "data", GetSigningData() } };
 
                await user.LinkWithAsync(Moralis.EthAddress, authData, CancellationToken.None);
            }
   
            var accounts = (await getWalletAccountsTask(null)).Select(a => a.ToLower()).ToList();

            if (user.Keys.Contains("accounts"))
            {
               
                accounts = accounts.Concat(user.Get<List<string>>("accounts")).Distinct().ToList();
            }
   
            user.Set("accounts", accounts);
            user.Set("ethAddress", Moralis.EthAddress);
            await user.SaveAsync();
            return user;
        }

        public async Task<ParseUser> UnlinkAsync(string account)
        {
            account = account.ToLower();
            var query = new ParseQuery<ParseObject>(Moralis.ServiceHub, "_EthAddress");
            var ethAddressRecord = await query.GetAsync(account);
            await ethAddressRecord.DeleteAsync();
            var user = await Moralis.ServiceHub.CurrentUserController.GetAsync(Moralis.ServiceHub);
            var accounts = user.Keys.Contains("accounts") ? user.Get<List<object>>("accounts").Select(s => s.ToString()) ?? new List<string>() : new List<string>();
            var nextAccounts = accounts.Where(v => v != account).ToArray();
            user.Set("accounts", nextAccounts);
            user.Set("ethAddress", nextAccounts.Length > 0 ? nextAccounts[0] : "");
            await user.SaveAsync();
            return user;
        }

        public string GetSigningData()
        {
            return "Moralis Authentication";
        }
    }
}