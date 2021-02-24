using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

//.NET AES-GCM is not available < .NET 5.0. Must use Win32 BCrypt API directly.
//https://docs.microsoft.com/en-us/windows/win32/api/bcrypt/nf-bcrypt-bcryptencrypt?redirectedfrom=MSDN
//https://github.com/dotnet/pinvoke/blob/master/src/BCrypt/BCrypt.cs
// see: https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography?view=net-5.0
//https://www.google.com/search?sxsrf=ALeKk01BbSglowEW7GwwyMrDUmxw-LdQ-A%3A1613762743184&ei=txAwYKjICqPe9AO48LPQBA&q=C%23+.NET+4.7+Microsoft+Crypto+AES256-GCM&oq=C%23+.NET+4.7+Microsoft+Crypto+AES256-GCM&gs_lcp=Cgdnd3Mtd2l6EAM6BwgAEEcQsANQzp06WOXMOmCi4jpoAnACeACAAacBiAHiB5IBAzguMpgBAKABAaoBB2d3cy13aXrIAQjAAQE&sclient=gws-wiz&ved=0ahUKEwjo_7vY1vbuAhUjL30KHTj4DEoQ4dUDCA0&uact=5
//https://docs.microsoft.com/en-us/windows/win32/api/bcrypt/nf-bcrypt-bcryptencrypt
//https://docs.microsoft.com/en-us/windows/win32/api/bcrypt/nf-bcrypt-bcryptdecrypt
//https://codereview.stackexchange.com/questions/13714/symmetric-encryption-decryption-routine-using-aes
//https://codereview.stackexchange.com/questions/175141/encrypt-and-decrypt-a-message-using-aes-256-with-gcm-mode-using-bouncy-castle-c
//https://stackoverflow.com/questions/30720414/how-to-chain-bcryptencrypt-and-bcryptdecrypt-calls-using-aes-in-gcm-mode
//https://github.com/Brebl/Aes-gcm/tree/master/Src/Crypt
//https://github.com/search?l=C&q=AES-GCM&type=Repositories
//https://www.codeproject.com/Articles/18713/Simple-Way-to-Crypt-a-File-with-CNG
//https://nsuchyme-2bdc78.ingress-bonde.easywp.com/2020/04/18/how-to-read-encrypted-google-chrome-cookies-in-c/
//Example Code: https://github.com/lellis1936/GcmCrypt
//https://docs.microsoft.com/en-us/windows/win32/api/bcrypt/nf-bcrypt-bcryptencrypt?redirectedfrom=MSDN
//https://github.com/dotnet/pinvoke/blob/master/src/BCrypt/BCrypt.cs

/// <summary>
/// Internal .NET interface for Sodium encryption library.
/// Extracted just what we need from 2017 nuget package libsodium-net by Adam Caudill 2013-2016. It's old but it works.
/// * Supports both 32-bit and 64-bit. 
/// * libsodium C binaries must be embedded so we can extract and use upon demand.
/// </summary>
namespace Sodium
{
    internal static class DynamicInvoke
    {
        public static T GetDynamicInvoke<T>(string function, string library)
        {
            TypeBuilder typeBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("DynamicDllInvoke"), AssemblyBuilderAccess.Run).DefineDynamicModule("DynamicDllModule").DefineType("DynamicDllInvokeType", TypeAttributes.Public | TypeAttributes.UnicodeClass);
            MethodInfo method = typeof(T).GetMethod("Invoke");
            Type[] array = ((IEnumerable<ParameterInfo>)method.GetParameters()).Select<ParameterInfo, Type>((Func<ParameterInfo, Type>)(param => param.ParameterType)).ToArray<Type>();
            MethodBuilder methodBuilder = typeBuilder.DefinePInvokeMethod(function, library, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl, CallingConventions.Standard, method.ReturnType, array, CallingConvention.Cdecl, CharSet.Ansi);
            methodBuilder.SetImplementationFlags(methodBuilder.GetMethodImplementationFlags() | MethodImplAttributes.PreserveSig);
            return (T)(object)Delegate.CreateDelegate(typeof(T), typeBuilder.CreateType().GetMethod(function), true);
        }
    }

    internal class LazyInvoke<T>
    {
        private readonly string _function;
        private readonly string _library;
        private T _method;
        private bool _missing;

        public LazyInvoke(string function, string library)
        {
            this._function = function;
            this._library = library;
            this._missing = true;
        }

        public T Method
        {
            get
            {
                if (this._missing)
                {
                    this._method = DynamicInvoke.GetDynamicInvoke<T>(this._function, this._library);
                    this._missing = false;
                }
                return this._method;
            }
        }
    }

    internal class SodiumLibrary
    {
        public delegate int _DecryptAes(
              IntPtr message,
              out long messageLength,
              byte[] nsec,
              byte[] cipher,
              long cipherLength,
              byte[] additionalData,
              long additionalDataLength,
              byte[] nonce,
              byte[] key);

        public static LazyInvoke<SodiumLibrary._DecryptAes> _crypto_aead_aes256gcm_decrypt = new LazyInvoke<SodiumLibrary._DecryptAes>(nameof(crypto_aead_aes256gcm_decrypt), SodiumLibrary.Name);

        public static SodiumLibrary._DecryptAes crypto_aead_aes256gcm_decrypt => SodiumLibrary._crypto_aead_aes256gcm_decrypt.Method;

        public static string Name => IntPtr.Size == 8 ? "libsodium-64.dll" : "libsodium.dll";
    }

    internal class SecretAeadAes
    {
        static SecretAeadAes()
        {
            var name = SodiumLibrary.Name;
            if (File.Exists(name)) return;
            var t = typeof(SecretAeadAes);
            using (var stream = File.Open(name, FileMode.Create, FileAccess.Write, FileShare.Read))
                t.Assembly.GetManifestResourceStream(t.Assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(name, StringComparison.OrdinalIgnoreCase)) ?? "NULL").CopyTo(stream);
        }

        public static byte[] Decrypt(byte[] cipher, byte[] nonce, byte[] key, byte[] additionalData = null)
        {
            if (additionalData == null) additionalData = new byte[0];
            if (key == null || key.Length != 32) throw new ArgumentOutOfRangeException(nameof(key), (object)(key == null ? 0 : key.Length), string.Format("key must be {0} bytes in length.", (object)32));
            if (nonce == null || nonce.Length != 12) throw new ArgumentOutOfRangeException(nameof(nonce), (object)(nonce == null ? 0 : nonce.Length), string.Format("nonce must be {0} bytes in length.", (object)12));
            byte[] destination = additionalData.Length <= 16 && additionalData.Length >= 0 ? new byte[cipher.Length - 16] : throw new ArgumentOutOfRangeException(string.Format("additionalData must be between {0} and {1} bytes in length.", (object)0, (object)16));
            IntPtr num1 = Marshal.AllocHGlobal(destination.Length);
            long messageLength;

            int num2 = SodiumLibrary.crypto_aead_aes256gcm_decrypt(num1, out messageLength, (byte[])null, cipher, (long)cipher.Length, additionalData, (long)additionalData.Length, nonce, key);

            Marshal.Copy(num1, destination, 0, (int)messageLength);
            Marshal.FreeHGlobal(num1);
            if (num2 != 0) throw new CryptographicException("Error decrypting message.");

            if ((long)destination.Length == messageLength) return destination;

            byte[] numArray = new byte[messageLength];
            Array.Copy((Array)destination, 0L, (Array)numArray, 0L, messageLength);

            return numArray;
        }
    }
}
