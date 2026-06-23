using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreelanceOps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "proposals",
                schema: "freelance_ops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConvertedProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProposalNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Scope = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    SubtotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proposals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "proposal_items",
                schema: "freelance_ops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SubtotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proposal_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proposal_items_proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalSchema: "freelance_ops",
                        principalTable: "proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_proposal_items_ProposalId",
                schema: "freelance_ops",
                table: "proposal_items",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_proposals_ConvertedProjectId",
                schema: "freelance_ops",
                table: "proposals",
                column: "ConvertedProjectId",
                unique: true,
                filter: "\"ConvertedProjectId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_proposals_WorkspaceId_ClientId",
                schema: "freelance_ops",
                table: "proposals",
                columns: new[] { "WorkspaceId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_proposals_WorkspaceId_ProposalNumber",
                schema: "freelance_ops",
                table: "proposals",
                columns: new[] { "WorkspaceId", "ProposalNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proposals_WorkspaceId_Status",
                schema: "freelance_ops",
                table: "proposals",
                columns: new[] { "WorkspaceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_proposals_WorkspaceId_Title",
                schema: "freelance_ops",
                table: "proposals",
                columns: new[] { "WorkspaceId", "Title" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "proposal_items",
                schema: "freelance_ops");

            migrationBuilder.DropTable(
                name: "proposals",
                schema: "freelance_ops");
        }
    }
}
