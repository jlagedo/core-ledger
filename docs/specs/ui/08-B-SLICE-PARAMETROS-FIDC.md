# SLICE 08-B: Etapa 7.1 - Parâmetros FIDC

## Objetivo

Implementar etapa condicional do wizard para configuração de parâmetros específicos de FIDCs. Esta etapa só é exibida quando `tipo_fundo` é `FIDC` ou `FIDC_NP`.

## Escopo

### Pré-condição

Esta etapa só aparece se:
- `tipo_fundo = FIDC` ou `tipo_fundo = FIDC_NP` (definido na Etapa 1)

### Modelo de Dados - Parâmetros FIDC

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `tipo_fidc` | enum | Sim | Tipo do FIDC |
| `tipo_recebiveis` | enum[] | Sim | Tipos de recebíveis aceitos (múltiplos) |
| `prazo_medio_carteira` | integer | Não | Prazo médio da carteira em dias |
| `indice_subordinacao_alvo` | decimal(8,4) | Não | Índice de subordinação alvo |
| `provisao_devedores_duvidosos` | decimal(8,4) | Não | % de PDD sobre carteira |
| `limite_concentracao_cedente` | decimal(8,4) | Não | Limite % por cedente |
| `limite_concentracao_sacado` | decimal(8,4) | Não | Limite % por sacado |
| `possui_coobrigacao` | boolean | Sim | Cedente possui coobrigação |
| `percentual_coobrigacao` | decimal(8,4) | Não | % de coobrigação |
| `permite_cessao_parcial` | boolean | Sim | Permite cessão parcial |
| `rating_minimo` | string(10) | Não | Rating mínimo exigido |
| `agencia_rating` | string(50) | Não | Agência de rating |
| `registradora_recebiveis` | enum | Não | Registradora utilizada |
| `conta_registradora` | string(50) | Não | Identificador na registradora |
| `integracao_registradora` | boolean | Sim | Integração automática com registradora |

---

### Valores dos Enums

**tipo_fidc:**
| Valor | Descrição |
|-------|-----------|
| `PADRONIZADO` | FIDC Padronizado |
| `NAO_PADRONIZADO` | FIDC Não Padronizado |

**tipo_recebiveis:**
| Valor | Descrição |
|-------|-----------|
| `DUPLICATAS` | Duplicatas mercantis |
| `CCB` | Cédula de Crédito Bancário |
| `CCI` | Cédula de Crédito Imobiliário |
| `CARTAO_CREDITO` | Recebíveis de cartão de crédito |
| `CHEQUES` | Cheques |
| `CONTRATOS_ALUGUEL` | Contratos de aluguel |
| `ENERGIA` | Recebíveis de energia |
| `FINANCIAMENTO_VEICULOS` | Financiamento de veículos |
| `CREDITO_CONSIGNADO` | Crédito consignado |
| `PRECATORIOS` | Precatórios |
| `CREDITOS_JUDICIAIS` | Créditos judiciais |
| `OUTROS` | Outros tipos |

**registradora_recebiveis:**
| Valor | Descrição |
|-------|-----------|
| `LAQUS` | Laqus Registradora |
| `CERC` | CERC |
| `TAG` | TAG Registradora |
| `B3` | B3 Registradora |

---

## Requisitos Funcionais

### RF-01: Visibilidade Condicional

- Esta etapa só é exibida se tipo_fundo = FIDC ou FIDC_NP
- Se não for FIDC, o wizard pula esta etapa automaticamente
- O stepper deve refletir a etapa condicional

### RF-02: Tipo de FIDC

- Se tipo_fundo = FIDC, padrão = PADRONIZADO
- Se tipo_fundo = FIDC_NP, padrão = NAO_PADRONIZADO
- Select com as duas opções

### RF-03: Seleção Múltipla de Recebíveis

- Permitir seleção de múltiplos tipos de recebíveis
- Interface de checkboxes ou multi-select
- Pelo menos um tipo deve ser selecionado

### RF-04: Coobrigação

- Se `possui_coobrigacao = true`, exibir campo `percentual_coobrigacao`
- Percentual entre 0 e 100%
- Tooltip: "Percentual de garantia prestada pelo cedente"

### RF-05: Limites de Concentração

- `limite_concentracao_cedente`: % máximo de exposição a um único cedente
- `limite_concentracao_sacado`: % máximo de exposição a um único sacado
- Valores entre 0 e 100%
- Sugerir valores padrão de mercado (20% cedente, 5% sacado)

### RF-06: Rating

- Campo opcional
- Se preenchido `rating_minimo`, `agencia_rating` torna-se obrigatório
- Sugerir agências: Moody's, S&P, Fitch, Austin, SR Rating

### RF-07: Integração com Registradora

- Se `registradora_recebiveis` preenchida, exibir:
  - Campo `conta_registradora`
  - Checkbox `integracao_registradora`
- Tooltip sobre benefícios da integração automática

### RF-08: Índice de Subordinação

- Campo informativo
- Tooltip: "Razão entre classes subordinadas e PL total"
- Valor alvo é referência, índice real será calculado após cotizações

---

## Frontend

### Componente

**WizardStep7BParametrosFidcComponent**

### Layout Sugerido

```
┌─────────────────────────────────────────────────────────────────┐
│ PARÂMETROS DO FIDC                                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│ Tipo de FIDC: [Padronizado ▼]                                  │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ TIPOS DE RECEBÍVEIS                                         │ │
│ │ [✓] Duplicatas       [ ] Cheques         [ ] Energia       │ │
│ │ [✓] CCB              [ ] Cartão Crédito  [ ] Precatórios   │ │
│ │ [ ] CCI              [ ] Fin. Veículos   [ ] Créditos Jud. │ │
│ │ [ ] Contratos Aluguel [ ] Créd. Consignado [ ] Outros      │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ──────────────────────────────────────────────────────────────  │
│                                                                 │
│ PARÂMETROS DA CARTEIRA                                         │
│                                                                 │
│ Prazo médio da carteira: [360  ] dias                          │
│ Índice de subordinação alvo: [25.00]%                          │
│ Provisão dev. duvidosos (PDD): [2.00]%                         │
│                                                                 │
│ ──────────────────────────────────────────────────────────────  │
│                                                                 │
│ LIMITES DE CONCENTRAÇÃO                                        │
│                                                                 │
│ Limite por cedente: [20.00]%                                   │
│ Limite por sacado: [5.00]%                                     │
│                                                                 │
│ ──────────────────────────────────────────────────────────────  │
│                                                                 │
│ COOBRIGAÇÃO                                                    │
│                                                                 │
│ [✓] Cedente possui coobrigação                                 │
│ Percentual de coobrigação: [10.00]%                            │
│                                                                 │
│ [✓] Permite cessão parcial                                     │
│                                                                 │
│ ──────────────────────────────────────────────────────────────  │
│                                                                 │
│ RATING (opcional)                                              │
│                                                                 │
│ Rating mínimo: [BBB    ]  Agência: [S&P ▼]                    │
│                                                                 │
│ ──────────────────────────────────────────────────────────────  │
│                                                                 │
│ REGISTRADORA DE RECEBÍVEIS                                     │
│                                                                 │
│ Registradora: [CERC ▼]                                         │
│ Conta na registradora: [FIDC-2024-001    ]                    │
│ [✓] Integração automática com registradora                    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Backend

### Entidade `FidcParametros`

| Campo DB | Tipo | Constraints |
|----------|------|-------------|
| `id` | BIGINT | PK, auto-increment |
| `fundo_id` | UUID | FK, unique |
| `tipo_fidc` | VARCHAR(20) | NOT NULL |
| `tipo_recebiveis` | VARCHAR(30)[] | NOT NULL |
| `prazo_medio_carteira` | SMALLINT | NULL |
| `indice_subordinacao_alvo` | DECIMAL(8,4) | NULL |
| `provisao_devedores_duvidosos` | DECIMAL(8,4) | NULL |
| `limite_concentracao_cedente` | DECIMAL(8,4) | NULL |
| `limite_concentracao_sacado` | DECIMAL(8,4) | NULL |
| `possui_coobrigacao` | BOOLEAN | NOT NULL DEFAULT false |
| `percentual_coobrigacao` | DECIMAL(8,4) | NULL |
| `permite_cessao_parcial` | BOOLEAN | NOT NULL DEFAULT true |
| `rating_minimo` | VARCHAR(10) | NULL |
| `agencia_rating` | VARCHAR(50) | NULL |
| `registradora_recebiveis` | VARCHAR(10) | NULL |
| `conta_registradora` | VARCHAR(50) | NULL |
| `integracao_registradora` | BOOLEAN | NOT NULL DEFAULT false |
| `created_at` | TIMESTAMP | NOT NULL |
| `updated_at` | TIMESTAMP | NOT NULL |

### Validações Backend

- `tipo_recebiveis` deve ter pelo menos um valor
- Se `possui_coobrigacao = true`, `percentual_coobrigacao` deve estar entre 0 e 100
- Se `rating_minimo` preenchido, `agencia_rating` é obrigatório
- Limites de concentração entre 0 e 100

---

## Critérios de Aceite

- [ ] Etapa só aparece para fundos FIDC/FIDC_NP
- [ ] Tipo FIDC é pré-selecionado baseado em tipo_fundo
- [ ] Multi-select de recebíveis funciona
- [ ] Pelo menos um recebível deve ser selecionado
- [ ] Campo coobrigação aparece/some conforme checkbox
- [ ] Rating exige agência quando preenchido
- [ ] Campos de registradora aparecem/somem conforme seleção
- [ ] Dados persistem ao navegar
- [ ] Stepper reflete etapa condicional

---

## Dependências

- Slice 01: Infraestrutura base
- Slice 02: Etapa 1 (tipo_fundo para determinar visibilidade)

## Ordem no Wizard

Esta etapa fica entre:
- Etapa 7 (Classes CVM 175) - Slice 08
- Etapa 8 (Vínculos) - Slice 09

Para FIDCs, a ordem é:
1. Identificação
2. Classificação
3. Características
4. Parâmetros Cota
5. Taxas
6. Prazos
7. Classes CVM 175
8. **Parâmetros FIDC** ← Esta etapa
9. Vínculos
10. Documentos
11. Revisão

## Próximo Slice

→ `09-SLICE-VINCULOS.md`
