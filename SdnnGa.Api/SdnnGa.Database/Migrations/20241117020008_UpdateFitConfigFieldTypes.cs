using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SdnnGa.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFitConfigFieldTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string[]>(
                name: "MetricFuncs",
                table: "FitConfigs",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(int[]),
                oldType: "integer[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LossFunc",
                table: "FitConfigs",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "FitMethod",
                table: "FitConfigs",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int[]>(
                name: "MetricFuncs",
                table: "FitConfigs",
                type: "integer[]",
                nullable: true,
                oldClrType: typeof(string[]),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LossFunc",
                table: "FitConfigs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "FitMethod",
                table: "FitConfigs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
