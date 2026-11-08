using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LowlandTech.TinyTools.Api.Domain.Migrations
{
    /// <inheritdoc />
    public partial class InitialTinyToolsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tinytools");

            migrationBuilder.CreateTable(
                name: "Packages",
                schema: "tinytools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PackageVersion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StepIntents",
                schema: "tinytools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Intent = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepIntents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemplateFlows",
                schema: "tinytools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateFlows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                schema: "tinytools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Imports",
                schema: "tinytools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Imports_Packages_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "tinytools",
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateSteps",
                schema: "tinytools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    Query = table.Column<string>(type: "text", nullable: false),
                    Conditions = table.Column<string>(type: "text", nullable: false),
                    FlowId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepIntentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateSteps_StepIntents_StepIntentId",
                        column: x => x.StepIntentId,
                        principalSchema: "tinytools",
                        principalTable: "StepIntents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TemplateSteps_TemplateFlows_FlowId",
                        column: x => x.FlowId,
                        principalSchema: "tinytools",
                        principalTable: "TemplateFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateTemplatePackage",
                schema: "tinytools",
                columns: table => new
                {
                    PackagesId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplatesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateTemplatePackage", x => new { x.PackagesId, x.TemplatesId });
                    table.ForeignKey(
                        name: "FK_TemplateTemplatePackage_Packages_PackagesId",
                        column: x => x.PackagesId,
                        principalSchema: "tinytools",
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TemplateTemplatePackage_Templates_TemplatesId",
                        column: x => x.TemplatesId,
                        principalSchema: "tinytools",
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateVersions",
                schema: "tinytools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: false),
                    DataType = table.Column<string>(type: "text", nullable: false),
                    Json = table.Column<string>(type: "text", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
                    IsValid = table.Column<bool>(type: "boolean", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateVersions_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "tinytools",
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Imports_PackageId",
                schema: "tinytools",
                table: "Imports",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateSteps_FlowId",
                schema: "tinytools",
                table: "TemplateSteps",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateSteps_StepIntentId",
                schema: "tinytools",
                table: "TemplateSteps",
                column: "StepIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTemplatePackage_TemplatesId",
                schema: "tinytools",
                table: "TemplateTemplatePackage",
                column: "TemplatesId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateVersions_TemplateId",
                schema: "tinytools",
                table: "TemplateVersions",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Imports",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "TemplateSteps",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "TemplateTemplatePackage",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "TemplateVersions",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "StepIntents",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "TemplateFlows",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "Packages",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "Templates",
                schema: "tinytools");
        }
    }
}
