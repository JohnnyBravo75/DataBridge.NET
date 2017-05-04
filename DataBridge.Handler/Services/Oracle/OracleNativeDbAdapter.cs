namespace DataBridge.Services
{
    public class OracleNativeDbAdapter : DbAdapter
    {
        public OracleNativeDbAdapter()
        {
            this.ConnectionInfo = new OracleNativeDbConnectionInfo();
        }
    }
}