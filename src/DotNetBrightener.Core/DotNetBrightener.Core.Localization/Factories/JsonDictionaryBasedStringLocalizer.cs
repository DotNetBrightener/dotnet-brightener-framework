using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetBrightener.Core.Localization.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.Localization.Factories
{
    public class JsonDictionaryBasedStringLocalizer : IStringLocalizer
    {
        private readonly ILocalizationManager _localizationManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger              _logger;

        public JsonDictionaryBasedStringLocalizer(ILocalizationManager                        localizationManager,
                                                  IHttpContextAccessor                        httpContextAccessor,
                                                  ILogger<JsonDictionaryBasedStringLocalizer> logger)
        {
            _localizationManager = localizationManager;
            _httpContextAccessor = httpContextAccessor;
            _logger              = logger;
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return this;
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var culture = _httpContextAccessor.HttpContext.GetCurrentCulture();

            return includeParentCultures
                       ? GetAllStringsFromCultureHierarchy(culture)
                       : GetAllStrings(culture);
        }

        private IEnumerable<LocalizedString> GetAllStringsFromCultureHierarchy(CultureInfo culture)
        {
            var currentCulture      = culture;
            var allLocalizedStrings = new List<LocalizedString>();

            do
            {
                var localizedStrings = GetAllStrings(currentCulture);

                if (localizedStrings != null)
                {
                    foreach (var localizedString in localizedStrings)
                    {
                        if (allLocalizedStrings.All(ls => ls.Name != localizedString.Name))
                        {
                            allLocalizedStrings.Add(localizedString);
                        }
                    }
                }

                currentCulture = currentCulture.Parent;
            } while (currentCulture != currentCulture.Parent);

            return allLocalizedStrings;
        }

        private IEnumerable<LocalizedString> GetAllStrings(CultureInfo culture)
        {
            var dictionary = _localizationManager.GetDictionary(culture);

            foreach (var translation in dictionary.Translations)
            {
                yield return new LocalizedString(translation.Key, translation.Value);
            }
        }

        public LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                
                var culture = _httpContextAccessor.HttpContext.GetCurrentCulture();

                var translation = Translate(name, culture);

                return new LocalizedString(name, translation ?? name, translation == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                
                var culture = _httpContextAccessor.HttpContext.GetCurrentCulture();

                var translation = Translate(name, culture, true);

                string formatted;

                try
                {
                    if (arguments.Length == 1)
                    {
                        formatted = FormatWith(translation, arguments.FirstOrDefault());
                    }
                    else
                    {
                        formatted = string.Format(translation, arguments);
                    }
                }
                catch
                {
                    formatted = string.Format(translation, arguments);
                }


                return new LocalizedString(name, formatted, translation == null);
            }
        }

        private string Translate(string name, CultureInfo culture, bool isPlural = false)
        {
            var key = CultureDictionaryRecord.GetKey(name);
            try
            {
                var dictionary = _localizationManager.GetDictionary(culture);

                var translation = dictionary[key, isPlural];

                // Should we search in the parent culture?
                if (translation == null && !Equals(culture.Parent, culture))
                {
                    dictionary = _localizationManager.GetDictionary(culture.Parent);

                    if (dictionary != null)
                    {
                        translation = dictionary[key, isPlural]; // fallback to the parent culture
                    }
                }

                return translation ?? name; // return the key if no translation found
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
            }

            return name;
        }

        public static string FormatWith(string format, object source)
        {
            return FormatWith(format, null, source);
        }

        public static string FormatWith(string format, IFormatProvider provider, object source)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            var r = new Regex(@"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+",
                              RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            var values = new List<object>();

            var rewrittenFormat = r.Replace(format, delegate(Match m)
                                                    {
                                                        var startGroup    = m.Groups["start"];
                                                        var propertyGroup = m.Groups["property"];
                                                        var formatGroup   = m.Groups["format"];
                                                        var endGroup      = m.Groups["end"];

                                                        var objectValue = propertyGroup.Value == "0"
                                                                              ? source
                                                                              : Eval(source, propertyGroup.Value);

                                                        values.Add(objectValue);

                                                        return new string('{', startGroup.Captures.Count) +
                                                               (values.Count - 1) + formatGroup.Value
                                                             + new string('}', endGroup.Captures.Count);
                                                    });

            return string.Format(provider, rewrittenFormat, values.ToArray());
        }

        private static object Eval(object source, string propertyName)
        {
            var prop = source.GetType()
                             .GetProperties()
                             .FirstOrDefault(_ => _.Name == propertyName);

            if (prop == null)
                return null;

            return prop.GetValue(source);
        }
    }
}