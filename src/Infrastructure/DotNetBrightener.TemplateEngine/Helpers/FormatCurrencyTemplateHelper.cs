// 
// DotNetBrightener-ECommerce - DotNetBrightener.TemplateEngine
// Copyright (c) 2021 DotNetBrightener.

using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using DotNetBrightener.TemplateEngine.Services;

namespace DotNetBrightener.TemplateEngine.Helpers;

public class FormatCurrencyTemplateHelper : ITemplateHelperProvider
{
    private static readonly ConcurrentDictionary<string, CultureInfo> Cultures =
        new ConcurrentDictionary<string, CultureInfo>();

    public string HelperName => "formatCurrency";

    public string UsageHint => "{{formatCurrency {your-data} [{format, default = 'O'}] [{culture}, default = 'en-US']}}";

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
            
        if (arguments.Length == 1)
        {
            output.Write(decimalValue.ToString("n0"));

            return;
        }

        if (arguments.Length == 2)
        {
            var culture = arguments[1].ToString();

            if (!Cultures.TryGetValue(culture!, out CultureInfo value))
            {
                value = new CultureInfo(culture);
                Cultures.TryAdd(culture, value);
            }

            output.Write(decimalValue.ToString("C0", value));
        }
    }
}