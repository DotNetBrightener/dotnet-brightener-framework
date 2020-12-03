using System;
using System.Linq;
using System.Reflection;

namespace DotNetBrightener.PublisherTool.CLI
{
    class Program
    {
        private const string BuildConfigPrefix = "-c=";
        private const string EntryProjectPrefix = "-e=";
        private static readonly string[] IgnoredParameters = new[] {"-s", "-S", EntryProjectPrefix, BuildConfigPrefix};

        static void Main(string[] args)
        {
            var entryProject = args.SingleOrDefault(_ => _.StartsWith(EntryProjectPrefix));
            if (entryProject == null)
            {
                Console.WriteLine($"Entry Project not configured. Please specify the entry project via -e option:");
                Console.WriteLine($"Usage: {Assembly.GetExecutingAssembly().GetName().Name} -e=[entry-project-name] [array-of-modules-to-build]");
                return;
            }

            var silenceMode = args.Any(_ => _ == "-s" || _ == "-S");

            var projectScanner = new DotNetBrightenerPublisher(entryProject.Replace(EntryProjectPrefix, string.Empty));
            projectScanner.Output += OnProjectBuildOutput;
            projectScanner.OutputInline += OnProjectBuildOutputInline;

            var buildConfiguration = "Debug";
            var buildConfig = args.FirstOrDefault(_ => _.StartsWith(BuildConfigPrefix));

            if (buildConfig != null)
            {
                buildConfiguration = buildConfig.Replace(BuildConfigPrefix, string.Empty);
            }

            var modulesToBuild = args.Where(_ => !IgnoredParameters.Contains(_) &&
                                                 IgnoredParameters.All(ignore => !_.StartsWith(ignore)))
                                     .ToArray();

            var projectsToBuild = projectScanner.RetrievesModuleFilesToBuild(modulesToBuild);

            projectScanner.ProcessBuild(projectsToBuild, buildConfiguration);

            if (!silenceMode)
            {
                Console.WriteLine($"Build finished, press any key to close...");
                Console.ReadKey();
            }
        }

        private static void OnProjectBuildOutput(object? sender, string e)
        {
            Console.WriteLine(e);
        }

        private static void OnProjectBuildOutputInline(object? sender, string e)
        {
            Console.Write(e);
        }
    }
}