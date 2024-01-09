using System.IO;
using DotNetBrightener.WebApi.GenericCRUD.Generator.SyntaxReceivers;
using DotNetBrightener.WebApi.GenericCRUD.Generator.Utils;
using Microsoft.CodeAnalysis;

namespace DotNetBrightener.WebApi.GenericCRUD.Generator.Generators;

[Generator]
public class ControllerClassGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AutoGenerateApiControllerSyntaxReceiver());
    }

    /// <summary>
    /// And consume the receiver here.
    /// </summary>
    public void Execute(GeneratorExecutionContext context)
    {
        var models = (context.SyntaxContextReceiver as AutoGenerateApiControllerSyntaxReceiver).Models;

        foreach (var modelClass in models)
        {
            var className = $"{modelClass.TargetEntity}Controller";

            var controllerSrc = $@"
{FileTemplates.GetFileHeader(className)}

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;

using DotNetBrightener.WebApi.GenericCRUD.Controllers;
using DotNetBrightener.DataAccess.Services;
using {modelClass.DataServiceNamespace};
using {modelClass.TargetEntityNamespace};

namespace {modelClass.ControllerNamespace};

public partial class {className} : BaseCRUDController<{modelClass.TargetEntity}>
{{
    
    internal {className}(
            I{modelClass.TargetEntity}DataService dataService,
            IHttpContextAccessor httpContextAccessor)
        : base(dataService, httpContextAccessor)
    {{
    }}
}}";

            var targetFolder = modelClass.ControllerPath;

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var gPathFile = Path.Combine(targetFolder, $"{className}.g.cs");

            File.WriteAllText(gPathFile, controllerSrc);

            var defaultPathFile = Path.Combine(targetFolder, $"{className}.cs");

            if (!File.Exists(defaultPathFile))
            {
                var defaultControllerFileContent = $@"
using Microsoft.AspNetCore.Mvc;
using {modelClass.DataServiceNamespace};
using {modelClass.TargetEntityNamespace};

namespace {modelClass.ControllerNamespace};

/// <summary>
///     Provide public APIs for <see cref=""{modelClass.TargetEntity}"" /> entity.
/// </summary>
/// 
/// Uncomment the next line to enable authorization for this controller
/// [Authorize]
[ApiController]
[Route(""api/[controller]"")]
public partial class {className}
{{
    private readonly ILogger _logger;

    public {className}(
            I{modelClass.TargetEntity}DataService dataService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<{className}> logger)
        : this(dataService, httpContextAccessor)
    {{
        _logger = logger;
    }}

    // Implement or override APIs here
}}";
                File.WriteAllText(defaultPathFile, defaultControllerFileContent);
            }
        }
    }
}