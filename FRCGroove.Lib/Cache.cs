using System;
using System.Collections.Generic;

namespace FRCGroove.Lib
{
    public class CachedItem<T>
    {
        public T Data { get; set; }
        public string ETag { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class Cache<T>
    {
        private readonly Dictionary<string, CachedItem<T>> _cache = new Dictionary<string, CachedItem<T>>();

        public CachedItem<T> Get(string key)
        {
            if (_cache.TryGetValue(key, out var cachedItem))
            {
                return cachedItem;
            }
            return null;
        }

        public void Set(string key, T data, string eTag, DateTime expiration)
        {
            _cache[key] = new CachedItem<T>
            {
                Data = data,
                ETag = eTag,
                Expiration = expiration
            };
        }

        public bool Contains(string key)
        {
            return _cache.ContainsKey(key);
        }
    }
}
