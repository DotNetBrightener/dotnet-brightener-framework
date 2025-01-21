using DotNetBrightener.CryptoEngine.Loaders;
using DotNetBrightener.CryptoEngine.Options;
using DotNetBrightener.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.CryptoEngine.Tests;

public class PasswordProviderTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task TestPasswordProvider_Generated_Encrypted_Password_Should_Be_Valid()
    {
        var host = XUnitTestHost.CreateTestHost(testOutputHelper,
                                                (context, services) =>
                                                {
                                                    services.AddScoped<IRSAKeysLoader, InMemoryRSAKeysLoader>();
                                                    services.AddScoped<ICryptoEngine, DefaultCryptoEngine>();
                                                    services
                                                       .AddScoped<IPasswordValidationProvider,
                                                            DefaultPasswordValidationProvider>();

                                                    services.Configure<CryptoEngineConfiguration>(x =>
                                                    {
                                                        x.RsaKeyLoader = "InMemory";
                                                    });
                                                });

        await host.StartAsync();

        using (var scope = host.Services.CreateScope())
        {
            var passwordProvider = scope.ServiceProvider.GetRequiredService<IPasswordValidationProvider>();

            var plainTextPassword = "thisIs@V3ry5tr0ngP@ssw0rd";

            var hashedPassword = passwordProvider.GenerateEncryptedPassword(plainTextPassword);

            var isPasswordValid =
                passwordProvider.ValidatePassword(plainTextPassword, hashedPassword.Item1, hashedPassword.Item2);

            isPasswordValid.ShouldBeTrue();
        }

        await host.StopAsync();
    }
}