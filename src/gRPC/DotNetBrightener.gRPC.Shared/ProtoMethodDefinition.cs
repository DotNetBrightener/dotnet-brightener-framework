using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetBrightener.gRPC;

public class ProtoMethodDefinition
{
    private string _restRouteTemplate;

    public string Name { get; set; }

    public ProtoMessageDefinition RequestType { get; set; }

    public ProtoMessageDefinition ResponseType { get; set; }

    public bool GenerateRestTranscoding { get; set; }

    public string RestMethod { get; set; }

    public string RestRouteTemplate
    {
        get => _restRouteTemplate;
        set
        {
            if (value.StartsWith("/"))
            {
                _restRouteTemplate = value;
            }
            else
            {
                _restRouteTemplate = "/" + value;
            }
        }
    }

    public string RestBody { get; set; } = "*";

    public string ToProtoDefinition()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"    rpc {Name} ({RequestType?.ProtobufType}) returns ({ResponseType?.ProtobufType}) {{");

        if (GenerateRestTranscoding)
        {
            stringBuilder.AppendLine("        option (google.api.http) = {");
            stringBuilder.AppendLine($"            {RestMethod}: \"{RestRouteTemplate}\"");

            if (!RestMethod.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
                !RestMethod.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
            {
                stringBuilder.AppendLine($"            body: \"{RestBody}\"");
            }

            stringBuilder.AppendLine("        };");
        }

        stringBuilder.AppendLine("    }");

        return stringBuilder.ToString();
    }

    public string ToCsMethodCall()
    {
        var parametersList = new List<string>();

        if (RequestType != null &&
            RequestType.Fields.Count > 0)
        {
            parametersList.Add(RequestType.IsSingleType
                                   ? $"request.{RequestType.Fields[0].Name.ToTitleCase()}"
                                   : $"request.To{RequestType.CsType
                                                             .Replace("<", "")
                                                             .Replace(">", "")}()");
        }

        var param = string.Join(", ", parametersList);
        var csCodeOutput =
            $@"{ResponseType.CsType} result = await _{{referencedServiceName}}.{Name}({param});";

        return csCodeOutput;
    }
}