namespace DataBridge
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public class DataCommandDescriptionAttribute : System.Attribute
    {
        /// <summary>
        /// Eine ViewDescription
        /// </summary>
        private DataCommandDescription dataCommandDescription = new DataCommandDescription();

        public DataCommandDescriptionAttribute()
        {
        }

        public DataCommandDescriptionAttribute(string name, string title)
        {
            this.Name = name;
            this.Title = title;
            this.dataCommandDescription = new DataCommandDescription(this.Name, this.Title);
        }

        public DataCommandDescriptionAttribute(string name, string title, string group, string image)
        {
            this.Name = name;
            this.Title = title;
            this.Group = group;
            this.Image = image;
            this.dataCommandDescription = new DataCommandDescription(this.Name, this.Title, this.Group, this.Image);
        }

        public string Name
        {
            get { return this.dataCommandDescription.Name; }
            set { this.dataCommandDescription.Name = value; }
        }

        public string Title
        {
            get { return this.dataCommandDescription.Title; }
            set { this.dataCommandDescription.Title = value; }
        }

        public string Group
        {
            get { return this.dataCommandDescription.Group; }
            set { this.dataCommandDescription.Group = value; }
        }

        public string Image
        {
            get { return this.dataCommandDescription.Image; }
            set { this.dataCommandDescription.Image = value; }
        }

        public bool MultiThreaded
        {
            get { return this.dataCommandDescription.MultiThreaded; }
            set { this.dataCommandDescription.MultiThreaded = value; }
        }

        public string CustomControlName
        {
            get { return this.dataCommandDescription.CustomControlName; }
            set { this.dataCommandDescription.CustomControlName = value; }
        }

        public DataCommandDescription DataCommandDescription
        {
            get
            {
                return this.dataCommandDescription;
            }
        }
    }
}