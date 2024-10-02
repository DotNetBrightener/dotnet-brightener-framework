using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DotNetBrightener.TestHelpers;

public class XunitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _outputHelper;

    public XunitLoggerProvider(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XunitLogger(_outputHelper, categoryName);
    }

    public void Dispose() { }
}