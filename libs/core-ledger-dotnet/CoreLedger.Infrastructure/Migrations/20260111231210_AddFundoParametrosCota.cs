using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFundoParametrosCota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fundo_parametros_cota",
                schema: "cadastros",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fundo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    casas_decimais_cota = table.Column<int>(type: "integer", nullable: false, defaultValue: 8),
                    casas_decimais_quantidade = table.Column<int>(type: "integer", nullable: false, defaultValue: 6),
                    casas_decimais_pl = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    tipo_cota = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    horario_corte = table.Column<TimeOnly>(type: "time", nullable: false),
                    cota_inicial = table.Column<decimal>(type: "numeric(20,8)", nullable: false),
                    data_cota_inicial = table.Column<DateOnly>(type: "date", nullable: false),
                    fuso_horario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "America/Sao_Paulo"),
                    permite_cota_estimada = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fundo_parametros_cota", x => x.id);
                    table.ForeignKey(
                        name: "FK_fundo_parametros_cota_fundo_fundo_id",
                        column: x => x.fundo_id,
                        principalSchema: "cadastros",
                        principalTable: "fundo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_parametros_cota_fundo",
                schema: "cadastros",
                table: "fundo_parametros_cota",
                column: "fundo_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fundo_parametros_cota",
                schema: "cadastros");
        }
    }
}
