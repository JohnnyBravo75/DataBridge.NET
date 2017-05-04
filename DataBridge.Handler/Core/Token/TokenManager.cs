using System.Collections.Generic;
using DataBridge.Extensions;

namespace DataBridge
{
    public class TokenManager
    {
        private const string DEFAULTSTORE = "Default";
        private static readonly TokenManager instance = new TokenManager();
        private IDictionary<string, IDictionary<string, object>> tokens = new Dictionary<string, IDictionary<string, object>>();
        private TokenProcessor tokenProcessor = new TokenProcessor();

        public static TokenManager Instance
        {
            get
            {
                return instance;
            }
        }

        public IDictionary<string, IDictionary<string, object>> Tokens
        {
            get
            {
                return this.tokens;
            }

            set
            {
                this.tokens = value;
            }
        }

        public bool SetToken(string token, object value, string store = DEFAULTSTORE)
        {
            var tokenStore = this.Tokens.GetValue(store);
            if (tokenStore == null)
            {
                this.Tokens.AddOrUpdate(store, new Dictionary<string, object>()
                {
                    { token, value }
                });
                return true;
            }

            tokenStore.AddOrUpdate(token, value);

            return true;
        }

        public bool SetTokens(IDictionary<string, object> tokens, string store = DEFAULTSTORE)
        {
            var tokenStore = this.Tokens.GetValue(store);
            if (tokenStore == null)
            {
                this.Tokens.AddOrUpdate(store, tokens);
                return true;
            }

            tokenStore.AddOrUpdateRange(tokens);

            return true;
        }

        public object GetTokenValue(string token, string store = DEFAULTSTORE)
        {
            var tokenStore = this.Tokens.GetValueOrAdd(store);
            if (tokenStore == null)
            {
                return null;
            }

            return tokenStore.GetValue(token);
        }

        public string ReplaceTokens(string str, IDictionary<string, object> extraTokens = null, string store = DEFAULTSTORE)
        {
            var tokenStore = this.Tokens.GetValueOrAdd(store);
            if (tokenStore != null)
            {
                str = TokenProcessor.ReplaceTokens(str, tokenStore);
            }

            str = TokenProcessor.ReplaceTokens(str, extraTokens);
            return str;
        }
    }
}