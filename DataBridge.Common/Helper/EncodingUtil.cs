namespace DataBridge.Helper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Hilfsklasse für das Encoding. Erkennt das Encoding anhand des BOM-Headers oder von besonderen Zeichen (z.B. Umlauten), die übergeben werden können.
    /// Sample: Encoding enc = DetectEncoding(stream, new Encoding[] { Encoding.ASCII, Encoding.UTF8, Encoding.Unicode, Encoding.UTF32, Encoding.UTF7 }, new string[] { "ö", "ü", "ä", "Ö", "ü", "Ä", "ß" }, 25);
    ///
    /// </summary>
    public class EncodingUtil
    {
        /// <summary>
        /// Gibt das Encoding eine Datei zurück.
        /// </summary>
        /// <param name="file">Die Datei, die geprüft werden soll.</param>
        /// <returns>Das entsprechend gefundene Encoding. Wenn keins gefunden wurde, wird Encoding.Default zurückgegeben.</returns>
        public static Encoding DetectEncoding(string file)
        {
            Encoding resultEnc = Encoding.Default;

            FileInfo finfo = new FileInfo(file);
            FileStream fstream = null;

            try
            {
                fstream = finfo.OpenRead();
                resultEnc = DetectEncoding(fstream);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (fstream != null)
                {
                    fstream.Close();
                    fstream.Dispose();
                    fstream = null;
                }
            }

            return resultEnc;
        }

        /// <summary>
        /// Gibt das Encoding eine Datei zurück.
        /// </summary>
        /// <param name="stream">Der Stream der geprüft werden soll.</param>
        /// <returns>Das entsprechend gefundene Encoding. Wenn keins gefunden wurde, wird Encoding.Default zurückgegeben.</returns>
        public static Encoding DetectEncoding(Stream stream)
        {
            return DetectEncoding(stream, new Encoding[] { Encoding.ASCII, Encoding.UTF8, Encoding.Unicode, Encoding.UTF7 }, new string[] { "ö", "ü", "ä", "Ö", "ü", "Ä", "ß" }, 25);
        }

        /// <summary>
        /// Gibt das Encoding eine Datei zurück.
        /// </summary>
        /// <param name="stream">Der Stream der geprüft werden soll.</param>
        /// <param name="toTest">Die Encodings und Codepages, die getestet werden sollen.</param>
        /// <returns>Das entsprechend gefundene Encoding. Wenn keins gefunden wurde, wird Encoding.Default zurückgegeben.</returns>
        public static Encoding DetectEncoding(Stream stream, Encoding[] toTest)
        {
            return DetectEncoding(stream, toTest, new string[] { "ö", "ü", "ä", "Ö", "ü", "Ä", "ß" }, 25);
        }

        /// <summary>
        /// Gibt das Encoding eine Datei zurück.
        /// </summary>
        /// <param name="stream">Der Stream der geprüft werden soll.</param>
        /// <param name="toTest">Die Encodings und Codepages, die getestet werden sollen.</param>
        /// <param name="manuChars">Wenn kein BOM-Header angegeben wurde, welche Zeichen einzeln geprüft werden sollen, ob diese enthalten sind.</param>
        /// <returns>Das entsprechend gefundene Encoding. Wenn keins gefunden wurde, wird Encoding.Default zurückgegeben.</returns>
        public static Encoding DetectEncoding(Stream stream, Encoding[] toTest, string[] manuChars)
        {
            return DetectEncoding(stream, toTest, manuChars, 25);
        }

        /// <summary>
        /// Gibt das Encoding eine Datei zurück.
        /// </summary>
        /// <param name="stream">Der Stream der geprüft werden soll.</param>
        /// <param name="testEncodings">Die Encodings und Codepages, die getestet werden sollen.</param>
        /// <param name="testChars">Wenn kein BOM-Header angegeben wurde, welche Zeichen einzeln geprüft werden sollen, ob diese enthalten sind.</param>
        /// <param name="testLines">Die Anzahl der Zeilen, die geprüft werden, wenn kein BOM-Header angegeben wurde.</param>
        /// <returns>Das entsprechend gefundene Encoding. Wenn keins gefunden wurde, wird Encoding.Default zurückgegeben.</returns>
        public static Encoding DetectEncoding(Stream stream, Encoding[] testEncodings, string[] testChars, int testLines)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (testEncodings == null) throw new ArgumentNullException("testEncodings");
            if (testChars == null) throw new ArgumentNullException("testChars");
            if (testLines < 0) throw new ArgumentOutOfRangeException("testLines");

            // save position in stream
            long position = stream.Position;
            Encoding resultEnc = null;

            try
            {
                // first check the preamble (BOM) of the stream
                for (int i = 0; i < testEncodings.Length; i++)
                {
                    stream.Position = 0;
                    byte[] preamble = testEncodings[i].GetPreamble();
                    bool isEqual = false;

                    if (preamble.Length > 0)
                    {
                        for (int j = 0; j < preamble.Length; j++)
                        {
                            isEqual = (preamble[j] == stream.ReadByte());
                            if (!isEqual) break;
                        }

                        if (isEqual)
                        {
                            resultEnc = testEncodings[i];
                            break;
                        }
                    }
                }

                // check, if the given chars occur in the stream
                if (resultEnc == null)
                {
                    var readers = new List<StreamReader>();
                    foreach (Encoding encoding in testEncodings)
                    {
                        stream.Position = 0;
                        int curline = 0;
                        string line;

                        var reader = new StreamReader(stream, encoding);
                        readers.Add(reader);
                        line = reader.ReadLine();
                        while (line != null && curline < testLines)
                        {
                            for (int i = 0; i < testChars.Length; i++)
                            {
                                if (line.Contains(testChars[i]))
                                {
                                    // char was found
                                    resultEnc = encoding;
                                    break;
                                }
                            }

                            if (resultEnc != null)
                            {
                                break;
                            }

                            line = reader.ReadLine();
                            curline++;
                        }

                        if (resultEnc != null)
                        {
                            break;
                        }
                    }

                    // destroy the readers
                    foreach (var reader in readers)
                    {
                        try
                        {
                            reader.Dispose();
                        }
                        catch (Exception ex)
                        {
                            // stream can be disposed, so a ObjectDisposedException can happen
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                // restore position in stream
                try
                {
                    if (stream != null)
                    {
                        stream.Position = position;
                    }
                }
                catch
                {
                }
            }

            if (resultEnc == null)
            {
                resultEnc = Encoding.Default;
            }

            return resultEnc;
        }

        /// <summary>
        /// Gets the name of the encoding equivalent to the name from the encodinginfo.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <returns>the name (eqivalent to encodinginfo)</returns>
        public static string GetEncodingName(Encoding encoding)
        {
            string encodingName = "";

            if (encoding == null)
            {
                return encodingName;
            }

            var encodingInfo = Encoding.GetEncodings().FirstOrDefault(x => x.CodePage == encoding.CodePage);
            if (encodingInfo != null)
            {
                encodingName = encodingInfo.Name;
            }

            return encodingName;
        }

        public static Encoding GetEncodingOrDefault(string encodingName)
        {
            Encoding encoding = Encoding.Default;
            if (!string.IsNullOrEmpty(encodingName))
            {
                encoding = Encoding.GetEncoding(encodingName) ?? Encoding.Default;
            }

            return encoding;
        }
    }
}