using System.Text;
using System.Security.Cryptography;
using System;
using System.IO;

namespace FoxCrypto
{
    public class CryptoInterface
    {
        public static string? Run(string data, string password, string direction = "enc")
        {
            byte[] salt = new byte[16];
            string passkey = null;
            using (SHA256 sha = SHA256.Create())
            {
                byte[] iBytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha.ComputeHash(iBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
                passkey = sb.ToString();
            }
            if (direction == "dec")
            {
                byte[] raw = Convert.FromBase64String(data);
                int index = Math.Min(raw.Length, salt.Length);
                Array.Copy(raw, salt, index);
                byte[] outdata = new byte[raw.Length - salt.Length];
                Array.Copy(raw, index, outdata, 0, outdata.Length);
                return Crypt(outdata, passkey, salt, direction);
            }
            else
            {
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }
                byte[] clearBytes = Encoding.UTF8.GetBytes(data);
                return Crypt(clearBytes, passkey, salt);
            }
        }

        private static string? Crypt(byte[] data, string passkey, byte[] salt, string direction = "enc")
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using (var key = new Rfc2898DeriveBytes(passkey, salt, 10000))
                {
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                }
                ICryptoTransform cryptor;
                if (direction == "dec")
                    cryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                else
                    cryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (cryptor)
                {
                    if (direction == "dec")
                    {
                        byte[]? decBytes = DoCrypto(data, cryptor);
                        if (decBytes == null) return null;
                        return Encoding.UTF8.GetString(decBytes);
                    }
                    else
                    {
                        byte[]? encBytes = DoCrypto(data, cryptor);
                        if (encBytes == null) return null;
                        byte[] dataOut = new byte[salt.Length + encBytes.Length];
                        Array.Copy(salt, 0, dataOut, 0, salt.Length);
                        Array.Copy(encBytes, 0, dataOut, salt.Length, encBytes.Length);
                        return Convert.ToBase64String(dataOut);
                    }
                }
            }
        }

        private static byte[]? DoCrypto(byte[] data, ICryptoTransform transform)
        {
            using (var memStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memStream, transform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    try
                    {
                        cryptoStream.FlushFinalBlock();
                    }
                    catch
                    {
                        return null;
                    }
                    return memStream.ToArray();
                }
            }
        }
    }
}
