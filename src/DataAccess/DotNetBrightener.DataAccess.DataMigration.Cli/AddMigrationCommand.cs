namespace DotNetBrightener.DataAccess.DataMigration.Cli;

internal static class AddMigrationCommand
{
    internal static void Execute(AddMigrationParams parameters)
    {
        var currentWorkingFolder = Directory.GetCurrentDirectory();
        var migrationName        = parameters.MigrationName;

        // look for csproj file 
        var csprojFiles = Directory.GetFiles(currentWorkingFolder, "*.csproj")
                                   .FirstOrDefault();

        if (string.IsNullOrEmpty(csprojFiles))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No csproj file found in the current directory. Data Migration CLI tool can only run in a .NET project. Go to the root folder of your .NET project and rerun the command.");
            return;
        }

        var dataMigrationsNamespace  = "DataMigrations";
        var migrationFolder = Path.Combine(currentWorkingFolder, dataMigrationsNamespace);

        if (!Directory.Exists(migrationFolder))
        {
            Directory.CreateDirectory(migrationFolder);
        }

        // look for same migration name
        var existingMigrationName = Directory.GetFiles(migrationFolder, "*.cs")
                                         .Select(Path.GetFileNameWithoutExtension)
                                         .FirstOrDefault(x => x!.EndsWith(migrationName));

        if (existingMigrationName is not null)
        {

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Migration name '{migrationName}' has already been used. Please select a new name.");
            return;
        }

        var namespaceName = Path.GetFileNameWithoutExtension(csprojFiles) + "." + dataMigrationsNamespace;
        var migrationId   = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{migrationName}";

        var migrationClassContent = $@"
using DotNetBrightener.DataAccess.DataMigration;

namespace {namespaceName};

[DataMigration(""{migrationId}"")]
internal class {migrationName} : IDataMigration
{{
    public {migrationName}()
    {{
        // Add custom constructor logic here, e.g. to inject dependencies
    }}

    public Task MigrateData() 
    {{
        // Add your migration logic here
    }}
}}
";
        var migrationFilePath = Path.Combine(migrationFolder, $"{migrationId}.cs");

        File.WriteAllText(migrationFilePath, migrationClassContent.Trim());

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Migration {migrationName} created. Please now implement your migration logic in `MigrateData()` method.");
    }
}