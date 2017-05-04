using DataBridge.Common.Services.Adapter;
using DataBridge.ConnectionInfos;
using DataBridge.Services;

namespace DataBridge.Handler.Services.Adapter
{
    public class AccessAdapter : DbAdapter, IDataAdapterBase
    {
        public AccessAdapter()
        {
            this.ConnectionInfo = new AccessConnectionInfo();
        }

        public string FileName
        {
            get { return (this.ConnectionInfo as AccessConnectionInfo).FileName; }
            set { (this.ConnectionInfo as AccessConnectionInfo).FileName = value; }
        }

        public bool CreateNewFile()
        {
            return this.CreateNewFile(this.ConnectionInfo.ConnectionString);
        }

        private bool CreateNewFile(string connectionString)
        {
            // create access database file with the ADO-ActiveX
            ADOX.Catalog catalog = new ADOX.Catalog();
            catalog.Create(connectionString);
            catalog = null;
            return true;
        }
    }
}