using System;

namespace DotNetBrightener.Core.Localization
{
    public class CultureDictionaryRecord
    {
        public string Key         { get; }
        public string Translation { get; }

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