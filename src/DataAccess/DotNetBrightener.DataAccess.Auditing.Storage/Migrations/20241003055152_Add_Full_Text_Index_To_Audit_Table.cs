using DotNetBrightener.DataAccess.EF.Auditing;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.DataAccess.Auditing.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Add_Full_Text_Index_To_Audit_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                                  IF (EXISTS(SELECT * FROM sys.configurations WHERE name = 'show advanced options' AND value_in_use = 1)
                                        AND EXISTS (SELECT * FROM sys.configurations WHERE name = 'full-text search' AND value_in_use = 1))
                                  BEGIN
                                      -- Check if full-text catalog exists, if not, create it
                                      IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'AuditFullTextCatalog')
                                      BEGIN
                                          CREATE FULLTEXT CATALOG AuditFullTextCatalog AS DEFAULT;
                                      END

                                      -- Create full-text index on the EntityIdentifier column in the 'Auditing' schema
                                      CREATE FULLTEXT INDEX ON Auditing.AuditEntity(EntityIdentifier)
                                      KEY INDEX PK_AuditEntity
                                      ON AuditFullTextCatalog;
                                  END
                                 
                                 """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
