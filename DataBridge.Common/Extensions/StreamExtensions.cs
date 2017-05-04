namespace DataBridge.Extensions
{
    using System.IO;

    public static class StreamExtensions
    {
        public static byte[] ToByteArray(this Stream stream)
        {
            if (stream is MemoryStream)
            {
                return ((MemoryStream)stream).ToArray();
            }
            else
            {
                stream.Position = 0;
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }
    }
}