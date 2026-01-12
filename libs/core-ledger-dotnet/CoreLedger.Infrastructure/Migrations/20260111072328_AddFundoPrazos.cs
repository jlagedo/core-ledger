using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFundoPrazos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fundo_prazo",
                schema: "cadastros",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fundo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    classe_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_prazo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    dias_cotizacao = table.Column<int>(type: "integer", nullable: false),
                    dias_liquidacao = table.Column<int>(type: "integer", nullable: false),
                    dias_carencia = table.Column<int>(type: "integer", nullable: true),
                    horario_limite = table.Column<TimeOnly>(type: "time", nullable: false),
                    dias_uteis = table.Column<bool>(type: "boolean", nullable: false),
                    calendario_id = table.Column<int>(type: "integer", nullable: true),
                    permite_parcial = table.Column<bool>(type: "boolean", nullable: false),
                    percentual_minimo = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    valor_minimo = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fundo_prazo", x => x.id);
                    table.ForeignKey(
                        name: "FK_fundo_prazo_fundo_classe_classe_id",
                        column: x => x.classe_id,
                        principalSchema: "cadastros",
                        principalTable: "fundo_classe",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fundo_prazo_fundo_fundo_id",
                        column: x => x.fundo_id,
                        principalSchema: "cadastros",
                        principalTable: "fundo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fundo_prazo_excecao",
                schema: "cadastros",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prazo_id = table.Column<long>(type: "bigint", nullable: false),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: false),
                    dias_cotizacao = table.Column<int>(type: "integer", nullable: false),
                    dias_liquidacao = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fundo_prazo_excecao", x => x.id);
                    table.ForeignKey(
                        name: "FK_fundo_prazo_excecao_fundo_prazo_prazo_id",
                        column: x => x.prazo_id,
                        principalSchema: "cadastros",
                        principalTable: "fundo_prazo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_prazo_classe",
                schema: "cadastros",
                table: "fundo_prazo",
                column: "classe_id");

            migrationBuilder.CreateIndex(
                name: "ix_prazo_fundo",
                schema: "cadastros",
                table: "fundo_prazo",
                column: "fundo_id");

            migrationBuilder.CreateIndex(
                name: "ix_prazo_tipo",
                schema: "cadastros",
                table: "fundo_prazo",
                columns: new[] { "fundo_id", "classe_id", "tipo_prazo" },
                unique: true,
                filter: "ativo = true");

            migrationBuilder.CreateIndex(
                name: "ix_prazo_excecao_prazo",
                schema: "cadastros",
                table: "fundo_prazo_excecao",
                column: "prazo_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fundo_prazo_excecao",
                schema: "cadastros");

            migrationBuilder.DropTable(
                name: "fundo_prazo",
                schema: "cadastros");
        }
    }
}
