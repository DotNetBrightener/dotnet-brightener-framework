namespace DotNetBrightener.Framework.Exceptions;

public class UnknownPropertyException : InvalidOperationException
{
    public UnknownPropertyException(string propName, Type inputType)
        : base($"Unknown property '{propName}' of type {inputType.FullName}")
    {
    }
}