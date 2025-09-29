using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.TemplateEngine.Data.Mssql.Entity;

namespace DotNetBrightener.TemplateEngine.Data.Mssql.Data;

public interface ITemplateRecordDataService : IBaseDataService<TemplateRecord>;

internal class InternalTemplateRecordDataService : BaseDataService<TemplateRecord>, ITemplateRecordDataService
{
    public InternalTemplateRecordDataService(TemplateEngineRepository repository)
        : base(repository)
    {
    }
}