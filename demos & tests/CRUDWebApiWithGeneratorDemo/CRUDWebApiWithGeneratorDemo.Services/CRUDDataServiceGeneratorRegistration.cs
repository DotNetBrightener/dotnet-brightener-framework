using CRUDWebApiWithGeneratorDemo.Core.Entities;

namespace CRUDWebApiWithGeneratorDemo.Services;

public class CRUDDataServiceGeneratorRegistration
{
    public List<Type> Entities =
    [
        typeof(Product),
        typeof(ProductCategory),
        typeof(ProductDocument),
    ];
}
