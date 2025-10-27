using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.TemplateEngine.Data.PostgreSql.Entity;

namespace DotNetBrightener.TemplateEngine.Data.PostgreSql.Data;

public interface ITemplateRecordDataService : IBaseDataService<TemplateRecord>;

internal class InternalTemplateRecordDataService : BaseDataService<TemplateRecord>, ITemplateRecordDataService
{
    public InternalTemplateRecordDataService(TemplateEngineRepository repository)
        : base(repository)
    {
    }
}