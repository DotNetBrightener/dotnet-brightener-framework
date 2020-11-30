using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace DotNetBrightener.Core.Localization.Services
{
    public interface ILocalizationFileLoader
    {
        IEnumerable<IFileInfo> LoadTranslations(string cultureName);
    }

    public class DefaultLocalizationFileLoader : ILocalizationFileLoader
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DefaultLocalizationFileLoader(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public IEnumerable<IFileInfo> LoadTranslations(string cultureName)
        {
            var localeFile = Path.Combine(_webHostEnvironment.ContentRootPath, "Locales", $"{cultureName}.json");

            return new[]
            {
                new PhysicalFileInfo(new FileInfo(localeFile))
            };
        }
    }
}