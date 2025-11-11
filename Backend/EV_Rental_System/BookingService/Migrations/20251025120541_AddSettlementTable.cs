using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService.Migrations
{
    /// <inheritdoc />
    public partial class AddSettlementTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    VehicleRating = table.Column<double>(type: "float", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.FeedbackId);
                });

            migrationBuilder.CreateTable(
                name: "Settlements",
                columns: table => new
                {
                    SettlementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ScheduledReturnTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualReturnTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OvertimeHours = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    OvertimeFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DamageCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DamageDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    InitialDeposit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAdditionalCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositRefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdditionalPaymentRequired = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvoiceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsFinalized = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settlements", x => x.SettlementId);
                    table.ForeignKey(
                        name: "FK_Settlements_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_CreatedAt",
                table: "Settlements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_IsFinalized",
                table: "Settlements",
                column: "IsFinalized");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_OrderId",
                table: "Settlements",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "Settlements");
        }
    }
}
