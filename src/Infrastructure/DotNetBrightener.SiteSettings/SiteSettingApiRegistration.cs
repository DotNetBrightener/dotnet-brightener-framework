using System.Reflection;
using DotNetBrightener.SiteSettings.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.SiteSettings;

public static class SiteSettingApiRegistration
{
    public static void RegisterSiteSettingApi(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(Assembly.GetExecutingAssembly());

        mvcBuilder.Services.AddScoped<ISiteSettingService, SiteSettingService>();
    }
}