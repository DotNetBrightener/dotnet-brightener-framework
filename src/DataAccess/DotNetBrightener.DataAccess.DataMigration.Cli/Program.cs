using System.Reflection;
using CommandLine;
using DotNetBrightener.DataAccess.DataMigration.Cli;

var versionInfo = Assembly.GetEntryAssembly()?
   .GetCustomAttribute<AssemblyInformationalVersionAttribute>();

var versionString = versionInfo?.InformationalVersion.Split("+").FirstOrDefault();

Console.WriteLine("*****************************************************************");
Console.WriteLine($"        Data Migration Tool - version {versionString}");
Console.WriteLine($"        Copyright (c) 2017 - {DateTime.Today:yyyy} Vampire Coder.");
Console.WriteLine($"        Author: Justin Nguyen <admin@vampirecoder.com>");
Console.WriteLine("*****************************************************************");
Console.WriteLine("");

if (args.Length == 0)
{
    Utils.PrintUsageHint();
    return;
}


var paramArray = new string[args.Length - 1];
Array.Copy(args, 1, paramArray, 0, args.Length - 1);

var command = args[0].ToLower();

switch (command)
{
    case "add":
        var parameters = new List<string>(paramArray);
        if (parameters.Count == 1)
        {
            parameters.Insert(0, "-n");
        }

        var parameter = Parser.Default.ParseArguments<AddMigrationParams>(parameters);

        if (parameter.Errors.Any())
        {
            return;
        }

        AddMigrationCommand.Execute(parameter.Value);

        break;
    default:
        Utils.PrintUsageHint();
        break;
}