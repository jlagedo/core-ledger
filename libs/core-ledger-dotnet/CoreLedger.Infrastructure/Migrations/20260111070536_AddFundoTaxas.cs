using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFundoTaxas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fundo_taxa",
                schema: "cadastros",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    fundo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    classe_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_taxa = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    percentual = table.Column<decimal>(type: "numeric(8,4)", nullable: false),
                    base_calculo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    periodicidade_provisao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    periodicidade_pagamento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    dia_pagamento = table.Column<int>(type: "integer", nullable: true),
                    valor_minimo = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    valor_maximo = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    data_inicio_vigencia = table.Column<DateOnly>(type: "date", nullable: false),
                    data_fim_vigencia = table.Column<DateOnly>(type: "date", nullable: true),
                    ativa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fundo_taxa", x => x.id);
                    table.ForeignKey(
                        name: "FK_fundo_taxa_fundo_classe_classe_id",
                        column: x => x.classe_id,
                        principalSchema: "cadastros",
                        principalTable: "fundo_classe",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fundo_taxa_fundo_fundo_id",
                        column: x => x.fundo_id,
                        principalSchema: "cadastros",
                        principalTable: "fundo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fundo_taxa_performance",
                schema: "cadastros",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    fundo_taxa_id = table.Column<long>(type: "bigint", nullable: false),
                    indexador_id = table.Column<int>(type: "integer", nullable: false),
                    percentual_benchmark = table.Column<decimal>(type: "numeric(8,4)", nullable: false),
                    metodo_calculo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    linha_dagua = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    periodicidade_cristalizacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    mes_cristalizacao = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fundo_taxa_performance", x => x.id);
                    table.ForeignKey(
                        name: "FK_fundo_taxa_performance_fundo_taxa_fundo_taxa_id",
                        column: x => x.fundo_taxa_id,
                        principalSchema: "cadastros",
                        principalTable: "fundo_taxa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fundo_taxa_performance_indexadores_indexador_id",
                        column: x => x.indexador_id,
                        principalTable: "indexadores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_taxa_classe",
                schema: "cadastros",
                table: "fundo_taxa",
                column: "classe_id");

            migrationBuilder.CreateIndex(
                name: "ix_taxa_fundo",
                schema: "cadastros",
                table: "fundo_taxa",
                column: "fundo_id");

            migrationBuilder.CreateIndex(
                name: "ix_taxa_tipo",
                schema: "cadastros",
                table: "fundo_taxa",
                column: "tipo_taxa");

            migrationBuilder.CreateIndex(
                name: "ix_taxa_unica_ativa",
                schema: "cadastros",
                table: "fundo_taxa",
                columns: new[] { "fundo_id", "classe_id", "tipo_taxa", "ativa" },
                unique: true,
                filter: "ativa = true");

            migrationBuilder.CreateIndex(
                name: "ix_taxa_performance_indexador",
                schema: "cadastros",
                table: "fundo_taxa_performance",
                column: "indexador_id");

            migrationBuilder.CreateIndex(
                name: "ix_taxa_performance_taxa",
                schema: "cadastros",
                table: "fundo_taxa_performance",
                column: "fundo_taxa_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fundo_taxa_performance",
                schema: "cadastros");

            migrationBuilder.DropTable(
                name: "fundo_taxa",
                schema: "cadastros");
        }
    }
}
