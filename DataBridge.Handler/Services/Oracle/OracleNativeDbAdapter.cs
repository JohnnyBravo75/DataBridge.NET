using DataConnectors.Adapter.DbAdapter;
using DataConnectors.Adapter.DbAdapter.ConnectionInfos;

namespace DataBridge.Services
{
    public class OracleNativeDbAdapter : OracleAdapter
    {
        public OracleNativeDbAdapter()
        {
            this.ConnectionInfo = new OracleNativeDbConnectionInfo();
        }
    }
}