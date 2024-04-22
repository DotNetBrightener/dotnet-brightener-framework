using DotNetBrightener.Infrastructure.AppClientManager.Models;

namespace DotNetBrightener.Infrastructure.AppClientManager.Middlewares;

public class AppClientIdentifyingResult
{
    internal bool IsSuccess { get; set; }

    internal bool IsCorsEnabled { get; set; }

    internal bool ShortCircuit { get; set; }

    public AppClient? AppClient { get; set; }

    public string RequestFromAppClientId { get; set; }

    public string RequestFromAppBundleId { get; set; }

    public string RequestFromAppDomain   { get; set; }

    internal void Success(AppClient appClient, bool isCorsEnabled)
    {
        IsSuccess     = true;
        AppClient     = appClient;
        IsCorsEnabled = isCorsEnabled;
    }
}