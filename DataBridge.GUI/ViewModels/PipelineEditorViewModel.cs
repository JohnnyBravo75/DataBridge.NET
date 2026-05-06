using DataBridge.GUI.Core.View;

namespace DataBridge.GUI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using DataBridge;
    using DataBridge.GUI.Core.View.ViewModels;
    using DataBridge.GUI.Model;
    using DataBridge.GUI.Services;
    using DataBridge.Runtime;
    using Microsoft.Practices.Prism.Commands;
    using Microsoft.Practices.Unity;

    public class PipelineEditorViewModel : ViewModelBase
    {
        private PipelineInfo currentPipelineInfo;
        private Pipeline currentPipeline;
        private DataBridgeInfo currentDataBridgeInfo;
        public string configDirectory;
        private DataCommand currentDataCommand;
        private DelegateCommand savePipelineCommand;
        private DelegateCommand addStepCommand;
        private DelegateCommand debugCommand;
        private DelegateCommand cancelDebugCommand;
        private DelegateCommand openPipelineXmlCommand;
        private readonly IDataCommandDebugService debugExecutionService;
        private CancellationTokenSource debugCancellationTokenSource;
        private bool isDebugRunning;
        private string debugStatusText;
        private string debugDurationText;

        [InjectionConstructor]
        public PipelineEditorViewModel()
            : this(new PipelineDebugExecutionService())
        {
        }

        public PipelineEditorViewModel(IDataCommandDebugService debugExecutionService)
        {
            this.debugExecutionService = debugExecutionService ?? new PipelineDebugExecutionService();
            this.DebugOutputEntries = new ObservableCollection<DebugLogEntry>();
            this.DebugErrorEntries = new ObservableCollection<DebugLogEntry>();
            this.DebugLogEntries = new ObservableCollection<DebugLogEntry>();
            this.DebugStatusText = "Bereit";
            this.DebugDurationText = "-";
        }

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
                    if (this.openPipelineXmlCommand != null)
                    {
                        this.openPipelineXmlCommand.RaiseCanExecuteChanged();
                    }
                    if (this.addStepCommand != null)
                    {
                        this.addStepCommand.RaiseCanExecuteChanged();
                    }
                    if (this.debugCommand != null)
                    {
                        this.debugCommand.RaiseCanExecuteChanged();
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
            set
            {
                this.configDirectory = value;
                if (this.openPipelineXmlCommand != null)
                {
                    this.openPipelineXmlCommand.RaiseCanExecuteChanged();
                }
            }
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

        public bool IsDebugRunning
        {
            get { return this.isDebugRunning; }
            private set
            {
                if (this.isDebugRunning != value)
                {
                    this.isDebugRunning = value;
                    this.RaisePropertyChanged("IsDebugRunning");
                    if (this.debugCommand != null)
                    {
                        this.debugCommand.RaiseCanExecuteChanged();
                    }
                    if (this.cancelDebugCommand != null)
                    {
                        this.cancelDebugCommand.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        public string DebugStatusText
        {
            get { return this.debugStatusText; }
            private set
            {
                if (this.debugStatusText != value)
                {
                    this.debugStatusText = value;
                    this.RaisePropertyChanged("DebugStatusText");
                }
            }
        }

        public string DebugDurationText
        {
            get { return this.debugDurationText; }
            private set
            {
                if (this.debugDurationText != value)
                {
                    this.debugDurationText = value;
                    this.RaisePropertyChanged("DebugDurationText");
                }
            }
        }

        public ObservableCollection<DebugLogEntry> DebugOutputEntries { get; private set; }

        public ObservableCollection<DebugLogEntry> DebugErrorEntries { get; private set; }

        public ObservableCollection<DebugLogEntry> DebugLogEntries { get; private set; }

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
                            if (this.debugCommand != null)
                            {
                                this.debugCommand.RaiseCanExecuteChanged();
                            }
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

        public DelegateCommand DebugCommand
        {
            get
            {
                return this.debugCommand ?? (this.debugCommand = new DelegateCommand(this.ExecuteDebug, this.CanExecuteDebug));
            }
        }

        public DelegateCommand CancelDebugCommand
        {
            get
            {
                return this.cancelDebugCommand ?? (this.cancelDebugCommand = new DelegateCommand(this.CancelDebug, () => this.IsDebugRunning));
            }
        }

        public DelegateCommand OpenPipelineXmlCommand
        {
            get
            {
                return this.openPipelineXmlCommand ?? (this.openPipelineXmlCommand = new DelegateCommand(this.OpenPipelineXml, this.CanOpenPipelineXml));
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

        private bool CanOpenPipelineXml()
        {
            if (this.CurrentPipelineInfo == null || string.IsNullOrWhiteSpace(this.ConfigDirectory))
            {
                return false;
            }

            var path = Path.Combine(this.ConfigDirectory, this.CurrentPipelineInfo.FileName);
            return File.Exists(path);
        }

        private void OpenPipelineXml()
        {
            if (!this.CanOpenPipelineXml())
            {
                return;
            }

            var path = Path.Combine(this.ConfigDirectory, this.CurrentPipelineInfo.FileName);

            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Fehler beim Öffnen der Pipeline-XML: {0}", ex.Message),
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteDebug()
        {
            return !this.IsDebugRunning && this.CurrentPipeline != null &&
                   this.CurrentPipeline.Commands != null &&
                   this.CurrentPipeline.Commands.Count > 0;
        }

        private async void ExecuteDebug()
        {
            if (!this.CanExecuteDebug())
            {
                return;
            }

            this.DebugOutputEntries.Clear();
            this.DebugErrorEntries.Clear();
            this.DebugLogEntries.Clear();

            this.IsDebugRunning = true;
            this.DebugStatusText = "Debug läuft...";
            this.DebugDurationText = "-";

            var startedAt = DateTime.Now;
            var cts = new CancellationTokenSource();
            this.debugCancellationTokenSource = cts;

            try
            {
                var progress = new Progress<DebugLogEntry>(this.AppendDebugEntry);
                var result = await this.debugExecutionService.ExecuteAsync(this.CurrentPipeline, progress, cts.Token);

                this.DebugStatusText = string.Format("{0} (ExitCode: {1})",
                    result.SummaryMessage ?? (result.Success ? "Erfolgreich" : "Fehlgeschlagen"),
                    result.ExitCode.HasValue ? result.ExitCode.Value.ToString() : "-");
                this.DebugDurationText = result.Duration.ToString(@"hh\:mm\:ss\.fff");
            }
            catch (OperationCanceledException)
            {
                this.DebugStatusText = "Debug-Ausführung wurde abgebrochen.";
                this.DebugDurationText = (DateTime.Now - startedAt).ToString(@"hh\:mm\:ss\.fff");
            }
            catch (Exception ex)
            {
                var entry = new DebugLogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = DebugLogLevel.Error,
                    Source = "PipelineEditor",
                    Message = ex.Message,
                    ExceptionText = ex.ToString()
                };

                this.AppendDebugEntry(entry);
                this.DebugStatusText = "Debug-Ausführung mit Exception fehlgeschlagen.";
                this.DebugDurationText = (DateTime.Now - startedAt).ToString(@"hh\:mm\:ss\.fff");
            }
            finally
            {
                if (ReferenceEquals(this.debugCancellationTokenSource, cts))
                {
                    this.debugCancellationTokenSource = null;
                }

                cts.Dispose();
                this.IsDebugRunning = false;
            }
        }

        private void CancelDebug()
        {
            if (!this.IsDebugRunning)
            {
                return;
            }

            var cts = this.debugCancellationTokenSource;
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
                this.DebugStatusText = "Abbruch angefordert...";
            }
        }

        private void AppendDebugEntry(DebugLogEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            this.DebugLogEntries.Add(entry);

            if (entry.Level == DebugLogLevel.Error || entry.Level == DebugLogLevel.Fatal)
            {
                this.DebugErrorEntries.Add(entry);
            }
            else
            {
                this.DebugOutputEntries.Add(entry);
            }
        }
    }
}