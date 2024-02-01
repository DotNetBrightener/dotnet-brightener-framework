namespace DotNetBrightener.gRPC;

internal class ProtoMethodDefinition
{
    private string _restRouteTemplate;
    public  string Name { get; set; }

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
}