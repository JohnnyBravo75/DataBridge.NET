using System.IO;
using log4net.Config;

namespace DataBridge
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("./Configs/LogConfig.log4net"));
        }
    }
}