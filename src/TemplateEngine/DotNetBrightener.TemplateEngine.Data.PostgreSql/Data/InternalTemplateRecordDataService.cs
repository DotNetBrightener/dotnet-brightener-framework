using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.TemplateEngine.Data.PostgreSql.Entity;

namespace DotNetBrightener.TemplateEngine.Data.PostgreSql.Data;

public interface ITemplateRecordDataService : IBaseDataService<TemplateRecord>;

internal class InternalTemplateRecordDataService(TemplateEngineRepository repository)
    : BaseDataService<TemplateRecord>(repository), ITemplateRecordDataService;