using System.Linq;
using Microsoft.CodeAnalysis;

namespace DotNetBrightener.gRPC.Generator.SyntaxReceivers;

public partial class AutoGenerateProtoFileReceiver
{
    private static readonly string[] _allowedRestMethods =
        new[]
            {
                "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD"
            }.Select(method => method.ToLower())
             .ToArray();
}