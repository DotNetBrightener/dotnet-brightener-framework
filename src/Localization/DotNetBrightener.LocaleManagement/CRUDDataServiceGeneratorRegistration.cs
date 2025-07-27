using LocaleManagement.Entities;
using DotNetBrightener.WebApi.GenericCRUD.Contracts;

namespace LocaleManagement;

internal class CRUDDataServiceGeneratorRegistration : ICRUDDataServiceGeneratorRegistration
{
    public List<Type> Entities { get; } =
    [
        typeof(AppLocaleDictionary),
        typeof(DictionaryEntry)
    ];
}
