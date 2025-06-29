using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace APIClothesEcommerceShop.Services.RedisCacheService
{
    public class RedisCacheService : IRedisCacheService
    {
        public readonly IDistributedCache? _cache;
        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public T? GetData<T>(string key)
        {
            var data = _cache?.GetString(key);
            if (data is null)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(data);
        }

        public void SetData<T>(string key, T data, double? cacheTime = 1)
        {
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime ?? 1)
            };

            _cache?.SetString(key, JsonSerializer.Serialize(data), options);
        }
    }
}