using med.common.library.configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace med.common.library.security
{
    public class Cryptography : ICryptography
    {
        private readonly CryptoSettings options;
        public Cryptography(IOptions<CryptoSettings> options)
        {
            this.options = options.Value;

        }

        public string Encrypt(string plainText)
        {
            using (RijndaelManaged rj = new()
            {
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC,
                KeySize = 256,
                BlockSize = 128
            })
            {
                var Key = Encoding.UTF8.GetBytes(this.options.SecretKey);
                var IV = Encoding.UTF8.GetBytes(this.options.SecretIv);

                var encryptor = rj.CreateEncryptor(Key, IV);

                using MemoryStream msEncrypt = new();
                using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter Encrypt = new(csEncrypt))
                    {
                        Encrypt.Write(plainText);
                    }

                    var encrypted = msEncrypt.ToArray();

                    return (Convert.ToBase64String(encrypted));
                };
            }
;

        }

        public string Decrypt(string cipherText)
        {
            string plaintext = null;
            var cipherTextByte = Convert.FromBase64String(cipherText);
            var Key = Encoding.UTF8.GetBytes(this.options.SecretKey);
            var IV = Encoding.UTF8.GetBytes(this.options.SecretIv);
            using (RijndaelManaged algo = new()
            {
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC,
                KeySize = 256,
                BlockSize = 128,
                Key = Key,
                IV = IV
            })
            {
                ICryptoTransform decryptor = algo.CreateDecryptor(algo.Key, algo.IV);
                MemoryStream msDecrypt = new(cipherTextByte);

                CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);

                using StreamReader Decrypt = new(csDecrypt);
                plaintext = Decrypt.ReadToEnd();
            }
            return plaintext;
        }
    }
}
