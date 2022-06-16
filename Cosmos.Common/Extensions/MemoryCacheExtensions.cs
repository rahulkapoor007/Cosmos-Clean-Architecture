using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Common.Extensions
{
    public static class MemoryCacheExtensions
    {
        public static void SetRecordAsync<T>(this IMemoryCache cache,
            string recordId,
            T data,
            bool isSerialize,
            TimeSpan? absoluteExpireTime = null,
            TimeSpan? unusedExpireTime = null)
        {
            var options = new MemoryCacheEntryOptions();

            options.AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60);
            options.SlidingExpiration = unusedExpireTime;
            if (isSerialize)
            {
                string jsonData = JsonConvert.SerializeObject(data);
                cache.Set(recordId, jsonData, options);
            }
            else
                cache.Set(recordId, data, options);
        }

        public static T GetRecordAsync<T>(this IMemoryCache cache, string recordId, bool isDeserialize)
        {
            var jsonData = cache.Get(recordId);

            if (jsonData is null)
            {
                return default(T);
            }
            if (isDeserialize)
                return JsonConvert.DeserializeObject<T>(jsonData.ToString());
            else
                return (T)jsonData;
        }
    }
}
