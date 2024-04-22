namespace DotNetBrightener.Infrastructure.AppClientManager.Models;

/// <summary>
///     Defines the type of the App Client
/// </summary>
public enum AppClientType
{
    /// <summary>
    ///     No restriction on the client type
    /// </summary>
    NoRestriction = 0,

    /// <summary>
    ///     The client is web app
    /// </summary>
    Web = 5,

    /// <summary>
    ///    The client is mobile app
    /// </summary>
    Mobile = 10,

    /// <summary>
    ///    The client is desktop app
    /// </summary>
    Desktop = 20
}