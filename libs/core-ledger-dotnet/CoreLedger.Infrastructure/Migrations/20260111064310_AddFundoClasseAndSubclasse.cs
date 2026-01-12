using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFundoClasseAndSubclasse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fundo_classe",
                schema: "cadastros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    fundo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cnpj_classe = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    codigo_classe = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nome_classe = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo_classe_fidc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ordem_subordinacao = table.Column<int>(type: "integer", nullable: true),
                    rentabilidade_alvo = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    responsabilidade_limitada = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    segregacao_patrimonial = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    valor_minimo_aplicacao = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ativa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fundo_classe", x => x.id);
                    table.ForeignKey(
                        name: "FK_fundo_classe_fundo_fundo_id",
                        column: x => x.fundo_id,
                        principalSchema: "cadastros",
                        principalTable: "fundo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fundo_subclasse",
                schema: "cadastros",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    classe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_subclasse = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nome_subclasse = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    serie = table.Column<int>(type: "integer", nullable: true),
                    valor_minimo_aplicacao = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    taxa_administracao_diferenciada = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    ativa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fundo_subclasse", x => x.id);
                    table.ForeignKey(
                        name: "FK_fundo_subclasse_fundo_classe_classe_id",
                        column: x => x.classe_id,
                        principalSchema: "cadastros",
                        principalTable: "fundo_classe",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_classe_codigo",
                schema: "cadastros",
                table: "fundo_classe",
                columns: new[] { "fundo_id", "codigo_classe" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_classe_fundo",
                schema: "cadastros",
                table: "fundo_classe",
                column: "fundo_id");

            migrationBuilder.CreateIndex(
                name: "ix_subclasse_classe",
                schema: "cadastros",
                table: "fundo_subclasse",
                column: "classe_id");

            migrationBuilder.CreateIndex(
                name: "ix_subclasse_codigo",
                schema: "cadastros",
                table: "fundo_subclasse",
                columns: new[] { "classe_id", "codigo_subclasse" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fundo_subclasse",
                schema: "cadastros");

            migrationBuilder.DropTable(
                name: "fundo_classe",
                schema: "cadastros");
        }
    }
}
