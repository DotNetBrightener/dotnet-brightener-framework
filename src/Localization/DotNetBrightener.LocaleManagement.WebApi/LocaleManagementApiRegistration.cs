using DotNetBrightener.LocaleManagement.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.LocaleManagement.WebApi;

public static class LocaleManagementApiRegistration
{
    public static void RegisterLocaleManagementApi(this IMvcBuilder mvcBuilder, 
                                                   IConfiguration configuration)
    {
        mvcBuilder.RegisterMeAsMvcModule();

        mvcBuilder.Services.AddLocaleManagementModule();
    }
}