using DotNetBrightener.TemplateEngine.Services;

namespace DotNetBrightener.TemplateEngine.Helpers;

public class SumTemplateHelper: ITemplateHelperProvider
{
    public string HelperName => "sum";

    public string UsageHint => "{{sum {your-values}}}";

    public void ResolveTemplate(TextWriter output, object context, object[] arguments)
    {
        if (context == null)
            return;

        decimal result = 0;
        foreach (var argument in arguments)
        {
            if (decimal.TryParse(argument.ToString(), out var val))
            {
                result += val;
            }
        }

        output.Write(result);

    }
}