using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSPWeatherService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeIndexesForAggregations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Measurements_Type_Timestamp",
                table: "Measurements");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_Type_Timestamp",
                table: "Measurements",
                columns: new[] { "Type", "Timestamp" })
                .Annotation("SqlServer:Include", new[] { "Value" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Measurements_Type_Timestamp",
                table: "Measurements");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_Type_Timestamp",
                table: "Measurements",
                columns: new[] { "Type", "Timestamp" });
        }
    }
}
