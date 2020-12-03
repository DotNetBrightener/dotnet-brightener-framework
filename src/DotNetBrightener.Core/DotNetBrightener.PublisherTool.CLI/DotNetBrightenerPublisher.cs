using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotNetBrightener.PublisherTool.CLI
{
    public class DotNetBrightenerPublisher
    {
        private readonly string _entryProjectName;
        public event EventHandler<string> Output;
        public event EventHandler<string> OutputInline;
        private readonly string _folderToScan;
        private const string EntryModuleName = "MainModule";
        
        public DotNetBrightenerPublisher(string entryProjectName)
        {
            _entryProjectName = entryProjectName;
            var currentWorkingDirectory = Directory.GetCurrentDirectory();

            while (true)
            {
                var dir           = new DirectoryInfo(currentWorkingDirectory);
                var solutionFiles = dir.GetFiles("*.sln");
                if (!solutionFiles.Any())
                {
                    currentWorkingDirectory = Path.GetDirectoryName(currentWorkingDirectory);
                    continue;
                }

                break;
            }

            _folderToScan = currentWorkingDirectory;
            Console.WriteLine($"[Publisher Tool] - Detected folder for building: {_folderToScan}");
        }

        public string PrepareBuildOutputFolder()
        {
            var parentFolder  = Path.GetDirectoryName(_folderToScan);
            var publishFolder = Path.Combine(parentFolder, "Published");

            if (Directory.Exists(publishFolder))
            {
                Directory.Delete(publishFolder, true);
            }

            Directory.CreateDirectory(publishFolder);

            return publishFolder;
        }

        public IEnumerable<ModuleDefinition> RetrievesModuleFilesToBuild(string[] modulesToBuild = null)
        {
            var projectsFolder = new DirectoryInfo(_folderToScan);
            var moduleFiles    = projectsFolder.GetFiles("Module.json", SearchOption.AllDirectories);

            var allModules = new List<ModuleDefinition>();

            foreach (var moduleFile in moduleFiles)
            {
                var moduleFolder = moduleFile.Directory;

                if (moduleFolder.FullName.Contains("\\bin\\"))
                    continue;

                var csProjectFile = moduleFolder.GetFiles("*.csproj", SearchOption.TopDirectoryOnly)
                                                .FirstOrDefault();
                if (csProjectFile == null)
                    continue;

                var moduleDefinition =
                    JsonConvert.DeserializeObject<ModuleDefinition>(File.ReadAllText(moduleFile.FullName));

                if (modulesToBuild == null ||
                    !modulesToBuild.Any() ||
                    modulesToBuild.Contains(moduleDefinition.ModuleId))
                {
                    moduleDefinition.AssociatedProjectFile = csProjectFile.FullName;
                    allModules.Add(moduleDefinition);
                }
            }


            var frameworkModules = allModules.Where(_ => _.ModuleType == ModuleType.Infrastructure)
                                             .Select(_ => _.ModuleId)
                                             .ToArray();

            // make all modules depends on the framework modules
            allModules.ForEach(module =>
                               {
                                   if (module.ModuleType == ModuleType.Infrastructure)
                                       return;

                                   module.Dependencies.InsertRange(0, frameworkModules);
                               });

            var orderedModules = allModules.SortByDependencies(definition => allModules
                                                                  .Where(module => definition
                                                                                  .Dependencies
                                                                                  .Contains(module.ModuleId)))
                                           .ToList();

            var mainEntryProject = projectsFolder.GetFiles($"{_entryProjectName}.csproj", SearchOption.AllDirectories)
                                                 .FirstOrDefault();

            if (mainEntryProject != null && mainEntryProject.Exists)
            {
                orderedModules.Insert(0, new ModuleDefinition
                {
                    ModuleId              = EntryModuleName,
                    AssociatedProjectFile = mainEntryProject.FullName
                });
            }

            if (Output != null)
            {
                Output.Invoke(this, "Detected Modules: ");
                foreach (var module in orderedModules)
                {
                    Output.Invoke(this,
                                   $"\t - {module.AssociatedProjectFile.Replace(_folderToScan, string.Empty)} -> {module.ModuleId}.");
                }
            }

            return orderedModules;
        }

        public void ProcessBuild(IEnumerable<ModuleDefinition> projectsToBuild, string configuration = "Debug")
        {
            var buildOutputFolder = PrepareBuildOutputFolder();
            var moduleOutputFolder = Path.Combine(buildOutputFolder, "Modules");

            var loadedDllFileNames = new List<string>();

            Output?.Invoke(this, string.Empty);
            Output?.Invoke(this, string.Empty);
            Output?.Invoke(this, $"Build starting...");
            Output?.Invoke(this, string.Empty);
            Output?.Invoke(this, string.Empty);
            foreach (var projectToBuild in projectsToBuild)
            {
                Output?.Invoke(this, string.Empty);

                var outputFolder = projectToBuild.ModuleId == EntryModuleName
                                       ? buildOutputFolder
                                       : Path.Combine(moduleOutputFolder, projectToBuild.ModuleId);

                OutputInline?.Invoke(this, $"Building module {projectToBuild.ModuleId} -> {outputFolder.Replace(buildOutputFolder, string.Empty)}");

                var buildCommand = string.Format(Constants.BuildCommand,
                                                 projectToBuild.AssociatedProjectFile,
                                                 configuration,
                                                 outputFolder);

                Output?.Invoke(this, $"Executing command: dotnet {buildCommand}");

                var processStartInfo = new ProcessStartInfo("dotnet", buildCommand)
                {
                    UseShellExecute = true,
                    CreateNoWindow  = true,
                    WindowStyle     = ProcessWindowStyle.Hidden,
                    //RedirectStandardOutput = true,
                };

                var threadExist = false;
                Task.Run(() =>
                         {
                             Process.Start(processStartInfo)?.WaitForExit();
                             threadExist = true;
                         });

                while (!threadExist)
                {
                    OutputInline?.Invoke(this, ".");
                    Thread.Sleep(TimeSpan.FromSeconds(1.5));
                }

                Output?.Invoke(this, string.Empty);

                var outputFolderDir = new DirectoryInfo(outputFolder);
                if (outputFolderDir == null || !outputFolderDir.Exists)
                {
                    Output?.Invoke(this, $"[WARNING] Module {projectToBuild.ModuleId} build cannot be found, perhaps it has failed. Please check carefully.");
                    continue;
                }
                var allDllFiles = outputFolderDir.GetFiles("*.dll");

                if (projectToBuild.ModuleId == EntryModuleName)
                {
                    projectToBuild.OutputDllFiles = allDllFiles;
                }
                else
                {
                    var dllToDeletes = new List<string>();
                    var dllToKeep = new List<FileInfo>();
                    foreach (var dllFile in allDllFiles)
                    {
                        if (loadedDllFileNames.Contains(dllFile.Name))
                        {
                            dllToDeletes.Add(dllFile.FullName);
                            var pdbFile = dllFile.FullName.Replace(".dll", ".pdb");
                            if (File.Exists(pdbFile))
                            {
                                dllToDeletes.Add(pdbFile);
                            }
                            var xmlFile = dllFile.FullName.Replace(".dll", ".xml");
                            if (File.Exists(xmlFile))
                            {
                                dllToDeletes.Add(xmlFile);
                            }
                        }
                        else
                        {
                            dllToKeep.Add(dllFile);
                        }
                    }

                    dllToDeletes.ForEach(File.Delete);
                    projectToBuild.OutputDllFiles = dllToKeep.ToArray();
                }

                loadedDllFileNames.AddRange(projectToBuild.OutputDllFiles.Select(_ => _.Name));
                Output?.Invoke(this, $"Module {projectToBuild.ModuleId} is built successfully.");
            }
        }
    }
}