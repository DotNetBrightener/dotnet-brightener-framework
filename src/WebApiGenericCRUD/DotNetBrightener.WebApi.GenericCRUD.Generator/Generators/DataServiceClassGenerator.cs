﻿using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using WebApi.GenericCRUD.Generator.SyntaxReceivers;
using WebApi.GenericCRUD.Generator.Utils;

namespace WebApi.GenericCRUD.Generator.Generators;

[Generator]
public class DataServiceClassGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register syntax provider instead of syntax receiver
        var syntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: AutoGenerateDataServiceSyntaxReceiver.IsCandidateForGeneration,
                transform: (ctx, _) => AutoGenerateDataServiceSyntaxReceiver.GetSemanticTargetForGeneration(ctx))
            .Where(m => m != null);

        context.RegisterSourceOutput(syntaxProvider.Collect(), Execute);
    }

    private void Execute(SourceProductionContext context, ImmutableArray<IEnumerable<CodeGenerationInfo>> models)
    {
        var allModels = models.SelectMany(x => x.Where(m => m is not null).Select(m => m))
                              .ToArray();
        
        if (!allModels.Any())
            return;

        foreach (var modelClass in allModels)
        {
            GenerateDataServiceClass(context, modelClass);
        }
    }

    private static void GenerateDataServiceClass(SourceProductionContext context, CodeGenerationInfo modelClass)
    {
        var className = $"{modelClass.TargetEntity}DataService";
        var interfaceName = $"I{className}";

        var dataServiceInterfaceSrc = $@"
{FileTemplates.GetFileHeader(className)}

using System;
using System.ComponentModel.DataAnnotations;

using DotNetBrightener;
using DotNetBrightener.DataAccess.Services;
using {modelClass.TargetEntityNamespace};

namespace {modelClass.DataServiceNamespace};

public partial interface {interfaceName} : IBaseDataService<{modelClass.TargetEntity}>;";

        var dataServiceSrc = $@"
{FileTemplates.GetFileHeader(className)}

using System;
using System.ComponentModel.DataAnnotations;

using DotNetBrightener;
using DotNetBrightener.DataAccess.Services;
using {modelClass.TargetEntityNamespace};

namespace {modelClass.DataServiceNamespace};

public partial class {modelClass.TargetEntity}DataService : BaseDataService<{modelClass.TargetEntity}>, {interfaceName} {{
    
    internal {modelClass.TargetEntity}DataService(IRepository repository)
        : base(repository)
    {{
    }}

}}";
        var targetFolder = modelClass.DataServicePath;

        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        var gInterfacePathFile = Path.Combine(targetFolder, $"{interfaceName}.g.cs");
        File.WriteAllText(gInterfacePathFile, dataServiceInterfaceSrc);

        var gPathFile = Path.Combine(targetFolder, $"{className}.g.cs");
        File.WriteAllText(gPathFile, dataServiceSrc);

        var defaultPathFile = Path.Combine(targetFolder, $"{className}.cs");

        if (!File.Exists(defaultPathFile))
        {
            var defaultServiceClassFileContent = $@"using DotNetBrightener.DataAccess.Services;
using Microsoft.Extensions.Logging;

using {modelClass.TargetEntityNamespace};

namespace {modelClass.DataServiceNamespace};

public partial class {className}
{{
    private readonly ILogger _logger;

    public {className}(
            IRepository repository, 
            ILogger<{className}> logger)
        : this(repository)
    {{
        _logger = logger;
    }}

    // Implement your custom methods here
}}";
            File.WriteAllText(defaultPathFile, defaultServiceClassFileContent);
        }

        var defaultInterfacePathFile = Path.Combine(targetFolder, $"{interfaceName}.cs");

        if (!File.Exists(defaultInterfacePathFile))
        {
            var interfaceFileContent = $@"using {modelClass.TargetEntityNamespace};

namespace {modelClass.DataServiceNamespace};

/// <summary>
///     Provides the data access methods for <see cref=""{modelClass.TargetEntity}"" /> entity.
/// </summary>
public partial interface {interfaceName}
{{
    // Provide your custom methods here
}}";
            File.WriteAllText(defaultInterfacePathFile, interfaceFileContent);
        }
    }
}