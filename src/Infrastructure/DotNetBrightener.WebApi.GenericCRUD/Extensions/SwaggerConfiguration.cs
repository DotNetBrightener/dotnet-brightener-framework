﻿using System;
using System.IO;
using System.Reflection;

namespace DotNetBrightener.WebApi.GenericCRUD.Extensions;

public static class SwaggerConfiguration
{
    /// <summary>
    ///     Registers the generated documentation for the generic CRUD controllers.
    /// </summary>
    /// <param name="registerAction">
    ///     The Swagger configuration action to register the generated documentation.
    /// </param>
    public static void RegisterGenericCRUDDocumentation(Action<string, bool> registerAction)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

        if (File.Exists(filePath))
        {
            registerAction?.Invoke(filePath, true);
        }
    }
}