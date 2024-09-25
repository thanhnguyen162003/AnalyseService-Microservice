using Application.Services.CacheService;
using Application.Services.CacheService.Intefaces;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;

namespace Application.Configurations;

public static class RedisConfig
{
    public static void AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        // services.AddSingleton<IValidationDistributedCache>(provider =>
        // {
        //     var options = provider.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
        //     options.Configuration = configuration["RedisValidate:RedisConfiguration"];
        //     options.InstanceName = configuration["RedisValidate:RedisInstance"];
        //     return new ValidationDistributedCache(Options.Create(options));
        // });

        // Configuration for Ordinary Cache
        services.AddSingleton<IOrdinaryDistributedCache>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
            options.Configuration = configuration["RedisCache:RedisConfiguration"];
            // options.InstanceName = configuration["RedisCache:RedisInstance"];
            return new OrdinaryDistributedCache(Options.Create(options));
        });
        // Optionally, add in-memory cache
        services.AddDistributedMemoryCache();
    }
}


