namespace DataBridge.Services
{
    public class OracleNativeDbAdapter : DbAdapter
    {
        public OracleNativeDbAdapter()
        {
            this.DbConnectionInfo = new OracleNativeDbConnectionInfo();
        }
    }
}