namespace DotNetBrightener.WebSocketExt.Services;

[AttributeUsage(AttributeTargets.Class)]
public class WebSocketCommandAttribute : Attribute
{
    public string CommandName { get; }

    public WebSocketCommandAttribute(string commandName)
    {
        CommandName = commandName;
    }
}