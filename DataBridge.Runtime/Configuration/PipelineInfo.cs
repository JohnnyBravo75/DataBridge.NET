using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DataBridge.Runtime
{
    [Serializable]
    public class PipelineInfo
    {
        private bool isActive = true;
        private string fileName = "";
        private string name = "";

        [XmlAttribute]
        public bool IsActive
        {
            get { return this.isActive; }
            set { this.isActive = value; }
        }

        [XmlAttribute]
        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }

        [XmlIgnore]
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(this.name))
                {
                    return Path.GetFileNameWithoutExtension(this.FileName);
                }

                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public void SetDefaultValues()
        {
            this.FileName = Path.Combine(Environment.CurrentDirectory, DataBridgeManager.Instance.ConfigFolderName, "pipeline.config");
        }

        [XmlIgnore]
        public string LogFile
        {
            get
            {
                return Path.Combine(LogManager.Instance.DefaultLogDirectory, this.Name + ".log");
            }
        }

        private Dictionary<string, object> properties = new Dictionary<string, object>();

        [XmlIgnore]
        public Dictionary<string, object> Properties
        {
            get { return this.properties; }
            set { this.properties = value; }
        }
    }
}