using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInstituicaoAndFundoVinculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "instituicao",
                schema: "cadastros",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    razao_social = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nome_fantasia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instituicao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fundo_vinculo",
                schema: "cadastros",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fundo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    instituicao_id = table.Column<int>(type: "integer", nullable: false),
                    tipo_vinculo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: true),
                    contrato_numero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    principal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fundo_vinculo", x => x.id);
                    table.ForeignKey(
                        name: "FK_fundo_vinculo_fundo_fundo_id",
                        column: x => x.fundo_id,
                        principalSchema: "cadastros",
                        principalTable: "fundo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fundo_vinculo_instituicao_instituicao_id",
                        column: x => x.instituicao_id,
                        principalSchema: "cadastros",
                        principalTable: "instituicao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fundo_vinculo_instituicao_id",
                schema: "cadastros",
                table: "fundo_vinculo",
                column: "instituicao_id");

            migrationBuilder.CreateIndex(
                name: "ix_vinculo_fundo",
                schema: "cadastros",
                table: "fundo_vinculo",
                column: "fundo_id");

            migrationBuilder.CreateIndex(
                name: "ix_vinculo_tipo",
                schema: "cadastros",
                table: "fundo_vinculo",
                column: "tipo_vinculo");

            migrationBuilder.CreateIndex(
                name: "ix_vinculo_vigente",
                schema: "cadastros",
                table: "fundo_vinculo",
                columns: new[] { "fundo_id", "tipo_vinculo" },
                filter: "data_fim IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_instituicao_cnpj",
                schema: "cadastros",
                table: "instituicao",
                column: "cnpj",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fundo_vinculo",
                schema: "cadastros");

            migrationBuilder.DropTable(
                name: "instituicao",
                schema: "cadastros");
        }
    }
}
