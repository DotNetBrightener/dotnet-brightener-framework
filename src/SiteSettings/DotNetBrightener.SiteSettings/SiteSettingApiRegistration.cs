using DotNetBrightener.SiteSettings.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DotNetBrightener.SiteSettings;

public static class SiteSettingApiRegistration
{
    extension(IMvcBuilder mvcBuilder)
    {
        public void RegisterSiteSettingApi()
        {
            mvcBuilder.AddApplicationPart(Assembly.GetExecutingAssembly());

            mvcBuilder.Services
                      .EnableCachingService();

            mvcBuilder.Services
                      .AddScoped<ISiteSettingService, SiteSettingService>();

            mvcBuilder.Services
                      .AddScoped<ISiteSettingDataService, SiteSettingDataService>();
        }
    }
}