using System.Collections.ObjectModel;

namespace DataBridge
{
    public class Conditions : ObservableCollection<Condition>
    {
        private ConnectionOperators connectionOperator = ConnectionOperators.And;
        private FilterTypes filterType = FilterTypes.Positve;

        public ConnectionOperators ConnectionOperator
        {
            get { return this.connectionOperator; }
            set { this.connectionOperator = value; }
        }

        public FilterTypes FilterType
        {
            get { return this.filterType; }
            set { this.filterType = value; }
        }
    }
}