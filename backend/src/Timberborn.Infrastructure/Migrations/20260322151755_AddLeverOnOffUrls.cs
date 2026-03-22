using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timberborn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeverOnOffUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "Levers");

            migrationBuilder.AddColumn<string>(
                name: "UrlOff",
                table: "Levers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlOn",
                table: "Levers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlOff",
                table: "Levers");

            migrationBuilder.DropColumn(
                name: "UrlOn",
                table: "Levers");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Levers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
