using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WebApp.CommonShared.Endpoints;
using Xunit;

namespace WebApp.CommonShared.Tests;

/// <summary>
///     Unit tests for endpoint module service registration extensions
/// </summary>
public class EndpointModuleServiceExtensionsTests
{
    [Fact]
    public void AddEndpointModules_WithAssemblies_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var assemblies = new[] { typeof(EndpointModuleBase).Assembly };

        // Act & Assert - should not throw
        Should.NotThrow(() => services.AddEndpointModules(assemblies));
    }

    [Fact]
    public void AddEndpointModules_WithNullAssemblies_ShouldHandleGracefully()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - should not throw
        Should.NotThrow(() => services.AddEndpointModules(Array.Empty<Assembly>()));
    }
}
