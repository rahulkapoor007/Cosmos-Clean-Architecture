using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Common.Extensions
{
    public static class HashingExtensions
    {
        public static string GetIdentifier<T>(T columns)
        {
            string val = "";
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                var value = prop.GetValue(columns)?.ToString();
                val += !string.IsNullOrEmpty(value) ? value.Trim() : string.Empty;
            }
            val = val?.ToLower();

            StringBuilder hashedSttring = new StringBuilder();
            using (var hash = SHA1.Create())
            {
                byte[] allbytes = Encoding.UTF8.GetBytes(val);
                byte[] hashedBytes = hash.ComputeHash(allbytes);
                foreach (byte hashedByte in hashedBytes)
                {
                    hashedSttring.Append(hashedByte.ToString("X2"));
                }
            }
            return hashedSttring.ToString();
        }
        public static string GetIdentifier(string uniqueString)
        {
            StringBuilder hashedSttring = new StringBuilder();
            using (var hash = SHA1.Create())
            {
                byte[] allbytes = Encoding.UTF8.GetBytes(uniqueString);
                byte[] hashedBytes = hash.ComputeHash(allbytes);
                foreach (byte hashedByte in hashedBytes)
                {
                    hashedSttring.Append(hashedByte.ToString("X2"));
                }
            }
            return hashedSttring.ToString();
        }
    }
}
