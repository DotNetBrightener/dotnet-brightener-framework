using CRUDWebApiWithGeneratorDemo.Core.Entities;
using DotNetBrightener.WebApi.GenericCRUD.Contracts;

namespace CRUDWebApiWithGeneratorDemo.Services;

public class CRUDDataServiceGeneratorRegistration : ICRUDDataServiceGeneratorRegistration
{
    public List<Type> Entities { get; } =
    [
        typeof(Product),
        typeof(ProductCategory),
        typeof(ProductDocument),
        typeof(GroupEntity),
    ];
}
