using System;

namespace DotNetBrightener.Core.Utils;

/// <summary>
/// Prevent the property that is marked with this attribute from being converted to dictionary, for mapping purpose
/// </summary>
public class DictionaryIgnoreAttribute: Attribute { }