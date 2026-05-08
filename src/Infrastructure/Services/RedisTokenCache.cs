using Application.Abstractions.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Services;

internal sealed class RedisTokenCache(IDistributedCache cache) : ITokenCache
{
    public async Task<string?> GetAsync(string key)
    {
        return await cache.GetStringAsync(key);
    }


    public async Task SetAsync(string key, string value, TimeSpan expiry)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        };

        await cache.SetStringAsync(key, value, options);
    }
}
