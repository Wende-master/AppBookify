using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using StackExchange.Redis;

namespace AppBookify.Helpers
{
    public static class HelperCacheMultiplexer
    {
    //    private static Lazy<ConnectionMultiplexer> CreateConnection(IConfiguration configuration) =>
    //new Lazy<ConnectionMultiplexer>(() =>
    //{
    //    var keyVaultUri = configuration["KeyVault:VaultUri"];
    //    var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
    //    var secretCache = secretClient.GetSecretAsync("CacheRedisBookify").Result;

    //    var cacheConnectionString = secretCache.Value.Value;
    //    return ConnectionMultiplexer.Connect(cacheConnectionString);
    //});

    //    public static ConnectionMultiplexer GetConnection(IConfiguration configuration)
    //    {
    //        return CreateConnection(configuration).Value;
    //    }

    }
}
