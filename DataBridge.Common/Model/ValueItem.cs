namespace DataBridge
{
    public class ValueItem : ModelBase
    {
        public ValueItem()
        {
        }

        public ValueItem(string value)
        {
            this.Title = value;
            this.Value = value;
        }

        public ValueItem(string title, string value)
        {
            this.Title = title;
            this.Value = value;
        }

        public string Description { get; set; }

        public string Title { get; set; }

        public object Value { get; set; }

        public override string ToString()
        {
            return this.Title + " = " + this.Value;
        }
    }
}