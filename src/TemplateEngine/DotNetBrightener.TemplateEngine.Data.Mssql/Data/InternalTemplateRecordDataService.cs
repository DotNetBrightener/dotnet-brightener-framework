using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.TemplateEngine.Data.Entity;
using DotNetBrightener.TemplateEngine.Data.Services;

namespace DotNetBrightener.TemplateEngine.Data.Mssql.Data;

internal class InternalTemplateRecordDataService : BaseDataService<TemplateRecord>, ITemplateRecordDataService
{
    public InternalTemplateRecordDataService(TemplateEngineRepository repository)
        : base(repository)
    {
    }
}