using System;
using System.Collections.Generic;
using LocaleManagement.Entities;

namespace LocaleManagement;

internal class CRUDDataServiceGeneratorRegistration
{
    public List<Type> Entities =
    [
        typeof(AppLocaleDictionary),
        typeof(DictionaryEntry)
    ];
}
