using System.Diagnostics;
using DotNetBrightener.DataAccess.EF.Auditing;
using DotNetBrightener.DataAccess.EF.Interceptors;

namespace DotNetBrightener.DataAccess.EF.Internal;

internal static class AuditEntityDebugViewBuilder
{
    public static void PrepareDebugView(this AuditEntity auditEntity)
    {
        var stackTrace = new StackTrace(0, true);

        var stackFrames = stackTrace.GetFrames();

        var stringBuilder = new List<string>();

        foreach (var frame in stackFrames)
        {
            var method        = frame.GetMethod();
            var declaringType = method?.DeclaringType;
            
            if (declaringType is null)
                continue;
            
            var namespaceName = declaringType.Namespace;

            if (namespaceName is null ||
                namespaceName.StartsWith("Microsoft.EntityFrameworkCore") ||
                namespaceName.StartsWith("System"))
                continue;

            var className  = declaringType.FullName;
            var methodName = method.Name;
            var lineNumber = frame.GetFileLineNumber();

            if (className is null || 
                className.Contains(nameof(AuditEnabledSavingChangesInterceptor)) ||
                className.Contains(nameof(AuditEntityDebugViewBuilder)))
                continue;

            stringBuilder.Add($"{className}->{methodName}@{lineNumber}");

            if (stringBuilder.Count == 15)
            {
                break; // Stop once 15 frames have been added
            }
        }

        auditEntity.DebugView = string.Join(Environment.NewLine, stringBuilder);
    }
}