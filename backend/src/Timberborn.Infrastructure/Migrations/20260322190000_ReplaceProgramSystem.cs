using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timberborn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceProgramSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "AutomationRuleId",
                table: "ActionLogs");

            migrationBuilder.AddColumn<Guid>(
                name: "ProgramId",
                table: "ActionLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    GraphJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Programs");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "ActionLogs");

            migrationBuilder.AddColumn<Guid>(
                name: "AutomationRuleId",
                table: "ActionLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AutomationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AdapterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LeverId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConditionsJson = table.Column<string>(type: "TEXT", nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRules_Adapters_AdapterId",
                        column: x => x.AdapterId,
                        principalTable: "Adapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AutomationRules_Levers_LeverId",
                        column: x => x.LeverId,
                        principalTable: "Levers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_AdapterId",
                table: "AutomationRules",
                column: "AdapterId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_LeverId",
                table: "AutomationRules",
                column: "LeverId");
        }
    }
}
