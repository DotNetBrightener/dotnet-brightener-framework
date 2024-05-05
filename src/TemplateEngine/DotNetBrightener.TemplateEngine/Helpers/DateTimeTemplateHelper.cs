using DotNetBrightener.TemplateEngine.Services;

namespace DotNetBrightener.TemplateEngine.Helpers;

public class DateTimeTemplateHelper : ITemplateHelperProvider
{
    public string HelperName => "formatDate";

    public string UsageHint => "{{formatDate {your-date-data} [{format, default = 'O'}]}}";

    public void ResolveTemplate(TextWriter output, object context, object[] arguments)
    {
        if (context == null)
            return;

        if (arguments.Length < 1)
            throw new ArgumentException("Invalid number of arguments");

        var fieldValue = arguments[0];

        if (fieldValue == null)
        {
            return;
        }

        var formatArg =
            new List<string>(arguments.Select(a => a.ToString()?.Trim('\''))
                                      .Where(argument => !string.IsNullOrEmpty(argument)));

        formatArg.RemoveAt(0);

        var format = formatArg.Count > 0
                         ? string.Join(" ", formatArg)
                         : "O";

        if (fieldValue is DateTime dateTime)
            output.Write(dateTime.ToString(format));

        if (fieldValue is DateTimeOffset dateTimeOffset)
            output.Write(dateTimeOffset.ToString(format));
    }
}