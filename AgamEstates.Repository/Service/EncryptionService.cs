using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AgamEstates.Repository.Service
{
    public class EncryptionService
    {
        private const string DefaultSalt = "AgamEstatesSecuritySalt2026";

        public static string GetUniqueKey()
        {
            var key = string.Format("AGAM{0}93B{1}28T{2}71-{3}",
                        DateTime.Now.ToString("MM"),
                        DateTime.Now.ToString("dd"),
                        DateTime.Now.ToString("yy"),
                        Guid.NewGuid().ToString());
            return key;
        }

        public static string Encrypt(string clearText, string encryptionKey)
        {
            var clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }, 10000, HashAlgorithmName.SHA256);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string encryptText, string encryptionKey)
        {
            var cipherBytes = Convert.FromBase64String(encryptText);
            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }, 10000, HashAlgorithmName.SHA256);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    encryptText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return encryptText;
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = $"{password}:{DefaultSalt}";
                var bytes = Encoding.UTF8.GetBytes(saltedPassword);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            var hashOfEntered = HashPassword(enteredPassword);
            return string.Equals(hashOfEntered, storedHash, StringComparison.Ordinal);
        }

        public static string GenerateRandomPassword()
        {
            const string smallCaseAlpha = "abcdefghijklmnopqrstuvwxyz";
            const string upperCaseAlpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbers = "1234567890";
            const string specialChars = "~!@#$%^&*";

            StringBuilder res = new StringBuilder();
            Random rnd = new Random();

            for (int i = 0; i < 3; i++)
                res.Append(smallCaseAlpha[rnd.Next(smallCaseAlpha.Length)]);
            for (int i = 0; i < 2; i++)
                res.Append(upperCaseAlpha[rnd.Next(upperCaseAlpha.Length)]);
            for (int i = 0; i < 2; i++)
                res.Append(numbers[rnd.Next(numbers.Length)]);
            for (int i = 0; i < 2; i++)
                res.Append(specialChars[rnd.Next(specialChars.Length)]);

            return res.ToString();
        }
    }
}
