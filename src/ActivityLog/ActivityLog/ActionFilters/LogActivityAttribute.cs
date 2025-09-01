namespace ActivityLog.ActionFilters;

[AttributeUsage(validOn: AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class LogActivityAttribute : Attribute
{
    public LogActivityAttribute(string name)
    {
        Name = name;
    }

    public LogActivityAttribute(string name, string descriptionFormat)
    {
        Name = name;
        DescriptionFormat = descriptionFormat;
    }

    public string Name { get; set; }

    public string Description { get; set; }
    
    public string TargetEntity { get; set; }

    /// <summary>
    ///     The format of the description of the activity.
    ///     The format can contain placeholders for method arguments
    ///     and will be logged as metadata
    /// </summary>
    public string DescriptionFormat { get; set; }
}