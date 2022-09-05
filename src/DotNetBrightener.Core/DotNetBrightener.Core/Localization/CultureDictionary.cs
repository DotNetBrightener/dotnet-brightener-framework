using System;
using System.Collections.Generic;

namespace DotNetBrightener.Core.Localization;

public class CultureDictionary
{
    public string CultureName { get; private set; }

    public PluralizationRuleDelegate PluralRule { get; private set; }

    public string this[string key] => this[key, false];

    public string this[string key, bool isPlural]
    {
        get
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            string translatedText = null;
            if (isPlural)
            {
                if (Translations.TryGetValue(key + "|plural", out translatedText))
                {
                    return translatedText;
                }
            }

            if (Translations.TryGetValue(key, out translatedText))
            {
                return translatedText;
            }

            return translatedText;
        }
    }

    public IDictionary<string, string> Translations { get; private set; }

    public CultureDictionary(string cultureName, PluralizationRuleDelegate pluralRule)
    {
        Translations = new Dictionary<string, string>();
        CultureName  = cultureName;
        PluralRule   = pluralRule;
    }

    public void MergeTranslations(IEnumerable<CultureDictionaryRecord> records)
    {
        foreach (var record in records)
        {
            Translations[record.Key] = record.Translation;
        }
    }
}