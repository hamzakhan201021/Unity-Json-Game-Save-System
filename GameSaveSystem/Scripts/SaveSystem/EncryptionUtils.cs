using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace HKGameSave
{
    public static class EncryptionUtils
    {
        /// <summary>
        /// Default Encryption Key for AES
        /// </summary>
        private static readonly string _defaultEncryptionKey = "f8GZxT3qK9vR2uY7NBmH5jdqXAp0LCsW";

        /// <summary>
        /// Encrypt plain text using AES, if not provided encryption key, will select default.
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="encryptionKey"></param>
        /// <returns></returns>
        public static string EncryptString(string plainText, string encryptionKey = null)
        {
            try
            {
                encryptionKey = encryptionKey != null ? encryptionKey : _defaultEncryptionKey;

                byte[] key = Encoding.UTF8.GetBytes(encryptionKey.Substring(0, 32));

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.GenerateIV();
                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (var msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (UnityException ex)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Failed to encrypt with error " + ex);
#endif
                return "";
            }
        }

        /// <summary>
        /// Decrypt encrypted plain text using AES, if not provided encryption key, will select default.
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <param name="encryptionKey"></param>
        /// <returns></returns>
        public static (bool success, string plainText) DecryptString(string encryptedText, string encryptionKey = null)
        {
            try
            {
                encryptionKey = encryptionKey != null ? encryptionKey : _defaultEncryptionKey;

                byte[] fullCipher = Convert.FromBase64String(encryptedText);
                byte[] iv = new byte[16];
                byte[] cipher = new byte[fullCipher.Length - 16];

                Array.Copy(fullCipher, iv, iv.Length);
                Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);

                byte[] key = Encoding.UTF8.GetBytes(encryptionKey.Substring(0, 32));

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                return new(true, srDecrypt.ReadToEnd());
                            }
                        }
                    }
                }
            }
            catch (UnityException ex)
            {
#if UNITY_EDITOR
                Debug.Log("Failed to decrypt data with error " + ex);
#endif

                return new(false, "");
            }
        }
    }
}