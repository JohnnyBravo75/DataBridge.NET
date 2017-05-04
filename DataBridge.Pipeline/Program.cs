using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using DataBridge.Commands;
using DataBridge.Extensions;
using DataBridge.Formatters;
using DataBridge.Models;
using DataBridge.Services;
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