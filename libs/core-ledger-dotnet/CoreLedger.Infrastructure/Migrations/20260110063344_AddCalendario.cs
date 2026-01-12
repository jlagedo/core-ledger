using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calendarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    data = table.Column<DateOnly>(type: "date", nullable: false),
                    dia_util = table.Column<bool>(type: "boolean", nullable: false),
                    tipo_dia = table.Column<int>(type: "integer", nullable: false),
                    praca = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by_user_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendarios", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_calendarios_data",
                table: "calendarios",
                column: "data");

            migrationBuilder.CreateIndex(
                name: "ix_calendarios_data_praca",
                table: "calendarios",
                columns: new[] { "data", "praca" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_calendarios_praca",
                table: "calendarios",
                column: "praca");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calendarios");
        }
    }
}
