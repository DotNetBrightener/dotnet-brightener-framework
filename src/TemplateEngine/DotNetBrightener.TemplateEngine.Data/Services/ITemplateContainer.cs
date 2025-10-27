using System.Collections.Concurrent;
using System.Reflection;
using DotNetBrightener.TemplateEngine.Attributes;
using DotNetBrightener.TemplateEngine.Data.Models;
using DotNetBrightener.TemplateEngine.Exceptions;

namespace DotNetBrightener.TemplateEngine.Data.Services;

/// <summary>
///     Represents the container of the templates
/// </summary>
public interface ITemplateContainer
{
    /// <summary>
    ///     Registers the template with the given type to the container
    /// </summary>
    /// <typeparam name="TTemplateType">Type of the template</typeparam>
    void RegisterTemplate<TTemplateType>();

    /// <summary>
    ///     Retrieves all the registered template types
    /// </summary>
    /// <returns></returns>
    List<Type> GetAllTemplateTypes();

    /// <summary>
    ///    Retrieves the template type by its name
    /// </summary>
    /// <param name="templateTypeName"></param>
    /// <returns></returns>
    Type GetTemplateTypeByName(string templateTypeName);

    /// <summary>
    ///    Retrieves the template information by its name
    /// </summary>
    /// <param name="templateTypeName"></param>
    /// <returns></returns>
    TemplateListItemModel GetTemplateInformation(string templateTypeName);
}

public class TemplateContainer : ITemplateContainer
{
    private readonly ConcurrentDictionary<string, Type> _templateTypesList = new();

    public void RegisterTemplate<TTemplateType>()
    {
        var typeName = typeof(TTemplateType).FullName;

        if (typeName == null)
            return;

        _templateTypesList.TryAdd(typeName, typeof(TTemplateType));
    }

    public List<Type> GetAllTemplateTypes()
    {
        return [.. _templateTypesList.Values];
    }

    public Type GetTemplateTypeByName(string templateTypeName)
    {
        return _templateTypesList.GetValueOrDefault(templateTypeName);
    }

    public TemplateListItemModel GetTemplateInformation(string templateTypeName)
    {
        var templateType = GetTemplateTypeByName(templateTypeName);

        if (templateType == null)
            throw new TemplateTypeNotFoundException(templateTypeName);

        var templateInformation = new TemplateListItemModel
        {
            TemplateName = GetFormattedTemplateName(templateType.Name),
            TemplateType = templateTypeName
        };

        var templateDescriptionAttribute = templateType.GetCustomAttribute<TemplateDescriptionAttribute>();

        if (templateDescriptionAttribute != null)
        {
            templateInformation.TemplateDescription    = templateDescriptionAttribute.TemplateDescription;
            templateInformation.TemplateDescriptionKey = templateDescriptionAttribute.TemplateDescriptionKey;
        }

        templateInformation.Fields         = TemplateFieldsUtils.RetrieveTemplateFields(templateType);
        templateInformation.FieldsMetadata = TemplateFieldsUtils.RetrieveTemplateFieldsMetadata(templateType);

        return templateInformation;
    }

    private static string GetFormattedTemplateName(string templateTypeName)
    {
        return templateTypeName.CamelFriendly();
    }
}