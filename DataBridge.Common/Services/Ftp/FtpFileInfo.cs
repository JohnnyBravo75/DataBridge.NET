using System;

namespace DataBridge.Services
{
    public class FtpFileInfo
    {
        private string fullName;

        public string Name { get; set; }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(this.fullName))
                {
                    return this.Name;
                }

                return this.fullName;
            }
            set
            {
                this.fullName = value;
            }
        }

        public int Size { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? LastWriteTime { get; set; }

        public string Flags { get; set; }

        public bool IsDirectory { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.FullName))
            {
                return base.ToString();
            }

            return this.FullName.ToString();
        }
    }
}