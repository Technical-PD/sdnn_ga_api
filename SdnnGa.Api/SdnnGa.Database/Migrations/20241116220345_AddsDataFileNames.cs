using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SdnnGa.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddsDataFileNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "XTrainFileName",
                table: "Sessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YTrainFileName",
                table: "Sessions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "XTrainFileName",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "YTrainFileName",
                table: "Sessions");
        }
    }
}
