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
                name: "TinyTools_Packages",
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
                    table.PrimaryKey("PK_TinyTools_Packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TinyTools_StepIntents",
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
                    table.PrimaryKey("PK_TinyTools_StepIntents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TinyTools_TemplateFlows",
                schema: "tinytools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinyTools_TemplateFlows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TinyTools_Templates",
                schema: "tinytools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinyTools_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TinyTools_Imports",
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
                    table.PrimaryKey("PK_TinyTools_Imports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TinyTools_Imports_TinyTools_Packages_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "tinytools",
                        principalTable: "TinyTools_Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TinyTools_TemplateSteps",
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
                    table.PrimaryKey("PK_TinyTools_TemplateSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TinyTools_TemplateSteps_TinyTools_StepIntents_StepIntentId",
                        column: x => x.StepIntentId,
                        principalSchema: "tinytools",
                        principalTable: "TinyTools_StepIntents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TinyTools_TemplateSteps_TinyTools_TemplateFlows_FlowId",
                        column: x => x.FlowId,
                        principalSchema: "tinytools",
                        principalTable: "TinyTools_TemplateFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TinyTools_TemplateTemplatePackage",
                schema: "tinytools",
                columns: table => new
                {
                    PackagesId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplatesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinyTools_TemplateTemplatePackage", x => new { x.PackagesId, x.TemplatesId });
                    table.ForeignKey(
                        name: "FK_TinyTools_TemplateTemplatePackage_TinyTools_Packages_Packag~",
                        column: x => x.PackagesId,
                        principalSchema: "tinytools",
                        principalTable: "TinyTools_Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TinyTools_TemplateTemplatePackage_TinyTools_Templates_Templ~",
                        column: x => x.TemplatesId,
                        principalSchema: "tinytools",
                        principalTable: "TinyTools_Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TinyTools_TemplateVersions",
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
                    table.PrimaryKey("PK_TinyTools_TemplateVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TinyTools_TemplateVersions_TinyTools_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "tinytools",
                        principalTable: "TinyTools_Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TinyTools_Imports_PackageId",
                schema: "tinytools",
                table: "TinyTools_Imports",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_TinyTools_TemplateSteps_FlowId",
                schema: "tinytools",
                table: "TinyTools_TemplateSteps",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_TinyTools_TemplateSteps_StepIntentId",
                schema: "tinytools",
                table: "TinyTools_TemplateSteps",
                column: "StepIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_TinyTools_TemplateTemplatePackage_TemplatesId",
                schema: "tinytools",
                table: "TinyTools_TemplateTemplatePackage",
                column: "TemplatesId");

            migrationBuilder.CreateIndex(
                name: "IX_TinyTools_TemplateVersions_TemplateId",
                schema: "tinytools",
                table: "TinyTools_TemplateVersions",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TinyTools_Imports",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "TinyTools_TemplateSteps",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "TinyTools_TemplateTemplatePackage",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "TinyTools_TemplateVersions",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "TinyTools_StepIntents",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "TinyTools_TemplateFlows",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "TinyTools_Packages",
                schema: "tinytools");

            migrationBuilder.DropTable(
                name: "TinyTools_Templates",
                schema: "tinytools");
        }
    }
}
