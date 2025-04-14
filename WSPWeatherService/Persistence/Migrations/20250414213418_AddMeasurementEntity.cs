using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSPWeatherService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Station = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_Station_Timestamp_Type",
                table: "Measurements",
                columns: new[] { "Station", "Timestamp", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_Timestamp",
                table: "Measurements",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_Type_Timestamp",
                table: "Measurements",
                columns: new[] { "Type", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Measurements");
        }
    }
}
