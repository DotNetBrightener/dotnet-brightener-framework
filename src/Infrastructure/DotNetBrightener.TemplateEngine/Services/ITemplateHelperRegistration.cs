using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetBrightener.TemplateEngine.Services;

public interface ITemplateHelperRegistration
{
    void RegisterHelpers();
}

public class TemplateHelperRegistration : ITemplateHelperRegistration
{
    private readonly IEnumerable<ITemplateHelperProvider> _templateHelperProviders;

    public TemplateHelperRegistration(IEnumerable<ITemplateHelperProvider> templateHelperProviders)
    {
        _templateHelperProviders = templateHelperProviders;
    }

    public void RegisterHelpers()
    {
        foreach (var templateHelperProvider in _templateHelperProviders)
        {
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