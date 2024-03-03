using System;
using System.Collections.Generic;
using DotNetBrightener.LocaleManagement.Entities;

namespace DotNetBrightener.LocaleManagement;

internal class CRUDDataServiceGeneratorRegistration
{
    public List<Type> Entities =
    [
        typeof(AppLocaleDictionary),
        typeof(DictionaryEntry)
    ];
}
