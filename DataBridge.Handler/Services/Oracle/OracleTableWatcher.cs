using System;
using System.Data;
using DataConnectors.Adapter.DbAdapter.ConnectionInfos;
using Oracle.DataAccess.Client;

namespace DataBridge.Services
{
    public class OracleTableWatcher : IDisposable
    {
        private OracleNativeDbAdapter dbAdapter = new OracleNativeDbAdapter();
        private OracleDependency oraDependency = new OracleDependency();
        private bool enableRaisingEvents = true;

        public event EventHandler<DataTable> OnChanged;

        public OracleTableWatcher()
        {
            this.oraDependency.QueryBasedNotification = false;
            this.oraDependency.OnChange += this.Dependency_OnChange;
        }

        public OracleNativeDbConnectionInfo ConnectionInfo
        {
            get
            {
                return this.dbAdapter.ConnectionInfo as OracleNativeDbConnectionInfo;
            }
            set
            {
                this.dbAdapter.ConnectionInfo = value;
            }
        }

        public bool EnableRaisingEvents
        {
            get { return this.enableRaisingEvents; }
            set { this.enableRaisingEvents = value; }
        }

        public bool Connect()
        {
            if (this.dbAdapter == null)
            {
                return false;
            }

            return this.dbAdapter.Connect();
        }

        public void AddWatchSource(string tableName)
        {
            var cmd = this.dbAdapter.Connection.CreateCommand() as OracleCommand;

            this.oraDependency.AddCommandDependency(cmd);

            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT * FROM " + tableName;
            cmd.Notification.IsNotifiedOnce = false;
            cmd.Notification.IsPersistent = true;
            cmd.AddRowid = true;
            cmd.ExecuteNonQuery();
        }

        private void Dependency_OnChange(object sender, OracleNotificationEventArgs eventArgs)
        {
            //DataTable dt = eventArgs.Details;

            //Debug.WriteLine("The following database objects were changed:");
            //foreach (string resource in eventArgs.ResourceNames)
            //{
            //    Debug.WriteLine(resource);
            //}

            //Debug.WriteLine("\n Details:");
            //Debug.Write(new string('*', 80));
            //for (int rows = 0; rows < dt.Rows.Count; rows++)
            //{
            //    Debug.WriteLine("Resource name: " + dt.Rows[rows].ItemArray[0]);
            //    string type = Enum.GetName(typeof(OracleNotificationInfo), dt.Rows[rows].ItemArray[1]);
            //    Debug.WriteLine("Change type: " + type);
            //    Debug.Write(new string('*', 80));
            //}

            if (this.OnChanged != null && this.EnableRaisingEvents)
            {
                this.OnChanged(this, eventArgs.Details);
            }
        }

        public bool Disconnect()
        {
            if (this.dbAdapter == null)
            {
                return false;
            }

            return this.dbAdapter.Disconnect();
        }

        public void Dispose()
        {
            if (this.dbAdapter != null)
            {
                if (this.oraDependency != null)
                {
                    this.oraDependency.OnChange -= this.Dependency_OnChange;

                    if (this.dbAdapter.Connection is OracleConnection)
                    {
                        this.oraDependency.RemoveRegistration(this.dbAdapter.Connection as OracleConnection);
                    }

                    this.oraDependency = null;
                }

                this.Disconnect();

                this.dbAdapter.Dispose();
                this.dbAdapter = null;
            }
        }
    }
}