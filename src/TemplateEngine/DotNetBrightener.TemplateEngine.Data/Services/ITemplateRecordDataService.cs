using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.TemplateEngine.Data.Entity;

namespace DotNetBrightener.TemplateEngine.Data.Services;

public interface ITemplateRecordDataService : IBaseDataService<TemplateRecord>
{

}

public class TemplateRecordDataService : BaseDataService<TemplateRecord>, ITemplateRecordDataService
{
    public TemplateRecordDataService(IRepository repository)
        : base(repository)
    {
    }
}