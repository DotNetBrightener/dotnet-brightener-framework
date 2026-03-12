using CRUDWebApiWithGeneratorDemo.Core.Entities;
using CRUDWebApiWithGeneratorDemo.Services;
using DotNetBrightener.WebApi.GenericCRUD.Contracts;

namespace CRUDWebApiWithGeneratorDemo;

public class CRUDWebApiGeneratorRegistration : ICRUDWebApiGeneratorRegistration
{
    public Type DataServiceRegistrationType { get; } = typeof(CRUDDataServiceGeneratorRegistration);

    public List<Type> Entities { get; } =
    [
        typeof(Product),
        typeof(ProductCategory),
        typeof(ProductDocument),
        typeof(GroupEntity),
    ];
}
