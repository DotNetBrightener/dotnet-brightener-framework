using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

internal static class ApplicationBuilderStaticServerExtensions
{ 
    public static void EnableStaticFileServer(this IApplicationBuilder app, IFileProvider moduleFileProvider, string requestPath)
    {
        var staticFileOptions = new StaticFileOptions
        {
            FileProvider = moduleFileProvider,
            RequestPath  = new PathString(requestPath),
        };

        var fileServerOptions = new FileServerOptions
        {
            FileProvider            = moduleFileProvider,
            RequestPath             = new PathString(requestPath),
            EnableDirectoryBrowsing = false
        };

        app.UseStaticFiles(staticFileOptions);
        app.UseFileServer(fileServerOptions);
    }

}