#!/usr/bin/env python3
"""Validate securityTypeId mappings in b3_instruments_enriched table."""

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
print("B3 SECURITY TYPE ID VALIDATION")
print("=" * 80)

# 1. Check for NULL security types
print("\n1. NULL SECURITY TYPES CHECK:")
print("-" * 80)
cur.execute("""
    SELECT
        COUNT(*) as null_count,
        ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM public.b3_instruments_enriched), 2) as null_pct
    FROM public.b3_instruments_enriched
    WHERE securityTypeId IS NULL;
""")
result = cur.fetchone()
print(f"NULL security types: {result[0]:,} ({result[1]}%)")

# 2. Distribution by security type
print("\n2. DISTRIBUTION BY SECURITY TYPE:")
print("-" * 80)
cur.execute("""
    SELECT
        securityTypeId,
        CASE securityTypeId
            WHEN 1 THEN 'Equity'
            WHEN 2 THEN 'Bond'
            WHEN 3 THEN 'Cash'
            WHEN 4 THEN 'MoneyMarket'
            WHEN 6 THEN 'ETF'
            WHEN 7 THEN 'REIT'
            WHEN 8 THEN 'Derivative'
            WHEN 9 THEN 'Hybrid'
            WHEN 10 THEN 'Future'
            WHEN 11 THEN 'OptionOnEquity'
            WHEN 12 THEN 'OptionOnFuture'
            WHEN 13 THEN 'Forward'
            WHEN 14 THEN 'Fund'
            WHEN 15 THEN 'Receipt'
            WHEN 16 THEN 'FX'
            WHEN 17 THEN 'Commodity'
            WHEN 18 THEN 'Index'
            ELSE 'NULL/Unknown'
        END as security_type_name,
        COUNT(*) as count,
        ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER (), 2) as percentage
    FROM public.b3_instruments_enriched
    GROUP BY securityTypeId
    ORDER BY count DESC;
""")
rows = cur.fetchall()
print(tabulate(rows, headers=["ID", "Security Type", "Count", "Percentage"], tablefmt="grid"))

# 3. Sample records by security type
print("\n3. SAMPLE RECORDS BY SECURITY TYPE:")
print("-" * 80)
cur.execute("""
    SELECT
        "TckrSymb",
        "MktNm",
        "SctyCtgyNm",
        instrument_name,
        securityTypeId,
        CASE securityTypeId
            WHEN 1 THEN 'Equity'
            WHEN 10 THEN 'Future'
            WHEN 11 THEN 'OptionOnEquity'
            WHEN 12 THEN 'OptionOnFuture'
            WHEN 15 THEN 'Receipt'
            WHEN 6 THEN 'ETF'
            WHEN 14 THEN 'Fund'
            ELSE 'Other'
        END as type_name
    FROM public.b3_instruments_enriched
    WHERE "TckrSymb" IN ('PETR4', 'WDOF26', 'A1MDA139', 'BOVA11', 'A1AP34', 'ABEVOF26')
    ORDER BY "TckrSymb";
""")
rows = cur.fetchall()
print(tabulate(rows, headers=["Ticker", "Market", "Category", "Instrument Name", "Type ID", "Type Name"], tablefmt="grid"))

# 4. Check for unmapped categories
print("\n4. UNMAPPED CATEGORIES (NULL securityTypeId):")
print("-" * 80)
cur.execute("""
    SELECT
        "MktNm",
        "SctyCtgyNm",
        COUNT(*) as count
    FROM public.b3_instruments_enriched
    WHERE securityTypeId IS NULL
    GROUP BY "MktNm", "SctyCtgyNm"
    ORDER BY count DESC
    LIMIT 10;
""")
rows = cur.fetchall()
if rows:
    print(tabulate(rows, headers=["Market", "Category", "Count"], tablefmt="grid"))
else:
    print("No unmapped categories found!")

# 5. Validate specific instrument types
print("\n5. VALIDATION BY INSTRUMENT TYPE:")
print("-" * 80)

# Futures
cur.execute("""
    SELECT COUNT(*) FROM public.b3_instruments_enriched
    WHERE "MktNm" = 'FUTURE' AND securityTypeId = 10;
""")
futures_count = cur.fetchone()[0]
print(f"✓ Futures (MktNm='FUTURE' → securityTypeId=10): {futures_count:,}")

# Equity Options
cur.execute("""
    SELECT COUNT(*) FROM public.b3_instruments_enriched
    WHERE "MktNm" = 'EQUITY-DERIVATE'
      AND "SctyCtgyNm" = 'OPTION ON EQUITIES'
      AND securityTypeId = 11;
""")
equity_options_count = cur.fetchone()[0]
print(f"✓ Equity Options → securityTypeId=11: {equity_options_count:,}")

# Options on Index/Futures
cur.execute("""
    SELECT COUNT(*) FROM public.b3_instruments_enriched
    WHERE ("MktNm" = 'EQUITY-DERIVATE' AND "SctyCtgyNm" = 'OPTION ON INDEX')
       OR "MktNm" = 'OPTIONS ON FUTURE'
       OR "MktNm" = 'OPTIONS ON SPOT'
    AND securityTypeId = 12;
""")
options_on_future_count = cur.fetchone()[0]
print(f"✓ Options on Index/Future/Spot → securityTypeId=12: {options_on_future_count:,}")

# Shares
cur.execute("""
    SELECT COUNT(*) FROM public.b3_instruments_enriched
    WHERE "SctyCtgyNm" = 'SHARES' AND securityTypeId = 1;
""")
shares_count = cur.fetchone()[0]
print(f"✓ Shares → securityTypeId=1: {shares_count:,}")

# BDRs
cur.execute("""
    SELECT COUNT(*) FROM public.b3_instruments_enriched
    WHERE "SctyCtgyNm" = 'BDR' AND securityTypeId = 15;
""")
bdr_count = cur.fetchone()[0]
print(f"✓ BDRs → securityTypeId=15: {bdr_count:,}")

# ETFs
cur.execute("""
    SELECT COUNT(*) FROM public.b3_instruments_enriched
    WHERE "SctyCtgyNm" IN ('ETF EQUITIES', 'ETF FOREIGN INDEX') AND securityTypeId = 6;
""")
etf_count = cur.fetchone()[0]
print(f"✓ ETFs → securityTypeId=6: {etf_count:,}")

# Funds (non-REIT)
cur.execute("""
    SELECT COUNT(*) FROM public.b3_instruments_enriched
    WHERE "SctyCtgyNm" = 'FUNDS' AND securityTypeId IN (7, 14);
""")
funds_count = cur.fetchone()[0]
print(f"✓ Funds (including REITs) → securityTypeId=7 or 14: {funds_count:,}")

# REITs specifically
cur.execute("""
    SELECT COUNT(*) FROM public.b3_instruments_enriched
    WHERE "SctyCtgyNm" = 'FUNDS'
      AND ("CrpnNm" ILIKE '%IMOB%' OR "CrpnNm" ILIKE '%FII%')
      AND securityTypeId = 7;
""")
reit_count = cur.fetchone()[0]
print(f"✓ REITs (FIIs) → securityTypeId=7: {reit_count:,}")

print("\n" + "=" * 80)
print("VALIDATION COMPLETE")
print("=" * 80)

cur.close()
conn.close()
