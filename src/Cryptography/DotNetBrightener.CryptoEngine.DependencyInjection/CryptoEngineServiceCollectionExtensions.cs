using System.IO;
using DotNetBrightener.CryptoEngine.Loaders;
using DotNetBrightener.CryptoEngine.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.CryptoEngine.DependencyInjection;

public static class CryptoEngineServiceCollectionExtensions
{
    public static void AddCryptoEngine(this IServiceCollection serviceCollection,
                                       IConfiguration          configuration,
                                       string                  encryptKeysRootPath = "")
    {
        serviceCollection
           .Configure<CryptoEngineConfiguration>(configuration.GetSection(nameof(CryptoEngineConfiguration)));

        serviceCollection.AddScoped<IRSAKeysLoader>(s =>
        {
            if (string.IsNullOrEmpty(encryptKeysRootPath))
            {
                encryptKeysRootPath = s.GetService<IWebHostEnvironment>()?.ContentRootPath;
            }

            if (string.IsNullOrEmpty(encryptKeysRootPath))
            {
                encryptKeysRootPath = Directory.GetCurrentDirectory();
            }

            return new FileRSAKeysLoader(encryptKeysRootPath);
        });

        serviceCollection.AddScoped<IRSAKeysLoader, EnvironmentVarISAKeysLoader>();
        serviceCollection.AddScoped<ICryptoEngine, DefaultCryptoEngine>();
    }
}