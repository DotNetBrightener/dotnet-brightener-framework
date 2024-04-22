namespace DotNetBrightener.Infrastructure.AppClientManager.Models;

/// <summary>
///     Defines the status of the App Client
/// </summary>
public enum AppClientStatus
{
    /// <summary>
    ///     The app is not active
    /// </summary>
    Inactive = 0,

    /// <summary>
    ///     The app is currently active and can access the resources
    /// </summary>
    Active = 30,


    /// <summary>
    ///     The app is suspended and cannot access the resources
    /// </summary>
    Suspended = 50
}