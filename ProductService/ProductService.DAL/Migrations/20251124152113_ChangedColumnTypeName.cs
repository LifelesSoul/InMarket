using System;
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

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "RegistrationDate",
                table: "Users",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreationDate",
                table: "Products",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RatingScore",
                table: "UserProfiles",
                newName: "float");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RegistrationDate",
                table: "Users",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "Products",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");
        }
    }
}
