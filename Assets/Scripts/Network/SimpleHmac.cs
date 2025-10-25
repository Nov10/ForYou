using System.Text;
using System.Security.Cryptography;

public static class SimpleHmac
{
    public static string Sign(string json, string secret)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
        {
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }
    }
}
