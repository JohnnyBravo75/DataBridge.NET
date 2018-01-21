using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using DataBridge.Extensions;
using DataBridge.PropertyChanged;

namespace DataBridge
{
    [Serializable]
    [XmlType(TypeName = "Command")]
    public class DataCommand : NotifyPropertyChangedBase, IDataCommand, IDisposable
    {
        private CommandParameters executeParameters = new CommandParameters();
        private CommandParameters parameters = new CommandParameters();
        private List<DataCommand> commands = new List<DataCommand>();
        private bool useStreaming = true;
        private List<ParameterCondition> parameterConditions = new List<ParameterCondition>();
        private int streamingBlockSize = 100000;
        private WeakReference<Pipeline> weakpipeLine;
        private string customControlName;
        private string group;

        [XmlIgnore]
        [Browsable(false)]
        public WeakReference<Pipeline> WeakPipeLine
        {
            get { return this.weakpipeLine; }
            set
            {
                this.weakpipeLine = value;

                if (this.Commands != null)
                {
                    foreach (var pipelineCommand in this.Commands)
                    {
                        pipelineCommand.WeakPipeLine = value;
                    }
                }
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        public string Name
        {
            get { return this.GetType().Name; }
        }

        [XmlIgnore]
        [Browsable(false)]
        public string Title
        {
            get
            {
                if (this.CurrentDescriptionAttribute == null || string.IsNullOrEmpty(this.CurrentDescriptionAttribute.Title))
                {
                    return this.Name;
                }

                return this.CurrentDescriptionAttribute.Title;
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        protected DataCommandDescriptionAttribute CurrentDescriptionAttribute
        {
            get
            {
                // get his own DataCommandDescription attribute
                var attributes = this.GetType().GetCustomAttributes(typeof(DataCommandDescriptionAttribute), false);

                if (attributes.IsNullOrEmpty())
                {
                    // when no attribute found create a default attribute
                    return new DataCommandDescriptionAttribute()
                    {
                        Name = this.GetType().Name,
                        Title = this.GetType().Name,
                        Image = ""
                    };
                }

                return (DataCommandDescriptionAttribute)attributes[0];
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        public string Image
        {
            get
            {
                if (string.IsNullOrEmpty(this.CurrentDescriptionAttribute.Image))
                {
                    return "\\Resources\\Images\\Converter_h24.png";
                }

                return this.CurrentDescriptionAttribute.Image;
            }
        }

        /// <summary>
        /// Gets or sets the group. the group is a string (can be a path) e.g. "Common\FieldConverters\Address"
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public string Group
        {
            get
            {
                if (this.CurrentDescriptionAttribute.Group != null)
                {
                    return this.CurrentDescriptionAttribute.Group;
                }

                return this.group;
            }

            set
            {
                this.group = value;
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        public bool UseStreaming

        {
            get { return this.useStreaming; }
            set { this.useStreaming = value; }
        }

        [XmlIgnore]
        [Browsable(false)]
        public int StreamingBlockSize
        {
            get { return this.streamingBlockSize; }
            set
            {
                if (value > 0)
                {
                    this.streamingBlockSize = value;
                }
            }
        }

        public DataCommand()
        {
            this.SetCurrentThreadName();
        }

        protected void SetCurrentThreadName()
        {
            if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
            {
                Thread.CurrentThread.Name = this.GetType().Name + "Thread";
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        public CommandParameters ExecuteParameters
        {
            get { return this.executeParameters; }
            set { this.executeParameters = value; }
        }

        [XmlIgnore]
        [Browsable(false)]
        public DataCommand Parent { get; set; }

        [Browsable(false)]
        public CommandParameters Parameters
        {
            get { return this.parameters; }
            set { this.parameters = value; }
        }

        [Browsable(false)]
        public List<ParameterCondition> ParameterConditions
        {
            get { return this.parameterConditions; }
            set { this.parameterConditions = value; }
        }

        /// <summary>
        /// Gets the name of the custom control.
        /// </summary>
        /// <value>
        /// The name of the custom control.
        /// </value>
        [XmlIgnore]
        [Browsable(false)]
        public string CustomControlName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.customControlName))
                {
                    return this.customControlName;
                }

                return this.CurrentDescriptionAttribute.CustomControlName;
            }

            set
            {
                this.customControlName = value;
            }
        }

        public virtual bool ShouldSerializeParameterConditions()
        {
            return this.ParameterConditions != null && this.ParameterConditions.Any();
        }

        public delegate bool OnSignalNextDelegate(InitializationResult executionResult);

        [XmlIgnore]
        [Browsable(false)]
        public bool IsInitialized { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public OnSignalNextDelegate OnSignalNext { get; set; }

        private void InitExecuteParameters()
        {
            this.ExecuteParameters.Clear();
            foreach (var origParameter in this.Parameters)
            {
                this.ExecuteParameters.Add(origParameter.Clone());
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        public int LoopCounter { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        protected bool IsFirstExecution
        {
            get
            {
                return this.LoopCounter == 0;
            }
        }

        public void SetParameters(CommandParameters inParameters, OnSignalNextDelegate onSignalNext, DataCommand prevCmd = null)
        {
            this.OnSignalNext = onSignalNext;
            this.TransferInParameters(inParameters);
        }

        public virtual void Initialize()
        {
            if (this.IsInitialized)
            {
                this.DeInitialize();
            }

            this.IsInitialized = true;
            this.LogDebugFormat("Initialize '{0}'", this.GetType().Name);
        }

        public virtual bool BeforeExecute()
        {
            ConditionEvaluator.Evaluate(this.ParameterConditions, this.Parameters);
            //this.LogDebugFormat("Before Execute '{0}', LoopCounter='{1}'", this.GetType().Name, this.LoopCounter);

            return this.OnSignalNext.Invoke(new InitializationResult(this, this.LoopCounter));
        }

        public virtual bool AfterExecute()
        {
            return true;
        }

        public IEnumerable<CommandParameters> ExecuteCommand(IEnumerable<CommandParameters> inParameters)
        {
            // try-catch only possible with outimplemented enumerator, not with foreach-yield
            //foreach (var param in this.Execute(inParameters))
            //{
            //    yield return this.TransferOutParameters(param);
            //}

            var enumerator = this.Execute(inParameters).GetEnumerator();
            while (true)
            {
                CommandParameters param = null;
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    param = enumerator.Current;
                }
                catch (Exception ex)
                {
                    if (this.ErrorHandling == ErrorHandlingOptions.Raise)
                    {
                        throw;
                    }

                    break;
                }

                // the yield statement is outside the try catch block
                yield return this.TransferOutParameters(param);
            }
        }

        protected virtual IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParameters)
        {
            yield return this.GetCurrentOutParameters();
        }

        [XmlIgnore]
        [Browsable(false)]
        public virtual List<DataCommand> Commands
        {
            get { return this.commands; }
            set
            {
                this.commands = value;
            }
        }

        /// <summary>
        /// Proxy for xml serialization, to keep the parent property in sync.
        /// </summary>
        /// <value>
        /// The pipeline commands proxy.
        /// </value>
        [XmlArray("Commands", IsNullable = false)]
        [Browsable(false)]
        public virtual DataCommand[] DataCommandsProxy
        {
            get
            {
                return this.commands.ToArray();
            }
            set
            {
                this.commands.Clear();
                if (value != null)
                {
                    foreach (var child in value)
                    {
                        this.commands.Add(child);
                        child.Parent = this;
                    }
                }
            }
        }

        public virtual bool ShouldSerializeCommandsProxy()
        {
            return this.Commands != null && this.Commands.Any();
        }

        public DataCommand GetNextNode()
        {
            if (this.HasChildCommands)
            {
                return this.Commands.First();
            }
            else
            {
                var nextSibling = this.GetNextSibling();
                if (nextSibling != null)
                {
                    return nextSibling;
                }
                else
                {
                    DataCommand current = this;
                    while (true)
                    {
                        var parent = current.Parent;
                        if (parent == null)
                        {
                            return null;
                        }
                        else
                        {
                            var nextSibling2 = parent.GetNextSibling();
                            if (nextSibling2 != null)
                            {
                                return nextSibling2;
                            }
                            else
                            {
                                current = parent;
                            }
                        }
                    }
                }
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        public bool HasChildCommands
        {
            get
            {
                return this.commands.Any();
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        public bool IsRoot
        {
            get
            {
                return this.Parent == null;
            }
        }

        private ErrorHandlingOptions errorHandling = ErrorHandlingOptions.Raise;

        [XmlAttribute("OnError")]
        public ErrorHandlingOptions ErrorHandling
        {
            get { return this.errorHandling; }
            set { this.errorHandling = value; }
        }

        public DataCommand GetNextSibling()
        {
            return this.GetNextSibling(this);
        }

        public virtual DataCommand GetFirstChild()
        {
            return this.Commands.FirstOrDefault();
        }

        public DataCommand GetNextSibling(DataCommand node)
        {
            if (this.Parent == null)
            {
                return null;
            }

            if (node == null)
            {
                return null;
            }

            var foundNode = this.Parent.Commands.SkipWhile(i => !i.Equals(node));

            var nextNode = foundNode.Skip(1).FirstOrDefault();
            return nextNode;
        }

        public bool IsFirstSibling()
        {
            return this.IsFirstSibling(this);
        }

        public bool IsFirstSibling(DataCommand node)
        {
            if (this.Parent == null)
            {
                return false;
            }

            if (node == null)
            {
                return false;
            }

            var foundNode = this.Parent.Commands.FirstOrDefault();

            return foundNode == node;
        }

        public bool IsLastSibling()
        {
            return this.IsLastSibling(this);
        }

        public bool IsLastSibling(DataCommand node)
        {
            if (this.Parent == null)
            {
                return false;
            }

            if (node == null)
            {
                return false;
            }

            var foundNode = this.Parent.Commands.LastOrDefault();

            return foundNode == node;
        }

        public void AddChild(DataCommand childNode, int index = -1)
        {
            if (index < -1)
            {
                throw new ArgumentException("The index can not be lower then -1");
            }
            if (index > this.Commands.Count() - 1)
            {
                throw new ArgumentException(string.Format("The index ({0}) can not be higher then index of the last item. Use the AddChild() method without an index to add at the end", index));
            }
            if (!childNode.IsRoot)
            {
                throw new ArgumentException(string.Format("The child node with value [{0}] can not be added because it is not a root node.", childNode.ToString()));
            }

            childNode.Parent = this;
            if (index == -1)
            {
                this.commands.Add(childNode);
            }
            else
            {
                this.commands.Insert(index, childNode);
            }
        }

        public DataCommand GetNextCommand()
        {
            return this.GetNextNode() as DataCommand;
        }

        public virtual void DeInitialize()
        {
            this.IsInitialized = false;
            this.LogDebugFormat("Deinitialize '{0}'", this.GetType().Name);
        }

        public virtual IList<string> Validate(CommandParameters parameters, ValidationContext context = ValidationContext.Static)
        {
            var messages = new List<string>();
            if (parameters == null)
            {
                parameters = this.Parameters;
            }

            foreach (var parameter in parameters)
            {
                if (parameter.NotNull && !parameter.HasValue)
                {
                    messages.Add(string.Format("The parameter '{0}' must not be empty", parameter.Name));
                }
            }

            return messages;
        }

        //**************************************************************************************

        protected CommandParameters GetCurrentInParameters()
        {
            var inParameters = new CommandParameters();
            inParameters.AddRange(this.ExecuteParameters.Where(x => x.Direction == Directions.In || x.Direction == Directions.InOut));
            return inParameters;
        }

        public CommandParameters GetCurrentOutParameters()
        {
            var outParameters = new CommandParameters();
            outParameters.AddRange(this.ExecuteParameters.Where(x => x.Direction == Directions.Out || x.Direction == Directions.InOut));
            return outParameters;
        }

        protected CommandParameters TransferOutParameters(CommandParameters parameters)
        {
            foreach (var parameter in parameters)
            {
                if (!string.IsNullOrEmpty(parameter.Token) && parameter.Name != parameter.Token)
                {
                    parameter.Name = parameter.Token;
                }
            }

            return parameters;
        }

        protected void TransferInParameters(CommandParameters inParameters)
        {
            if (inParameters == null)
            {
                inParameters = new CommandParameters();
            }

            this.InitExecuteParameters();

            foreach (var inParameter in inParameters)
            {
                var parameter = this.ExecuteParameters.FirstOrDefault(x => x.Token == inParameter.Name);
                if (parameter != null && !parameter.HasValue)
                {
                    parameter.Value = inParameter.Value;
                }
                else
                {
                    if (!this.ExecuteParameters.Any(x => x.Name == inParameter.Name))
                    {
                        this.ExecuteParameters.AddOrUpdate(inParameter);
                    }
                }
            }

            var currentInParameters = this.GetCurrentInParameters();

            foreach (var currentInParameter in currentInParameters)
            {
                string newValue = null;
                if (TokenProcessor.ReplaceTokens(currentInParameter.Value.ToStringOrEmpty(), inParameters.ToDictionary(), out newValue))
                {
                    currentInParameter.Value = newValue;
                }

                if (currentInParameter.DataType == DataTypes.String)
                {
                    currentInParameter.Value = TokenProcessor.ExpandEnvironmentVariables(currentInParameter.Value.ToStringOrEmpty());
                }
            }
        }

        public virtual void Dispose()
        {
            this.DeInitialize();

            this.OnSignalNext = null;

            if (this.WeakPipeLine != null)
            {
                this.WeakPipeLine.SetTarget(null);
                this.WeakPipeLine = null;
            }

            if (this.commands != null)
            {
                foreach (var pipelineCommand in this.commands)
                {
                    pipelineCommand.Dispose();
                }

                this.commands.Clear();
            }
        }

        protected void LogDebug(string logText)
        {
            this.LogDebugFormat(logText);
        }

        [Browsable(false)]
        protected string PipelineName
        {
            get
            {
                if (this.WeakPipeLine == null)
                {
                    return string.Empty;
                }
                Pipeline pipeline;
                this.WeakPipeLine.TryGetTarget(out pipeline);
                if (pipeline != null)
                {
                    return pipeline.Name;
                }

                return string.Empty;
            }
        }

        protected void LogDebugFormat(string logText, params object[] args)
        {
            LogManager.Instance.LogNamedDebugFormat(this.PipelineName, this.GetType(), logText, args);
        }

        public void LogErrorFormat(string logText, params object[] args)
        {
            LogManager.Instance.LogNamedErrorFormat(this.PipelineName, this.GetType(), logText, args);
        }

        protected void SetToken(string token, object value)
        {
            TokenManager.Instance.SetToken(token, value, this.PipelineName);
        }

        protected void SetTokens(IDictionary<string, object> tokens)
        {
            TokenManager.Instance.SetTokens(tokens, this.PipelineName);
        }

        protected IDictionary<string, object> GetTokens()
        {
            return TokenManager.Instance.Tokens.GetValue(this.PipelineName);
        }

        public event Action<CommandParameters> OnParametersIncoming;
    }

    public class InitializationResult
    {
        public InitializationResult(DataCommand command, DataCommand prevCmd = null)
        {
            this.Command = command;
            this.PrevCommand = prevCmd;
        }

        public InitializationResult(DataCommand command, int loopCounter)
        {
            this.Command = command;
            this.LoopCounter = loopCounter;
        }

        public DataCommand Command { get; private set; }

        public DataCommand PrevCommand { get; private set; }

        public int LoopCounter { get; set; }
    }

    public class InitializationParameter
    {
        private bool useStreaming = true;

        public bool UseStreaming
        {
            get { return this.useStreaming; }
            set { this.useStreaming = value; }
        }
    }

    public enum ErrorHandlingOptions
    {
        Raise = 0,
        Continue = 1
    }
}