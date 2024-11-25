namespace DotNetBrightener.TimeBasedOtp;

public class OneTimePasswordException : Exception
{
    public OneTimePasswordException()
        : base()
    {
    }

    public OneTimePasswordException(string message)
        : base(message)
    {
    }

    public OneTimePasswordException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}