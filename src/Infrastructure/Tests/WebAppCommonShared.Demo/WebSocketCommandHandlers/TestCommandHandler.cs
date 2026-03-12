using System.Security.Claims;
using DotNetBrightener.WebSocketExt.Messages;
using DotNetBrightener.WebSocketExt.Services;

namespace WebAppCommonShared.Demo.WebSocketCommandHandlers;

[WebSocketCommand("hello")]
public class TestCommandHandler : IWebsocketCommandHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TestCommandHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ResponseMessage?> HandleCommandAsync(RequestMessage    payload,
                                                           CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null ||
            user.Identity?.IsAuthenticated != true)
            return payload.Unauthorized();

        var userName = user?.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Name)?.Value;

        var millisecondsDelay = new Random().Next(500, 2000);

        await Task.Delay(millisecondsDelay, cancellationToken);

        var testPayload = payload.PayloadAs<TestPayload>()!;

        return payload.ResponseWithPayload(new TestPayload
        {
            Message =
                $"Hello {userName}. You have sent the message '{testPayload.Name}'. Responded with delay ${TimeSpan.FromMilliseconds(millisecondsDelay)}"
        });
    }
}

public class TestPayload : BasePayload
{
    public override string Action => "Hello world";

    public string Name { get; set; }

    public string Message { get; set; }
}