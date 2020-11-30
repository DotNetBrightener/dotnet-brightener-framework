using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace DotNetBrightener.Integration.MultiTenancy
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// whose path is under tenant specific folder of 'wwwroot' folder.
    /// </summary>
    public class TenantSpecificStaticFileProvider : IFileProvider
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string              _tenantName;

        public TenantSpecificStaticFileProvider(IWebHostEnvironment env,
                                                string              tenantName)
        {
            _environment = env;
            _tenantName  = tenantName;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            // not allowing view directory content
            return NotFoundDirectoryContents.Singleton;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == null)
            {
                return new NotFoundFileInfo(subpath);
            }

            var path = NormalizePath(subpath);

            // prioritize the path based on tenant folder
            var fileTenantBasedPath = Path.Combine(_environment.ContentRootPath, "wwwroot", _tenantName, path);

            if (File.Exists(fileTenantBasedPath))
            {
                return new PhysicalFileInfo(new FileInfo(fileTenantBasedPath));
            }

            // if specified file on tenant folder not found, check the site level folder
            var fileSiteBasedPath = Path.Combine(_environment.ContentRootPath, "wwwroot", path);
            return File.Exists(fileSiteBasedPath)
                       ? new PhysicalFileInfo(new FileInfo(fileSiteBasedPath))
                       : (IFileInfo) new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Trim('/').Replace("//", "/");
        }
    }
}