using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataBridge.Commands
{
    public class ConditionChecker : DataCommand
    {
        private CommandCondition matchingCondition = null;
        private List<CommandCondition> commandConditions = new List<CommandCondition>();

        public List<CommandCondition> CommandConditions
        {
            get { return this.commandConditions; }
            set { this.commandConditions = value; }
        }

        public override bool ShouldSerializeCommandsProxy()
        {
            return false;
        }

        [XmlIgnore]
        public override List<DataCommand> Commands
        {
            get
            {
                if (this.matchingCondition == null)
                {
                    return new List<DataCommand>();
                }

                return this.matchingCondition.Commands;
            }
            set
            {
                if (this.matchingCondition == null)
                {
                    throw new Exception("Setting the collection is not allowed");
                }

                this.matchingCondition.Commands = value;
            }
        }

        public ConditionChecker()
        {
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            this.matchingCondition = ConditionEvaluator.GetFirstMatchingCondition(this.CommandConditions, inParameters.ToDictionary()) as CommandCondition;

            var outParameters = this.GetCurrentOutParameters();
            yield return outParameters;
        }
    }
}