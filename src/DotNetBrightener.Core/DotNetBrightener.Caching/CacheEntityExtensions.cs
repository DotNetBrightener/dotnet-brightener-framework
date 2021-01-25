using System;

namespace DotNetBrightener.Caching
{
    public static class CacheEntityExtensions
    {
        /// <summary>
        /// Get key for caching the entity
        /// </summary>
        /// <param name="id">Entity id</param>
        /// <returns>Key for caching the entity</returns>
        public static string GetEntityCacheKey<TEntityType>(object id)
        {
            return string.Format(CacheDefaultSettings.EntityCacheKey, typeof(TEntityType).Name.ToLower(), id);
        }

        /// <summary>
        /// Get key for caching the entity
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <param name="id">Entity id</param>
        /// <returns>Key for caching the entity</returns>
        public static string GetEntityCacheKey(Type entityType, object id)
        {
            return string.Format(CacheDefaultSettings.EntityCacheKey, entityType.Name.ToLower(), id);
        }
    }
}