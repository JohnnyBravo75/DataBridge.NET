using System.Collections.Generic;

namespace DataBridge
{
    public class CommandCondition : Condition
    {
        private List<DataCommand> commands = new List<DataCommand>();

        public List<DataCommand> Commands
        {
            get { return this.commands; }
            set { this.commands = value; }
        }
    }
}