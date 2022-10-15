using System.Text;
using DotNetBrightener.Core.ApplicationShell;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.LayoutRenderers;
using NLog.Web.LayoutRenderers;

namespace DotNetBrightener.Core.Logging;

[LayoutRenderer(LayoutRendererName)]
public class TenantLayoutRenderer : AspNetLayoutRendererBase
{
    public const string LayoutRendererName = "tenant-name";

    protected override void DoAppend(StringBuilder builder, LogEventInfo logEvent)
    {
        var context = HttpContextAccessor.HttpContext;

        var appHostContext = context.RequestServices.GetService<IAppHostContext>();
        var tenantName     = appHostContext.RetrieveState<string>(CoreConstants.TenantName) ?? "Default";
        builder.Append(tenantName);
    }
}