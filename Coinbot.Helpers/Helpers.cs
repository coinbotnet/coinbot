using System;
using System.Security.Cryptography;
using System.Text;

namespace Coinbot
{
    public static class Helpers
    {
        public static int GetUnixTimeInSeconds ()
        {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static long GetUnixTimeInMilliseconds()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static string GetHashSHA512(String data, String key)
        {
            using (var hmac = new HMACSHA512(Encoding.ASCII.GetBytes(key)))
            {
                var messagebyte = Encoding.ASCII.GetBytes(data);
                var hashmessage = hmac.ComputeHash(messagebyte);
                var sign = BitConverter.ToString(hashmessage).Replace("-", "");

                return sign.ToLowerInvariant();
            }
        }

        public static string GetHashSHA256(String data, String key)
        {
            using (var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(key)))
            {
                var messagebyte = Encoding.ASCII.GetBytes(data);
                var hashmessage = hmac.ComputeHash(messagebyte);
                var sign = BitConverter.ToString(hashmessage).Replace("-", "");

                return sign.ToLowerInvariant();
            }
        }
    }
}
