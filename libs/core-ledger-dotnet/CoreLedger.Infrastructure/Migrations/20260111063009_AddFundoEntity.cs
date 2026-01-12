using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFundoEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cadastros");

            migrationBuilder.CreateTable(
                name: "fundo",
                schema: "cadastros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    razao_social = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nome_fantasia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nome_curto = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    data_constituicao = table.Column<DateOnly>(type: "date", nullable: true),
                    data_inicio_atividade = table.Column<DateOnly>(type: "date", nullable: true),
                    tipo_fundo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    classificacao_cvm = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    classificacao_anbima = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    codigo_anbima = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    situacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    prazo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    publico_alvo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tributacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    condominio = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    exclusivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    reservado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    permite_alavancagem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    aceita_cripto = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    percentual_exterior = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    wizard_completo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    progresso_cadastro = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fundo", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fundo_cnpj",
                schema: "cadastros",
                table: "fundo",
                column: "cnpj",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_fundo_situacao",
                schema: "cadastros",
                table: "fundo",
                column: "situacao");

            migrationBuilder.CreateIndex(
                name: "ix_fundo_tipo",
                schema: "cadastros",
                table: "fundo",
                column: "tipo_fundo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fundo",
                schema: "cadastros");
        }
    }
}
