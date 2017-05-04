using DataBridge.GUI.Core.View;

namespace DataBridge.GUI.ViewModels
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Controls.Primitives;
    using DataBridge.GUI.Core.View.ViewModels;
    using DataBridge.Runtime;
    using Microsoft.Practices.Prism.Commands;

    public class PipelineEditorViewModel : ViewModelBase
    {
        private PipelineInfo currentPipelineInfo;
        private Pipeline currentPipeline;
        private DataBridgeInfo currentDataBridgeInfo;
        public string configDirectory;
        private DataCommand currentDataCommand;

        public DataBridgeInfo CurrentDataBridgeInfo
        {
            get { return this.currentDataBridgeInfo; }
            set
            {
                if (this.currentDataBridgeInfo != value)
                {
                    this.currentDataBridgeInfo = value;
                    this.RaisePropertyChanged("CurrentDataBridgeInfo");
                }
            }
        }

        public Pipeline CurrentPipeline
        {
            get { return this.currentPipeline; }
            set
            {
                if (this.currentPipeline != value)
                {
                    this.currentPipeline = value;
                    this.RaisePropertyChanged("CurrentPipeline");
                }
            }
        }

        public PipelineInfo CurrentPipelineInfo
        {
            get { return this.currentPipelineInfo; }
            set
            {
                if (this.currentPipelineInfo != value)
                {
                    this.currentPipelineInfo = value;
                    this.RaisePropertyChanged("CurrentPipelineInfo");

                    if (this.currentPipelineInfo != null)
                    {
                        var path = Path.Combine(this.ConfigDirectory, this.currentPipelineInfo.FileName);
                        this.CurrentPipeline = Pipeline.Load(path);
                    }
                }
            }
        }

        public string ConfigDirectory
        {
            get { return this.configDirectory; }
            set { this.configDirectory = value; }
        }

        public DataCommand CurrentDataCommand
        {
            get { return this.currentDataCommand; }
            set
            {
                if (this.currentDataCommand != value)
                {
                    this.currentDataCommand = value;
                    this.RaisePropertyChanged("CurrentDataCommand");
                }
            }
        }

        public void EditCurrentDataCommand()
        {
            this.EditDataCommand(this.CurrentDataCommand);
        }

        public void EditDataCommand(DataCommand dataCommand)
        {
            var viewModel = new DataCommandViewModel();
            viewModel.CurrentDataCommand = dataCommand;
            ViewManager.Instance.ShowWindow("DataCommandWindow",
              (result, resultType) =>
              {
                  if (result is DataCommandViewModel)
                  {
                      //var currentDataCommand = (result as DataCommandViewModel).CurrentDataCommand;
                      //if (currentDataCommand != null)
                      //{
                      //    if (addToList)
                      //    {
                      //        this.CurrentProject.DataCommands.Add(currentDataCommand);
                      //        this.RaisePropertyChanged("DataCommands");
                      //    }
                      //}
                  }
              },
              viewModel);
        }
    }
}