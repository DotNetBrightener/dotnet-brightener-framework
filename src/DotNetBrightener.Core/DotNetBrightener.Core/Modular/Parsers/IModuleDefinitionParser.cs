using System.Collections.Generic;

namespace DotNetBrightener.Core.Modular.Parsers;

public interface IModuleDefinitionParser
{
    List<ModuleEntry> LoadAndParseModulesFromFolder(string moduleFolder);
}