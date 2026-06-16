using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreelanceOps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "project_tasks",
                schema: "freelance_ops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                schema: "freelance_ops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_tasks_AssignedToUserId",
                schema: "freelance_ops",
                table: "project_tasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_project_tasks_ProjectId",
                schema: "freelance_ops",
                table: "project_tasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_project_tasks_WorkspaceId",
                schema: "freelance_ops",
                table: "project_tasks",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_project_tasks_WorkspaceId_ProjectId_Status",
                schema: "freelance_ops",
                table: "project_tasks",
                columns: new[] { "WorkspaceId", "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_project_tasks_WorkspaceId_Status",
                schema: "freelance_ops",
                table: "project_tasks",
                columns: new[] { "WorkspaceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_projects_ClientId",
                schema: "freelance_ops",
                table: "projects",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_WorkspaceId",
                schema: "freelance_ops",
                table: "projects",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_WorkspaceId_Name",
                schema: "freelance_ops",
                table: "projects",
                columns: new[] { "WorkspaceId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_projects_WorkspaceId_Status",
                schema: "freelance_ops",
                table: "projects",
                columns: new[] { "WorkspaceId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_tasks",
                schema: "freelance_ops");

            migrationBuilder.DropTable(
                name: "projects",
                schema: "freelance_ops");
        }
    }
}
