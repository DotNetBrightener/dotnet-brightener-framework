using System;
using System.IO;
using DotNetBrightener.TemplateEngine.Services;

namespace DotNetBrightener.TemplateEngine.Helpers
{
    public class DateTimeTemplateHelper : ITemplateHelperProvider
    {
        public string HelperName => "formatDate";

        public string UsageHint => "{{formatDate {your-date-data} [{format, default = 'O'}]}}";

        public void ResolveTemplate(TextWriter output, object context, object[] arguments)
        {
            if (arguments.Length != 2 || context == null)
                return;

            var fieldValue = arguments[0];
            if (fieldValue == null)
            {
                return;
            }

            var format = arguments[1].ToString();

            if (fieldValue is DateTime dateTime)
                output.Write(string.IsNullOrEmpty(format) ? dateTime.ToString("O") : dateTime.ToString(format));

            if (fieldValue is DateTimeOffset dateTimeOffset)
                output.Write(string.IsNullOrEmpty(format)
                                 ? dateTimeOffset.ToString("O")
                                 : dateTimeOffset.ToString(format));
        }
    }
}