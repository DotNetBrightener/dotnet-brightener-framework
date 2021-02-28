using System;
using System.Linq;

namespace DotNetBrightener.SharedDataAccessService
{
    internal static class ReflectionExtensions
    {
        public static bool HasProperty<TType>(this Type type, string propertyName)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var property = type.GetProperties()
                               .FirstOrDefault(_ => _.Name == propertyName && _.PropertyType == typeof(TType));

            return property != null;
        }
    }
}