using System.Reflection;

namespace DotNetBrightener.DataAccess.DataMigration.Cli;

internal static class Utils
{
    internal static void PrintUsageHint()
    {
        Console.WriteLine("");
        Console.WriteLine($"Usage : {Assembly.GetEntryAssembly()?.GetName().Name} <command>");
        Console.WriteLine("");
        Console.WriteLine("");
        Console.WriteLine("Command Lists: ");
        Console.WriteLine($"- add [migration_name]: Create a new migration with given name 'migration_name'");
        Console.WriteLine("");
        Console.WriteLine($"Use: {Assembly.GetEntryAssembly()?.GetName().Name} <command> --help to show hints on options for the <command>");
        Console.WriteLine("");
    }
}