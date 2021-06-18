namespace DotNetBrightener.Caching.Redis
{
    public class RedisCacheConfiguration
    {
        public string ServerAddress               { get; set; }
        public int?   DefaultDatabase             { get; set; }
        public string RedisPassword               { get; set; }
        public int    RedisPort                   { get; set; }
        public int    KeepAlive                   { get; set; }
        public bool   IgnoreRedisTimeoutException { get; set; }
    }
}