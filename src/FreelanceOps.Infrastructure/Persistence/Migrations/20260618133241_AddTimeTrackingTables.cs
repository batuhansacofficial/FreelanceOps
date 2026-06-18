using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreelanceOps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeTrackingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "time_entries",
                schema: "freelance_ops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_UserId_EndedAtUtc",
                schema: "freelance_ops",
                table: "time_entries",
                columns: new[] { "UserId", "EndedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_WorkspaceId",
                schema: "freelance_ops",
                table: "time_entries",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_WorkspaceId_ProjectId",
                schema: "freelance_ops",
                table: "time_entries",
                columns: new[] { "WorkspaceId", "ProjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_WorkspaceId_TaskId",
                schema: "freelance_ops",
                table: "time_entries",
                columns: new[] { "WorkspaceId", "TaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_WorkspaceId_UserId",
                schema: "freelance_ops",
                table: "time_entries",
                columns: new[] { "WorkspaceId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "time_entries",
                schema: "freelance_ops");
        }
    }
}
