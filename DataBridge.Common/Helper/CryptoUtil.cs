namespace DataBridge.Helper
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public class CryptoUtil
    {
        private static SymmetricAlgorithm cryptoProviderDES = new DESCryptoServiceProvider();

        public static SymmetricAlgorithm CryptoProviderDES
        {
            get
            {
                return cryptoProviderDES;
            }
        }

        public static string Decrypt(string encryptedString, string password, SymmetricAlgorithm cryptoProvider)
        {
            var encoding = Encoding.Unicode;

            if (!string.IsNullOrEmpty(password))
            {
                cryptoProvider.Key = encoding.GetBytes(password);
                cryptoProvider.IV = encoding.GetBytes(password);
            }

            byte[] encryptedBytes = Convert.FromBase64String(encryptedString);

            // byte[] encryptedBytes = encoding.GetBytes(encryptedString);

            using (var msDecrypt = new MemoryStream(encryptedBytes))
            {
                using (var cryptoStream = new CryptoStream(msDecrypt, cryptoProvider.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (var outputStream = new MemoryStream())
                    {
                        byte[] buffer = new byte[1024];
                        int readCount = 0;
                        while ((readCount = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            outputStream.Write(buffer, 0, readCount);
                        }

                        string decryptedTextString = encoding.GetString(outputStream.ToArray());
                        return decryptedTextString;
                    }
                }
            }
        }

        public static string Encrypt(string plaintextString, string password, SymmetricAlgorithm cryptoProvider)
        {
            var encoding = Encoding.Unicode;

            if (!string.IsNullOrEmpty(password))
            {
                cryptoProvider.Key = encoding.GetBytes(password);
                cryptoProvider.IV = encoding.GetBytes(password);
            }

            byte[] plaintextBytes = encoding.GetBytes(plaintextString);

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, cryptoProvider.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    csEncrypt.Write(plaintextBytes, 0, plaintextBytes.Length);
                    csEncrypt.Close();
                    byte[] encryptedTextBytes = msEncrypt.ToArray();
                    msEncrypt.Close();

                    //string encryptedString = encoding.GetString(encryptedTextBytes);
                    //return encryptedString;

                    return Convert.ToBase64String(encryptedTextBytes);
                }
            }
        }

        /// <summary>
        /// Creates a hash from a string
        /// </summary>
        /// <param name="inputStr">The input STR.</param>
        /// <param name="hashAlgorithm">The hash algorithm.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// hashAlgorithm
        /// or
        /// inputStr
        /// </exception>
        public static string CreateHash(string inputStr, HashAlgorithm hashAlgorithm, Encoding encoding)
        {
            if (hashAlgorithm == null) throw new ArgumentNullException("hashAlgorithm");
            if (string.IsNullOrEmpty(inputStr)) throw new ArgumentNullException("inputStr");

            byte[] byteString = encoding.GetBytes(inputStr);
            byte[] byteHash = hashAlgorithm.ComputeHash(byteString);

            return BitConverter.ToString(byteHash);
        }
    }
}