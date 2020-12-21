using DotNetBrightener.Mvc.HandlebarsViewEngine.TemplateCaching;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace WebEdFramework.TemplateServices
{
    /// <summary>
    /// Provides the service for registering helpers for the template service
    /// </summary>
    public interface ITemplateHelperRegistration
    {
        /// <summary>
        /// Registers the template helpers
        /// </summary>
        void RegisterHelpers();
    }

    public class TemplateHelperRegistration : ITemplateHelperRegistration
    {
        private readonly IEnumerable<ITemplateHelperProvider> _templateHelperProviders;
        private readonly IHttpContextAccessor                 _contextAccessor;
        private readonly ILogger                              _logger;

        public TemplateHelperRegistration(IEnumerable<ITemplateHelperProvider> templateHelperProviders,
                                          IHttpContextAccessor                 contextAccessor,
                                          ILogger<TemplateHelperRegistration>  logger)
        {
            _templateHelperProviders = templateHelperProviders;
            _contextAccessor         = contextAccessor;
            _logger                  = logger;
        }

        public void RegisterHelpers()
        {
            foreach (var templateHelperProvider in _templateHelperProviders)
            {
                RegisterHelper(templateHelperProvider);
            }
        }

        private void RegisterHelper<TTemplateHelper>(TTemplateHelper instance) where TTemplateHelper : ITemplateHelperProvider
        {
            var helperType = instance.GetType();

            _logger.LogInformation($"Registering template helper: {instance.TemplateSyntaxPrefix}, {helperType.FullName}");

            void HandlebarsHelper(EncodedTextWriter output, Context context, Arguments arguments)
            {
                _logger.LogInformation($"Resolving template of type: ", helperType.FullName);
                if (_contextAccessor.HttpContext.RequestServices.GetService(helperType) is ITemplateHelperProvider templateHelper)
                {
                    templateHelper.ResolveTemplate(output, context, arguments.ToArray());
                }
            }

            Handlebars.RegisterHelper(instance.TemplateSyntaxPrefix, HandlebarsHelper);
        }
    }
}