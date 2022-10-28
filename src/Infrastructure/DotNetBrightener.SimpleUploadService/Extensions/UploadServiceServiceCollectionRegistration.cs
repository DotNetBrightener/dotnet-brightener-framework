// ReSharper disable CheckNamespace

using DotNetBrightener.SimpleUploadService.Extensions;
using DotNetBrightener.SimpleUploadService.IO;
using DotNetBrightener.SimpleUploadService.Services;
using System;
using DotNetBrightener.SimpleUploadService;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class UploadServiceServiceCollectionRegistration
{
    /// <summary>
    ///     Registers the Simple Upload APIs into the <paramref name="serviceCollection"/>
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static UploadServiceConfigurationBuilder RegisterSimpleUploadService(this IServiceCollection serviceCollection, 
                                                                                string uploadFolderName = null)
    {
        var builder = new UploadServiceConfigurationBuilder
        {
            ServiceCollection = serviceCollection
        };

        if (!string.IsNullOrEmpty(uploadFolderName))
        {
            builder.UploadFolder = uploadFolderName;
        }

        serviceCollection.AddSingleton(builder);
        serviceCollection.AddScoped<IMediaFileProvider, MediaFileProvider>();
        serviceCollection.AddScoped<IUploadService, UploadService>();

        builder.AddUploadServiceProvider<DefaultUploadServiceProvider>();
        builder.UseImageResizer<NoneImageResizer>();
        builder.UseUploadRootPath<DefaultUploadRootPathProvider>();

        return builder;
    }

    /// <summary>
    ///     Adds a provider for handling upload request
    /// </summary>
    /// <typeparam name="TUploadServiceProvider"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static UploadServiceConfigurationBuilder AddUploadServiceProvider<TUploadServiceProvider>(
        this UploadServiceConfigurationBuilder builder)
        where TUploadServiceProvider : class, IUploadServiceProvider
    {
        var serviceDescriptor = ServiceDescriptor.Scoped<IUploadServiceProvider, TUploadServiceProvider>();
        builder.ServiceCollection.Add(serviceDescriptor);

        return builder;
    }

    /// <summary>
    ///     Change the implementation of <see cref="IUploadRootPathProvider"/> service
    /// </summary>
    /// <typeparam name="TUploadPathResolver"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static UploadServiceConfigurationBuilder UseUploadRootPath<TUploadPathResolver>(
        this UploadServiceConfigurationBuilder builder)
        where TUploadPathResolver : class, IUploadRootPathProvider
    {
        var serviceDescriptor = ServiceDescriptor.Scoped<IUploadRootPathProvider, TUploadPathResolver>();
        builder.ServiceCollection.Replace(serviceDescriptor);

        return builder;
    }

    public static UploadServiceConfigurationBuilder UseImageResizer<TImageResizer>(
        this UploadServiceConfigurationBuilder builder)
        where TImageResizer : class, IImageResizer
    {
        var serviceDescriptor = ServiceDescriptor.Scoped<IImageResizer, TImageResizer>();
        builder.ServiceCollection.Replace(serviceDescriptor);

        return builder;
    }
}