using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFieldsToFundoEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "linha_dagua_global",
                schema: "cadastros",
                table: "fundo_taxa",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "permite_resgate_programado",
                schema: "cadastros",
                table: "fundo_prazo",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "prazo_maximo_programacao",
                schema: "cadastros",
                table: "fundo_prazo",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_calendario",
                schema: "cadastros",
                table: "fundo_prazo",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "NACIONAL");

            migrationBuilder.AddColumn<DateOnly>(
                name: "data_encerramento",
                schema: "cadastros",
                table: "fundo_classe",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_encerramento",
                schema: "cadastros",
                table: "fundo_classe",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "permite_resgate_antecipado",
                schema: "cadastros",
                table: "fundo_classe",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "linha_dagua_global",
                schema: "cadastros",
                table: "fundo_taxa");

            migrationBuilder.DropColumn(
                name: "permite_resgate_programado",
                schema: "cadastros",
                table: "fundo_prazo");

            migrationBuilder.DropColumn(
                name: "prazo_maximo_programacao",
                schema: "cadastros",
                table: "fundo_prazo");

            migrationBuilder.DropColumn(
                name: "tipo_calendario",
                schema: "cadastros",
                table: "fundo_prazo");

            migrationBuilder.DropColumn(
                name: "data_encerramento",
                schema: "cadastros",
                table: "fundo_classe");

            migrationBuilder.DropColumn(
                name: "motivo_encerramento",
                schema: "cadastros",
                table: "fundo_classe");

            migrationBuilder.DropColumn(
                name: "permite_resgate_antecipado",
                schema: "cadastros",
                table: "fundo_classe");
        }
    }
}
