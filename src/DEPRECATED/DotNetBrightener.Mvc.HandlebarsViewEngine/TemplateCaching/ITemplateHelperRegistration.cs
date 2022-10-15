using System.Linq;

namespace WebEdFramework.TemplateServices;

/// <summary>
/// Provides the service for registering helpers for the template service
/// </summary>
public interface ITemplateHelperRegistration
{
    /// <summary>
    /// Registers the template helpers
    /// </summary>
    void RegisterHelpers();
}