using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.Modular.Services
{
	public interface IModuleAssembliesLoader
	{
        /// <summary>
		/// Loads the assemblies for specified module
		/// </summary>
		/// <param name="moduleEntry"></param>
		/// <returns></returns>
		Assembly[] LoadModuleAssemblies(ModuleEntry moduleEntry);

        AssemblyName[] LoadAssemblyNames();

		Assembly ResolveAssembly(object sender, ResolveEventArgs resolveArgs);
	}

	public class ModuleAssembliesLoader : IModuleAssembliesLoader
    {
        private readonly HashSet<AssemblyName> _loadedAssemblyNames = new HashSet<AssemblyName>();
		private readonly IDictionary<string, Assembly> _preloadedAssemblies = new ConcurrentDictionary<string, Assembly>();
		private readonly List<FileInfo> _loadedAssemblyFiles = new List<FileInfo>();
		private readonly ILogger _logger;

		public ModuleAssembliesLoader(ILogger<ModuleAssembliesLoader> logger)
        {
            _logger = logger;
        }
		
		public Assembly[] LoadModuleAssemblies(ModuleEntry moduleEntry)
		{
			var p = moduleEntry.BinPath;
			if (!p.EndsWith("bin"))
				p = Path.Combine(p, "bin");

			var binDirectory = Directory.Exists(p) ? new DirectoryInfo(p) : new DirectoryInfo(moduleEntry.BinPath);

            moduleEntry.BinPath = binDirectory.FullName;

            var searchOption = moduleEntry.Name == ModuleEntry.MainModuleIdentifier &&
                               !binDirectory.FullName.EndsWith("\\bin")
                                   ? SearchOption.TopDirectoryOnly
                                   : SearchOption.AllDirectories;

			var moduleDllFiles = binDirectory.GetFiles("*.dll", searchOption);

			var loadedAssemblies = moduleDllFiles.Where(dllFile => _loadedAssemblyFiles.Any(loadedAssembly => loadedAssembly.Name == dllFile.Name));
            
            // delete all loaded assemblies, we only need the extension modules and not duplicated ones.
			foreach (var loadedAssembly in loadedAssemblies)
			{
				try
				{
					loadedAssembly.Delete();
                    File.Delete(loadedAssembly.FullName.Replace(".dll", ".pdb"));
				}
				catch (Exception exception)
				{
					_logger.LogError(exception, @"Error while trying to clean up the loaded assemblies");
				}
			}

#if RELEASE
            var modulePdbFiles = binDirectory.GetFiles("*.pdb", searchOption);
            // delete all pdb files
            foreach (var modulePdbFile in modulePdbFiles)
            {
                try
                {
                    File.Delete(modulePdbFile.FullName);
                }
                catch
                {
                    // ignore the error
                }
            }
#endif

            moduleDllFiles = moduleDllFiles.Where(x => _loadedAssemblyFiles.All(f => f.Name != x.Name)).ToArray();

			_loadedAssemblyFiles.AddRange(moduleDllFiles);

			var moduleAssemblies = new List<Assembly>();
			foreach (var fi in moduleDllFiles)
			{
				var          fileName = fi.FullName;
				AssemblyName assemblyName;

				try
				{
					assemblyName = AssemblyName.GetAssemblyName(fileName);
				}
				catch (Exception exception)
				{
					continue;
				}

                if (_preloadedAssemblies.ContainsKey(assemblyName.FullName))
                    continue;

				try
				{
                    var assembly = Assembly.LoadFrom(fileName);
					
                    _preloadedAssemblies.Add(assemblyName.FullName, assembly);
                    _loadedAssemblyNames.Add(assemblyName);

                    moduleAssemblies.Add(assembly);
				}
				catch (Exception ex)
				{
				}
			}

            if (!moduleAssemblies.Any() && moduleEntry.ModuleId == ModuleEntry.MainModuleIdentifier)
            {
                moduleAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                                            .FilterSkippedAssemblies()
                                            .ToList();
            }
            else
            {
                moduleAssemblies = moduleAssemblies.FilterSkippedAssemblies().ToList();
            }

            moduleEntry.ModuleAssemblies.AddRange(moduleAssemblies);

			return moduleEntry.ModuleAssemblies.ToArray();
		}

        public AssemblyName[] LoadAssemblyNames()
        {
            return _loadedAssemblyNames.ToArray();
        }

        public Assembly ResolveAssembly(object sender, ResolveEventArgs resolveArgs)
		{
			var cachedAssembly = _preloadedAssemblies.ContainsKey(resolveArgs.Name);
			if (cachedAssembly)
				return _preloadedAssemblies[resolveArgs.Name];

			return null;
		}
	}
}