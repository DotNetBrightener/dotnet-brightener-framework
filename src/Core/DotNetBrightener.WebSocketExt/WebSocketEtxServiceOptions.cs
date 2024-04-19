namespace DotNetBrightener.WebSocketExt;

public class WebSocketExtOptions
{
    /// <summary>
    ///     Specifies the time-to-live access token in minutes.
    ///     When an authenticated user attempts to connect to the WebSocket server, the server
    ///     will generate an exchange token that has access to the WebSocket server for a specified period.
    /// </summary>
    public double TimeToLiveAccessTokenInMinutes { get; set; } = 24 * 60;

    /// <summary>
    ///     The path to the WebSocket server that will handle all the WebSocket requests
    /// </summary>
    public string Path { get; set; } = "/wss";

    public string AuthInitializePath { get; set; } = "wss_auth";

    public string ConnectionTokenQueryParamName { get; set; } = "wss_token";

    public string DebugIndicatorQueryName       { get; set; } = "wss_debug";

    public double KeepAliveIntervalInSeconds { get; set; } = 120;

    public int SendReceiveBufferSizeInBytes { get; set; } = 1024 * 4;
}