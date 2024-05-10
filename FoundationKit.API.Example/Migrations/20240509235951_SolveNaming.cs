using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoundationKit.API.Example.Migrations
{
    /// <inheritdoc />
    public partial class SolveNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdateAt",
                table: "Persons",
                newName: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Persons",
                newName: "UpdateAt");
        }
    }
}
