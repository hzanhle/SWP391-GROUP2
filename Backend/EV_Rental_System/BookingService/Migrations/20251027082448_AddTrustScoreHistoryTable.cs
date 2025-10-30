using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService.Migrations
{
    /// <inheritdoc />
    public partial class AddTrustScoreHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrustScoreHistories",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    ChangeAmount = table.Column<int>(type: "int", nullable: false),
                    PreviousScore = table.Column<int>(type: "int", nullable: false),
                    NewScore = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AdjustedByAdminId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustScoreHistories", x => x.HistoryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrustScoreHistories_ChangeType",
                table: "TrustScoreHistories",
                column: "ChangeType");

            migrationBuilder.CreateIndex(
                name: "IX_TrustScoreHistories_CreatedAt",
                table: "TrustScoreHistories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrustScoreHistories_OrderId",
                table: "TrustScoreHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_TrustScoreHistories_UserId",
                table: "TrustScoreHistories",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrustScoreHistories");
        }
    }
}
