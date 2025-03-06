using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ChatServer.Main.Services.Helper
{
    public interface ICipherHelper
    {
        string Encrypt(string text);
        string Decrypt(string text);
    }

    public class CipherHelper : ICipherHelper
    {
        private readonly string encryptionKey;

        public CipherHelper(IConfigurationRoot configuration)
        {
            encryptionKey = configuration["EncryptionKey"]!;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Encrypt(string text)
        {
            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                var key = Encoding.UTF8.GetBytes(encryptionKey);
                aes.Key = key.Take(aes.KeySize / 8).ToArray();
                aes.IV = new byte[aes.BlockSize / 8];

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new System.Security.Cryptography.CryptoStream(ms, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(text);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Decrypt(string text)
        {
            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                var key = Encoding.UTF8.GetBytes(encryptionKey);
                aes.Key = key.Take(aes.KeySize / 8).ToArray();
                aes.IV = new byte[aes.BlockSize / 8];

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream(Convert.FromBase64String(text)))
                {
                    using (var cs = new System.Security.Cryptography.CryptoStream(ms, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
