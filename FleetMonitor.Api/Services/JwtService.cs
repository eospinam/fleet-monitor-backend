using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FleetMonitor.Api.Services {
    public class JwtService {
        private readonly byte[] _key;
        public JwtService(string secret) => _key = Encoding.UTF8.GetBytes(secret);

        private static string Base64UrlEncode(byte[] input){
            return Convert.ToBase64String(input).TrimEnd('=').Replace('+','-').Replace('/','_');
        }

        private static byte[] Base64UrlDecode(string input){
            string s = input.Replace('-','+').Replace('_','/');
            switch(s.Length % 4){ case 2: s += "=="; break; case 3: s += "="; break; }
            return Convert.FromBase64String(s);
        }

        public string CreateToken(string subject, string username, string role, TimeSpan ttl){
            var header = JsonSerializer.Serialize(new { alg="HS256", typ="JWT" });
            var exp = DateTimeOffset.UtcNow.Add(ttl).ToUnixTimeSeconds();
            var payload = JsonSerializer.Serialize(new { sub=subject, username, role, exp });
            var header64 = Base64UrlEncode(Encoding.UTF8.GetBytes(header));
            var payload64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
            var signingInput = $"{header64}.{payload64}";
            using var hmac = new HMACSHA256(_key);
            var sig = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
            return $"{signingInput}.{Base64UrlEncode(sig)}";
        }

        public (bool valid, Dictionary<string,object> claims) ValidateToken(string token){
            try {
                var parts = token.Split('.');
                if(parts.Length != 3) return (false, null);
                var signingInput = parts[0] + "." + parts[1];
                var sigBytes = Base64UrlDecode(parts[2]);
                using var hmac = new HMACSHA256(_key);
                var expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
                if (!CryptographicOperations.FixedTimeEquals(expected, sigBytes)) return (false, null);
                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
                var doc = JsonDocument.Parse(payloadJson);
                if (doc.RootElement.TryGetProperty("exp", out var expEl)) {
                    var exp = expEl.GetInt64();
                    if (exp < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return (false, null);
                }
                var dict = new Dictionary<string, object>();
                foreach(var prop in doc.RootElement.EnumerateObject()) {
                    if (prop.Value.ValueKind == JsonValueKind.Number) dict[prop.Name] = prop.Value.GetInt64();
                    else dict[prop.Name] = prop.Value.GetString();
                }
                return (true, dict);
            } catch {
                return (false, null);
            }
        }
    }
}
