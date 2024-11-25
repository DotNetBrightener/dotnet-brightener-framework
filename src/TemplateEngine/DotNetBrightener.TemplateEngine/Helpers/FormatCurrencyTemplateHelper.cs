using System.Collections.Concurrent;
using System.Globalization;
using DotNetBrightener.TemplateEngine.Services;

namespace DotNetBrightener.TemplateEngine.Helpers;

public class FormatCurrencyTemplateHelper : ITemplateHelperProvider
{
    private static readonly ConcurrentDictionary<string, CultureInfo> Cultures =
        new ConcurrentDictionary<string, CultureInfo>();

    public string HelperName => "formatCurrency";

    public string UsageHint => "{{formatCurrency {your-data} [{format, default = 'C0'}] [{culture, default = 'en-US'}]}}";

    public FormatCurrencyTemplateHelper()
    {
        if (!Cultures.TryGetValue("en-US", out var _))
        {
            var c = new CultureInfo("en-US");
            Cultures.TryAdd("en-US", c);
        }
    }

    public void ResolveTemplate(TextWriter output, object context, object[] arguments)
    {
        if (arguments.Length < 1 || context == null)
            return;

        var fieldValue = arguments[0];
        if (fieldValue == null || fieldValue is not decimal decimalValue)
        {
            return;
        }

        var argumentsList = new List<object>(3);

        argumentsList.AddRange(arguments);

        var format  = argumentsList.Count > 1 ? argumentsList[1].ToString() : "";
        var culture = argumentsList.Count > 2 ? argumentsList[2].ToString() : "en-US";

        if (string.IsNullOrWhiteSpace(format))
        {
            format = "C";
        }

        if (!Cultures.TryGetValue(culture!, out CultureInfo value))
        {
            try
            {

                value = new CultureInfo(culture);
                Cultures.TryAdd(culture, value);
            }
            catch (Exception)
            {
                value = new CultureInfo("en-US");
            }
        }

        if (!format.StartsWith("C", StringComparison.OrdinalIgnoreCase) && 
            !format.EndsWith("0"))
        {
            format += "0";
        }

        output.Write(decimalValue.ToString(format, value));
    }
}