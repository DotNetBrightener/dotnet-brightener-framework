using CommandLine;

namespace DotNetBrightener.DataAccess.DataMigration.Cli;

internal class AddMigrationParams
{
    [Option('n', "name", Required = true, HelpText = "Name of the project to create")]
    public string MigrationName { get; set; }
}