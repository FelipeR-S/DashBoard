using System.Security.Cryptography;
using System.Text;

namespace DashBoard.Repositories
{
    public interface IEncryptData
    {
        /// <summary>
        /// Metodo para encriptar dados
        /// </summary>
        /// <param name="data">dado decriptado</param>
        /// <returns>Retorna <see cref="string"/> encriptada</returns>
        string Encrypt(string data);

        /// <summary>
        /// Metodo para decriptar dados
        /// </summary>
        /// <param name="data">dado encriptado</param>
        /// <returns>Retorna <see cref="string"/> decriptada</returns>
        string Decrypt(string data);
    }

    public class EncryptData : IEncryptData
    {
        private static readonly string _key = "B86ca6637D2e4531bdch2ea2315a1916";
        public string Encrypt(string data)
        {
            if (data == null || data == string.Empty) return "";

            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aesEncript = Aes.Create())
            {
                aesEncript.Key = Encoding.UTF8.GetBytes(_key);
                aesEncript.IV = iv;

                ICryptoTransform encryptor = aesEncript.CreateEncryptor(aesEncript.Key, aesEncript.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(data);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }
        public string Decrypt(string data)
        {
            if (data == null || data == string.Empty) return "";

            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(data);

            using (Aes aesDecript = Aes.Create())
            {
                aesDecript.Key = Encoding.UTF8.GetBytes(_key);
                aesDecript.IV = iv;

                ICryptoTransform decryptor = aesDecript.CreateDecryptor(aesDecript.Key, aesDecript.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
