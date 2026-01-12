using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCoreJobsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO core_jobs (reference_id, status, job_description, creation_date, running_date, finished_date, created_at, updated_at)
                SELECT
                    'REF-' || LPAD(i::text, 5, '0') as reference_id,
                    CASE
                        WHEN i % 5 = 0 THEN 4  -- Failed (20%)
                        ELSE 3                  -- Complete (80%)
                    END as status,
                    CASE (i % 10)
                        WHEN 0 THEN 'Daily NAV calculation for fund portfolio'
                        WHEN 1 THEN 'Monthly portfolio reconciliation process'
                        WHEN 2 THEN 'End-of-day transaction batch processing'
                        WHEN 3 THEN 'Security price import from market data feed'
                        WHEN 4 THEN 'Fund performance report generation'
                        WHEN 5 THEN 'Account balance verification and adjustment'
                        WHEN 6 THEN 'Journal entry posting to general ledger'
                        WHEN 7 THEN 'Investment compliance check and validation'
                        WHEN 8 THEN 'Trade settlement reconciliation process'
                        ELSE 'Financial statement consolidation job'
                    END as job_description,
                    NOW() - (INTERVAL '1 day' * (7 - (i % 7))) - (INTERVAL '1 hour' * FLOOR(RANDOM() * 12)) as creation_date,
                    NOW() - (INTERVAL '1 day' * (7 - (i % 7))) - (INTERVAL '1 hour' * FLOOR(RANDOM() * 8)) as running_date,
                    NOW() - (INTERVAL '1 day' * (7 - (i % 7))) - (INTERVAL '1 minute' * FLOOR(RANDOM() * 120)) as finished_date,
                    NOW() - (INTERVAL '1 day' * (7 - (i % 7))) - (INTERVAL '1 hour' * FLOOR(RANDOM() * 12)) as created_at,
                    NOW() - (INTERVAL '1 day' * (7 - (i % 7))) - (INTERVAL '1 minute' * FLOOR(RANDOM() * 120)) as updated_at
                FROM generate_series(1, 100) as i;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM core_jobs
                WHERE reference_id LIKE 'REF-%'
                AND reference_id ~ '^REF-[0-9]{5}$';
            ");
        }
    }
}
