using HandlebarsDotNet;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TemplateEngine.Services;

public interface ITemplateHelperRegistration
{
    void RegisterHelpers();
}

public class TemplateHelperRegistration(IEnumerable<ITemplateHelperProvider> templateHelperProviders,
                                        ILogger<TemplateHelperRegistration> logger)
    : ITemplateHelperRegistration
{
    public void RegisterHelpers()
    {
        foreach (var templateHelperProvider in templateHelperProviders)
        {
            logger.LogInformation("Registering template helper {templateHelperName}", templateHelperProvider.HelperName);

            RegisterHelper(templateHelperProvider.HelperName,
                           (writer, context, arg3) =>
                           {
                               templateHelperProvider.ResolveTemplate(writer.CreateWrapper(),
                                                                      context,
                                                                      arg3.ToArray());
                           });
        }
    }

    private void RegisterHelper(string helperName, Action<EncodedTextWriter, Context, Arguments> resolveToken)
    {
        Handlebars.RegisterHelper(helperName,
                                  (output, context, arguments) => resolveToken(output, context, arguments));
    }
}