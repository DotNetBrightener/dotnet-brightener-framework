namespace DotNetBrightener.WebApi.GenericCRUD.Contracts;

/// <summary>
///     Interface that must be implemented by classes that register entities for CRUD Web API controller generation.
///     This interface provides compile-time safety and explicit contract definition for the source generator.
/// </summary>
public interface ICRUDWebApiGeneratorRegistration
{
    /// <summary>
    ///     Gets the collection of entity types for which CRUD Web API controllers should be generated.
    /// </summary>
    List<Type> Entities { get; }

    /// <summary>
    ///     Gets the type of the data service registration class that contains the data service entity definitions.
    ///     This is used to establish the relationship between Web API controllers and their corresponding data services.
    /// </summary>
    Type DataServiceRegistrationType { get; }
}
