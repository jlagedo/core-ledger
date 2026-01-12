using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTransactionCreatedOutboxTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_tansactioncreated_outbox_message",
                table: "tansactioncreated_outbox_message");

            migrationBuilder.RenameTable(
                name: "tansactioncreated_outbox_message",
                newName: "transaction_created_outbox_message");

            migrationBuilder.RenameIndex(
                name: "IX_tansactioncreated_outbox_message_status",
                table: "transaction_created_outbox_message",
                newName: "IX_transaction_created_outbox_message_status");

            migrationBuilder.RenameIndex(
                name: "IX_tansactioncreated_outbox_message_occurred_on",
                table: "transaction_created_outbox_message",
                newName: "IX_transaction_created_outbox_message_occurred_on");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transaction_created_outbox_message",
                table: "transaction_created_outbox_message",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_transaction_created_outbox_message",
                table: "transaction_created_outbox_message");

            migrationBuilder.RenameTable(
                name: "transaction_created_outbox_message",
                newName: "tansactioncreated_outbox_message");

            migrationBuilder.RenameIndex(
                name: "IX_transaction_created_outbox_message_status",
                table: "tansactioncreated_outbox_message",
                newName: "IX_tansactioncreated_outbox_message_status");

            migrationBuilder.RenameIndex(
                name: "IX_transaction_created_outbox_message_occurred_on",
                table: "tansactioncreated_outbox_message",
                newName: "IX_tansactioncreated_outbox_message_occurred_on");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tansactioncreated_outbox_message",
                table: "tansactioncreated_outbox_message",
                column: "id");
        }
    }
}
