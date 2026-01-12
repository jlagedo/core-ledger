using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFundoParametrosFIDC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fundo_parametros_fidc",
                schema: "cadastros",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fundo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_fidc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tipos_recebiveis = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    prazo_medio_carteira = table.Column<int>(type: "integer", nullable: true),
                    indice_subordinacao_alvo = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    indice_subordinacao_minimo = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    provisao_devedores_duvidosos = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    limite_concentracao_cedente = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    limite_concentracao_sacado = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    possui_coobrigacao = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    percentual_coobrigacao = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    registradora_recebiveis = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    integracao_registradora = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    codigo_registradora = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fundo_parametros_fidc", x => x.id);
                    table.ForeignKey(
                        name: "FK_fundo_parametros_fidc_fundo_fundo_id",
                        column: x => x.fundo_id,
                        principalSchema: "cadastros",
                        principalTable: "fundo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_parametros_fidc_fundo",
                schema: "cadastros",
                table: "fundo_parametros_fidc",
                column: "fundo_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fundo_parametros_fidc",
                schema: "cadastros");
        }
    }
}
