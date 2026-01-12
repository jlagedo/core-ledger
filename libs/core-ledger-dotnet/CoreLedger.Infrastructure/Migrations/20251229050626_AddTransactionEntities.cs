using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transaction_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    short_description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    long_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transaction_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    short_description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    long_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transaction_subtypes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    short_description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    long_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_subtypes", x => x.id);
                    table.ForeignKey(
                        name: "FK_transaction_subtypes_transaction_types_type_id",
                        column: x => x.type_id,
                        principalTable: "transaction_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fund_id = table.Column<int>(type: "integer", nullable: false),
                    security_id = table.Column<int>(type: "integer", nullable: true),
                    transaction_subtype_id = table.Column<int>(type: "integer", nullable: false),
                    trade_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    settle_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_transactions_funds_fund_id",
                        column: x => x.fund_id,
                        principalTable: "funds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transactions_securities_security_id",
                        column: x => x.security_id,
                        principalTable: "securities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transactions_transaction_statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "transaction_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transactions_transaction_subtypes_transaction_subtype_id",
                        column: x => x.transaction_subtype_id,
                        principalTable: "transaction_subtypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_transaction_statuses_short_description",
                table: "transaction_statuses",
                column: "short_description");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_subtypes_short_description",
                table: "transaction_subtypes",
                column: "short_description");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_subtypes_type_id",
                table: "transaction_subtypes",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_types_short_description",
                table: "transaction_types",
                column: "short_description");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_fund_id",
                table: "transactions",
                column: "fund_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_fund_trade_date",
                table: "transactions",
                columns: new[] { "fund_id", "trade_date" });

            migrationBuilder.CreateIndex(
                name: "ix_transactions_security_id",
                table: "transactions",
                column: "security_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_settle_date",
                table: "transactions",
                column: "settle_date");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_status_id",
                table: "transactions",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_trade_date",
                table: "transactions",
                column: "trade_date");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_transaction_subtype_id",
                table: "transactions",
                column: "transaction_subtype_id");

            // Seed TransactionStatus data
            migrationBuilder.Sql(@"
                INSERT INTO transaction_statuses (short_description, long_description, created_at) VALUES
                ('NEW', 'Trade created/imported but not yet confirmed', NOW()),
                ('EXECUTED', 'Trade confirmed by broker (price + quantity final)', NOW()),
                ('BOOKED', 'GL entries generated (trade posted to accounting)', NOW()),
                ('PENDING_SETTLEMENT', 'Waiting for settlement date (e.g., D+2)', NOW()),
                ('SETTLED', 'Cash and position updated; trade fully completed', NOW()),
                ('CANCELED', 'Trade canceled before settlement', NOW()),
                ('REVERSED', 'Trade reversed after settlement (requires accounting reversal)', NOW()),
                ('FAILED', 'Trade failed settlement or failed validation', NOW());
            ");

            // Seed TransactionType data
            migrationBuilder.Sql(@"
                INSERT INTO transaction_types (short_description, long_description, created_at) VALUES
                ('EQUITY', 'EQUITY', NOW()),
                ('ETF', 'ETF', NOW()),
                ('FIXED_INCOME', 'FIXED_INCOME', NOW()),
                ('DERIVATIVE_FUTURE', 'DERIVATIVE_FUTURE', NOW()),
                ('DERIVATIVE_OPTION', 'DERIVATIVE_OPTION', NOW()),
                ('DERIVATIVE_SWAP', 'DERIVATIVE_SWAP', NOW()),
                ('FX', 'FX', NOW()),
                ('MONEY_MARKET', 'MONEY_MARKET', NOW());
            ");

            // Seed TransactionSubType data
            migrationBuilder.Sql(@"
                INSERT INTO transaction_subtypes (type_id, short_description, long_description, created_at)
                SELECT id, 'BUY', 'Purchase of shares', NOW() FROM transaction_types WHERE short_description = 'EQUITY'
                UNION ALL SELECT id, 'SELL', 'Sale of shares', NOW() FROM transaction_types WHERE short_description = 'EQUITY'
                UNION ALL SELECT id, 'BUY_CANCEL', 'Cancel a buy trade', NOW() FROM transaction_types WHERE short_description = 'EQUITY'
                UNION ALL SELECT id, 'SELL_CANCEL', 'Cancel a sell trade', NOW() FROM transaction_types WHERE short_description = 'EQUITY'
                UNION ALL SELECT id, 'SHORT_SELL', 'Initiate a short position', NOW() FROM transaction_types WHERE short_description = 'EQUITY'
                UNION ALL SELECT id, 'SHORT_COVER', 'Close a short position', NOW() FROM transaction_types WHERE short_description = 'EQUITY'
                UNION ALL SELECT id, 'BUY', 'Purchase of ETF units', NOW() FROM transaction_types WHERE short_description = 'ETF'
                UNION ALL SELECT id, 'SELL', 'Sale of ETF units', NOW() FROM transaction_types WHERE short_description = 'ETF'
                UNION ALL SELECT id, 'PURCHASE', 'Buy a bond or note', NOW() FROM transaction_types WHERE short_description = 'FIXED_INCOME'
                UNION ALL SELECT id, 'SALE', 'Sell a bond or note', NOW() FROM transaction_types WHERE short_description = 'FIXED_INCOME'
                UNION ALL SELECT id, 'AMORTIZATION', 'Principal repayment', NOW() FROM transaction_types WHERE short_description = 'FIXED_INCOME'
                UNION ALL SELECT id, 'ACCRUAL_INTEREST', 'Interest accrual', NOW() FROM transaction_types WHERE short_description = 'FIXED_INCOME'
                UNION ALL SELECT id, 'COUPON', 'Coupon received', NOW() FROM transaction_types WHERE short_description = 'FIXED_INCOME'
                UNION ALL SELECT id, 'OPEN', 'Open a futures position', NOW() FROM transaction_types WHERE short_description = 'DERIVATIVE_FUTURE'
                UNION ALL SELECT id, 'CLOSE', 'Close a futures position', NOW() FROM transaction_types WHERE short_description = 'DERIVATIVE_FUTURE'
                UNION ALL SELECT id, 'BUY', 'Buy an option', NOW() FROM transaction_types WHERE short_description = 'DERIVATIVE_OPTION'
                UNION ALL SELECT id, 'SELL', 'Sell an option', NOW() FROM transaction_types WHERE short_description = 'DERIVATIVE_OPTION'
                UNION ALL SELECT id, 'EXERCISE', 'Exercise an option', NOW() FROM transaction_types WHERE short_description = 'DERIVATIVE_OPTION'
                UNION ALL SELECT id, 'EXPIRY', 'Option expires worthless', NOW() FROM transaction_types WHERE short_description = 'DERIVATIVE_OPTION'
                UNION ALL SELECT id, 'INITIATION', 'Start a swap', NOW() FROM transaction_types WHERE short_description = 'DERIVATIVE_SWAP'
                UNION ALL SELECT id, 'SETTLEMENT', 'Swap cashflow settlement', NOW() FROM transaction_types WHERE short_description = 'DERIVATIVE_SWAP'
                UNION ALL SELECT id, 'SPOT', 'FX spot trade', NOW() FROM transaction_types WHERE short_description = 'FX'
                UNION ALL SELECT id, 'FORWARD', 'FX forward contract', NOW() FROM transaction_types WHERE short_description = 'FX'
                UNION ALL SELECT id, 'SWAP', 'FX swap (two-leg trade)', NOW() FROM transaction_types WHERE short_description = 'FX'
                UNION ALL SELECT id, 'SETTLEMENT', 'Settlement of FX contract', NOW() FROM transaction_types WHERE short_description = 'FX'
                UNION ALL SELECT id, 'PURCHASE', 'Buy MM instrument', NOW() FROM transaction_types WHERE short_description = 'MONEY_MARKET'
                UNION ALL SELECT id, 'REDEMPTION', 'Redeem MM instrument', NOW() FROM transaction_types WHERE short_description = 'MONEY_MARKET';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "transaction_statuses");

            migrationBuilder.DropTable(
                name: "transaction_subtypes");

            migrationBuilder.DropTable(
                name: "transaction_types");
        }
    }
}
