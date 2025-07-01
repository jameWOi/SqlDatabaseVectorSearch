using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlDatabaseVectorSearch.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class KB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArticleNumber",
                table: "DocumentChunks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArticleUrl",
                table: "DocumentChunks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArticleNumber",
                table: "DocumentChunks");

            migrationBuilder.DropColumn(
                name: "ArticleUrl",
                table: "DocumentChunks");
        }
    }
}
