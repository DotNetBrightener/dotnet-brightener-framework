using Newtonsoft.Json;

namespace DotNetBrightener.Core.Authentication.Configs
{
    public class JwtConfig
    {
        public const string JwtDefaultIssuer = "https://validissuer.dotnetbrightener.com";

        public const int DefaultExpiration = 30;

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string SigningKey { get; set; }

        public int ExpireAfterMinutes { get; set; }

        [JsonIgnore]
        public string KID { get; set; } = DefaultJwtKId;

        public const string DefaultJwtKId = "_DEFAULT_KID_VALUE";
    }
}