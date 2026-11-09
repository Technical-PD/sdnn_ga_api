using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SdnnGa.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    XTrainFileName = table.Column<string>(type: "TEXT", nullable: true),
                    YTrainFileName = table.Column<string>(type: "TEXT", nullable: true),
                    RecCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RecModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Epochs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ModelCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsTrained = table.Column<bool>(type: "INTEGER", nullable: false),
                    EpochNo = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", nullable: false),
                    RecCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RecModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Epochs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Epochs_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FitConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MaxEpoches = table.Column<int>(type: "INTEGER", nullable: false),
                    FitMethod = table.Column<string>(type: "TEXT", nullable: false),
                    MetricFuncs = table.Column<string>(type: "TEXT", nullable: true),
                    LossFunc = table.Column<string>(type: "TEXT", nullable: false),
                    Alpha = table.Column<float>(type: "REAL", nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", nullable: false),
                    RecCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RecModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FitConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FitConfigs_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneticConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MaxEpoches = table.Column<int>(type: "INTEGER", nullable: false),
                    CountOfModelsInEpoch = table.Column<int>(type: "INTEGER", nullable: false),
                    ActFuncMutationProb = table.Column<float>(type: "REAL", nullable: false),
                    CountOfNeuronMutationProb = table.Column<float>(type: "REAL", nullable: false),
                    CountOfInternalLayerMutationProb = table.Column<float>(type: "REAL", nullable: false),
                    StopAccValue = table.Column<float>(type: "REAL", nullable: false),
                    StopLossValue = table.Column<float>(type: "REAL", nullable: false),
                    BiasMutationProb = table.Column<float>(type: "REAL", nullable: false),
                    SelectionCriterion = table.Column<string>(type: "TEXT", nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", nullable: false),
                    RecCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RecModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneticConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneticConfigs_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NeuralNetworkModel",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    IsTrained = table.Column<bool>(type: "INTEGER", nullable: false),
                    WeightsFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ModelConfigFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ModelConfigDotNetFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    FitHistory = table.Column<string>(type: "TEXT", nullable: true),
                    LossValue = table.Column<float>(type: "REAL", nullable: false),
                    AccuracyValue = table.Column<float>(type: "REAL", nullable: false),
                    FitError = table.Column<string>(type: "TEXT", nullable: true),
                    SessionId = table.Column<string>(type: "TEXT", nullable: false),
                    EpocheId = table.Column<string>(type: "TEXT", nullable: false),
                    RecCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RecModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NeuralNetworkModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NeuralNetworkModel_Epochs_EpocheId",
                        column: x => x.EpocheId,
                        principalTable: "Epochs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NeuralNetworkModel_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Epochs_SessionId",
                table: "Epochs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_FitConfigs_SessionId",
                table: "FitConfigs",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneticConfigs_SessionId",
                table: "GeneticConfigs",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NeuralNetworkModel_EpocheId",
                table: "NeuralNetworkModel",
                column: "EpocheId");

            migrationBuilder.CreateIndex(
                name: "IX_NeuralNetworkModel_SessionId",
                table: "NeuralNetworkModel",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FitConfigs");

            migrationBuilder.DropTable(
                name: "GeneticConfigs");

            migrationBuilder.DropTable(
                name: "NeuralNetworkModel");

            migrationBuilder.DropTable(
                name: "Epochs");

            migrationBuilder.DropTable(
                name: "Sessions");
        }
    }
}
