using System;

namespace DotNetBrightener.Core.Localization
{

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