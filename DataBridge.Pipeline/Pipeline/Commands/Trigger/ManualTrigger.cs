using System.Collections.Generic;

namespace DataBridge.Commands
{
    public class ManualTrigger : DataCommand
    {
        public override bool BeforeExecute()
        {
            this.LogDebugFormat("Triggered manually");

            return this.OnSignalNext.Invoke(new InitializationResult(this, this.LoopCounter));
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParameters)
        {
            return base.Execute(inParameters);
        }

        public override void Dispose()
        {
            this.DeInitialize();
        }
    }
}