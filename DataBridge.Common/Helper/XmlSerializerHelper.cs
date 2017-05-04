namespace DataBridge.Helper
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using DataBridge.Utils;

    public class XmlSerializerHelper<T>
    {
        private XmlSerializer xmlSerializer;

        public string FileName { get; set; }

        private XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

        public XmlSerializerHelper()
        {
            this.ns.Add("", "");

            KnownTypesProvider.LoadAllAssemblies = true;
            Type[] knownTypes = KnownTypesProvider.GetKnownTypes("Data", null, new Type[] { typeof(Exception) }).ToArray();
            this.xmlSerializer = new XmlSerializer(typeof(T), knownTypes);
        }

        public T Load()
        {
            TextReader reader = null;
            string path = this.FileName;
            try
            {
                reader = new StreamReader(path);
                T obj = (T)this.xmlSerializer.Deserialize(reader);
                return obj;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        public void Save(T obj)
        {
            TextWriter writer = null;
            string path = this.FileName;

            try
            {
                writer = new StreamWriter(path);
                this.xmlSerializer.Serialize(writer, obj);
            }
            catch (Exception ex)
            {
                throw new Exception("Error when serialising '" + obj.ToString() + "'", ex);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                }
            }
        }

        public string SerializeToString(T objectInstance)
        {
            var xml = new StringBuilder();

            using (TextWriter writer = new StringWriter(xml))
            {
                this.xmlSerializer.Serialize(writer, objectInstance);
            }

            return xml.ToString();
        }

        public T DeserializeFromString(string xml)
        {
            return (T)this.DeserializeFromString(xml, typeof(T));
        }

        public object DeserializeFromString(string xml, Type type)
        {
            object result;

            using (TextReader reader = new StringReader(xml))
            {
                result = this.xmlSerializer.Deserialize(reader);
            }

            return result;
        }
    }
}