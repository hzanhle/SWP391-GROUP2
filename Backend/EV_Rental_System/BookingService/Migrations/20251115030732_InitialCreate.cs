using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataId = table.Column<int>(type: "int", nullable: true),
                    StaffId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OnlineContractId = table.Column<int>(type: "int", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InitialTrustScore = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                });

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

            migrationBuilder.CreateTable(
                name: "TrustScores",
                columns: table => new
                {
                    TrustScoreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustScores", x => x.TrustScoreId);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OnlineContracts",
                columns: table => new
                {
                    OnlineContractId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContractFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SignatureData = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TemplateVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineContracts", x => x.OnlineContractId);
                    table.ForeignKey(
                        name: "FK_OnlineContracts_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleCheckIns",
                columns: table => new
                {
                    CheckInId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OdometerReading = table.Column<int>(type: "int", nullable: true),
                    FuelLevel = table.Column<int>(type: "int", nullable: true),
                    ImageUrls = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConfirmedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleCheckIns", x => x.CheckInId);
                    table.ForeignKey(
                        name: "FK_VehicleCheckIns_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VehicleReturns",
                columns: table => new
                {
                    ReturnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ReturnTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OdometerReading = table.Column<int>(type: "int", nullable: true),
                    FuelLevel = table.Column<int>(type: "int", nullable: true),
                    ImageUrls = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ConditionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HasDamage = table.Column<bool>(type: "bit", nullable: false),
                    DamageDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DamageCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConfirmedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleReturns", x => x.ReturnId);
                    table.ForeignKey(
                        name: "FK_VehicleReturns_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SettlementId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentGatewayResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
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
                    FinalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundStatus = table.Column<int>(type: "int", nullable: false),
                    RefundMethod = table.Column<int>(type: "int", nullable: false),
                    RefundProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundProcessedBy = table.Column<int>(type: "int", nullable: true),
                    RefundNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefundTransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RefundGatewayResponse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RefundProofDocumentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefundProofUploadedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OriginalPaymentId = table.Column<int>(type: "int", nullable: true),
                    OriginalPaymentPaymentId = table.Column<int>(type: "int", nullable: true),
                    AdditionalPaymentStatus = table.Column<int>(type: "int", nullable: false),
                    AdditionalPaymentId = table.Column<int>(type: "int", nullable: true),
                    AdditionalPaymentPaymentId = table.Column<int>(type: "int", nullable: true),
                    AdditionalPaymentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
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
                    table.ForeignKey(
                        name: "FK_Settlements_Payments_AdditionalPaymentPaymentId",
                        column: x => x.AdditionalPaymentPaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId");
                    table.ForeignKey(
                        name: "FK_Settlements_Payments_OriginalPaymentPaymentId",
                        column: x => x.OriginalPaymentPaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_OrderId",
                table: "Feedbacks",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_Rating",
                table: "Feedbacks",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_UserId",
                table: "Feedbacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_UserId_CreatedAt",
                table: "Feedbacks",
                columns: new[] { "UserId", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Created",
                table: "Notifications",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DataType_DataId",
                table: "Notifications",
                columns: new[] { "DataType", "DataId" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineContracts_ContractNumber",
                table: "OnlineContracts",
                column: "ContractNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnlineContracts_OrderId",
                table: "OnlineContracts",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FromDate_ToDate",
                table: "Orders",
                columns: new[] { "FromDate", "ToDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_VehicleId",
                table: "Orders",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SettlementId",
                table: "Payments",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionId",
                table: "Payments",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_AdditionalPaymentPaymentId",
                table: "Settlements",
                column: "AdditionalPaymentPaymentId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_OriginalPaymentPaymentId",
                table: "Settlements",
                column: "OriginalPaymentPaymentId");

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

            migrationBuilder.CreateIndex(
                name: "IX_TrustScores_CreatedAt",
                table: "TrustScores",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrustScores_UserId",
                table: "TrustScores",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleCheckIns_OrderId",
                table: "VehicleCheckIns",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleReturns_OrderId",
                table: "VehicleReturns",
                column: "OrderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Settlements_SettlementId",
                table: "Payments",
                column: "SettlementId",
                principalTable: "Settlements",
                principalColumn: "SettlementId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Settlements_Orders_OrderId",
                table: "Settlements");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Settlements_SettlementId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OnlineContracts");

            migrationBuilder.DropTable(
                name: "TrustScoreHistories");

            migrationBuilder.DropTable(
                name: "TrustScores");

            migrationBuilder.DropTable(
                name: "VehicleCheckIns");

            migrationBuilder.DropTable(
                name: "VehicleReturns");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Settlements");

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
