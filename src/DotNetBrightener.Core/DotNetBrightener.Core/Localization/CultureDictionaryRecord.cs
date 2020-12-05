using System;
using System.Collections.Generic;

namespace DotNetBrightener.Core.Localization
{
    public class CultureDictionaryJsonFormat : Dictionary<string, string>
    {
        public CultureDictionaryJsonFormat()
        {

        }

        public CultureDictionaryJsonFormat(Dictionary<string, string> otherDictionary) : base(otherDictionary)
        {

        }
    }

    public class CultureDictionaryRecord
    {
        public string Key { get; private set; }
        public string Translation { get; private set; }

        public CultureDictionaryRecord(string messageId, string translation)
        {
            Key          = GetKey(messageId);
            Translation = translation;
        }

        public static string GetKey(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                throw new ArgumentException("MessageId can't be empty.", nameof(messageId));
            }

            return messageId;
        }
    }
}