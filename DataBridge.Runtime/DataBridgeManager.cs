using System;
using System.IO;
using System.Linq;
using DataBridge.Commands;
using DataBridge.Formatters;
using DataBridge.Helper;

namespace DataBridge.Runtime
{
    public class DataBridgeManager : Singleton<DataBridgeManager>
    {
        public string ConfigName
        {
            get
            {
                return "DataBridge.config";
            }
        }

        public string ConfigFolderName
        {
            get
            {
                return "Configs";
            }
        }

        public DataBridgeInfo LoadDataBridgeInDirectory(string configDirectory, bool createNewWhenNotExist = false)
        {
            if (string.IsNullOrEmpty(configDirectory))
            {
                return null;
            }

            DataBridgeInfo currentDataBridgeInfo = null;

            var path = Path.Combine(configDirectory, this.ConfigName);
            if (createNewWhenNotExist)
            {
                currentDataBridgeInfo = LoadOrCreateNewDataBridge(path);
            }
            else
            {
                currentDataBridgeInfo = LoadDataBridge(path);
            }

            return currentDataBridgeInfo;
        }

        public DataBridgeInfo LoadOrCreateNewDataBridge(string fileName)
        {
            DataBridgeInfo dataBridgeInfo = null;

            if (File.Exists(fileName))
            {
                try
                {
                    dataBridgeInfo = this.LoadDataBridge(fileName);
                }
                catch (Exception ex)
                {
                    throw;
                    // CreateNewDataBridge(fileName);
                }
            }
            else
            {
                dataBridgeInfo = this.CreateNewDataBridge(fileName);
            }

            return dataBridgeInfo;
        }

        public DataBridgeInfo CreateNewDataBridge(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var dataBridgeInfo = new DataBridgeInfo();
            dataBridgeInfo.SetDefaultValues();

            var configDirectory = Path.GetDirectoryName(fileName);
            DirectoryUtil.CreateDirectoryIfNotExists(configDirectory);

            this.SaveDataBridge(fileName, dataBridgeInfo);

            var defaultPipeline = this.CreateDefaultPipeline();
            Pipeline.Save(dataBridgeInfo.PipelineInfos.First().FileName, defaultPipeline);

            return dataBridgeInfo;
        }

        public Pipeline CreateAllCommandsDataBridge(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var pipeline = new Pipeline();
            var allCommands = Pipeline.GetAllAvailableCommands();
            pipeline.Commands.AddRange(allCommands);
            Pipeline.Save(fileName, pipeline);

            return pipeline;
        }

        public DataBridgeInfo LoadDataBridge(Stream stream)
        {
            var serializer = new XmlSerializerHelper<DataBridgeInfo>();
            var dataBridgeInfo = serializer.Load(stream);
            return dataBridgeInfo;
        }

        public DataBridgeInfo LoadDataBridge(string fileName)
        {
            var serializer = new XmlSerializerHelper<DataBridgeInfo>();
            var dataBridgeInfo = serializer.Load(fileName);
            return dataBridgeInfo;
        }

        public void SaveDataBridge(string fileName, DataBridgeInfo dataBridgeInfo)
        {
            var serializer = new XmlSerializerHelper<DataBridgeInfo>();
            serializer.Save(fileName, dataBridgeInfo);
        }

        public void SaveDataBridge(Stream stream, DataBridgeInfo dataBridgeInfo)
        {
            var serializer = new XmlSerializerHelper<DataBridgeInfo>();
            serializer.Save(stream, dataBridgeInfo);
        }

        private Pipeline CreateDefaultPipeline()
        {
            var pipeline = new Pipeline();

            return pipeline;
        }
    }
}