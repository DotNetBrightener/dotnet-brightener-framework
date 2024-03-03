using System.Collections.Generic;

namespace DotNetBrightener.LocaleManagement.Models;

public class LocaleInformation
{
    public long?                      Id                { get; set; }
    public string                     AppId             { get; set; }
    public string                     AppName           { get; set; }
    public string                     Description       { get; set; }
    public string                     LocaleCode        { get; set; }
    public string                     CountryCode       { get; set; }
    public Dictionary<string, string> DictionaryEntries { get; set; }
}