using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timberborn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillAdapterLastState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Adapters SET LastState = (
                    SELECT State FROM AdapterLogs
                    WHERE AdapterId = Adapters.Id AND State IS NOT NULL
                    ORDER BY ReceivedAt DESC
                    LIMIT 1
                )
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Adapters SET LastState = NULL");
        }
    }
}
