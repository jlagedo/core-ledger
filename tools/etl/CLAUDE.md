# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **Meltano ETL project** for processing financial instruments data. Meltano is an open-source DataOps platform that orchestrates ELT (Extract, Load, Transform) pipelines using the Singer ecosystem.

The project appears to process Brazilian B3 stock exchange data (BDRs, funds, equities) from CSV files for core ledger integration.

---

## Technology Stack

- **Meltano 4.0.8**: DataOps platform for ELT orchestration
- **Python 3.13**: Runtime environment
- **Singer**: Data integration framework (taps/extractors and targets/loaders)
- **SQLite**: Meltano system database (`.meltano/meltano.db`)

---

## Project Structure

```
/
├── extract/          # Data extraction configurations
├── load/             # Data loading configurations
├── transform/        # DBT or SQL transformation models
├── analyze/          # Analysis notebooks/scripts
├── orchestrate/      # Pipeline orchestration definitions
├── notebook/         # Jupyter notebooks for exploration
├── output/           # Pipeline output files (gitignored)
├── .meltano/         # Meltano internal state and database
├── meltano.yml       # Main Meltano configuration file
└── requirements.txt  # Python dependencies

```

**Important Notes:**
- Most directories currently contain only `.gitkeep` files - the project is in initial setup phase
- `output/` directory is gitignored except for `.gitignore` itself
- `.meltano/` directory is gitignored and managed by Meltano

---

## Development Commands

### Project Setup

```bash
# Install Meltano (if not already installed)
# Via pipx (recommended):
pipx install meltano

# Via pip:
pip install meltano

# Install project dependencies
meltano install

# Upgrade Meltano and project
meltano upgrade
```

### Environment Management

Meltano supports multiple environments (dev, staging, prod):

```bash
# List environments
meltano environment list

# Run commands in specific environment
meltano --environment=dev <command>
meltano --environment=staging <command>
meltano --environment=prod <command>

# Run without any environment
meltano --no-environment <command>
```

**Default environment**: `dev` (configured in `meltano.yml`)

### Plugin Management

```bash
# Add a new plugin (tap/extractor)
meltano add extractor <plugin-name>

# Add a loader/target
meltano add loader <plugin-name>

# Add a transformer
meltano add transformer dbt-postgres

# Remove a plugin
meltano remove <plugin-type> <plugin-name>

# List installed plugins
# Note: Currently no plugins are installed in the project

# Lock plugin versions
meltano lock --all

# Search for plugins on Meltano Hub
meltano hub <search-term>
```

### Configuration

```bash
# View plugin configuration
meltano config <plugin-name>

# Set configuration value
meltano config <plugin-name> set <key> <value>

# Test configuration
meltano config <plugin-name> test
```

### Running Pipelines

```bash
# Run an EL pipeline (Extract and Load only)
meltano el <tap-name> <target-name>

# Run an ELT pipeline (Extract, Load, and Transform)
meltano elt <tap-name> <target-name> <transformer-name>

# Run a series of plugins
meltano run <plugin-1> <plugin-2> <plugin-3>

# Invoke a single plugin
meltano invoke <plugin-name> [args]
```

### State Management

```bash
# View pipeline state
meltano state list

# Get state for specific pipeline
meltano state get <state-id>

# Set state
meltano state set <state-id> <state-json>

# Clear state
meltano state clear <state-id>
```

### Selection Patterns

```bash
# Manage which streams/entities to extract
meltano select <tap-name> --list
meltano select <tap-name> <stream-name> <field-pattern>
meltano select <tap-name> --exclude <stream-name>
```

### Job Management

```bash
# List jobs
meltano job list

# View job logs
meltano logs <job-id>

# Manage schedules
meltano schedule list
meltano schedule add <name> --extractor <tap> --loader <target> --interval "@daily"
meltano schedule remove <name>
```

### Database Schema

```bash
# Update Meltano system database schema
meltano schema update
```

### Testing and Validation

```bash
# Run plugin tests
meltano test <plugin-name>
```

---

## Configuration Files

### meltano.yml

Main configuration file defining:
- Project ID: `019b660c-23f3-7921-b6d9-b7924c6d6508`
- Default environment: `dev`
- Available environments: `dev`, `staging`, `prod`
- Plugins (taps, targets, transformers)
- Schedules
- Jobs

**Current state**: Minimal configuration with only project ID and environments defined. No plugins, schedules, or jobs configured yet.

### .meltano/meltano.db

SQLite database storing:
- Plugin settings
- Job runs
- State information
- User configurations
- OAuth tokens

**Do not modify directly** - use `meltano` CLI commands.

---

## Data Files

Sample data file: `InstrumentsConsolidatedFile_20251226_1.csv`
- Source: Brazilian B3 stock exchange
- Format: Semicolon-delimited CSV
- Contains: Financial instruments data (BDRs, funds, stocks)
- Fields: Ticker symbols, ISIN codes, asset descriptions, trading dates, pricing info, corporate actions
- Size: ~22 MB (approximately 150k+ instruments)

---

## Common Workflows

### Setting Up a New Pipeline

1. Add extractor (tap): `meltano add extractor tap-csv` or similar
2. Configure extractor: `meltano config <tap-name> set <key> <value>`
3. Add loader (target): `meltano add loader target-postgres` or similar
4. Configure loader: `meltano config <target-name> set <key> <value>`
5. Test connection: `meltano config <plugin-name> test`
6. Select data streams: `meltano select <tap-name> <stream-name> "*"`
7. Run pipeline: `meltano el <tap-name> <target-name>`

### Adding Transformations

1. Add DBT: `meltano add transformer dbt-postgres`
2. Initialize DBT project in `transform/`
3. Create models in `transform/models/`
4. Run with transformation: `meltano elt <tap> <target> dbt-postgres`

---

## Logging and Debugging

Meltano supports various log levels and formats:

```bash
# Set log level
meltano --log-level debug <command>
meltano --log-level info <command>
meltano --log-level warning <command>

# Change log format
meltano --log-format json <command>
meltano --log-format plain <command>
meltano --log-format colored <command>
```

---

## Environment Variables

Create a `.env` file in the project root for sensitive configuration:

```bash
# Database connection strings
TARGET_POSTGRES_HOST=localhost
TARGET_POSTGRES_PORT=5432
TARGET_POSTGRES_USER=user
TARGET_POSTGRES_PASSWORD=password
TARGET_POSTGRES_DATABASE=database

# API credentials
TAP_API_KEY=your_key_here
TAP_API_SECRET=your_secret_here
```

**Security**: `.env` files are gitignored by default.

---

## Architecture Notes

### Meltano ELT Pattern

Meltano follows the modern ELT (Extract, Load, Transform) pattern:

1. **Extract**: Singer taps pull data from sources (APIs, databases, files)
2. **Load**: Singer targets write raw data to a data warehouse
3. **Transform**: DBT (data build tool) transforms data within the warehouse

This differs from traditional ETL where transformation happens before loading.

### Singer Protocol

- **Taps** (extractors): Read data from sources, output Singer messages
- **Targets** (loaders): Consume Singer messages, write to destinations
- **Messages**: SCHEMA, RECORD, STATE (JSON lines format)
- Communication: Taps stdout → Targets stdin (Unix pipe pattern)

### Directory Purposes

- `extract/`: Custom tap configurations, plugins, or Python scripts
- `load/`: Custom target configurations or loading scripts
- `transform/`: DBT models, macros, and SQL transformations
- `orchestrate/`: Airflow DAGs or other orchestration code
- `analyze/`: Analytics notebooks or ad-hoc analysis scripts
- `notebook/`: Jupyter notebooks for data exploration
- `output/`: Pipeline outputs, reports, or exported files

---

## Best Practices

### Configuration Management

- Use environment variables for secrets (via `.env` files)
- Use `meltano config <plugin> set` for non-sensitive configs
- Environment-specific configs go in `meltano.yml` under `environments:`

### Pipeline Development

- Start with small data samples during development
- Use `meltano invoke <tap> --discover` to see available streams
- Test extraction before adding loaders: `meltano invoke <tap> | head -100`
- Use `--select` and `--exclude` to limit data during testing

### State Management

- Meltano automatically manages incremental state
- State is stored per pipeline (tap-target combination)
- Clear state to do full refresh: `meltano state clear <state-id>`

### Performance

- Use `--full-refresh` flag sparingly (forces complete reload)
- Configure batch sizes in target configs for optimal performance
- Monitor job logs for performance issues

---

## Troubleshooting

### Plugin Not Found

If you see "Plugin 'X' is not known to Meltano":
- Check plugin name spelling
- Ensure plugin is added: `meltano add <type> <name>`
- Run `meltano install` if just added via `meltano.yml` edit

### Configuration Issues

```bash
# Validate configuration
meltano config <plugin-name> test

# Check what Meltano sees
meltano config <plugin-name>
```

### Database Lock Errors

If `.meltano/meltano.db` is locked:
- Ensure no other Meltano processes are running
- Delete lock file: `rm .meltano/.meltano.lock` (if safe)

---

## Documentation

- Official Meltano docs: https://docs.meltano.com
- Meltano Hub (plugin directory): https://hub.meltano.com
- Singer specification: https://github.com/singer-io/getting-started
- Open docs in browser: `meltano docs`

---

## Notes

- This project is in initial setup phase - no plugins are currently configured
- The `meltano.yml` file contains minimal configuration
- Most feature directories contain only `.gitkeep` placeholder files
- Sample data file suggests this ETL pipeline will process Brazilian financial instruments
