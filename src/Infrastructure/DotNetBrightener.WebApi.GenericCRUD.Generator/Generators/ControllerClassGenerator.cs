using System.IO;
using System.Linq;
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

        if (!models.Any())
            return;

        var controllerAssemblyPath = models.First().ControllerAssemblyPath;

        var programCsFile = Path.Combine(controllerAssemblyPath, "Program.cs");

        if (File.Exists(programCsFile))
        {
            InjectSwaggerConfigIfNeeded(programCsFile);
        }
        else
        {
            var startupCsFile = Path.Combine(controllerAssemblyPath, "Startup.cs");

            if (File.Exists(startupCsFile))
            {
                InjectSwaggerConfigIfNeeded(startupCsFile);
            }
        }


        foreach (var modelClass in models)
        {
            GenerateControllerClass(modelClass);
        }
    }

    private void InjectSwaggerConfigIfNeeded(string inputFile)
    {
        var  fileContent = File.ReadAllText(inputFile);

        const string usingStatement              = "using DotNetBrightener.WebApi.GenericCRUD.Extensions;";
        const string usingReflectionStatement    = "using System.Reflection;";
        const string defaultSwaggerGenLookUp     = "builder.Services.AddSwaggerGen();";
        const string startupFileSwaggerGenLookUp = "services.AddSwaggerGen();";

        if (!fileContent.Contains(defaultSwaggerGenLookUp) &&
            !fileContent.Contains(startupFileSwaggerGenLookUp))
        {
            return;
        }

        if (fileContent.Contains("SwaggerConfiguration.RegisterGenericCRUDDocumentation("))
        {
            return;
        }

        if (!fileContent.Contains(usingStatement))
        {
            fileContent = $@"{usingStatement}
{fileContent}";
        }

        if (!fileContent.Contains(usingReflectionStatement))
        {
            fileContent = $@"{usingReflectionStatement}
{fileContent}";
        }

        const string swaggerConfigStr = @"c =>
{
    SwaggerConfiguration.RegisterGenericCRUDDocumentation(c.IncludeXmlComments);

    var filePath = Path.Combine(AppContext.BaseDirectory, $""{Assembly.GetExecutingAssembly().GetName().Name}.xml"");
    c.IncludeXmlComments(filePath, true);
}";

        fileContent = fileContent.Replace(defaultSwaggerGenLookUp,
                                          $@"builder.Services.AddSwaggerGen({swaggerConfigStr});");

        fileContent = fileContent.Replace(startupFileSwaggerGenLookUp,
                                          $@"services.AddSwaggerGen({swaggerConfigStr});");

        File.WriteAllText(inputFile, fileContent);
    }

    private static void GenerateControllerClass(CodeGenerationInfo modelClass)
    {
        var className = $"{modelClass.TargetEntity}Controller";

        var controllerSrc = $@"
{FileTemplates.GetFileHeader(className)}

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

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


    /// <summary>
    ///     Retrieves the collection of records of type <see cref=""{modelClass.TargetEntity}"" />.
    /// </summary>
    /// <response code=""200"">
    ///     The collection of records of type <see cref=""{modelClass.TargetEntity}"" />.
    /// </response>
    /// <response code=""401"">
    ///     Unauthorized request to retrieve filtered collection of <see cref=""{modelClass.TargetEntity}"" /> records.
    /// </response> 
    /// <response code=""500"">
    ///     Unknown internal server error.
    /// </response>
    [ProducesResponseType<List<{modelClass.TargetEntity}>>(200)]
    public override partial Task<IActionResult> GetList();
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


    public override partial Task<IActionResult> GetList()
    {{
        // override the base method to add your custom logic of loading collection of {modelClass.TargetEntity} here

        return base.GetList();
    }}

    #region Override Authorization Methods

    protected override Task<bool> CanRetrieveList()
    {{
        // override the base method to add your custom logic of checking
        // if the current user can retrieve the list of {modelClass.TargetEntity} records

        return base.CanRetrieveList();
    }}

    protected override Task<bool> CanRetrieveItem(long id)
    {{
        // override the base method to add your custom logic of checking
        // if the current user can retrieve the {modelClass.TargetEntity} item by its id

        return base.CanRetrieveItem(id);
    }}

    protected override Task<bool> CanCreateItem({modelClass.TargetEntity} entityItem)
    {{
        // override the base method to add your custom logic of checking
        // if the current user can create a new {modelClass.TargetEntity} item

        return base.CanCreateItem(entityItem);
    }}

    protected override Task<(bool, {modelClass.TargetEntity}, IActionResult)> CanUpdateItem(long id)
    {{
        // override the base method to add your custom logic of checking
        // if the current user can update the {modelClass.TargetEntity} item

        return base.CanUpdateItem(id);
    }}

    protected override Task<bool> CanDeleteItem(long id)
    {{
        // override the base method to add your custom logic of checking
        // if the current user can delete the {modelClass.TargetEntity} item

        return base.CanDeleteItem(id);
    }}

    protected override Task<bool> CanRestoreDeletedItem(long id)
    {{
        // override the base method to add your custom logic of checking
        // if the current user can restore the {modelClass.TargetEntity} item

        return base.CanRestoreDeletedItem(id);
    }}

    #endregion
}}";
            File.WriteAllText(defaultPathFile, defaultControllerFileContent);
        }
    }
}