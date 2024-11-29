using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SdnnGa.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGeneticConfigTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MutationCof",
                table: "GeneticConfigs",
                newName: "CountOfNeuronMutationProb");

            migrationBuilder.AddColumn<float>(
                name: "ActFuncMutationProb",
                table: "GeneticConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "BiasMutationProb",
                table: "GeneticConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "CountOfInternalLayerMutationProb",
                table: "GeneticConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "CountOfModelsInEpoch",
                table: "GeneticConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActFuncMutationProb",
                table: "GeneticConfigs");

            migrationBuilder.DropColumn(
                name: "BiasMutationProb",
                table: "GeneticConfigs");

            migrationBuilder.DropColumn(
                name: "CountOfInternalLayerMutationProb",
                table: "GeneticConfigs");

            migrationBuilder.DropColumn(
                name: "CountOfModelsInEpoch",
                table: "GeneticConfigs");

            migrationBuilder.RenameColumn(
                name: "CountOfNeuronMutationProb",
                table: "GeneticConfigs",
                newName: "MutationCof");
        }
    }
}
