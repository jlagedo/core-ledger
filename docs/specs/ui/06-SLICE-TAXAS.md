# SLICE 06: Etapa 5 - Taxas do Fundo

## Objetivo

Implementar a quinta etapa do wizard para configuração das taxas do fundo (administração, gestão, performance, etc.).

## Escopo

### Modelo de Dados - Taxa

Cada fundo pode ter múltiplas taxas. Cada taxa possui:

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `tipo_taxa` | enum | Sim | Tipo da taxa |
| `percentual` | decimal(8,6) | Sim | Percentual da taxa (ex: 0.015 = 1.5%) |
| `percentual_minimo` | decimal(8,6) | Não | Taxa mínima (taxas escalonadas) |
| `percentual_maximo` | decimal(8,6) | Não | Taxa máxima (taxas escalonadas) |
| `base_calculo` | enum | Sim | Base de cálculo |
| `forma_cobranca` | enum | Sim | Forma de cobrança |
| `data_inicio_vigencia` | date | Sim | Início da vigência |
| `data_fim_vigencia` | date | Não | Fim da vigência |
| `benchmark_id` | integer | Condicional | FK para Indexador (se performance) |
| `percentual_benchmark` | decimal(8,6) | Condicional | % do benchmark (se performance) |
| `possui_hurdle` | boolean | Condicional | Tem hurdle rate? |
| `possui_high_water_mark` | boolean | Condicional | Tem HWM? |
| `linha_dagua_global` | boolean | Não | Linha d'água global ou por cotista |
| `classe_id` | uuid | Não | FK para Classe (taxa específica da classe) |
| `ativo` | boolean | Sim | Indica se taxa está ativa |

---

### Valores dos Enums

**tipo_taxa:**
| Valor | Descrição |
|-------|-----------|
| `ADMINISTRACAO` | Taxa de Administração |
| `GESTAO` | Taxa de Gestão |
| `CUSTODIA` | Taxa de Custódia |
| `PERFORMANCE` | Taxa de Performance |
| `ENTRADA` | Taxa de Entrada (aplicação) |
| `SAIDA` | Taxa de Saída (resgate) |
| `DISTRIBUICAO` | Taxa de Distribuição |
| `CONSULTORIA` | Taxa de Consultoria (FIDCs) |
| `ESCRITURACAO` | Taxa de Escrituração |
| `ESTRUTURACAO` | Taxa de Estruturação (FIDCs/FIPs) |

**base_calculo:**
| Valor | Descrição |
|-------|-----------|
| `PL_MEDIO` | Patrimônio Líquido Médio |
| `PL_FINAL` | Patrimônio Líquido Final |
| `RENDIMENTO` | Rendimento do Período |
| `RENDIMENTO_ACIMA_BENCHMARK` | Rendimento acima do Benchmark |

**forma_cobranca:**
| Valor | Descrição |
|-------|-----------|
| `DIARIA` | Provisão Diária |
| `MENSAL` | Cobrança Mensal |
| `SEMESTRAL` | Cobrança Semestral |
| `ANUAL` | Cobrança Anual |
| `EVENTO` | Cobrança por Evento (ex: estruturação) |

---

## Requisitos Funcionais

### RF-01: Taxas Obrigatórias

- Taxa de ADMINISTRACAO é obrigatória
- Ao entrar na etapa, se não houver taxa de administração, adicionar automaticamente com valores default

### RF-02: Adicionar/Remover Taxas

- Interface de lista dinâmica
- Botão "Adicionar Taxa" cria nova linha
- Botão "Remover" em cada linha (exceto administração)
- Máximo de 10 taxas por fundo

### RF-03: Taxa de Performance - Campos Condicionais

Quando `tipo_taxa = PERFORMANCE`:
- Exibir campo `benchmark_id` (obrigatório)
- Exibir campo `percentual_benchmark` (default: 100%)
- Exibir checkbox `possui_hurdle`
- Exibir checkbox `possui_high_water_mark`

### RF-04: Busca de Indexadores

Para o campo benchmark_id:
- Implementar autocomplete/select com busca
- Endpoint:
  ```
  GET /api/v1/indexadores?busca={termo}
  ```
- Exibir: código e nome do indexador

### RF-05: Validação de Percentuais

- Taxa de administração: máximo 10% a.a.
- Taxa de gestão: máximo 5% a.a.
- Taxa de performance: máximo 50%
- Taxa de ingresso/saída: máximo 5%

### RF-06: Formato de Exibição

- Percentuais exibidos como "X.XX% a.a."
- Input permite digitação como decimal ou percentual

---

## Frontend

### Componente

**WizardStep5TaxasComponent**

### Layout Sugerido

```
┌─────────────────────────────────────────────────────────────────┐
│ TAXAS DO FUNDO                                    [+ Adicionar] │
├─────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ Tipo: [Administração ▼]  Percentual: [1.50 ]% a.a.         │ │
│ │ Base: [PL Médio ▼]       Cobrança: [Diária ▼]              │ │
│ │ Vigência: [01/02/2024] até [________]                  [🗑] │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ Tipo: [Performance ▼]    Percentual: [20.00]%              │ │
│ │ Base: [Rend. acima Bench ▼]  Cobrança: [Semestral ▼]       │ │
│ │ Benchmark: [CDI ▼] × [100]%                                │ │
│ │ [✓] High Water Mark    [✓] Hurdle Rate                     │ │
│ │ Vigência: [01/02/2024] até [________]                  [🗑] │ │
│ └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### FormArray

Usar `FormArray` do Angular para gerenciar lista dinâmica de taxas.

---

## Backend

### Entidade `FundoTaxa`

| Campo DB | Tipo | Constraints |
|----------|------|-------------|
| `id` | BIGINT | PK, auto-increment |
| `fundo_id` | UUID | FK |
| `tipo_taxa` | VARCHAR(20) | NOT NULL |
| `percentual` | DECIMAL(8,6) | NOT NULL |
| `base_calculo` | VARCHAR(30) | NOT NULL |
| `forma_cobranca` | VARCHAR(20) | NOT NULL |
| `data_inicio_vigencia` | DATE | NOT NULL |
| `data_fim_vigencia` | DATE | NULL |
| `benchmark_id` | INTEGER | FK indexador, NULL |
| `percentual_benchmark` | DECIMAL(5,4) | NULL |
| `possui_hurdle` | BOOLEAN | DEFAULT false |
| `possui_high_water_mark` | BOOLEAN | DEFAULT false |
| `ativo` | BOOLEAN | DEFAULT true |

### Validações Backend

- Pelo menos uma taxa de ADMINISTRACAO
- benchmark_id obrigatório se tipo = PERFORMANCE
- Percentuais dentro dos limites
- data_fim_vigencia >= data_inicio_vigencia (se informada)

---

## Critérios de Aceite

- [ ] Lista dinâmica de taxas funciona
- [ ] Taxa de administração é pré-adicionada
- [ ] Campos condicionais de performance aparecem/somem
- [ ] Autocomplete de indexadores funciona
- [ ] Validação de percentuais máximos
- [ ] Formatação de percentual correta
- [ ] Adicionar/remover taxas funciona
- [ ] Não permite remover taxa de administração
- [ ] Dados persistem ao navegar

---

## Dependências

- Slice 01: Infraestrutura base
- Feature de Indexadores deve existir (endpoint GET /api/v1/indexadores)

## Próximo Slice

→ `07-SLICE-PRAZOS.md`
