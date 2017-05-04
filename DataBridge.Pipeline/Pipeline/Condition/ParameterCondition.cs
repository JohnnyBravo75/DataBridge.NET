using System.Collections.Generic;

namespace DataBridge
{
    public class ParameterCondition : Condition
    {
        private List<ParameterAction> actions = new List<ParameterAction>();

        public List<ParameterAction> Actions
        {
            get { return this.actions; }
            set { this.actions = value; }
        }
    }
}