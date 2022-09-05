using System;

namespace DotNetBrightener.Core.ApplicationShell;

/// <summary>
///     Represents the context of the application host at the request scope level
/// </summary>
public interface IRequestWorkContext : IWorkContext, IDisposable
{
}