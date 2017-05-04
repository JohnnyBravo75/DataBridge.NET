using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace DataBridge.Helper
{
    /// &lt;span class="code-SummaryComment">&lt;summary>&lt;/span>
    /// Provides extension methods that deal with
    /// string encryption/decryption and
    /// &lt;span class="code-SummaryComment">&lt;see cref="SecureString"/> encapsulation.&lt;/span>
    /// &lt;span class="code-SummaryComment">&lt;/summary>&lt;/span>
    public static class DataProtectionUtil
    {
        private static byte[] entropy = Encoding.Unicode.GetBytes("salt and pepper");

        /// &lt;span class="code-SummaryComment">&lt;summary>&lt;/span>
        /// Specifies the data protection scope of the DPAPI.
        /// &lt;span class="code-SummaryComment">&lt;/summary>&lt;/span>
        private const DataProtectionScope Scope = DataProtectionScope.LocalMachine;

        /// &lt;span class="code-SummaryComment">&lt;summary>&lt;/span>
        /// Encrypts a given password and returns the encrypted data
        /// as a base64 string.
        /// &lt;span class="code-SummaryComment">&lt;/summary>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;param name="plainText">An unencrypted string that needs&lt;/span>
        /// to be secured.&lt;span class="code-SummaryComment">&lt;/param>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;returns>A base64 encoded string that represents the encrypted&lt;/span>
        /// binary data.
        /// &lt;span class="code-SummaryComment">&lt;/returns>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;remarks>This solution is not really secure as we are&lt;/span>
        /// keeping strings in memory. If runtime protection is essential,
        /// &lt;span class="code-SummaryComment">&lt;see cref="SecureString"/> should be used.&lt;/remarks>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;exception cref="ArgumentNullException">If &lt;paramref name="plainText"/>&lt;/span>
        /// is a null reference.&lt;span class="code-SummaryComment">&lt;/exception>&lt;/span>
        public static string Encrypt(string plainText)
        {
            if (plainText == null)
            {
                throw new ArgumentNullException("plainText");
            }

            //encrypt data
            var data = Encoding.Unicode.GetBytes(plainText);
            byte[] encrypted = ProtectedData.Protect(data, entropy, Scope);

            //return as base64 string
            return Convert.ToBase64String(encrypted);
        }

        /// &lt;span class="code-SummaryComment">&lt;summary>&lt;/span>
        /// Decrypts a given string.
        /// &lt;span class="code-SummaryComment">&lt;/summary>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;param name="cipher">A base64 encoded string that was created&lt;/span>
        /// through the &lt;span class="code-SummaryComment">&lt;see cref="Encrypt(string)"/> or&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;see cref="Encrypt(SecureString)"/> extension methods.&lt;/param>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;returns>The decrypted string.&lt;/returns>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;remarks>Keep in mind that the decrypted string remains in memory&lt;/span>
        /// and makes your application vulnerable per se. If runtime protection
        /// is essential, &lt;span class="code-SummaryComment">&lt;see cref="SecureString"/> should be used.&lt;/remarks>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;exception cref="ArgumentNullException">If &lt;paramref name="cipher"/>&lt;/span>
        /// is a null reference.&lt;span class="code-SummaryComment">&lt;/exception>&lt;/span>
        public static string Decrypt(string cipher)
        {
            if (cipher == null)
            {
                throw new ArgumentNullException("cipher");
            }

            //parse base64 string
            byte[] data = Convert.FromBase64String(cipher);

            //decrypt data
            byte[] decrypted = ProtectedData.Unprotect(data, entropy, Scope);
            return Encoding.Unicode.GetString(decrypted);
        }

        /// &lt;span class="code-SummaryComment">&lt;summary>&lt;/span>
        /// Encrypts the contents of a secure string.
        /// &lt;span class="code-SummaryComment">&lt;/summary>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;param name="value">An unencrypted string that needs&lt;/span>
        /// to be secured.&lt;span class="code-SummaryComment">&lt;/param>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;returns>A base64 encoded string that represents the encrypted&lt;/span>
        /// binary data.
        /// &lt;span class="code-SummaryComment">&lt;/returns>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;exception cref="ArgumentNullException">If &lt;paramref name="value"/>&lt;/span>
        /// is a null reference.&lt;span class="code-SummaryComment">&lt;/exception>&lt;/span>
        public static string Encrypt(SecureString value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            IntPtr ptr = Marshal.SecureStringToCoTaskMemUnicode(value);
            try
            {
                char[] buffer = new char[value.Length];
                Marshal.Copy(ptr, buffer, 0, value.Length);

                byte[] data = Encoding.Unicode.GetBytes(buffer);
                byte[] encrypted = ProtectedData.Protect(data, entropy, Scope);

                //return as base64 string
                return Convert.ToBase64String(encrypted);
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUnicode(ptr);
            }
        }

        /// &lt;span class="code-SummaryComment">&lt;summary>&lt;/span>
        /// Decrypts a base64 encrypted string and returns the decrpyted data
        /// wrapped into a &lt;span class="code-SummaryComment">&lt;see cref="SecureString"/> instance.&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;/summary>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;param name="cipher">A base64 encoded string that was created&lt;/span>
        /// through the &lt;span class="code-SummaryComment">&lt;see cref="Encrypt(string)"/> or&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;see cref="Encrypt(SecureString)"/> extension methods.&lt;/param>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;returns>The decrypted string, wrapped into a&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;see cref="SecureString"/> instance.&lt;/returns>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;exception cref="ArgumentNullException">If &lt;paramref name="cipher"/>&lt;/span>
        /// is a null reference.&lt;span class="code-SummaryComment">&lt;/exception>&lt;/span>
        public static SecureString DecryptSecure(string cipher)
        {
            if (cipher == null)
            {
                throw new ArgumentNullException("cipher");
            }

            //parse base64 string
            byte[] data = Convert.FromBase64String(cipher);

            //decrypt data
            byte[] decrypted = ProtectedData.Unprotect(data, entropy, Scope);

            var secured = new SecureString();

            //parse characters one by one - doesn't change the fact that
            //we have them in memory however...
            int count = Encoding.Unicode.GetCharCount(decrypted);
            int bc = decrypted.Length / count;
            for (int i = 0; i < count; i++)
            {
                secured.AppendChar(Encoding.Unicode.GetChars(decrypted, i * bc, bc)[0]);
            }

            //mark as read-only
            secured.MakeReadOnly();
            return secured;
        }

        /// &lt;span class="code-SummaryComment">&lt;summary>&lt;/span>
        /// Wraps a managed string into a &lt;span class="code-SummaryComment">&lt;see cref="SecureString"/> &lt;/span>
        /// instance.
        /// &lt;span class="code-SummaryComment">&lt;/summary>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;param name="value">A string or char sequence that &lt;/span>
        /// should be encapsulated.&lt;span class="code-SummaryComment">&lt;/param>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;returns>A &lt;see cref="SecureString"/> that encapsulates the&lt;/span>
        /// submitted value.&lt;span class="code-SummaryComment">&lt;/returns>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;exception cref="ArgumentNullException">If &lt;paramref name="value"/>&lt;/span>
        /// is a null reference.&lt;span class="code-SummaryComment">&lt;/exception>&lt;/span>
        public static SecureString ToSecureString(IEnumerable<char> value)

        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var secured = new SecureString();

            var charArray = value.ToArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                secured.AppendChar(charArray[i]);
            }

            secured.MakeReadOnly();
            return secured;
        }

        /// &lt;span class="code-SummaryComment">&lt;summary>&lt;/span>
        /// Unwraps the contents of a secured string and
        /// returns the contained value.
        /// &lt;span class="code-SummaryComment">&lt;/summary>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;param name="value">&lt;/param>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;returns>&lt;/returns>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;remarks>Be aware that the unwrapped managed string can be&lt;/span>
        /// extracted from memory.&lt;span class="code-SummaryComment">&lt;/remarks>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;exception cref="ArgumentNullException">If &lt;paramref name="value"/>&lt;/span>
        /// is a null reference.&lt;span class="code-SummaryComment">&lt;/exception>&lt;/span>
        public static string Unwrap(SecureString value)
        {
            if (value == null) throw new ArgumentNullException("value");

            IntPtr ptr = Marshal.SecureStringToCoTaskMemUnicode(value);
            try
            {
                return Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUnicode(ptr);
            }
        }

        /// &lt;span class="code-SummaryComment">&lt;summary>&lt;/span>
        /// Checks whether a &lt;span class="code-SummaryComment">&lt;see cref="SecureString"/> is either&lt;/span>
        /// null or has a &lt;span class="code-SummaryComment">&lt;see cref="SecureString.Length"/> of 0.&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;/summary>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;param name="value">The secure string to be inspected.&lt;/param>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;returns>True if the string is either null or empty.&lt;/returns>&lt;/span>
        public static bool IsNullOrEmpty(SecureString value)
        {
            return value == null || value.Length == 0;
        }

        /// &lt;span class="code-SummaryComment">&lt;summary>&lt;/span>
        /// Performs bytewise comparison of two secure strings.
        /// &lt;span class="code-SummaryComment">&lt;/summary>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;param name="value">&lt;/param>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;param name="other">&lt;/param>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;returns>True if the strings are equal.&lt;/returns>&lt;/span>
        public static bool Matches(SecureString value, SecureString other)
        {
            if (value == null && other == null) return true;
            if (value == null || other == null) return false;
            if (value.Length != other.Length) return false;
            if (value.Length == 0 && other.Length == 0) return true;

            IntPtr ptrA = Marshal.SecureStringToCoTaskMemUnicode(value);
            IntPtr ptrB = Marshal.SecureStringToCoTaskMemUnicode(other);
            try
            {
                //parse characters one by one - doesn't change the fact that
                //we have them in memory however...
                byte byteA = 1;
                byte byteB = 1;

                int index = 0;
                while (((char)byteA) != '\0' && ((char)byteB) != '\0')
                {
                    byteA = Marshal.ReadByte(ptrA, index);
                    byteB = Marshal.ReadByte(ptrB, index);
                    if (byteA != byteB) return false;
                    index += 2;
                }

                return true;
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUnicode(ptrA);
                Marshal.ZeroFreeCoTaskMemUnicode(ptrB);
            }
        }
    }
}