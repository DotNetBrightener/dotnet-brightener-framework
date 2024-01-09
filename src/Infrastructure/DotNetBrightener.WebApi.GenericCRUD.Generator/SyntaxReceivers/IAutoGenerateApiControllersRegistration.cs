using System;
using System.Collections.Generic;

namespace DotNetBrightener.WebApi.GenericCRUD.Generator.SyntaxReceivers;

public interface IAutoGenerateApiControllersRegistration
{
    Type DataServiceRegistrationType { get; }

    /// <summary>
    ///     Defines the entities that need to generate API controllers for.
    /// </summary>
    List<Type> Entities { get; }
}