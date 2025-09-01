using System.ComponentModel.DataAnnotations.Schema;
using DotNetBrightener.DataAccess.Models;

namespace DotNetBrightener.TemplateEngine.Data.PostgreSql.Entity;

public class TemplateRecord : BaseEntityWithAuditInfo
{
    public string TemplateType { get; set; }

    public string TemplateTitle { get; set; }

    public string TemplateContent { get; set; }

    [NotMapped]
    public List<string> Fields
    {
        get =>
            FieldsString?.Split([
                                    ';'
                                ],
                                StringSplitOptions.RemoveEmptyEntries)
                         .ToList() ??
            [];
        set => FieldsString = string.Join(";", value);
    }

    [Column("Fields")]
    public string FieldsString { get; set; }

    public string FromAssemblyName { get; set; }
}