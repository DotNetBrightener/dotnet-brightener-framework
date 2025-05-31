using CRUDWebApiWithGeneratorDemo.Core.Entities;
using CRUDWebApiWithGeneratorDemo.Services;

namespace CRUDWebApiWithGeneratorDemo;

public class CRUDWebApiGeneratorRegistration
{
    private Type DataServiceRegistrationType = typeof(CRUDDataServiceGeneratorRegistration);

    public List<Type> Entities =
    [
        typeof(Product),
        typeof(ProductCategory),
        typeof(ProductDocument),
        typeof(GroupEntity),
    ];
}