using System.IO;
using System.Threading;
using System.Xml.Serialization;
using DataBridge.Common.Helper;
using DataBridge.Extensions;

namespace DataBridge.Commands
{
    public class FileSystemTrigger : DataCommand
    {
        private FileSystemWatcher watcher;
        private bool isFirstWatch = true;
        private bool startInitial = true;
        private FileSystemEventArgs lastFileEventArgs;

        public enum WatchModes
        {
            Created,
            Deleted,
            Changed,
            All
        }

        public FileSystemTrigger()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Directory", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "FileFilter", Direction = Directions.In, Value = "*.*" });
            this.Parameters.Add(new CommandParameter() { Name = "WatchMode", Direction = Directions.In, Value = WatchModes.Created });
            this.Parameters.Add(new CommandParameter() { Name = "IncludeSubdirectories", Direction = Directions.In, Value = false });
        }

        [XmlIgnore]
        public string Directory
        {
            get
            {
                var directory = this.Parameters.GetValueOrDefault<string>("Directory", (this.watcher != null ? this.watcher.Path : ""));
                EnvironmentHelper.ExpandEnvironmentVariables(directory);
                return directory;
            }
            set { this.Parameters.SetOrAddValue("Directory", value); }
        }

        [XmlIgnore]
        public string FileFilter
        {
            get { return this.Parameters.GetValueOrDefault<string>("FileFilter", "*.*"); }
            set { this.Parameters.SetOrAddValue("FileFilter", value); }
        }

        [XmlIgnore]
        public WatchModes WatchMode
        {
            get { return this.Parameters.GetValueOrDefault<WatchModes>("WatchMode", WatchModes.Created); }
            set { this.Parameters.SetOrAddValue("WatchMode", value); }
        }

        [XmlIgnore]
        public bool IncludeSubdirectories
        {
            get { return this.Parameters.GetValueOrDefault<bool>("IncludeSubdirectories", false); }
            set { this.Parameters.SetOrAddValue("IncludeSubdirectories", value); }
        }

        [XmlIgnore]
        public bool WaitForIdleFile
        {
            get { return this.Parameters.GetValueOrDefault<bool>("WaitForIdleFile", false); }
            set { this.Parameters.SetOrAddValue("WaitForIdleFile", value); }
        }

        public override void Initialize()
        {
            base.Initialize();

            this.watcher = new FileSystemWatcher();
            this.watcher.Changed += this.Watcher_Changed;
            this.watcher.Created += this.Watcher_Created;
            this.watcher.Deleted += this.Watcher_Deleted;
            this.watcher.NotifyFilter = NotifyFilters.LastWrite
                                      | NotifyFilters.FileName
                                      | NotifyFilters.DirectoryName;

            this.watcher.IncludeSubdirectories = this.IncludeSubdirectories;
            this.watcher.Path = this.Directory;
            this.watcher.Filter = this.FileFilter;

            this.LogDebugFormat("Listening for '{0}' files in '{1}'...", this.watcher.Filter, this.Directory);
        }

        public override void DeInitialize()
        {
            if (this.watcher != null)
            {
                this.watcher.Changed -= this.Watcher_Changed;
                this.watcher.Created -= this.Watcher_Created;
                this.watcher.Deleted -= this.Watcher_Deleted;
                this.watcher.Dispose();
                this.watcher = null;
            }

            base.DeInitialize();
        }

        public override bool BeforeExecute()
        {
            if (this.isFirstWatch && this.startInitial || !this.isFirstWatch)
            {
                //watcher.EnableRaisingEvents = false;
                this.LogDebugFormat("Triggered in '{0}' by '{1}' action", this.Directory, (this.isFirstWatch
                                                                                                ? "initial"
                                                                                                : (this.lastFileEventArgs != null
                                                                                                        ? this.lastFileEventArgs.ChangeType.ToStringOrEmpty()
                                                                                                        : "")));
                return this.OnSignalNext.Invoke(new InitializationResult(this, this.LoopCounter));
            }

            return true;
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            this.SetCurrentThreadName();
            this.lastFileEventArgs = e;

            switch (this.WatchMode)
            {
                case WatchModes.Deleted:
                case WatchModes.All:

                    this.Watcher_Execute(e);
                    break;
            }
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            this.SetCurrentThreadName();
            this.lastFileEventArgs = e;

            switch (this.WatchMode)
            {
                case WatchModes.Created:
                case WatchModes.All:

                    this.Watcher_Execute(e);
                    break;
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.SetCurrentThreadName();
            this.lastFileEventArgs = e;

            switch (this.WatchMode)
            {
                case WatchModes.Changed:
                case WatchModes.All:

                    this.Watcher_Execute(e);
                    break;
            }
        }

        private void Watcher_Execute(FileSystemEventArgs e)
        {
            Thread.Sleep(200);

            if (!this.WaitForIdleFile ||
                (this.WaitForIdleFile && FileUtil.WaitForIdleFile(e.FullPath)))
            {
                this.isFirstWatch = false;
                this.BeforeExecute();
            }
        }

        public override bool AfterExecute()
        {
            this.watcher.EnableRaisingEvents = true;
            return base.AfterExecute();
        }

        public override void Dispose()
        {
            this.DeInitialize();
        }
    }
}