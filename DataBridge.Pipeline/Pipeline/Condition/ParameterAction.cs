namespace DataBridge
{
    public class ParameterAction
    {
        private string token = "";

        private object @value;

        private string valueToken = "";

        public string Token
        {
            get
            {
                return this.token;
            }

            set
            {
                this.token = value;
            }
        }

        public object Value
        {
            get
            {
                if (!string.IsNullOrEmpty(this.ValueToken))
                {
                    object val = TokenManager.Instance.GetTokenValue(this.ValueToken);

                    if (val != null)
                    {
                        return val;
                    }
                }

                return this.@value;
            }

            set
            {
                if (this.@value != value)
                {
                    this.@value = value;
                }
            }
        }

        public string ValueToken
        {
            get
            {
                return this.valueToken;
            }

            set
            {
                if (this.valueToken != value)
                {
                    this.valueToken = value;
                }
            }
        }
    }
}