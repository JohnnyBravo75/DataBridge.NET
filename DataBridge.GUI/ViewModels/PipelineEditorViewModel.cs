using DataBridge.GUI.Core.View;

namespace DataBridge.GUI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
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
        private DelegateCommand savePipelineCommand;
        private DelegateCommand addStepCommand;

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
                    this.RaisePropertyChanged("CurrentPipelineCommands");
                    this.RaisePropertyChanged("CurrentPipelineSteps");
                    if (this.savePipelineCommand != null)
                    {
                        this.savePipelineCommand.RaiseCanExecuteChanged();
                    }
                    if (this.addStepCommand != null)
                    {
                        this.addStepCommand.RaiseCanExecuteChanged();
                    }
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
                    if (this.savePipelineCommand != null)
                    {
                        this.savePipelineCommand.RaiseCanExecuteChanged();
                    }

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

        /// <summary>
        /// Returns a new list instance wrapping Pipeline.Commands so that
        /// WPF detects a change when RaisePropertyChanged is called.
        /// </summary>
        public IList<DataCommand> CurrentPipelineCommands
        {
            get
            {
                if (this.currentPipeline == null)
                {
                    return null;
                }
                return new System.Collections.Generic.List<DataCommand>(this.currentPipeline.Commands);
            }
        }

        /// <summary>
        /// Returns PipelineStepViewModels for all top-level commands, each carrying
        /// the command itself, matched parameter flow to the next step, and validation state.
        /// </summary>
        public IList<PipelineStepViewModel> CurrentPipelineSteps
        {
            get
            {
                if (this.currentPipeline == null)
                {
                    return null;
                }

                var commands = this.currentPipeline.Commands;
                var steps = new List<PipelineStepViewModel>(commands.Count);
                for (int i = 0; i < commands.Count; i++)
                {
                    var next = i + 1 < commands.Count ? commands[i + 1] : null;
                    var previous = commands.Take(i).ToList();
                    steps.Add(new PipelineStepViewModel(commands[i], next, previous));
                }
                return steps;
            }
        }

        public DelegateCommand AddStepCommand
        {
            get
            {
                return this.addStepCommand ?? (this.addStepCommand = new DelegateCommand(
                    this.AddStep,
                    () => this.CurrentPipeline != null));
            }
        }

        public void AddStep()
        {
            if (this.CurrentPipeline == null)
            {
                return;
            }

            var viewModel = new AddCommandViewModel();
            ViewManager.Instance.ShowWindow("AddCommandWindow",
                (result, resultType) =>
                {
                    if (result is AddCommandViewModel)
                    {
                        var addVm = (AddCommandViewModel)result;
                        if (addVm.SelectedCommand != null)
                        {
                            this.CurrentPipeline.Commands.Add(addVm.SelectedCommand);
                            // List<T> has no change notification — force ItemsControl refresh
                            // by raising change on a sub-path that WPF re-evaluates
                            this.RaisePropertyChanged("CurrentPipeline");
                            this.RaisePropertyChanged("CurrentPipelineCommands");
                            this.RaisePropertyChanged("CurrentPipelineSteps");
                        }
                    }
                },
                viewModel);
        }

        public DelegateCommand SavePipelineCommand
        {
            get
            {
                return this.savePipelineCommand ?? (this.savePipelineCommand = new DelegateCommand(
                    this.SaveCurrentPipeline,
                    () => this.CurrentPipeline != null && this.CurrentPipelineInfo != null));
            }
        }

        public void SaveCurrentPipeline()
        {
            if (this.CurrentPipeline == null || this.CurrentPipelineInfo == null)
            {
                return;
            }

            try
            {
                var path = Path.Combine(this.ConfigDirectory, this.currentPipelineInfo.FileName);
                Pipeline.Save(path, this.CurrentPipeline);
                MessageBox.Show(string.Format("Pipeline '{0}' wurde gespeichert.", this.CurrentPipelineInfo.Name),
                    "Speichern", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Fehler beim Speichern: {0}", ex.Message),
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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