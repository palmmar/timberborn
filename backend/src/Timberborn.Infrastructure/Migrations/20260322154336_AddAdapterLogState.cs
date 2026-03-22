using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timberborn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdapterLogState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "AdapterLogs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "AdapterLogs");
        }
    }
}
