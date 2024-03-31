using System.Collections.Generic;

namespace LocaleManagement.Models;

public class DictionaryEntriesImportRequest
{
    /// <summary>
    ///     The id of the dictionary to import the entries to
    /// </summary>
    public long DictionaryId { get; init; }

    /// <summary>
    ///     Indicates whether to override existing values in other dictionaries
    /// </summary>
    public bool OverrideExistingValuesInOtherDictionaries { get; init; }

    /// <summary>
    ///     The dictionary entries of the records to be inserted or updated
    /// </summary>
    public Dictionary<string, string> Entries { get; init; }
}