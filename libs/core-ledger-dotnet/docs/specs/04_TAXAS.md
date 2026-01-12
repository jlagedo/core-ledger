# Etapa 04 - Taxas do Fundo

## Objetivo
Implementar estrutura de taxas (administração, gestão, performance, custódia).

## Localização
- Entidade: `FundAccounting.Domain/Cadastros/Entities/FundoTaxa.cs`

## Modelo de Dados

### Tabela: cadastros.fundo_taxa

| Coluna | Tipo | Null | Descrição |
|--------|------|------|-----------|
| id | BIGINT | N | PK SERIAL |
| fundo_id | UUID | N | FK fundo |
| classe_id | UUID | Y | FK classe (taxa por classe) |
| tipo_taxa | VARCHAR(20) | N | Enum TipoTaxa |
| percentual | DECIMAL(8,4) | N | % a.a. |
| base_calculo | VARCHAR(20) | N | Enum BaseCalculoTaxa |
| periodicidade_provisao | VARCHAR(20) | N | DIARIA/MENSAL |
| periodicidade_pagamento | VARCHAR(20) | N | MENSAL/TRIMESTRAL/SEMESTRAL |
| dia_pagamento | INTEGER | Y | Dia do mês (1-28) |
| valor_minimo | DECIMAL(18,2) | Y | Valor mínimo mensal |
| valor_maximo | DECIMAL(18,2) | Y | Cap de taxa |
| data_inicio_vigencia | DATE | N | |
| data_fim_vigencia | DATE | Y | |
| ativa | BOOLEAN | N | Default: true |
| created_at | TIMESTAMP | N | |
| updated_at | TIMESTAMP | Y | |

### Tabela: cadastros.fundo_taxa_performance

| Coluna | Tipo | Null | Descrição |
|--------|------|------|-----------|
| id | BIGINT | N | PK SERIAL |
| fundo_taxa_id | BIGINT | N | FK taxa |
| indexador_id | INTEGER | N | FK indexador (benchmark) |
| percentual_benchmark | DECIMAL(8,4) | N | Ex: 100% CDI |
| metodo_calculo | VARCHAR(30) | N | COTA_AJUSTADA/HIGH_WATER_MARK |
| linha_dagua | BOOLEAN | N | Default: true |
| periodicidade_cristalizacao | VARCHAR(20) | N | SEMESTRAL/ANUAL |
| mes_cristalizacao | INTEGER | Y | 1-12 |
| created_at | TIMESTAMP | N | |

### Índices
```sql
CREATE INDEX ix_taxa_fundo ON cadastros.fundo_taxa(fundo_id);
CREATE INDEX ix_taxa_classe ON cadastros.fundo_taxa(classe_id);
CREATE INDEX ix_taxa_tipo ON cadastros.fundo_taxa(tipo_taxa);
```

## Regras de Negócio
1. Taxa de administração: obrigatória para todos fundos
2. Taxa de performance: requer benchmark (indexador)
3. Apenas uma taxa ativa por tipo/fundo/classe
4. data_fim_vigencia preenchida = taxa histórica

## DTOs
- TaxaCreateDto
- TaxaPerformanceCreateDto
- TaxaUpdateDto
- TaxaResponseDto

## Critérios de Aceite
- [x] Migration para tabelas de taxa
- [x] FK para indexador (já existente)
- [x] Validação: taxa duplicada ativa
- [x] Validação: performance requer benchmark
