using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.TemplateEngine.Entity;

namespace DotNetBrightener.TemplateEngine.Services;

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