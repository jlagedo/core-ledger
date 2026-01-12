#!/usr/bin/env python3
"""Validate generated instrument names in b3_instruments_enriched table."""

import psycopg2
from tabulate import tabulate

# Database connection
conn = psycopg2.connect(
    host="localhost",
    port=5432,
    database="core_ledger_db",
    user="postgres",
    password="postgres"
)
cur = conn.cursor()

print("=" * 80)
print("B3 INSTRUMENT NAME VALIDATION")
print("=" * 80)

# 1. Check for NULL names
print("\n1. NULL NAMES CHECK:")
print("-" * 80)
cur.execute("""
    SELECT
        COUNT(*) as total_records,
        COUNT(instrument_name) as records_with_names,
        COUNT(*) - COUNT(instrument_name) as null_names
    FROM public.b3_instruments_enriched;
""")
result = cur.fetchone()
print(f"Total records: {result[0]:,}")
print(f"Records with names: {result[1]:,}")
print(f"NULL names: {result[2]:,}")

# 2. Check unknown instrument count
print("\n2. UNKNOWN INSTRUMENT TYPES:")
print("-" * 80)
cur.execute("""
    SELECT
        COUNT(*) AS unknown_count,
        ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM public.b3_instruments_enriched), 2) AS unknown_pct
    FROM public.b3_instruments_enriched
    WHERE instrument_name LIKE 'Unknown%';
""")
result = cur.fetchone()
print(f"Unknown instruments: {result[0]:,} ({result[1]}%)")

# 3. Sample names by security category (top 10)
print("\n3. SAMPLE NAMES BY SECURITY CATEGORY (Top 10):")
print("-" * 80)
cur.execute("""
    WITH samples AS (
        SELECT
            "SctyCtgyNm",
            instrument_name,
            ROW_NUMBER() OVER (PARTITION BY "SctyCtgyNm" ORDER BY "TckrSymb") as rn
        FROM public.b3_instruments_enriched
    )
    SELECT
        "SctyCtgyNm" as category,
        COUNT(*) as count,
        MAX(CASE WHEN rn = 1 THEN instrument_name END) as sample_1,
        MAX(CASE WHEN rn = 2 THEN instrument_name END) as sample_2
    FROM samples
    GROUP BY "SctyCtgyNm"
    ORDER BY count DESC
    LIMIT 10;
""")
rows = cur.fetchall()
print(tabulate(rows, headers=["Category", "Count", "Sample 1", "Sample 2"], tablefmt="grid"))

# 4. Validate specific examples from different categories
print("\n4. SPECIFIC EXAMPLE VALIDATIONS:")
print("-" * 80)
cur.execute("""
    SELECT
        "TckrSymb" as ticker,
        "SctyCtgyNm" as category,
        "MktNm" as market,
        instrument_name
    FROM public.b3_instruments_enriched
    WHERE "TckrSymb" IN ('WDOF26', 'PETR4', 'A1MDA139', 'BOVA11', 'A1AP34', 'WINF26')
    ORDER BY "TckrSymb";
""")
rows = cur.fetchall()
print(tabulate(rows, headers=["Ticker", "Category", "Market", "Instrument Name"], tablefmt="grid"))

# 5. Check futures naming
print("\n5. FUTURES EXAMPLES:")
print("-" * 80)
cur.execute("""
    SELECT
        "TckrSymb" as ticker,
        "SctyCtgyNm" as category,
        instrument_name
    FROM public.b3_instruments_enriched
    WHERE "MktNm" = 'FUTURE'
    ORDER BY "TckrSymb"
    LIMIT 5;
""")
rows = cur.fetchall()
print(tabulate(rows, headers=["Ticker", "Category", "Instrument Name"], tablefmt="grid"))

# 6. Check options naming
print("\n6. OPTIONS EXAMPLES:")
print("-" * 80)
cur.execute("""
    SELECT
        "TckrSymb" as ticker,
        "SctyCtgyNm" as category,
        instrument_name
    FROM public.b3_instruments_enriched
    WHERE "SctyCtgyNm" LIKE '%OPTION%'
    ORDER BY "TckrSymb"
    LIMIT 5;
""")
rows = cur.fetchall()
print(tabulate(rows, headers=["Ticker", "Category", "Instrument Name"], tablefmt="grid"))

# 7. Check shares naming
print("\n7. SHARES EXAMPLES:")
print("-" * 80)
cur.execute("""
    SELECT
        "TckrSymb" as ticker,
        instrument_name,
        "CrpnNm" as company
    FROM public.b3_instruments_enriched
    WHERE "SctyCtgyNm" = 'SHARES'
    ORDER BY "TckrSymb"
    LIMIT 5;
""")
rows = cur.fetchall()
print(tabulate(rows, headers=["Ticker", "Instrument Name", "Company"], tablefmt="grid"))

print("\n" + "=" * 80)
print("VALIDATION COMPLETE")
print("=" * 80)

cur.close()
conn.close()
