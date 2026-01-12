using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoreJobsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "core_jobs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reference_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    job_description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    running_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    finished_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_jobs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_core_jobs_reference_id",
                table: "core_jobs",
                column: "reference_id");

            migrationBuilder.CreateIndex(
                name: "IX_core_jobs_status",
                table: "core_jobs",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "core_jobs");
        }
    }
}
