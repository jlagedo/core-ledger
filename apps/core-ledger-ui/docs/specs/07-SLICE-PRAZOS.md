# SLICE 07: Etapa 6 - Prazos Operacionais

## Objetivo

Implementar a sexta etapa do wizard para configuração dos prazos de aplicação e resgate do fundo.

## Escopo

### Modelo de Dados - Prazo

Cada fundo pode ter múltiplos prazos (aplicação e resgate). Cada prazo possui:

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `tipo_operacao` | enum | Sim | Aplicação ou Resgate |
| `prazo_cotizacao` | integer | Sim | D+X para cotização |
| `prazo_liquidacao` | integer | Sim | D+X para liquidação |
| `tipo_calendario` | string(20) | Sim | Praça do calendário para D+X |
| `valor_minimo_inicial` | decimal(18,2) | Não | Valor mínimo primeira aplicação |
| `valor_minimo_adicional` | decimal(18,2) | Não | Valor mínimo aplicação adicional |
| `valor_minimo_resgate` | decimal(18,2) | Não | Valor mínimo de resgate |
| `valor_minimo_permanencia` | decimal(18,2) | Não | Valor mínimo de permanência |
| `prazo_carencia_dias` | integer | Não | Carência para resgate (dias corridos) |
| `permite_resgate_total` | boolean | Sim | Permite zeragem da posição |
| `permite_resgate_programado` | boolean | Sim | Permite agendamento de resgate |
| `prazo_maximo_programacao` | integer | Não | Máximo dias úteis para agendar |
| `classe_id` | uuid | Não | FK para Classe (prazo específico) |
| `ativo` | boolean | Sim | Indica se prazo está ativo |

---

### Valores dos Enums

**tipo_operacao:**
| Valor | Descrição |
|-------|-----------|
| `APLICACAO` | Prazos para Aplicação |
| `RESGATE` | Prazos para Resgate |
| `RESGATE_CRISE` | Prazos para Resgate em cenário de crise |

**tipo_calendario:**
| Valor | Descrição |
|-------|-----------|
| `NACIONAL` | Calendário nacional (B3/ANBIMA) |
| `SAO_PAULO` | Praça de São Paulo |
| `RIO_DE_JANEIRO` | Praça do Rio de Janeiro |
| `EXTERIOR_EUA` | Calendário EUA |
| `EXTERIOR_EUR` | Calendário Europa |

---

## Requisitos Funcionais

### RF-01: Prazos Obrigatórios

- Prazo de APLICACAO é obrigatório
- Prazo de RESGATE é obrigatório
- Ao entrar na etapa, pré-criar os dois prazos com valores default

### RF-02: Valores Default por Tipo de Fundo

| Tipo Fundo | Cotização Aplicação | Liquidação Aplicação | Cotização Resgate | Liquidação Resgate |
|------------|--------------------|--------------------|-------------------|-------------------|
| RF | D+0 | D+0 | D+0 | D+1 |
| Ações | D+0 | D+0 | D+1 | D+2 |
| Multimercado | D+0 | D+0 | D+1 | D+1 |
| FII | D+0 | D+0 | D+30 | D+2 |
| FIDC | D+1 | D+1 | D+30 | D+30 |

### RF-03: Notação D+X

- Exibir os campos como "D+" seguido do número
- Input permite apenas números não negativos
- Validar: prazo_liquidacao >= prazo_cotizacao

### RF-04: Tipo de Calendário

- Padrão: NACIONAL
- Permite seleção de praça específica
- Para fundos com ativos no exterior, considerar calendário combinado

### RF-05: Carência

- Campo visível apenas para tipo RESGATE
- Se informado, validar que é número positivo
- Tooltip: "Período mínimo de permanência no fundo (dias corridos)"

### RF-06: Valores Mínimos

- `valor_minimo_inicial`: Primeira aplicação
- `valor_minimo_adicional`: Aplicações subsequentes (default = valor_minimo_inicial)
- `valor_minimo_resgate`: Valor mínimo por operação de resgate
- `valor_minimo_permanencia`: Saldo mínimo que deve permanecer
- Todos opcionais, formatação monetária (R$)

### RF-07: Resgate Total e Programado

- `permite_resgate_total`: Checkbox, default true
- Se false, exibir nota: "Cotista não poderá zerar posição"
- `permite_resgate_programado`: Checkbox, default false
- Se true, exibir campo `prazo_maximo_programacao`

---

## Frontend

### Componente

**WizardStep6PrazosComponent**

### Layout Sugerido

```
┌─────────────────────────────────────────────────────────────────┐
│ PRAZO DE APLICAÇÃO                                              │
├─────────────────────────────────────────────────────────────────┤
│ Cotização: D+ [0 ]  (Dias [Úteis ▼])                           │
│ Liquidação: D+ [0 ]                                             │
│                                                                 │
│ Valor mínimo: R$ [1.000,00    ]                                │
│ Valor máximo: R$ [____________]                                │
│                                                                 │
│ Horário limite específico: [    ] (opcional)                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ PRAZO DE RESGATE                                                │
├─────────────────────────────────────────────────────────────────┤
│ Cotização: D+ [1 ]  (Dias [Úteis ▼])                           │
│ Liquidação: D+ [2 ]                                             │
│                                                                 │
│ Carência: [   ] dias (opcional)                                │
│                                                                 │
│ Valor mínimo: R$ [1.000,00    ]                                │
│ Valor máximo: R$ [____________]                                │
│                                                                 │
│ [✓] Permite resgate parcial                                    │
│                                                                 │
│ Horário limite específico: [    ] (opcional)                   │
└─────────────────────────────────────────────────────────────────┘
```

### Validações Visuais

- Destaque vermelho se dias_liquidacao < dias_cotizacao
- Destaque se valor_maximo < valor_minimo

---

## Backend

### Entidade `FundoPrazo`

| Campo DB | Tipo | Constraints |
|----------|------|-------------|
| `id` | BIGINT | PK, auto-increment |
| `fundo_id` | UUID | FK |
| `tipo_prazo` | VARCHAR(20) | NOT NULL |
| `dias_cotizacao` | SMALLINT | NOT NULL, >= 0 |
| `dias_liquidacao` | SMALLINT | NOT NULL, >= 0 |
| `tipo_dia` | VARCHAR(10) | NOT NULL |
| `horario_limite` | TIME | NULL |
| `valor_minimo` | DECIMAL(18,2) | NULL |
| `valor_maximo` | DECIMAL(18,2) | NULL |
| `carencia_dias` | SMALLINT | NULL |
| `permite_parcial` | BOOLEAN | DEFAULT true |
| `ativo` | BOOLEAN | DEFAULT true |

### Validações Backend

- Obrigatório: um prazo de APLICACAO e um de RESGATE
- dias_liquidacao >= dias_cotizacao
- valor_maximo >= valor_minimo (se ambos informados)
- carencia_dias >= 0 (se informado)

---

## Critérios de Aceite

- [ ] Seções de aplicação e resgate renderizam
- [ ] Valores default aplicados por tipo de fundo
- [ ] Inputs D+X funcionam corretamente
- [ ] Validação dias_liquidacao >= dias_cotizacao
- [ ] Campo carência aparece apenas em resgate
- [ ] Valores monetários formatados
- [ ] Checkbox resgate parcial funciona
- [ ] Dados persistem ao navegar

---

## Dependências

- Slice 01: Infraestrutura base
- Slice 02: Etapa 1 (tipo_fundo para defaults)
- Slice 03: Etapa 2 (classificacao_cvm para defaults)

## Próximo Slice

→ `08-SLICE-CLASSES-CVM175.md`
