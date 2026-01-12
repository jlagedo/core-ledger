using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexadorAndHistoricoIndexador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "indexadores",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    fonte = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    periodicidade = table.Column<int>(type: "integer", nullable: false),
                    fator_acumulado = table.Column<decimal>(type: "numeric(20,12)", nullable: true),
                    data_base = table.Column<DateTime>(type: "date", nullable: true),
                    url_fonte = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    importacao_automatica = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indexadores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "historicos_indexadores",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    indexador_id = table.Column<int>(type: "integer", nullable: false),
                    data_referencia = table.Column<DateTime>(type: "date", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(20,12)", nullable: false),
                    fator_diario = table.Column<decimal>(type: "numeric(20,12)", nullable: true),
                    variacao_percentual = table.Column<decimal>(type: "numeric(12,8)", nullable: true),
                    fonte = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    importacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historicos_indexadores", x => x.id);
                    table.ForeignKey(
                        name: "FK_historicos_indexadores_indexadores_indexador_id",
                        column: x => x.indexador_id,
                        principalTable: "indexadores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_historicos_indexadores_data_referencia",
                table: "historicos_indexadores",
                column: "data_referencia");

            migrationBuilder.CreateIndex(
                name: "ix_historicos_indexadores_indexador_data",
                table: "historicos_indexadores",
                columns: new[] { "indexador_id", "data_referencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_historicos_indexadores_indexador_id",
                table: "historicos_indexadores",
                column: "indexador_id");

            migrationBuilder.CreateIndex(
                name: "ix_indexadores_ativo",
                table: "indexadores",
                column: "ativo");

            migrationBuilder.CreateIndex(
                name: "ix_indexadores_codigo",
                table: "indexadores",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_indexadores_tipo",
                table: "indexadores",
                column: "tipo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historicos_indexadores");

            migrationBuilder.DropTable(
                name: "indexadores");
        }
    }
}
