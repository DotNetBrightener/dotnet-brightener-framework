using System.Threading.Tasks;

namespace DotNetBrightener.Core.StartupTask;

/// <summary>
///     Represents a task that is executed at the application startup
/// </summary>
public interface IStartupTask
{
    /// <summary>
    ///     The order of execution. Lower number is executed first
    /// </summary>
    int Order { get; }

    /// <summary>
    ///     The execution logic
    /// </summary>
    /// <returns></returns>
    Task Execute();
}