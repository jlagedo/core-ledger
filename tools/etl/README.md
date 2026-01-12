# Core Ledger ETL

Meltano-based ETL pipeline for processing B3 financial instruments data with automatic name generation.

## Overview

This project implements an ELT (Extract, Load, Transform) pipeline that:

1. **Extracts** B3 financial instruments data from CSV files
2. **Loads** the data into PostgreSQL
3. **Transforms** the data using DBT to generate human-readable Portuguese instrument names

### Pipeline Architecture

```
┌─────────────────┐     ┌─────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Preprocess     │────▶│  tap-csv    │────▶│ target-postgres  │────▶│  dbt-postgres   │
│  (UTF-8, clean) │     │  (extract)  │     │     (load)       │     │  (transform)    │
└─────────────────┘     └─────────────┘     └──────────────────┘     └─────────────────┘
                                                                              │
                                                                              ▼
                                        ┌──────────────────────────────────────────────────┐
                                        │  PostgreSQL Tables:                              │
                                        │  • b3_instruments (raw, 52 cols)                 │
                                        │  • stg_b3_instruments (staging view)             │
                                        │  • b3_instruments_enriched (final, 53 cols)      │
                                        └──────────────────────────────────────────────────┘
```

## Prerequisites

- Python 3.9+
- PostgreSQL 18+ running on localhost:5432
- Database `core_ledger_db` created
- User `postgres` with password `postgres`

## Quick Start

### 1. Install Meltano and Plugins

```bash
# Install Meltano (if not already installed)
pip install meltano

# Install all pipeline plugins
meltano install
```

### 2. Set Environment Variables

```bash
# Required for DBT transformer
export TARGET_POSTGRES_PASSWORD=postgres
```

### 3. Prepare Data

Place your B3 instruments CSV file in the project root:

```bash
# File should be named: InstrumentsConsolidatedFile_<date>_1.csv
# Example: InstrumentsConsolidatedFile_20251226_1.csv
```

### 4. Run the Full Pipeline

```bash
# Preprocess CSV + Extract + Load + Transform
bash extract/preprocess_b3_instruments.sh && meltano run b3_instruments_pipeline
```

## Pipeline Components

### 1. Preprocessing (`extract/preprocess_b3_instruments.sh`)

**Purpose:** Prepare the CSV file for processing

**Actions:**
- Removes first metadata line ("Status do Arquivo: Final")
- Converts encoding from ISO-8859-1 (Latin-1) to UTF-8
- Outputs clean CSV: `InstrumentsConsolidatedFile_clean.csv`

**Usage:**
```bash
bash extract/preprocess_b3_instruments.sh
```

### 2. Extract (tap-csv)

**Purpose:** Read CSV file and emit records in Singer format

**Configuration:** `meltano.yml`
```yaml
- name: tap-csv
  config:
    files:
      - entity: b3_instruments
        path: InstrumentsConsolidatedFile_clean.csv
        keys: [TckrSymb]
        delimiter: ;
```

### 3. Load (target-postgres)

**Purpose:** Load data into PostgreSQL

**Configuration:** `meltano.yml`
```yaml
- name: target-postgres
  config:
    host: localhost
    port: 5432
    database: core_ledger_db
    user: postgres
    default_target_schema: public
    load_method: overwrite  # Cleans table before each load
```

**Output Table:** `public.b3_instruments` (52 columns, 99,190+ records)

### 4. Transform (dbt-postgres)

**Purpose:** Generate human-readable instrument names

**Location:** `transform/` directory

**Models:**
- **Staging:** `stg_b3_instruments.sql` - Creates helper columns
  - `expiration_year` - Extracted from XprtnCd
  - `expiration_month_name` - Portuguese month name
  - `option_type_pt` - Call/Put in Portuguese
  - `strike_formatted` - Cleaned strike price
  - `expiration_date_formatted` - DD/MM/YYYY format
  - `is_mini_contract` - Boolean flag

- **Marts:** `b3_instruments_enriched.sql` - Generates instrument names
  - Implements 26+ naming patterns for different instrument types
  - Covers: Futures, Options, Equities, ETFs, Funds, Derivatives, Fixed Income
  - Fallback: "Unknown Instrument Type: [TckrSymb]" for unmapped types

**Output Table:** `public.b3_instruments_enriched` (53 columns = 52 original + `instrument_name`)

## Running Individual Steps

### Run Only DBT Transformation

```bash
# Run all DBT models
TARGET_POSTGRES_PASSWORD=postgres meltano invoke dbt-postgres run

# Run tests
TARGET_POSTGRES_PASSWORD=postgres meltano invoke dbt-postgres test

# Run specific model
TARGET_POSTGRES_PASSWORD=postgres meltano invoke dbt-postgres run --select b3_instruments_enriched
```

### Run Only Extract and Load

```bash
meltano run tap-csv target-postgres
```

### Validate Results

Run the validation script to check generated names:

```bash
python3 validate_instruments.py
```

**Expected Results:**
- Total records: 99,190
- NULL names: 0 (100% coverage)
- Unknown types: < 0.05% (excellent mapping)

## Example Generated Names

| Instrument Type | Ticker | Generated Name |
|----------------|--------|----------------|
| Mini Future | WDOF26 | Mini Minicontrato de Dólar Comercial Futuro Janeiro 2026 |
| Stock Future | ABEVOF26 | FUTURO DE ABEV3 Futuro Janeiro 2026 |
| Equity Option | A1MDA139 | Opção de A1MDA139 Call 139.00 16/01/2026 |
| Share | PETR4 | PETROLEO BRASILEIRO S.A. PETROBRAS PN EDJ N2 PETR4 |
| BDR | A1AP34 | ADVANCE AUTO PARTS INC BDR DRN |
| ETF | BOVA11 | ISHARES IBOVESPA FUNDO DE ÍNDICE (ETF) |

## Database Tables

### Raw Table: `public.b3_instruments`

**Columns:** 52 original columns from B3 CSV
- TckrSymb (Primary Key)
- Asst, AsstDesc, SgmtNm, MktNm, SctyCtgyNm
- XprtnDt, XprtnCd, OptnTp, ExrcPric
- CrpnNm, SpcfctnCd
- ... and 40 more columns

**Load Method:** Overwrite (table is cleaned before each load)

### Staging View: `public.stg_b3_instruments`

**Type:** View (not persisted)

**Purpose:** Adds helper columns for name generation

### Final Table: `public.b3_instruments_enriched`

**Columns:** 53 columns
- All 52 original columns from `b3_instruments`
- `instrument_name` (TEXT) - Generated human-readable name

**Tests:**
- TckrSymb: unique, not null
- instrument_name: not null

## Configuration Files

### meltano.yml

Central configuration for the entire pipeline:
- Plugin definitions (extractors, loaders, transformers)
- Pipeline jobs
- Plugin configurations

### transform/dbt_project.yml

DBT project configuration:
- Materialization strategies (views vs tables)
- Model paths and naming

### transform/profiles.yml

DBT database connection:
- PostgreSQL connection details
- Uses `TARGET_POSTGRES_PASSWORD` environment variable

### transform/models/staging/_sources.yml

DBT source definitions:
- Defines `b3_instruments` table as a source
- Source data tests (unique, not_null)

### transform/models/marts/schema.yml

DBT model documentation and tests:
- Column descriptions
- Data quality tests

## Instrument Naming Patterns

The pipeline implements 26+ naming patterns based on `b3_mapping.md`:

### Futures
1. Mini Futures: `Mini <Asset> Futuro <Month> <Year>`
2. Stock Futures: `<Asset> Futuro <Month> <Year>`
3. Financial Futures: `<Asset> Futuro <Month> <Year>`
4. Agribusiness Futures: `<Asset> Futuro <Month> <Year>`

### Options
5. Equity Options: `Opção de <Ticker> <Call/Put> <Strike> <Month> <Year>`
6. Index Options: `Opção de <Index> <Call/Put> <Strike> <Month> <Year>`
7. Options on Futures: `Opção de <Asset> <Call/Put> <Strike> <Month> <Year>`
8. Options on Spot: `Opção de <Asset> <Call/Put> <Strike> <Date>`

### Equities
9. Shares: `<Company> <Class> <Ticker>`
10. BDRs: `<Company> BDR <Level>`
11. Units: `<Company> Unit <Ticker>`

### ETFs & Funds
12. ETF Equities: `<Name> (ETF)`
13. ETF Foreign Index: `<Name> (ETF Estrangeiro)`
14. Funds: `Fundo <Name>`

### Derivatives
15. Common Equities Forward: `<Ticker> a Termo <Date>`
16. FX Swap: `Swap <Asset> <Date>`

### Fixed Income
17. Fixed Income Instruments: `<Description> <Date>`

### Other Types
18. Receipts, Rights, Warrants, Indexes, EDS, FRA, Forward Points

### Fallback
- Unknown types: `Unknown Instrument Type: <TckrSymb> (<Category>)`

## Troubleshooting

### Pipeline Fails at DBT Step

**Issue:** DBT cannot connect to PostgreSQL

**Solution:**
```bash
# Ensure PostgreSQL is running
# Ensure environment variable is set
export TARGET_POSTGRES_PASSWORD=postgres

# Test DBT connection
TARGET_POSTGRES_PASSWORD=postgres meltano invoke dbt-postgres debug
```

### CSV Encoding Errors

**Issue:** UnicodeDecodeError when reading CSV

**Solution:**
```bash
# Ensure preprocessing script ran successfully
bash extract/preprocess_b3_instruments.sh

# Check output file exists
ls -lh InstrumentsConsolidatedFile_clean.csv
```

### Zero Records Loaded

**Issue:** No data in `b3_instruments` table

**Solution:**
```bash
# Check CSV file has data (should have 99,190+ lines)
wc -l InstrumentsConsolidatedFile_clean.csv

# Verify preprocessing removed only 1 metadata line
wc -l InstrumentsConsolidatedFile_*.csv
```

### Unknown Instrument Types

**Issue:** Too many "Unknown Instrument Type" names

**Solution:**
1. Run validation: `python3 validate_instruments.py`
2. Check which categories are unmapped
3. Add new naming patterns to `transform/models/marts/b3_instruments_enriched.sql`
4. Refer to `b3_mapping.md` for naming rules
5. Run DBT: `TARGET_POSTGRES_PASSWORD=postgres meltano invoke dbt-postgres run`

## Maintenance

### Adding New Instrument Types

1. Update `b3_mapping.md` with new naming pattern
2. Add CASE branch in `transform/models/marts/b3_instruments_enriched.sql`
   - Place BEFORE the final ELSE clause
   - Use same pattern: detect by MktNm + SctyCtgyNm
3. Run DBT transformation:
   ```bash
   TARGET_POSTGRES_PASSWORD=postgres meltano invoke dbt-postgres run --select b3_instruments_enriched
   ```
4. Validate results:
   ```bash
   python3 validate_instruments.py
   ```

### Monitoring Unknown Types

```sql
-- Check for unmapped instrument types
SELECT DISTINCT "SctyCtgyNm", COUNT(*)
FROM public.b3_instruments_enriched
WHERE instrument_name LIKE 'Unknown%'
GROUP BY "SctyCtgyNm"
ORDER BY COUNT(*) DESC;
```

### Updating Source Data

To process a new B3 instruments file:

1. Place new CSV in project root
2. Update filename in preprocessing script if needed
3. Run full pipeline:
   ```bash
   bash extract/preprocess_b3_instruments.sh && meltano run b3_instruments_pipeline
   ```

## Data Quality Tests

The pipeline includes 5 automated data quality tests:

1. **Source TckrSymb not null** - Raw table has no NULL tickers
2. **Source TckrSymb unique** - Raw table has no duplicate tickers
3. **Enriched TckrSymb not null** - Final table has no NULL tickers
4. **Enriched TckrSymb unique** - Final table has no duplicate tickers
5. **instrument_name not null** - All instruments have generated names

Run tests:
```bash
TARGET_POSTGRES_PASSWORD=postgres meltano invoke dbt-postgres test
```

## Performance

**Full Pipeline Execution Time:** ~20-30 seconds for 99,190 records
- Preprocessing: <1 second
- Extract & Load: ~15-20 seconds
- DBT Transform: ~5 seconds
- DBT Tests: ~1 second

## Project Structure

```
core-ledger-etl/
├── README.md                               # This file
├── meltano.yml                             # Meltano configuration
├── b3_mapping.md                           # Instrument naming rules
├── validate_instruments.py                 # Validation script
├── extract/
│   └── preprocess_b3_instruments.sh       # CSV preprocessing
├── transform/                              # DBT project
│   ├── dbt_project.yml
│   ├── profiles.yml
│   ├── macros/
│   │   └── month_code_to_name.sql         # Month mapping macro
│   └── models/
│       ├── staging/
│       │   ├── _sources.yml               # Source definitions
│       │   └── stg_b3_instruments.sql     # Staging view
│       └── marts/
│           ├── schema.yml                 # Model tests
│           └── b3_instruments_enriched.sql # Name generation
├── InstrumentsConsolidatedFile_*.csv      # Input files (not committed)
└── .meltano/                               # Meltano runtime (not committed)
```

## License

[Add your license here]

## Contributors

[Add contributors here]
