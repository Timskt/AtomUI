using System.Collections.Concurrent;

namespace AtomUI.Controls;

internal static class IconProviderCache
{
    /// <summary>
    /// Maximum number of enum types to cache. When exceeded, oldest entries are removed.
    /// </summary>
    private const int MaxCacheSize = 256;
    
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, Type>> TypeCache = 
        new();
    
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, Func<Icon>>> CreatorCache = 
        new();
    
    private static readonly ConcurrentDictionary<Type, Func<Icon>> TypeToCreator = 
        new();
    
    /// <summary>
    /// Tracks insertion order for FIFO eviction when cache exceeds MaxCacheSize.
    /// </summary>
    private static readonly Queue<Type> CacheInsertionOrder = new();
    
    private static readonly object _lockObject = new();
    
    public static Type GetOrAddType(Type enumType, object enumValue, Func<object, Type> typeFactory)
    {
        EnsureCacheSize();
        
        var cache = TypeCache.GetOrAdd(enumType, _ =>
        {
            lock (_lockObject)
            {
                CacheInsertionOrder.Enqueue(enumType);
            }
            return new ConcurrentDictionary<object, Type>();
        });
        
        return cache.GetOrAdd(enumValue, typeFactory);
    }
    
    public static Func<Icon> GetOrAddCreator(Type enumType, object enumValue, 
        Func<object, Type> typeFactory, Func<Type, Func<Icon>> creatorFactory)
    {
        EnsureCacheSize();
        
        var cache = CreatorCache.GetOrAdd(enumType, _ =>
        {
            lock (_lockObject)
            {
                if (!CacheInsertionOrder.Contains(enumType))
                {
                    CacheInsertionOrder.Enqueue(enumType);
                }
            }
            return new ConcurrentDictionary<object, Func<Icon>>();
        });
        
        return cache.GetOrAdd(enumValue, value =>
        {
            var type = GetOrAddType(enumType, value, typeFactory);
            return TypeToCreator.GetOrAdd(type, creatorFactory);
        });
    }
    
    /// <summary>
    /// Ensures cache size doesn't exceed MaxCacheSize by removing oldest entries.
    /// </summary>
    private static void EnsureCacheSize()
    {
        lock (_lockObject)
        {
            while (TypeCache.Count > MaxCacheSize && CacheInsertionOrder.Count > 0)
            {
                var oldestType = CacheInsertionOrder.Dequeue();
                TypeCache.TryRemove(oldestType, out _);
                CreatorCache.TryRemove(oldestType, out _);
            }
        }
    }
    
    public static void ClearCache(Type enumType)
    {
        lock (_lockObject)
        {
            TypeCache.TryRemove(enumType, out _);
            CreatorCache.TryRemove(enumType, out _);
            // Note: We don't remove from CacheInsertionOrder to avoid lock contention
            // The queue will naturally be cleaned up during EnsureCacheSize operations
        }
    }
    
    public static void ClearAllCache()
    {
        lock (_lockObject)
        {
            TypeCache.Clear();
            CreatorCache.Clear();
            TypeToCreator.Clear();
            CacheInsertionOrder.Clear();
        }
    }
}
