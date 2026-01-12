using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionCreatedOutboxMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tansactioncreated_outbox_message",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    occurred_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    type = table.Column<string>(type: "text", nullable: false),
                    payload = table.Column<byte[]>(type: "bytea", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    published_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tansactioncreated_outbox_message", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tansactioncreated_outbox_message_occurred_on",
                table: "tansactioncreated_outbox_message",
                column: "occurred_on");

            migrationBuilder.CreateIndex(
                name: "IX_tansactioncreated_outbox_message_status",
                table: "tansactioncreated_outbox_message",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tansactioncreated_outbox_message");
        }
    }
}
