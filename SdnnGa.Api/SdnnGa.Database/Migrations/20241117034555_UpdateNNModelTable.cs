using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SdnnGa.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNNModelTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Loss",
                table: "NeuralNetworkModel");

            migrationBuilder.DropColumn(
                name: "Metric",
                table: "NeuralNetworkModel");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "NeuralNetworkModel");

            migrationBuilder.AddColumn<string>(
                name: "FitHistory",
                table: "NeuralNetworkModel",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FitHistory",
                table: "NeuralNetworkModel");

            migrationBuilder.AddColumn<int>(
                name: "Loss",
                table: "NeuralNetworkModel",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int[]>(
                name: "Metric",
                table: "NeuralNetworkModel",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "NeuralNetworkModel",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
