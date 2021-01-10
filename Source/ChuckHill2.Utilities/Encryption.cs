using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ChuckHill2.Extensions;

namespace ChuckHill2
{
    public static class Encryption
    {
        private static readonly AesCryptoServiceProvider AesProvider = null;

        static Encryption()
        {
            var pwd = "my!pa$$w0rd";
            // Pre-computes, on-demand, new key and initialization vector for 256-bit AES string encryption.
            byte[] salt = Encoding.UTF8.GetBytes("What I do is appending a random salt bytes in front of the original bytes before encryption, and remove it after decryption.");
            Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(pwd, salt);
            byte[] KEY = keyGenerator.GetBytes(32); //AES max key size=256 bits
            byte[] IV = keyGenerator.GetBytes(16);  //AES has only 1 possible block size=128 bits
            AesProvider = new AesCryptoServiceProvider() { Padding = PaddingMode.PKCS7, KeySize = KEY.Length * 8, Key = KEY, IV = IV };
        }

        /// <summary>
        /// Get or set FIPS compliance flag.
        /// A hacky way to allow non-FIPS compliant algorthms to run.
        /// Non-FIPS compliant algorthims are:
        ///     MD5CryptoServiceProvider,
        ///     RC2CryptoServiceProvider,
        ///     RijndaelManaged,
        ///     RIPEMD160Managed,
        ///     SHA1Managed,
        ///     SHA256Managed,
        ///     SHA384Managed,
        ///     SHA512Managed,
        ///     AesManaged,
        ///     MD5Cng. 
        /// In particular, enables use of fast MD5 hash to create unique identifiers for internal use.
        /// </summary>
        public static bool FIPSCompliance
        {
            get { return CryptoConfig.AllowOnlyFipsAlgorithms; }
            set
            {
                FieldInfo fi;
                fi = typeof(CryptoConfig).GetField("s_fipsAlgorithmPolicy", BindingFlags.Static | BindingFlags.NonPublic);
                if (fi != null) fi.SetValue(null, value);
                fi = typeof(CryptoConfig).GetField("s_haveFipsAlgorithmPolicy", BindingFlags.Static | BindingFlags.NonPublic);
                if (fi != null) fi.SetValue(null, true);
            }
        }

        /// <summary>
        /// Standard 256-bit AES string encryption. 
        /// Note: 256-bit AES is a newer variant of Rijndael encryption.
        /// If the string was already encrypted, the string is not encrypted again.
        /// </summary>
        /// <param name="stringToEncrypt"></param>
        /// <returns>Hexidecimal string</returns>
        public static string EncryptToHex(string stringToEncrypt)
        {
            if (IsHexEncrypted(stringToEncrypt)) return stringToEncrypt; //do not doubly encrypt!
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (var cryptor = AesProvider.CreateEncryptor())
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, cryptor, CryptoStreamMode.Write))
                        using (StreamWriter sw = new StreamWriter(csEncrypt))
                            sw.Write(stringToEncrypt);
                return msEncrypt.ToArray().ToHex();
            }
        }

        /// <summary>
        /// Standard 256-bit AES string encryption
        /// Note: 256-bit AES is a newer variant of Rijndael encryption.
        /// If the string was already encrypted, the string is not encrypted again.
        /// </summary>
        /// <param name="stringToEncrypt"></param>
        /// <returns>Base64 string</returns>
        public static string EncryptToBase64(string stringToEncrypt)
        {
            if (IsBase64Encrypted(stringToEncrypt)) return stringToEncrypt; //do not doubly encrypt!
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt64 = new CryptoStream(msEncrypt, new ToBase64Transform(), CryptoStreamMode.Write))
                    using (CryptoStream csEncrypt = new CryptoStream(csEncrypt64, AesProvider.CreateEncryptor(), CryptoStreamMode.Write))
                        using (StreamWriter sw = new StreamWriter(csEncrypt))
                             sw.Write(stringToEncrypt);
                return Encoding.UTF8.GetString(msEncrypt.ToArray());
            }
        }

        private static bool IsHexEncrypted(string s)
        {
            try
            {
                string result;
                if (!s.IsHex() || s.Length < 32) return false; //AES encryption is in multiples of 16 bytes
                byte[] bytes = s.FromHex();
                using (var ms = new MemoryStream(bytes))
                    using (var cryptor = AesProvider.CreateDecryptor())
                        using (var csDecrypt = new CryptoStream(ms, cryptor, CryptoStreamMode.Read))
                            using (var sr = new StreamReader(csDecrypt, Encoding.UTF8))
                                result = sr.ReadToEnd();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsBase64Encrypted(string s)
        {
            try
            {
                string result;
                if (!s.IsBase64() || s.Length < 24) return false; //AES encryption is in multiples of 16 bytes
                byte[] bytes = Convert.FromBase64String(s);
                if ((bytes.Length % 16) != 0) return false;
                using (var ms = new MemoryStream(bytes))
                    using (var cryptor = AesProvider.CreateDecryptor())
                        using (var csDecrypt = new CryptoStream(ms, cryptor, CryptoStreamMode.Read))
                            using (var sr = new StreamReader(csDecrypt, Encoding.UTF8))
                                result = sr.ReadToEnd();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Standard String Decryption. 
        /// Note: 256-bit AES is a newer variant of Rijndael encryption.
        /// If string was just encoded (hex) but not encrypted, the decoded string is returned.
        /// If string was not encoded, it is just returned as-is.
        /// Note: Exceptions are never thrown.
        /// </summary>
        /// <param name="stringToDecrypt">Hexidecimal string to decrypt</param>
        /// <returns>Decrypted string</returns>
        public static string DecryptFromHex(string stringToDecrypt)
        {
            byte[] bytes = null;
            try
            {
                if (!stringToDecrypt.IsHex() || (stringToDecrypt.Length % 32) != 0) return stringToDecrypt; //AES encryption is in multiples of 16 bytes
                bytes = stringToDecrypt.FromHex();
                using (var ms = new MemoryStream(bytes))
                    using (var cryptor = AesProvider.CreateDecryptor())
                        using (var csDecrypt = new CryptoStream(ms, cryptor, CryptoStreamMode.Read))
                            using (var sr = new StreamReader(csDecrypt, Encoding.UTF8))
                                return sr.ReadToEnd();
            }
            catch 
            {
                //Decryption failed. Apparently 'stringToDecrypt' was not an encrypted hex string.
                //Due to potential internationalization, we cannot detect if this is because of encryption corruption or simply unencrypted.
            }
            //If we get this far, the string was merely encoded as Hex.
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Standard String Decryption. 
        /// Note: 256-bit AES is a newer variant of Rijndael encryption.
        /// Also handles legacy product encrypted strings.
        /// If string was just encoded (base64) but not encrypted, the decoded string is returned.
        /// If string was not encoded, it is just returned as-is.
        /// Note: Exceptions are not thrown.
        /// </summary>
        /// <param name="stringToDecrypt">Base64 string to decrypt</param>
        /// <returns>Decrypted string</returns>
        public static string DecryptFromBase64(string stringToDecrypt)
        {
            byte[] bytes = null;
            try
            {
                if (!stringToDecrypt.IsBase64()) return stringToDecrypt;
                bytes = Convert.FromBase64String(stringToDecrypt); //Can't use crypto FromBase64Transform() because StringReader is not a type of StreamReader().
                if ((bytes.Length % 16) == 0)
                {
                    using (var ms = new MemoryStream(bytes))
                        using (var cryptor = AesProvider.CreateDecryptor())
                            using (var csDecrypt = new CryptoStream(ms, cryptor, CryptoStreamMode.Read))
                                using (var sr = new StreamReader(csDecrypt, Encoding.UTF8))
                                    return sr.ReadToEnd();
                }
            }
            catch
            {
                //Decryption failed. Apparently 'stringToDecrypt' was not an encrypted base64 string.
                //Due to potential internationalization, we cannot detect if this is because of encryption corruption or simply unencrypted.
            }

            //Legacy did not support encrypting/decrypting from Hexidecimal, so we don't try.
            try { return LegacyAesDecryptString(bytes); } catch { }
            try { return LegacyRijndaelDecryptString(bytes); } catch { }

            //If we get this far, tne string was encoded as Base64 but not encrypted.
            return Encoding.UTF8.GetString(bytes);
        }

        #region Legacy AES Crypto to Base64
        private const string legacyPassword = "\x50\x61\x6E\x64\x6F\x72\x61";
        private static byte[] AesKEY = null;
        private static byte[] AesIV = null;

        private static string LegacyAesDecryptString(byte[] bytes)
        {
            CryptoStream csDecrypt = null;
            StreamReader sr = null;
            try
            {
                if (AesKEY == null)
                {
                    byte[] salt = Encoding.Default.GetBytes("abcdefgh");
                    Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(legacyPassword, salt);
                    AesKEY = keyGenerator.GetBytes(16);
                    AesIV = keyGenerator.GetBytes(16);
                }
                csDecrypt = new CryptoStream(new MemoryStream(bytes), new AesCryptoServiceProvider() { Key = AesKEY, IV = AesIV }.CreateDecryptor(), CryptoStreamMode.Read);
                sr = new StreamReader(csDecrypt, Encoding.UTF8);
                return sr.ReadToEnd();
            }
            finally
            {
                if (csDecrypt != null) csDecrypt.Dispose();
                if (sr != null) sr.Dispose();
            }
        }
        /// <summary>
        /// [Deprecated]
        /// Legacy 128-bit AES encryption to base64 string.
        /// Use EncryptToBase64() instead.
        /// </summary>
        /// <param name="stringToEncrypt"></param>
        /// <returns></returns>
        public static string LegacyAesEncryptString(string stringToEncrypt)
        {
            if (AesKEY == null)
            {
                byte[] salt = Encoding.Default.GetBytes("abcdefgh");
                Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(legacyPassword, salt);
                AesKEY = keyGenerator.GetBytes(16);
                AesIV = keyGenerator.GetBytes(16);
            }

            using (var aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = AesKEY;
                aesAlg.IV = AesIV;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt64 = new CryptoStream(msEncrypt, new ToBase64Transform(), CryptoStreamMode.Write))
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(csEncrypt64, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(stringToEncrypt);
                            }
                        }
                    }
                    return Encoding.UTF8.GetString(msEncrypt.ToArray());
                }
            }
        }
        #endregion

        #region Legacy Rijndael Crypto to Base64
        private static byte[] RijndaelKEY = { 82, 152, 54, 255, 183, 222, 126, 48, 134, 13, 68, 67, 216, 219, 132, 231, 38, 241, 89, 32, 51, 192, 159, 231, 43, 132, 137, 64, 20, 71, 236, 74 };
        private static byte[] RijndaelIV = { 62, 233, 35, 215, 49, 197, 201, 134, 19, 221, 61, 121, 70, 253, 149, 242 };
        /// <summary>
        /// [Deprecated]
        /// Legacy 256-bit Rijndael encryption to base64 string. Rijndael is an older variant of 256-bit AES.
        /// Use EncryptToBase64() instead.
        /// </summary>
        /// <param name="unencryptedString"></param>
        /// <returns></returns>
        public static string LegacyRijndaelEncryptString(string unencryptedString)
        {
            MemoryStream mStream = new MemoryStream();
            AesCryptoServiceProvider RM = new AesCryptoServiceProvider(); RM.Key = RijndaelKEY; RM.IV = RijndaelIV;
            CryptoStream cStream = new CryptoStream(mStream, RM.CreateEncryptor(), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cStream);
            sw.Write(unencryptedString); sw.Flush(); sw.Close();
            byte[] ret = mStream.ToArray();
            cStream.Close();
            mStream.Close();
            return Convert.ToBase64String(ret);
        }
        private static string LegacyRijndaelDecryptString(byte[] encrypted)
        {
            MemoryStream msDecrypt = null;
            AesCryptoServiceProvider RM = null;
            CryptoStream csDecrypt = null;
            StreamReader sr = null;
            try
            {
                msDecrypt = new MemoryStream(encrypted);
                RM = new AesCryptoServiceProvider(); RM.Key = RijndaelKEY; RM.IV = RijndaelIV;
                csDecrypt = new CryptoStream(msDecrypt, RM.CreateDecryptor(), CryptoStreamMode.Read);
                sr = new StreamReader(csDecrypt);
                return sr.ReadToEnd();
            }
            finally
            {
                if (sr != null) sr.Dispose();
                if (csDecrypt != null) csDecrypt.Dispose();
                if (RM != null) RM.Dispose();
                if (msDecrypt != null) msDecrypt.Dispose();
            }
        }
        #endregion

        /// <summary>
        /// Compute unique hash of specified file
        /// Use System.LinQ 'hash1.SequenceEqual(hash2)' for equality test.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>hash bytes</returns>
        public static byte[] CreateUniqueHash(string filename)
        {
            if (filename.IsNullOrEmpty()) return null;
            if (!File.Exists(filename)) return null;
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return CreateUniqueHash(stream);
            }
        }
        /// <summary>
        /// Compute unique hash of byte array.
        /// Use System.LinQ 'hash1.SequenceEqual(hash2)' for equality test.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>hash bytes</returns>
        public static byte[] CreateUniqueHash(byte[] bytes)
        {
            if (bytes==null || bytes.Length<16) return bytes;
            using (var stream = new MemoryStream(bytes))
            {
                return CreateUniqueHash(stream);
            }
        }
        /// <summary>
        /// Compute unique hash of byte stream.
        /// Use System.LinQ 'hash1.SequenceEqual(hash2)' for equality test.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>hash bytes</returns>
        public static byte[] CreateUniqueHash(Stream stream)
        {
            if (stream == null) return null;
            if (!stream.CanRead) return null;
            //MD5 is not FIPS compliant, but this is not used for security, fits very nicely in a guid, and is FAST.
            //using (var provider = MD5.Create()) { return provider.ComputeHash(stream); }
            using (var provider = new SHA256CryptoServiceProvider()) { return provider.ComputeHash(stream); }
        }

        /// <summary>
        /// Decrypt, cleanup, and fixup the ConfigurationManager values as necessary BEFORE anyone, 
        /// including 3rd-party tools, attempts to reference these values. This only applies to 
        /// the in-memory representation of the ConfigurationManager. The source app.config is 
        /// not touched.
        /// </summary>
        /// <param name="key">AppConfig ConnectionStrings key name</param>
        public static void DecryptConfigurationManagerConnectionString(string key)
        {
            //Fixup the security connection string, as necessary.
            ConnectionStringSettings css = ConfigurationManager.ConnectionStrings[key];
            if (css != null) //Is connection string in the ConfigurationManager?
            {
                string cs = css.ConnectionString;

                SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(cs);
                if (!csb.Password.IsNullOrEmpty())
                {
                    csb.Password = Encryption.DecryptFromHex(csb.Password);  //Decrypt the password.

                    //Put the connection string back into the in-memory ConfigurationManager. This is the magic.
                    var fiModified = typeof(System.Configuration.ConfigurationElement).GetField("_bModified", BindingFlags.Instance | BindingFlags.NonPublic);
                    var fiReadOnly = typeof(System.Configuration.ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (fiModified != null && fiReadOnly != null)
                    {
                        fiReadOnly.SetValue(css, false);

                        css.ConnectionString = csb.ConnectionString;

                        fiReadOnly.SetValue(css, true);
                        fiModified.SetValue(css, false);
                    }
                }
            }
        }
    } 
}
