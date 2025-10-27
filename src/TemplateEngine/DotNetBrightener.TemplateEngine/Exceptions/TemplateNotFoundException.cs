namespace DotNetBrightener.TemplateEngine.Exceptions;

public class TemplateNotFoundException: Exception;

public class TemplateTypeNotFoundException : Exception
{
    public string TemplateTypeName { get; set; }

    public TemplateTypeNotFoundException(string templateTypeName)
        : this(templateTypeName, 
               $"The requested template with type {templateTypeName} could not be found")
    {
    }

    public TemplateTypeNotFoundException(string templateTypeName, 
                                         Exception innerException)
        : this(templateTypeName,
               $"The requested template with type {templateTypeName} could not be found",
               innerException)
    {
    }

    public TemplateTypeNotFoundException(string templateTypeName, string message)
        : base(message)
    {
        TemplateTypeName = templateTypeName;
    }

    public TemplateTypeNotFoundException(string templateTypeName, 
                                         string message, 
                                         Exception innerException)
        : base(message, innerException)
    {
        TemplateTypeName = templateTypeName;
    }
}