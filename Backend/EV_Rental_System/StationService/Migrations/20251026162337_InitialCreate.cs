using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffShifts_Stations_StationId",
                table: "StaffShifts");

            migrationBuilder.AddColumn<double>(
                name: "Lat",
                table: "Stations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Lng",
                table: "Stations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StaffShifts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualCheckInTime",
                table: "StaffShifts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualCheckOutTime",
                table: "StaffShifts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "StaffShifts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "StaffShifts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StaffShifts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Stations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Lat", "Lng" },
                values: new object[] { 10.776899999999999, 106.7009 });

            migrationBuilder.UpdateData(
                table: "Stations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Lat", "Lng" },
                values: new object[] { 10.8505, 106.7717 });

            migrationBuilder.UpdateData(
                table: "Stations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Lat", "Lng" },
                values: new object[] { 10.801399999999999, 106.7105 });

            migrationBuilder.CreateIndex(
                name: "IX_StaffShift_Status",
                table: "StaffShifts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_StationId",
                table: "Feedbacks",
                column: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffShifts_Stations_StationId",
                table: "StaffShifts",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffShifts_Stations_StationId",
                table: "StaffShifts");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_StaffShift_Status",
                table: "StaffShifts");

            migrationBuilder.DropColumn(
                name: "Lat",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "Lng",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "ActualCheckInTime",
                table: "StaffShifts");

            migrationBuilder.DropColumn(
                name: "ActualCheckOutTime",
                table: "StaffShifts");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "StaffShifts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "StaffShifts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StaffShifts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StaffShifts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffShifts_Stations_StationId",
                table: "StaffShifts",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
