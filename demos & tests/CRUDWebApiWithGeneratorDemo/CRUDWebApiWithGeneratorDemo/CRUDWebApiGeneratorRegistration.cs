using CRUDWebApiWithGeneratorDemo.Core.Entities;
using CRUDWebApiWithGeneratorDemo.Services;

namespace CRUDWebApiWithGeneratorDemo;

public class CRUDWebApiGeneratorRegistration
{
    Type DataServiceRegistrationType = typeof(CRUDDataServiceGeneratorRegistration);

    public List<Type> Entities =
    [
        typeof(Product),
        typeof(ProductCategory),
        typeof(ProductDocument),
    ];
}
