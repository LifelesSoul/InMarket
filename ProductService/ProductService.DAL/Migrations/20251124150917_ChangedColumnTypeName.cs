using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ChangedColumnTypeName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "float",
                table: "UserProfiles",
                newName: "RatingScore");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RatingScore",
                table: "UserProfiles",
                newName: "float");
        }
    }
}
