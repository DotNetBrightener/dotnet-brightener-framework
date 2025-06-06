﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using WebApi.GenericCRUD.Generator.SyntaxReceivers;
using WebApi.GenericCRUD.Generator.Utils;

namespace WebApi.GenericCRUD.Generator.Generators;

[Generator]
public class ControllerClassGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register syntax provider instead of syntax receiver
        var syntaxProvider = context.SyntaxProvider
                                    .CreateSyntaxProvider(
                                                          predicate: AutoGenerateApiControllerSyntaxReceiver
                                                             .IsCandidateForGeneration,
                                                          transform: (ctx, _) =>
                                                              AutoGenerateApiControllerSyntaxReceiver
                                                                 .GetSemanticTargetForGeneration(ctx))
                                    .Where(m => m != null);

        context.RegisterSourceOutput(syntaxProvider.Collect(), Execute);
    }

    private void Execute(SourceProductionContext                         context,
                         ImmutableArray<IEnumerable<CodeGenerationInfo>> immutableArray)
    {
        var models = immutableArray.SelectMany(i => i.Where(item => item is not null)
                                                     .Select(item => item))
                                   .ToImmutableArray();

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
            GenerateControllerClass(context, modelClass);
        }
    }

    private void InjectSwaggerConfigIfNeeded(string inputFile)
    {
        var fileContent = File.ReadAllText(inputFile);

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

    // Modified to accept SourceProductionContext
    private static void GenerateControllerClass(SourceProductionContext context, CodeGenerationInfo modelClass)
    {
        var className = $"{modelClass.TargetEntity}Controller";

        var controllerSrc = $@"
{FileTemplates.GetFileHeader(className)}

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using DotNetBrightener.WebApi.GenericCRUD.Controllers;
using DotNetBrightener.DataAccess.Services;
using {modelClass.DataServiceNamespace};
using {modelClass.TargetEntityNamespace};

/****************************************************

This partial class is added with customized document comments that are fit for the linked type {modelClass.TargetEntity}.

Any overriden logic should be done in the other part of the file, {className}.cs.

****************************************************/

namespace {modelClass.ControllerNamespace};

public partial class {className}
{{
    
    internal {className}(
        I{modelClass.TargetEntity}DataService dataService)
        : base(dataService)
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
            var defaultControllerFileContent = $@"using DotNetBrightener.WebApi.GenericCRUD.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using {modelClass.DataServiceNamespace};
using {modelClass.TargetEntityNamespace};

namespace {modelClass.ControllerNamespace};

/// <summary>
///     Provide public APIs for <see cref=""{modelClass.TargetEntity}"" /> entity.
/// </summary>
/// 
// Uncomment the next line to enable authorization for this controller
// [Authorize]
[ApiController]
[Route(""api/[controller]"")]
public partial class {className}: BaseCRUDController<{modelClass.TargetEntity}>
{{
    private readonly ILogger _logger;

    public {className}(
        I{modelClass.TargetEntity}DataService dataService,
        ILogger<{className}> logger)
        : this(dataService)
    {{
        _logger = logger;
    }}

    public override partial Task<IActionResult> GetList()
    {{
        // override the base method to add your custom logic of loading collection of {modelClass.TargetEntity} here

        return base.GetList();
    }}

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

    protected override Task<(bool, {modelClass.TargetEntity}, IActionResult)> CanDeleteItem(long id)
    {{
        // override the base method to add your custom logic of checking
        // if the current user can delete the {modelClass.TargetEntity} item

        return base.CanDeleteItem(id);
    }}

    protected override Task<(bool, {modelClass.TargetEntity}, IActionResult)> CanRestoreDeletedItem(long id)
    {{
        // override the base method to add your custom logic of checking
        // if the current user can restore the {modelClass.TargetEntity} item

        return base.CanRestoreDeletedItem(id);
    }}
}}";
            File.WriteAllText(defaultPathFile, defaultControllerFileContent);
        }
    }
}