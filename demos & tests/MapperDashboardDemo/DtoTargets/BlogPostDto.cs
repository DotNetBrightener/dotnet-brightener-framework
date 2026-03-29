using DotNetBrightener.Mapper;
using MapperDashboardDemo.Entities;

namespace MapperDashboardDemo.DtoTargets;

// Feature 12: GenerateDtos - generates Create, Update, Response, Query, Upsert, Patch DTOs
// Dashboard shows: multiple generated target types from this single attribute
[GenerateDtos(
    Types = DtoTypes.All,
    OutputType = OutputType.Record,
    ExcludeProperties = [nameof(BlogPost.Content)],
    ExcludeAuditFields = true,
    Prefix = "Blog",
    Suffix = "ViewModel"
)]
[MappingTarget<BlogPost>(nameof(BlogPost.Content))]
public partial record BlogPostDto;
