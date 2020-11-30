using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace DotNetBrightener.Core.Localization.Factories
{
    public class JsonBasedStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public JsonBasedStringLocalizerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            return _serviceProvider.GetService<JsonDictionaryBasedStringLocalizer>();
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return _serviceProvider.GetService<JsonDictionaryBasedStringLocalizer>();
        }
    }
}