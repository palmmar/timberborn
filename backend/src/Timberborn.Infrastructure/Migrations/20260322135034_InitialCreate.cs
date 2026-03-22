using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timberborn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Adapters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adapters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Levers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    HttpMethod = table.Column<string>(type: "TEXT", nullable: false),
                    BodyTemplate = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Levers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdapterLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AdapterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RawPayload = table.Column<string>(type: "TEXT", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TriggeredAnyRule = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdapterLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdapterLogs_Adapters_AdapterId",
                        column: x => x.AdapterId,
                        principalTable: "Adapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LeverId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AutomationRuleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AdapterLogId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RequestBody = table.Column<string>(type: "TEXT", nullable: true),
                    ResponseStatusCode = table.Column<int>(type: "INTEGER", nullable: true),
                    ResponseBody = table.Column<string>(type: "TEXT", nullable: true),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    CalledAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionLogs_Levers_LeverId",
                        column: x => x.LeverId,
                        principalTable: "Levers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_ActionLogs_CalledAt",
                table: "ActionLogs",
                column: "CalledAt");

            migrationBuilder.CreateIndex(
                name: "IX_ActionLogs_LeverId",
                table: "ActionLogs",
                column: "LeverId");

            migrationBuilder.CreateIndex(
                name: "IX_AdapterLogs_AdapterId",
                table: "AdapterLogs",
                column: "AdapterId");

            migrationBuilder.CreateIndex(
                name: "IX_AdapterLogs_ReceivedAt",
                table: "AdapterLogs",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Adapters_Slug",
                table: "Adapters",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_AdapterId",
                table: "AutomationRules",
                column: "AdapterId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_LeverId",
                table: "AutomationRules",
                column: "LeverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionLogs");

            migrationBuilder.DropTable(
                name: "AdapterLogs");

            migrationBuilder.DropTable(
                name: "AutomationRules");

            migrationBuilder.DropTable(
                name: "Adapters");

            migrationBuilder.DropTable(
                name: "Levers");
        }
    }
}
