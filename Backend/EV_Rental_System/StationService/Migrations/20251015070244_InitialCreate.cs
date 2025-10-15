using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaffShifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    ShiftDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Scheduled"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffShifts_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Stations",
                columns: new[] { "Id", "IsActive", "Location", "ManagerId", "Name" },
                values: new object[,]
                {
                    { 1, true, "123 Nguyễn Huệ, Quận 1, TP.HCM", null, "Trạm Đăng Kiểm Quận 1" },
                    { 2, true, "456 Võ Văn Ngân, Thủ Đức, TP.HCM", null, "Trạm Đăng Kiểm Thủ Đức" },
                    { 3, true, "789 Xô Viết Nghệ Tĩnh, Bình Thạnh, TP.HCM", null, "Trạm Đăng Kiểm Bình Thạnh" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffShift_Station_Date",
                table: "StaffShifts",
                columns: new[] { "StationId", "ShiftDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffShift_User_Date",
                table: "StaffShifts",
                columns: new[] { "UserId", "ShiftDate" });

            migrationBuilder.CreateIndex(
                name: "UQ_StaffShift_UniqueShift",
                table: "StaffShifts",
                columns: new[] { "UserId", "StationId", "ShiftDate", "StartTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffShifts");

            migrationBuilder.DropTable(
                name: "Stations");
        }
    }
}
