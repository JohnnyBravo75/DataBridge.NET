using System.Collections.Generic;

namespace DataBridge.Commands
{
    public class ValueLooper : DataCommand
    {
        private List<ValueItem> valueItems = new List<ValueItem>();

        public ValueLooper()
        {
        }

        public List<ValueItem> ValueItems
        {
            get { return this.valueItems; }
            set { this.valueItems = value; }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParameters)
        {
            //inParameters = GetCurrentInParameters();

            foreach (var valueItem in this.ValueItems)
            {
                var outParameters = this.GetCurrentOutParameters();
                foreach (var parameter in valueItem)
                {
                    outParameters.AddOrUpdate(new CommandParameter(parameter));
                }

                yield return outParameters;
            }
        }
    }
}