using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecuritiesFullTextSearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE INDEX ix_securities_search_vector
                ON securities
                USING GIN (to_tsvector('simple', coalesce(ticker, '') || ' ' || coalesce(name, '')));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_securities_search_vector;");
        }
    }
}
