using System;
using System.IO;
using System.Linq;
using System.Net;
using DataBridge.Commands;
using DataBridge.Common.Helper;
using DataBridge.ConnectionInfos;
using DataBridge.Extensions;
using DataBridge.Helper;
using DataBridge.Services;
using DataConnectors.Adapter.DbAdapter.ConnectionInfos;
using DataConnectors.Common.Model;
using DataConnectors.Formatters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataBridge.Test
{
    [TestClass]
    public class UnitTest1
    {
        private string testDataPath = @"..\..\TestData\";

        private string resultPath = Environment.ExpandEnvironmentVariables(@"%TEMP%\TestResultData\");

        [TestInitialize]
        public void TestInitialize()
        {
            DirectoryUtil.CreateDirectoryIfNotExists(this.resultPath);
            DirectoryUtil.ClearDirectory(this.resultPath);
        }

        #region ******************************** File Tests ********************************

        //[TestMethod]
        //public void Test_ReadXml_WriteCsv_Address()
        //{
        //    var testPipeline = new Pipeline();

        //    var reader = new XmlFileReader()
        //    {
        //        File = this.testDataPath + @"GetAddressResponse.xml",
        //        RowXPath = "/GetAddressResponse/GetAddressResult/result/address"
        //    };
        //    reader.Formatter = new XmlToDataTableFormatter() { UseAttributes = true };
        //    testPipeline.Commands.Add(reader);

        //    var writer = new FlatFileWriter() { File = this.resultPath + @"flatxml.csv" };
        //    writer.Formatter = new DataTableToCsvFormatter();
        //    reader.AddChild(writer);

        //    testPipeline.ExecutePipeline();

        //    // check
        //    var targetlineCount = File.ReadLines(this.resultPath + @"flatxml.csv").Count();

        //    Assert.AreEqual(3, targetlineCount);
        //}

        [TestMethod]
        public void Test_ReadXml_WriteCsv_Kunden()
        {
            using (var testPipeline = new Pipeline())
            {
                var reader = new XmlFileReader()
                {
                    File = this.testDataPath + @"kunden.xml",
                    RowXPath = "/adre/kunde"
                };
                reader.Formatter = new XPathToDataTableFormatter();
                testPipeline.Commands.Add(reader);

                var writer = new FlatFileWriter() { File = this.resultPath + @"kunden.csv" };
                writer.Formatter = new DataTableToCsvFormatter();
                reader.AddChild(writer);

                testPipeline.ExecutePipeline();
            }

            // check
            var targetlineCount = File.ReadLines(this.resultPath + @"kunden.csv").Count();

            Assert.AreEqual(3, targetlineCount);
        }

        [TestMethod]
        public void Test_ReadCsv_WriteCsv_Cd()
        {
            using (var testPipeline = new Pipeline())
            {
                var reader = new FlatFileReader() { File = this.testDataPath + @"cd2.csv" };
                reader.Formatter = new CsvToDataTableFormatter() { Separator = ";" };
                testPipeline.Commands.Add(reader);

                var writer = new FlatFileWriter() { File = this.resultPath + @"cd2_copy.csv" };
                writer.Formatter = new DataTableToCsvFormatter() { Separator = ";" };
                reader.AddChild(writer);

                //testPipeline.CommandHook = (cmd) =>
                //{
                //};
                testPipeline.OnExecuteCommand += (cmd) =>
                {
                };

                testPipeline.ExecutePipeline();
            }

            // check
            var sourcelineCount = File.ReadLines(this.testDataPath + @"cd2.csv").Count();
            var targetlineCount = File.ReadLines(this.resultPath + @"cd2_copy.csv").Count();

            Assert.AreEqual(sourcelineCount, targetlineCount);

            if (!FileUtil.CompareFiles(this.testDataPath + "cd2.csv", this.resultPath + "cd2_copy.csv"))
            {
                throw new Exception("Original and copied file do not match");
            }
        }

        [TestMethod]
        public void Test_LoadPipeline_SavePipeline()
        {
            var originalPipeline = new Pipeline();
            var allCommands = Pipeline.GetAllAvailableCommands();
            originalPipeline.Commands.AddRange(allCommands);
            Pipeline.Save(this.resultPath + "testpipeline_beforeload.xml", originalPipeline);

            var loadedPipeline = Pipeline.Load(this.resultPath + "testpipeline_beforeload.xml");
            Pipeline.Save(this.resultPath + "testpipeline_afterload.xml", loadedPipeline);

            // check
            if (!FileUtil.CompareFiles(this.resultPath + "testpipeline_beforeload.xml", this.resultPath + "testpipeline_afterload.xml"))
            {
                throw new Exception("Original an serialized file do not match");
            }
        }

        [TestMethod]
        public void Test_ReadFixedLength_WriteCsv_Fixed2()
        {
            using (var testPipeline = new Pipeline())
            {
                var looper = new FileLooper() { SourceDirectory = this.testDataPath, FileFilter = @"FixedText2.txt" };
                testPipeline.Commands.Add(looper);

                var formatter = new FixedLengthToDataTableFormatter();
                formatter.FieldDefinitions.Add(new FieldDefinition(new Field("Header1", 15)));
                formatter.FieldDefinitions.Add(new FieldDefinition(new Field("Header2", 25)));
                formatter.FieldDefinitions.Add(new FieldDefinition(new Field("Header3", 10)));

                var reader = new FlatFileReader() { File = "{File}", Formatter = formatter };
                looper.AddChild(reader);

                var writer = new FlatFileWriter() { File = this.resultPath + @"{FileName}" };
                writer.Formatter = new DataTableToCsvFormatter();
                reader.AddChild(writer);

                testPipeline.ExecutePipeline();
            }

            // check
            var sourcelineCount = File.ReadLines(this.testDataPath + @"FixedText2.txt").Count();
            var targetlineCount = File.ReadLines(this.resultPath + @"FixedText2.txt").Count();

            Assert.AreEqual(sourcelineCount, targetlineCount);
        }

        [TestMethod]
        public void Test_ReadCsv_WriteFixedLength_Cd2()
        {
            using (var testPipeline = new Pipeline())
            {
                var looper = new FileLooper() { SourceDirectory = this.testDataPath, FileFilter = @"cd2.csv" };
                testPipeline.Commands.Add(looper);

                var reader = new FlatFileReader() { File = "{File}" };
                reader.Formatter = new CsvToDataTableFormatter() { Separator = ";", Enclosure = "\"" };
                looper.AddChild(reader);

                var formatter = new DataTableToFixedLengthFormatter();
                formatter.FieldDefinitions.Add(new FieldDefinition(new Field("name", 15)));
                formatter.FieldDefinitions.Add(new FieldDefinition(new Field("addr", 25)));
                formatter.FieldDefinitions.Add(new FieldDefinition(new Field("telefon", 10)));

                var writer = new FlatFileWriter() { File = this.resultPath + @"{FileName}" };
                writer.Formatter = formatter;
                reader.AddChild(writer);

                testPipeline.ExecutePipeline();
            }

            // check
            var sourcelineCount = File.ReadLines(this.testDataPath + @"cd2.csv").Count();
            var targetlineCount = File.ReadLines(this.resultPath + @"cd2.csv").Count();

            Assert.AreEqual(sourcelineCount, targetlineCount);
        }

        [TestMethod]
        public void Test_FolderSync()
        {
            using (var testPipeline = new Pipeline())
            {
                var syncer = new FolderSync() { SourceDirectory = this.testDataPath, TargetDirectory = this.resultPath, FileFilter = @"*.txt" };
                testPipeline.Commands.Add(syncer);

                testPipeline.ExecutePipeline();
            }

            // check
            var sourceFileCount = Directory.EnumerateFiles(this.testDataPath, "*.txt", SearchOption.TopDirectoryOnly).Count();
            var targetFileCount = Directory.EnumerateFiles(this.resultPath, "*.*", SearchOption.TopDirectoryOnly).Count();

            Assert.AreEqual(sourceFileCount, targetFileCount);
        }

        [TestMethod]
        public void Test_ReadWriteCopyCsv()
        {
            using (var testPipeline = new Pipeline())
            {
                var looper = new FileLooper() { SourceDirectory = this.testDataPath, FileFilter = @"*.csv" };
                testPipeline.Commands.Add(looper);

                var reader = new FlatFileReader() { File = "{File}" };
                reader.Formatter = new CsvToDataTableFormatter() { Separator = ";" };
                looper.AddChild(reader);

                reader.AddChild(new TableFilter() { });
                var writer = new FlatFileWriter() { File = this.resultPath + @"pipeline\{FileName}" };
                writer.Formatter = new DataTableToCsvFormatter();
                reader.AddChild(writer);

                looper.AddChild(new FileMover() { SourceFile = @"{File}", TargetDirectory = this.resultPath + @"Archive", Mode = FileMover.FileMoveModes.Copy });
                looper.AddChild(new FileZipper() { SourceFile = @"{File}", TargetDirectory = this.resultPath + @"Archive\Zipped", ZipName = "Archive_{yyyyMMdd}.zip", RemoveSourceFile = true });

                testPipeline.ExecutePipeline();
            }

            // check
            int sourceFileCount = Directory.GetFiles(this.testDataPath, @"*.csv", SearchOption.TopDirectoryOnly).Length;
            int targetFileCount = Directory.GetFiles(this.resultPath + @"pipeline", @"*.csv", SearchOption.TopDirectoryOnly).Length;
            int archiveFileCount = Directory.GetFiles(this.resultPath + @"Archive", @"*.csv", SearchOption.TopDirectoryOnly).Length;
            int zipFileCount = Directory.GetFiles(this.resultPath + @"Archive\Zipped", @"Archive_*.zip", SearchOption.TopDirectoryOnly).Length;

            Assert.AreEqual(sourceFileCount, targetFileCount);
            Assert.AreEqual(0, archiveFileCount);
            Assert.AreEqual(1, zipFileCount);
        }

        #endregion ******************************** File Tests ********************************

        #region ******************************** Ftp Tests ********************************

        //[TestMethod]
        //public void TestFtpUpload()
        //{
        //    var testPipeline = new Pipeline();

        //    var looper = new FileLooper() { SourceDirectory = testDataPath, FileFilter = @"FtpUploadTest.zip" };
        //    testPipeline.Commands.Add(looper);

        //    var ftpUploader = new FtpUploader() { Host = "ftp://speedtest.tele2.net", User = "", Password = "", RemoteDirectory = @"\\upload", File = "{File}" };
        //    looper.AddChild(ftpUploader);

        //    testPipeline.ExecutePipeline();

        //    // check
        //    var ftp = new Ftp();
        //    ftp.SetConnectionInfos("ftp://speedtest.tele2.net");
        //    ftp.GetDirectoryList("/upload").First(x => x.Name == "FtpUploadTest.zip");
        //}

        [TestMethod]
        public void Test_FtpDownload()
        {
            using (var testPipeline = new Pipeline())
            {
                var ftpDownloader = new FtpFileDownloader()
                {
                    Host = "ftp://speedtest.tele2.net",
                    RemoteDirectory = @"\\1KB.zip",
                    LocalDirectory = this.resultPath,
                    User = "",
                    Password = ""
                };

                testPipeline.Commands.Add(ftpDownloader);

                testPipeline.ExecutePipeline();

                // check
                if (!File.Exists(this.resultPath + @"1KB.zip"))
                {
                    throw new Exception("Downloaded file was not found");
                }
            }
        }

        #endregion ******************************** Ftp Tests ********************************

        #region ******************************** Webservice Tests ********************************

        [TestMethod]
        public void Test_RestService_GetCountries()
        {
            using (var testPipeline = new Pipeline())
            {
                var httpClient = new HttpClient()
                {
                    Url = @"http://services.groupkt.com/country/get/all",
                    Formatter = new JsonToDataTableFormatter() { RowXPath = "/RestResponse/result" }
                };

                testPipeline.Commands.Add(httpClient);

                var writer = new FlatFileWriter() { File = this.resultPath + @"allcountries.txt", DeleteBefore = true };
                writer.Formatter = new DataTableToCsvFormatter() { Separator = ";" };
                httpClient.AddChild(writer);

                testPipeline.ExecutePipeline();
            }

            // check
            var targetlineCount = File.ReadLines(this.resultPath + @"allcountries.txt").Count();

            Assert.AreEqual(250, targetlineCount);
        }

        //[TestMethod]
        //public void Test_RestService_ReadMapQuest()
        //{
        //    var testPipeline = new Pipeline();

        //    var reader = new HttpClient() { Url = @"http://open.mapquestapi.com/nominatim/v1/search.php" };
        //    reader.DataMappings.Add(new DataMapping() { Name = "street", Value = "1 Bahnstr." });
        //    reader.DataMappings.Add(new DataMapping() { Name = "city", Value = "Geisenheim" });
        //    reader.DataMappings.Add(new DataMapping() { Name = "postalcode", Value = "65366" });
        //    reader.DataMappings.Add(new DataMapping() { Name = "format", Value = "json" });
        //    reader.DataMappings.Add(new DataMapping() { Name = "addressdetails", Value = "0" });
        //    reader.DataMappings.Add(new DataMapping() { Name = "limit", Value = "1" });

        //    var formatter = new JsonToDataTableFormatter();
        //    reader.Formatter = formatter;
        //    testPipeline.Commands.Add(reader);

        //    var writer = new FlatFileWriter() { File = this.resultPath + @"mapquestapi.csv" };
        //    writer.Formatter = new DataTableToCsvFormatter();
        //    reader.AddChild(writer);

        //    testPipeline.ExecutePipeline();

        //    // check
        //    var targetlineCount = File.ReadLines(this.resultPath + @"mapquestapi.csv").Count();

        //    Assert.AreEqual(100, targetlineCount);
        //}

        [TestMethod]
        public void Test_Soap_ReadCustomerService()
        {
            using (var testPipeline = new Pipeline())
            {
                var reader = new SoapClient()
                {
                    Wsdl = "http://www.predic8.com:8080/crm/CustomerService?wsdl",
                    MethodName = "getAll",
                    Formatter = new XPathToDataTableFormatter() { RowXPath = "/getAllResponse/customer" }
                };

                testPipeline.Commands.Add(reader);

                var writer = new FlatFileWriter()
                {
                    File = this.resultPath + @"CustomerService.csv",
                    Formatter = new DataTableToCsvFormatter()
                };

                reader.AddChild(writer);

                testPipeline.ExecutePipeline();
            }

            // check
            var targetlineCount = File.ReadLines(this.resultPath + @"CustomerService.csv").Count();

            Assert.AreNotSame(0, targetlineCount);

            var resultContent = File.ReadAllText(this.resultPath + @"CustomerService.csv");
            if (resultContent.Length <= 4)
            {
                throw new AssertFailedException("No content in CustomerService.csv");
            }
        }

        [TestMethod]
        public void Test_Soap_ReadMedicareSupplier()
        {
            using (var testPipeline = new Pipeline())
            {
                var reader = new SoapClient()
                {
                    Wsdl = "http://www.webservicex.net/medicareSupplier.asmx?WSDL",
                    MethodName = "GetSupplierByCity",
                    Formatter = new XPathToDataTableFormatter() { RowXPath = "/GetSupplierByCityResponse/SupplierDataLists/SupplierDatas/SupplierData" }
                };

                reader.DataMappings.Add(new DataMapping() { Name = "City", Value = "Fort Worth" });

                testPipeline.Commands.Add(reader);

                var writer = new FlatFileWriter()
                {
                    File = this.resultPath + @"MedicareSupplier.csv",
                    Formatter = new DataTableToCsvFormatter()
                };

                reader.AddChild(writer);
                //testPipeline.OnExecuteCommand += (cmd) =>
                //{
                //};
                testPipeline.ExecutePipeline();
            }

            // check
            var targetlineCount = File.ReadLines(this.resultPath + @"MedicareSupplier.csv").Count();

            Assert.AreNotSame(0, targetlineCount);

            var resultContent = File.ReadAllText(this.resultPath + @"MedicareSupplier.csv");
            if (resultContent.Length <= 4)
            {
                throw new AssertFailedException("No content in MedicareSupplier.csv");
            }
        }

        #endregion ******************************** Webservice Tests ********************************

        #region ******************************** Mixed Tests ********************************

        [TestMethod]
        public void Test_EmailDownloader()
        {
            using (var testPipeline = new Pipeline())
            {
                var emailer = new EmailDownloader();
                emailer.Host = "pop.gmx.de";
                emailer.User = "";
                emailer.Password = "";
                emailer.EnableSecure = true;
                emailer.FilterConditions.Add(new Condition() { Token = "EmailFrom", Operator = ConditionOperators.Contains, Value = "pearl" });

                testPipeline.Commands.Add(emailer);

                var writer = new FlatFileWriter() { File = this.resultPath + "{File}", DeleteBefore = true };
                writer.Formatter = new DefaultFormatter();
                emailer.AddChild(writer);
                testPipeline.OnExecuteCommand += delegate (DataCommand cmd)
                {
                };
                testPipeline.ExecutePipeline();
            }

            int emailFileCount = Directory.GetFiles(this.resultPath, @"*.html", SearchOption.TopDirectoryOnly).Length;

            Assert.IsTrue(emailFileCount > 0, "No emails were downloaded");
        }

        [TestMethod]
        public void Test_Emailer()
        {
            var emailer = new EmailSender();
            emailer.From = "";
            emailer.To = "";
            emailer.Subject = "Test";
            emailer.Body = "Test";
            emailer.Attachments.Add(@"C:\Temp\builds\temp\*.log");

            emailer.ConnectionInfo = new SmtpConnectionInfo() { SmtpServer = "smtp.gmx.net", UserName = "", Password = "" };

            var testPipeline = new Pipeline();
            testPipeline.Commands.Add(emailer);
            testPipeline.ExecutePipeline();
        }

        [TestMethod]
        public void Test_Blackout()
        {
            bool isBlackoutTest = true;

            bool result;
            using (var testPipeline = new Pipeline())
            {
                var reader = new FlatFileReader() { File = this.testDataPath + @"cd2.csv" };
                reader.Formatter = new CsvToDataTableFormatter() { Separator = ";" };
                testPipeline.Commands.Add(reader);
                result = true;
                testPipeline.OnExecutionCanceled += (sender, args) =>
                {
                    if (isBlackoutTest)
                    {
                        if (args.Result == "Blackout")
                        {
                            // ok
                            result = false;
                        }
                    }
                    else
                    {
                        // ok
                        result = true;
                    }
                };

                // set blackout to current
                testPipeline.BlackoutStart = DateTime.Now.Subtract(new TimeSpan(1, 0, 0));
                testPipeline.BlackoutEnd = DateTime.Now.Add(new TimeSpan(1, 0, 0));

                result = testPipeline.ExecutePipeline();

                // check
                Assert.IsFalse(result);

                // set blackout to another time
                testPipeline.BlackoutStart = DateTime.Now.Subtract(new TimeSpan(2, 0, 0));
                testPipeline.BlackoutEnd = DateTime.Now.Subtract(new TimeSpan(1, 0, 0));

                result = testPipeline.ExecutePipeline();
            }

            // check
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_TableFilter()
        {
            using (var testPipeline = new Pipeline())
            {
                var reader = new FlatFileReader() { File = this.testDataPath + @"cd2.csv" };
                reader.Formatter = new CsvToDataTableFormatter() { Separator = ";", Enclosure = "\"" };
                testPipeline.Commands.Add(reader);

                var filter = new TableFilter();
                filter.FilterConditions.Add(new Condition() { Token = "name", Operator = ConditionOperators.Contains, Value = "restaur" });
                reader.AddChild(filter);

                var writer = new FlatFileWriter() { File = this.resultPath + "{File}" };
                writer.Formatter = new DataTableToCsvFormatter() { Separator = ";" };
                reader.AddChild(writer);

                var result = testPipeline.ValidatePipeline();
                if (result.Any())
                {
                    throw new ArgumentException(string.Join(Environment.NewLine, result));
                }

                testPipeline.ExecutePipeline();
            }
        }

        [TestMethod]
        public void Test_SqlTableExport()
        {
            using (var testPipeline = new Pipeline() { StreamingBlockSize = 20 })
            {
                var looper = new ValueLooper();
                looper.ValueItems.Add(new ValueItem()
                {
                    new Parameter() { Name = "TableName", Value = "mis.tb_companies" }
                });

                looper.ValueItems.Add(new ValueItem()
                {
                    new Parameter() { Name = "TableName", Value = "mis.tb_persons" }
                });

                testPipeline.Commands.Add(looper);

                var sqlReader = new SqlDataReader()
                {
                    ConnectionInfo = new OracleNativeDbConnectionInfo() { UserName = "", Password = "", Database = "ORACLE01", Host = "COMPUTER01" },
                    SqlTemplate = " SELECT * FROM {TableName} WHERE ROWNUM <= 100 "
                };

                looper.AddChild(sqlReader);

                sqlReader.AddChild(new Excel2007Writer()
                {
                    File = this.resultPath + @"{TableName}.xlsx",
                    DeleteBefore = true
                });

                testPipeline.ExecutePipeline();
            }
        }

        [TestMethod]
        public void Test_SqlTableImport()
        {
            using (var testPipeline = new Pipeline() { StreamingBlockSize = 20 })
            {
                var looper = new FileLooper() { SourceDirectory = this.resultPath, FileFilter = "mis.*.txt" };

                testPipeline.Commands.Add(looper);

                var reader = new FlatFileReader()
                {
                    File = "{File}",
                    Formatter = new CsvToDataTableFormatter() { Separator = ";" }
                };

                looper.AddChild(reader);

                var tableWriter = new DbTableWriter
                {
                    ConnectionInfo = new OracleNativeDbConnectionInfo() { UserName = "", Password = "", Database = "ORACLE01", Host = "COMPUTER01" },
                    DeleteBefore = false,
                    TableName = "{DataName}_bak"
                };

                reader.AddChild(tableWriter);

                testPipeline.ExecutePipeline();
            }
        }

        [TestMethod]
        public void Test_HttpTrigger()
        {
            using (var testPipeline = new Pipeline())
            {
                var httpTrigger = new HttpTrigger()
                {
                    Port = 8080,
                    UseAuthentication = true,
                    User = "Hello",
                    Password = "World"
                };

                testPipeline.Commands.Add(httpTrigger);

                var writer = new FlatFileWriter() { File = this.resultPath + @"http-parameter.txt", DeleteBefore = false };
                writer.Formatter = new DataTableToCsvFormatter() { Separator = ";" };
                httpTrigger.AddChild(writer);

                testPipeline.ExecutePipeline();
            }

            using (var webClient = new WebClient())
            {
                webClient.SetCredentials(new NetworkCredential("Hello", "World"));
                webClient.QueryString.Add("Name", "Donald T.");
                webClient.QueryString.Add("Address", "Washington");

                string response = webClient.DownloadString("http://localhost:8080/");

                Assert.AreEqual(response, "<Message>Triggered successfull</Message>");
            }
        }

        #endregion ******************************** Mixed Tests ********************************
    }
}