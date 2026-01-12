# Core Ledger Documentation

This directory contains all project documentation organized by category.

## Documentation Structure

```
docs/
├── specs/              # Feature specifications
│   ├── api/            # .NET API specifications
│   └── ui/             # Angular UI specifications
├── testing/            # Testing documentation
├── etl/                # ETL pipeline documentation
└── compliance/         # Compliance documentation
```

## Specifications

### API Specifications (`specs/api/`)

| Document | Description |
|----------|-------------|
| [00_VISAO_GERAL_IMPLEMENTACAO](specs/api/00_VISAO_GERAL_IMPLEMENTACAO.md) | Implementation overview |
| [01_ENTIDADES_BASE](specs/api/01_ENTIDADES_BASE.md) | Base entities |
| [02_FUNDO_CORE](specs/api/02_FUNDO_CORE.md) | Core fund entity |
| [03_CLASSES_SUBCLASSES](specs/api/03_CLASSES_SUBCLASSES.md) | Asset classes and subclasses |
| [04_TAXAS](specs/api/04_TAXAS.md) | Rates and taxes |
| [05_PRAZOS_OPERACIONAIS](specs/api/05_PRAZOS_OPERACIONAIS.md) | Operational terms |
| [06_VINCULOS_INSTITUCIONAIS](specs/api/06_VINCULOS_INSTITUCIONAIS.md) | Institutional relationships |
| [07_PARAMETROS_FIDC](specs/api/07_PARAMETROS_FIDC.md) | FIDC parameters |
| [08_API_ENDPOINTS](specs/api/08_API_ENDPOINTS.md) | API endpoints |
| [09_VALIDACOES_REGRAS](specs/api/09_VALIDACOES_REGRAS.md) | Validation rules |
| [10_TESTES](specs/api/10_TESTES.md) | Testing documentation |

### UI Specifications (`specs/ui/`)

| Document | Description |
|----------|-------------|
| [00-VISAO-GERAL-IMPLEMENTACAO](specs/ui/00-VISAO-GERAL-IMPLEMENTACAO.md) | Implementation overview |
| [01-SLICE-INFRAESTRUTURA-BASE](specs/ui/01-SLICE-INFRAESTRUTURA-BASE.md) | Base infrastructure |
| [02-SLICE-IDENTIFICACAO-FUNDO](specs/ui/02-SLICE-IDENTIFICACAO-FUNDO.md) | Fund identification |
| [03-SLICE-CLASSIFICACAO](specs/ui/03-SLICE-CLASSIFICACAO.md) | Classification |
| [05-SLICE-PARAMETROS-COTA](specs/ui/05-SLICE-PARAMETROS-COTA.md) | Quote parameters |
| [06-SLICE-TAXAS](specs/ui/06-SLICE-TAXAS.md) | Rates and taxes |
| [07-SLICE-PRAZOS](specs/ui/07-SLICE-PRAZOS.md) | Terms and periods |
| [08-SLICE-CLASSES-CVM175](specs/ui/08-SLICE-CLASSES-CVM175.md) | CVM 175 classes |
| [08-B-SLICE-PARAMETROS-FIDC](specs/ui/08-B-SLICE-PARAMETROS-FIDC.md) | FIDC parameters |
| [09-SLICE-VINCULOS](specs/ui/09-SLICE-VINCULOS.md) | Relationships |
| [14-ANALISE-GAPS-ESPECIFICACAO](specs/ui/14-ANALISE-GAPS-ESPECIFICACAO.md) | Gap analysis |
| [15-SLICE-API-WIZARD-BACKEND](specs/ui/15-SLICE-API-WIZARD-BACKEND.md) | API wizard backend |
| [16-PLANO-IMPLEMENTACAO-API-EXISTENTE](specs/ui/16-PLANO-IMPLEMENTACAO-API-EXISTENTE.md) | Existing API implementation |
| [17-ESPECIFICACAO-LOOKUP-ANBIMA](specs/ui/17-ESPECIFICACAO-LOOKUP-ANBIMA.md) | ANBIMA lookup |
| [18-MAPEAMENTO-APIs-EXISTENTES](specs/ui/18-MAPEAMENTO-APIs-EXISTENTES.md) | Existing APIs mapping |

## Testing (`testing/`)

| Document | Description |
|----------|-------------|
| [BRAZILIAN_TEST_DATA](testing/BRAZILIAN_TEST_DATA.md) | Brazilian financial test data reference |
| [TEST_COVERAGE_SUMMARY](testing/TEST_COVERAGE_SUMMARY.md) | E2E test coverage summary |
| [E2E_HELPERS_GUIDE](testing/E2E_HELPERS_GUIDE.md) | E2E test helpers guide |

## ETL (`etl/`)

| Document | Description |
|----------|-------------|
| [README](etl/README.md) | ETL pipeline documentation |
| [b3_mapping](etl/b3_mapping.md) | B3 instruments mapping |
| [b3_type_mapping](etl/b3_type_mapping.md) | B3 instrument type mapping |

## Compliance (`compliance/`)

| Document | Description |
|----------|-------------|
| [ANGULAR_COMPLIANCE_REVIEW](compliance/ANGULAR_COMPLIANCE_REVIEW.md) | Angular 21 compliance review |
| [COMPLIANCE_QUICK_CHECK](compliance/COMPLIANCE_QUICK_CHECK.md) | Quick compliance check reference |

## Claude Code Instructions

For AI-assisted development guidance, see the `CLAUDE.md` files:

- [`/CLAUDE.md`](../CLAUDE.md) - Monorepo overview and commands
- [`/apps/core-ledger-ui/CLAUDE.md`](../apps/core-ledger-ui/CLAUDE.md) - Angular UI patterns
- [`/tools/etl/CLAUDE.md`](../tools/etl/CLAUDE.md) - Meltano ETL guidance
