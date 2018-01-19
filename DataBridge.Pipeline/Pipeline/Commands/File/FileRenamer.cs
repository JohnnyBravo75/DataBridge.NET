using System.Collections.Generic;
using System.Xml.Serialization;
using DataBridge.Common.Helper;

namespace DataBridge.Commands
{
    public class FileRenamer : DataCommand
    {
        public FileRenamer()
        {
            this.Parameters.Add(new CommandParameter() { Name = "SourceDirectory" });
            this.Parameters.Add(new CommandParameter() { Name = "FileFilter" });
            this.Parameters.Add(new CommandParameter() { Name = "SearchString" });
            this.Parameters.Add(new CommandParameter() { Name = "ReplaceString" });
            this.Parameters.Add(new CommandParameter() { Name = "NewPrefix" });
            this.Parameters.Add(new CommandParameter() { Name = "NewSuffix" });
        }

        [XmlIgnore]
        public string SourceDirectory
        {
            get { return this.Parameters.GetValue<string>("SourceDirectory"); }
            set { this.Parameters.SetOrAddValue("SourceDirectory", value); }
        }

        [XmlIgnore]
        public string SearchString
        {
            get { return this.Parameters.GetValue<string>("SearchString"); }
            set { this.Parameters.SetOrAddValue("SearchString", value); }
        }

        [XmlIgnore]
        public string FileFilter
        {
            get { return this.Parameters.GetValue<string>("FileFilter"); }
            set { this.Parameters.SetOrAddValue("FileFilter", value); }
        }

        [XmlIgnore]
        public string ReplaceString
        {
            get { return this.Parameters.GetValue<string>("ReplaceString"); }
            set { this.Parameters.SetOrAddValue("ReplaceString", value); }
        }

        [XmlIgnore]
        public string NewPrefix
        {
            get { return this.Parameters.GetValue<string>("NewPrefix"); }
            set { this.Parameters.SetOrAddValue("NewPrefix", value); }
        }

        [XmlIgnore]
        public string NewSuffix
        {
            get { return this.Parameters.GetValue<string>("NewSuffix"); }
            set { this.Parameters.SetOrAddValue("NewSuffix", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();

                var sourceDirectory = inParameters.GetValue<string>("SourceDirectory");
                var searchString = inParameters.GetValue<string>("SearchString");
                var replaceString = inParameters.GetValue<string>("ReplaceString");
                var fileFilter = inParameters.GetValue<string>("FileFilter");
                var newPrefix = inParameters.GetValue<string>("NewPrefix");
                var newSuffix = inParameters.GetValue<string>("NewSuffix");

                FileUtil.RenameFiles(sourceDirectory, searchString, replaceString, fileFilter, newPrefix, newSuffix);

                var outParameters = this.GetCurrentOutParameters();
                //outParameters.AddOrUpdate(new CommandParameter() { Name = "File", Value = targetFile });
                yield return outParameters;
            }
        }
    }
}