using StackExchange.Redis;

namespace AppBookify.Helpers
{
    public static class HelperCacheMultiplexer
    {
        private static Lazy<ConnectionMultiplexer>
            CreateConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                string cacheRedisKeys = "cachebookify.redis.cache.windows.net:6380,password=bJ92xKhYUEENAsJ7fckbrenN3igv6fVHPAzCaMFRxfU=,ssl=True,abortConnect=False";
                return ConnectionMultiplexer.Connect(cacheRedisKeys);
            });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return CreateConnection.Value;
            }
        }
    }
}
