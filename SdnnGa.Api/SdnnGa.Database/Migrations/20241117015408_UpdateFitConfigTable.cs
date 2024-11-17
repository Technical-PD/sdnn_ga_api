using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SdnnGa.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFitConfigTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "XDataFileName",
                table: "FitConfigs");

            migrationBuilder.DropColumn(
                name: "YDataFileName",
                table: "FitConfigs");

            migrationBuilder.AlterColumn<int[]>(
                name: "MetricFuncs",
                table: "FitConfigs",
                type: "integer[]",
                nullable: true,
                oldClrType: typeof(int[]),
                oldType: "integer[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int[]>(
                name: "MetricFuncs",
                table: "FitConfigs",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0],
                oldClrType: typeof(int[]),
                oldType: "integer[]",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XDataFileName",
                table: "FitConfigs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YDataFileName",
                table: "FitConfigs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
