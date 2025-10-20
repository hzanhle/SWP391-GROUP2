using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSerivce.Migrations
{
    /// <inheritdoc />
    public partial class Stage1_SoftLockAndOrderEnhancements : Migration
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
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    TotalDays = table.Column<int>(type: "int", nullable: false),
                    ModelPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreviewToken = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrustScoreAtBooking = table.Column<int>(type: "int", nullable: false),
                    DepositPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                });

            migrationBuilder.CreateTable(
                name: "SoftLocks",
                columns: table => new
                {
                    LockToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftLocks", x => x.LockToken);
                });

            migrationBuilder.CreateTable(
                name: "OnlineContracts",
                columns: table => new
                {
                    ContractId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Terms = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SignatureData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignedFromIpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    PdfFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TemplateVersion = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineContracts", x => x.ContractId);
                    table.ForeignKey(
                        name: "FK_OnlineContracts_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDeposited = table.Column<bool>(type: "bit", nullable: false),
                    DepositedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DepositTransactionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsFullyPaid = table.Column<bool>(type: "bit", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FullPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransactionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "VehicleInspectionReports",
                columns: table => new
                {
                    InspectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    InspectionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InspectorId = table.Column<int>(type: "int", nullable: false),
                    CurrentMileage = table.Column<int>(type: "int", nullable: false),
                    FuelLevel = table.Column<int>(type: "int", nullable: false),
                    OverallCondition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HasDamage = table.Column<bool>(type: "bit", nullable: false),
                    CompensationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompensationStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GeneralNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerSignature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerSignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InspectorSignature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleInspectionReports", x => x.InspectionId);
                    table.ForeignKey(
                        name: "FK_VehicleInspectionReports_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionDetails",
                columns: table => new
                {
                    DetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HasIssue = table.Column<bool>(type: "bit", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IssueDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiresCompensation = table.Column<bool>(type: "bit", nullable: false),
                    CompensationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionDetails", x => x.DetailId);
                    table.ForeignKey(
                        name: "FK_InspectionDetails_VehicleInspectionReports_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "VehicleInspectionReports",
                        principalColumn: "InspectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionImages",
                columns: table => new
                {
                    ImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InspectionId = table.Column<int>(type: "int", nullable: false),
                    DetailId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionImages", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_InspectionImages_InspectionDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "InspectionDetails",
                        principalColumn: "DetailId");
                    table.ForeignKey(
                        name: "FK_InspectionImages_VehicleInspectionReports_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "VehicleInspectionReports",
                        principalColumn: "InspectionId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionDetails_Category",
                table: "InspectionDetails",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionDetails_HasIssue",
                table: "InspectionDetails",
                column: "HasIssue");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionDetails_InspectionId",
                table: "InspectionDetails",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionImages_DetailId",
                table: "InspectionImages",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionImages_ImageType",
                table: "InspectionImages",
                column: "ImageType");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionImages_InspectionId",
                table: "InspectionImages",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Created",
                table: "Notifications",
                column: "Created");

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
                name: "IX_OnlineContracts_Status",
                table: "OnlineContracts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

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
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionCode",
                table: "Payments",
                column: "TransactionCode");

            migrationBuilder.CreateIndex(
                name: "IX_SoftLocks_ExpiresAt",
                table: "SoftLocks",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_SoftLocks_Status",
                table: "SoftLocks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SoftLocks_UserId",
                table: "SoftLocks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftLocks_VehicleId",
                table: "SoftLocks",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftLocks_VehicleId_Status_ExpiresAt",
                table: "SoftLocks",
                columns: new[] { "VehicleId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspectionReports_InspectionType",
                table: "VehicleInspectionReports",
                column: "InspectionType");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspectionReports_OrderId",
                table: "VehicleInspectionReports",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleInspectionReports_Status",
                table: "VehicleInspectionReports",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionImages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OnlineContracts");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "SoftLocks");

            migrationBuilder.DropTable(
                name: "InspectionDetails");

            migrationBuilder.DropTable(
                name: "VehicleInspectionReports");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
