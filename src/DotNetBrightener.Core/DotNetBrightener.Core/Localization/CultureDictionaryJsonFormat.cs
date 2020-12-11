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
}