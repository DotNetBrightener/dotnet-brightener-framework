using DotNetBrightener.Mapper;
using MapperDashboardDemo.Entities;

namespace MapperDashboardDemo.DtoTargets;

// Feature 6: Class type target (default is record)
// Dashboard shows: TypeKind = "class"
[MappingTarget<Product>(nameof(Product.InternalSku))]
public partial class ProductDto;

// Feature 8: NullableProperties - all properties become nullable (useful for query/patch models)
// Dashboard shows: NullableProperties=true, all members nullable
[MappingTarget<Product>(
    nameof(Product.InternalSku),
    nameof(Product.CreatedAt),
    nameof(Product.UpdatedAt),
    NullableProperties = true
)]
public partial class ProductQueryDto;

// Feature 9: CopyAttributes - preserves source validation attributes on generated members
// Dashboard shows: CopyAttributes=true, members include copied attributes
[MappingTarget<Product>(
    nameof(Product.InternalSku),
    CopyAttributes = true
)]
public partial class ProductValidatedDto;

// Include-only product DTO for dropdown/lookup scenarios
[MappingTarget<Product>(Include = [nameof(Product.Id), nameof(Product.Name), nameof(Product.Price)])]
public partial record ProductLookupDto;
