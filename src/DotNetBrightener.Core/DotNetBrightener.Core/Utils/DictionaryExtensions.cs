using System.Collections.Generic;

namespace DotNetBrightener.Core.Utils
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue obj;
            return dictionary.TryGetValue(key, out obj) ? obj : default(TValue);
        }

        public static bool GetIntValue(this IDictionary<string, object> dictionary, string key, out int value)
        {
            value = 0;
            if (!dictionary.ContainsKey(key))
                return false;

            var objValue = dictionary[key];
            return int.TryParse(objValue.ToString(), out value);
        }

        public static bool GetLongValue(this IDictionary<string, object> dictionary, string key, out long value)
        {
            value = 0;
            if (!dictionary.ContainsKey(key))
                return false;

            var objValue = dictionary[key];
            return long.TryParse(objValue.ToString(), out value);
        }

        public static bool GetFloatValue(this IDictionary<string, object> dictionary, string key, out float value)
        {
            value = 0;
            if (!dictionary.ContainsKey(key))
                return false;

            var objValue = dictionary[key];
            return float.TryParse(objValue.ToString(), out value);
        }

        public static bool GetBoolValue(this IDictionary<string, object> dictionary, string key, out bool value)
        {
            value = false;
            if (!dictionary.ContainsKey(key))
                return false;

            var objValue = dictionary[key];
            return bool.TryParse(objValue.ToString(), out value);
        }

        public static bool GetStringValue(this IDictionary<string, object> dictionary, string key, out string value)
        {
            value = string.Empty;
            if (!dictionary.ContainsKey(key))
                return false;

            var objValue = dictionary[key];
            value = objValue.ToString();
            return true;
        }
    }
}