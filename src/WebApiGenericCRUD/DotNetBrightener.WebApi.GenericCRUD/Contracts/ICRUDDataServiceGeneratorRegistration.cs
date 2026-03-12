namespace DotNetBrightener.WebApi.GenericCRUD.Contracts;

/// <summary>
/// Interface that must be implemented by classes that register entities for CRUD data service generation.
/// This interface provides compile-time safety and explicit contract definition for the source generator.
/// </summary>
public interface ICRUDDataServiceGeneratorRegistration
{
    /// <summary>
    /// Gets the collection of entity types for which CRUD data service interfaces and classes should be generated.
    /// </summary>
    List<Type> Entities { get; }
}
