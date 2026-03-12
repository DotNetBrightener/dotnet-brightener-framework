namespace DotNetBrightener.TemplateEngine.Data.Services;

public class TemplateFieldMetadata
{
    public string FieldName   { get; set; }
    public string Description { get; set; }
}

public static class TemplateFieldsUtils
{
    public static List<string> RetrieveTemplateFields(Type templateType)
    {
        var fieldNamesFromType = GetFieldNamesFromType(templateType);

        return fieldNamesFromType;
    }

    public static List<TemplateFieldMetadata> RetrieveTemplateFieldsMetadata(Type templateType)
    {
        var fieldNamesFromType = GetFieldsMetadataFromType(templateType);

        return fieldNamesFromType;
    }

    private static List<string> GetFieldNamesFromType(this Type    type,
                                                      string       name          = "",
                                                      List<string> recursiveList = null,
                                                      int          level         = 0)
    {
        if (recursiveList == null)
            recursiveList = [];

        if (level > 3)
            return recursiveList;

        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            recursiveList.Add($"{name}.{property.Name}".Trim('.'));

            if (property.PropertyType.IsClass &&
                property.PropertyType.IsNotSystemType())
            {
                GetFieldNamesFromType(property.PropertyType,
                                      $"{name}.{property.Name}".Trim('.'),
                                      recursiveList,
                                      level + 1);
            }
        }

        return recursiveList;
    }

    private static List<TemplateFieldMetadata> GetFieldsMetadataFromType(this Type type,
                                                                         string    name = "",
                                                                         List<TemplateFieldMetadata> recursiveList =
                                                                             null,
                                                                         int level = 0)
    {
        if (recursiveList == null)
            recursiveList = [];

        if (level > 3)
            return recursiveList;

        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            var fieldName = $"{name}.{property.Name}".Trim('.');

            recursiveList.Add(new TemplateFieldMetadata
            {
                FieldName   = fieldName,
                Description = property.GetXmlDocumentation()
            });

            if (property.PropertyType.IsClass &&
                property.PropertyType.IsNotSystemType())
            {
                GetFieldsMetadataFromType(property.PropertyType,
                                          fieldName,
                                          recursiveList,
                                          level + 1);
            }
        }

        return recursiveList;
    }
}