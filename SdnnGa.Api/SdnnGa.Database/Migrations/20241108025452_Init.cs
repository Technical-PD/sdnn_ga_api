using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SdnnGa.Database.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RecCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Epochs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ModelCount = table.Column<int>(type: "integer", nullable: false),
                    IsTrained = table.Column<bool>(type: "boolean", nullable: false),
                    EpochNo = table.Column<int>(type: "integer", nullable: false),
                    SessionId = table.Column<string>(type: "text", nullable: false),
                    RecCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
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
                    Id = table.Column<string>(type: "text", nullable: false),
                    MaxEpoches = table.Column<int>(type: "integer", nullable: false),
                    FitMethod = table.Column<int>(type: "integer", nullable: false),
                    MetricFuncs = table.Column<int[]>(type: "integer[]", nullable: false),
                    LossFunc = table.Column<int>(type: "integer", nullable: false),
                    Alpha = table.Column<float>(type: "real", nullable: false),
                    XDataFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    YDataFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SessionId = table.Column<string>(type: "text", nullable: false),
                    RecCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
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
                    Id = table.Column<string>(type: "text", nullable: false),
                    MaxEpoches = table.Column<int>(type: "integer", nullable: false),
                    MutationCof = table.Column<float>(type: "real", nullable: false),
                    SessionId = table.Column<string>(type: "text", nullable: false),
                    RecCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
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
                    Id = table.Column<string>(type: "text", nullable: false),
                    IsTrained = table.Column<bool>(type: "boolean", nullable: false),
                    WeightsFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModelConfigFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Metric = table.Column<int[]>(type: "integer[]", nullable: false),
                    LossValue = table.Column<int>(type: "integer", nullable: false),
                    AccuracyValue = table.Column<float>(type: "real", nullable: false),
                    SessionId = table.Column<string>(type: "text", nullable: false),
                    EpocheId = table.Column<string>(type: "text", nullable: false),
                    RecCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
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
