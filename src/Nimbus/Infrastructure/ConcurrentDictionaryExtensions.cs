using System.Collections.Concurrent;

namespace Nimbus.Infrastructure
{
    public static class ConcurrentDictionaryExtensions
    {
        public static void SafeAddKey<T>(this ConcurrentDictionary<T, object> dictionary, T key)
        {
            dictionary.AddOrUpdate(key, (object)null, (k, e) => null);
        }

        public static void SafeRemoveKey<T>(this ConcurrentDictionary<T, object> dictionary, T key)
        {
            object val;
            dictionary.TryRemove(key, out val);
        }
    }
}