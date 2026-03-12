using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetBrightener.TemplateEngine.Data.Mssql.Migrations
{
    /// <inheritdoc />
    public partial class Add_Configuration_Fields_To_TemplateRecord_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemplateContentEditorConfig",
                schema: "TemplateEngine",
                table: "TemplateRecord",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateTitleEditorConfig",
                schema: "TemplateEngine",
                table: "TemplateRecord",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateContentEditorConfig",
                schema: "TemplateEngine",
                table: "TemplateRecord");

            migrationBuilder.DropColumn(
                name: "TemplateTitleEditorConfig",
                schema: "TemplateEngine",
                table: "TemplateRecord");
        }
    }
}
