namespace DataBridge
{
    public class DataCommandDescription
    {
        private string group;
        private bool multiThreaded = true;
        private string customControlName;

        public DataCommandDescription()
        {
        }

        public DataCommandDescription(string name, string title)
        {
            this.Name = name;
            this.Title = title;
        }

        public DataCommandDescription(string name, string title, string group, string image)
        {
            this.Name = name;
            this.Title = title;
            this.Group = group;
            this.Image = image;
        }

        public string Name { get; internal set; }

        public string Title { get; internal set; }

        public string Group
        {
            get { return this.group; }
            internal set { this.group = value; }
        }

        public bool MultiThreaded
        {
            get { return this.multiThreaded; }
            internal set { this.multiThreaded = value; }
        }

        public string CustomControlName
        {
            get { return this.customControlName; }
            internal set { this.customControlName = value; }
        }

        public string Image { get; internal set; }
    }
}