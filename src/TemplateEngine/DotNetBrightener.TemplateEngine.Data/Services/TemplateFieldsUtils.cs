namespace DotNetBrightener.TemplateEngine.Data.Services;

public static class TemplateFieldsUtils
{
    public static List<string> RetrieveTemplateFields(Type templateType)
    {
        var fieldNamesFromType = GetFieldNamesFromType(templateType);

        return fieldNamesFromType;
    }

    static List<string> GetFieldNamesFromType(this Type    type,
                                              string       name          = "",
                                              List<string> recursiveList = null,
                                              int          level         = 0)
    {
        if (recursiveList == null)
            recursiveList = new List<string>();

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
}