using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AHPDecisionMaker.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Goal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectId);
                });

            migrationBuilder.CreateTable(
                name: "DecisionModels",
                columns: table => new
                {
                    ModelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Goal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionModels", x => x.ModelId);
                    table.ForeignKey(
                        name: "FK_DecisionModels_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alternatives",
                columns: table => new
                {
                    AlternativeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alternatives", x => x.AlternativeId);
                    table.ForeignKey(
                        name: "FK_Alternatives_DecisionModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "DecisionModels",
                        principalColumn: "ModelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Criteria",
                columns: table => new
                {
                    CriterionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Criteria", x => x.CriterionId);
                    table.ForeignKey(
                        name: "FK_Criteria_DecisionModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "DecisionModels",
                        principalColumn: "ModelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlternativeComparisons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelId = table.Column<int>(type: "int", nullable: false),
                    CriterionId = table.Column<int>(type: "int", nullable: false),
                    AlternativeAId = table.Column<int>(type: "int", nullable: false),
                    AlternativeBId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternativeComparisons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlternativeComparisons_Alternatives_AlternativeAId",
                        column: x => x.AlternativeAId,
                        principalTable: "Alternatives",
                        principalColumn: "AlternativeId");
                    table.ForeignKey(
                        name: "FK_AlternativeComparisons_Alternatives_AlternativeBId",
                        column: x => x.AlternativeBId,
                        principalTable: "Alternatives",
                        principalColumn: "AlternativeId");
                    table.ForeignKey(
                        name: "FK_AlternativeComparisons_Criteria_CriterionId",
                        column: x => x.CriterionId,
                        principalTable: "Criteria",
                        principalColumn: "CriterionId");
                    table.ForeignKey(
                        name: "FK_AlternativeComparisons_DecisionModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "DecisionModels",
                        principalColumn: "ModelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CriteriaComparisons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelId = table.Column<int>(type: "int", nullable: false),
                    CriterionAId = table.Column<int>(type: "int", nullable: false),
                    CriterionBId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaComparisons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriteriaComparisons_Criteria_CriterionAId",
                        column: x => x.CriterionAId,
                        principalTable: "Criteria",
                        principalColumn: "CriterionId");
                    table.ForeignKey(
                        name: "FK_CriteriaComparisons_Criteria_CriterionBId",
                        column: x => x.CriterionBId,
                        principalTable: "Criteria",
                        principalColumn: "CriterionId");
                    table.ForeignKey(
                        name: "FK_CriteriaComparisons_DecisionModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "DecisionModels",
                        principalColumn: "ModelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeComparisons_AlternativeAId",
                table: "AlternativeComparisons",
                column: "AlternativeAId");

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeComparisons_AlternativeBId",
                table: "AlternativeComparisons",
                column: "AlternativeBId");

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeComparisons_CriterionId",
                table: "AlternativeComparisons",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeComparisons_ModelId",
                table: "AlternativeComparisons",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Alternatives_ModelId",
                table: "Alternatives",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Criteria_ModelId",
                table: "Criteria",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaComparisons_CriterionAId",
                table: "CriteriaComparisons",
                column: "CriterionAId");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaComparisons_CriterionBId",
                table: "CriteriaComparisons",
                column: "CriterionBId");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaComparisons_ModelId",
                table: "CriteriaComparisons",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionModels_ProjectId",
                table: "DecisionModels",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlternativeComparisons");

            migrationBuilder.DropTable(
                name: "CriteriaComparisons");

            migrationBuilder.DropTable(
                name: "Alternatives");

            migrationBuilder.DropTable(
                name: "Criteria");

            migrationBuilder.DropTable(
                name: "DecisionModels");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
