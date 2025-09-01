namespace ActivityLog.ActionFilters;

[AttributeUsage(validOn: AttributeTargets.Method)]
public class LogActivityAttribute(string name, string descriptionFormat) : Attribute
{
    public LogActivityAttribute(string name)
        : this(name, null)
    {
    }

    public string Name { get; set; } = name;

    public string Description { get; set; }
    
    public string TargetEntity { get; set; }
    
    public string DescriptionFormat { get; set; } = descriptionFormat;
}