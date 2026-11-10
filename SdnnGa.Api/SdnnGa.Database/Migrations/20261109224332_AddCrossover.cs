using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SdnnGa.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCrossover : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Origin",
                table: "NeuralNetworkModel",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentModelNames",
                table: "NeuralNetworkModel",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrossoverMethod",
                table: "GeneticConfigs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvolutionStrategy",
                table: "GeneticConfigs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstParentCriterion",
                table: "GeneticConfigs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondParentCriterion",
                table: "GeneticConfigs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origin",
                table: "NeuralNetworkModel");

            migrationBuilder.DropColumn(
                name: "ParentModelNames",
                table: "NeuralNetworkModel");

            migrationBuilder.DropColumn(
                name: "CrossoverMethod",
                table: "GeneticConfigs");

            migrationBuilder.DropColumn(
                name: "EvolutionStrategy",
                table: "GeneticConfigs");

            migrationBuilder.DropColumn(
                name: "FirstParentCriterion",
                table: "GeneticConfigs");

            migrationBuilder.DropColumn(
                name: "SecondParentCriterion",
                table: "GeneticConfigs");
        }
    }
}
