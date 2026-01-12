# SLICE 05: Etapa 4 - Parâmetros de Cota

## Objetivo

Implementar a quarta etapa do wizard para configuração dos parâmetros de cálculo e exibição da cota do fundo.

## Escopo

### Campos do Formulário

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `casas_decimais_cota` | integer | Sim | Precisão da cota (padrão: 8) |
| `casas_decimais_quantidade` | integer | Sim | Precisão da quantidade (padrão: 6) |
| `casas_decimais_pl` | integer | Sim | Precisão do PL (padrão: 2) |
| `tipo_cota` | enum | Sim | Tipo de cálculo da cota |
| `horario_corte` | time | Sim | Horário de corte para movimentações |
| `cota_inicial` | decimal(20,8) | Sim | Valor da cota na constituição |
| `data_cota_inicial` | date | Sim | Data da primeira cota |
| `fuso_horario` | string(50) | Sim | Fuso horário (padrão: America/Sao_Paulo) |
| `permite_cota_estimada` | boolean | Sim | Permite divulgação de cota estimada |

---

### Valores do Enum `tipo_cota`

| Valor | Descrição |
|-------|-----------|
| `FECHAMENTO` | Cota calculada no fechamento do dia |
| `ABERTURA` | Cota de abertura (estimada) |

---

## Requisitos Funcionais

### RF-01: Casas Decimais

- Casas decimais cota: padrão 8, range 4-10
- Casas decimais quantidade: padrão 6, range 4-8
- Casas decimais PL: padrão 2, range 2-4
- Exibir preview do formato: "Ex: 1,23456789"

### RF-02: Horário de Corte

- Formato: HH:mm
- Range: 00:00 a 23:59
- Valor padrão: 14:00
- Tooltip explicativo: "Movimentações recebidas após este horário serão processadas no dia útil seguinte"

### RF-03: Valor e Data da Cota Inicial

- Valor padrão sugerido: R$ 1,00 ou R$ 1.000,00
- Mínimo: R$ 0,01
- Máximo: R$ 1.000.000,00
- Data cota inicial: padrão = data_inicio_atividade do fundo
- Exibir com formatação monetária

### RF-04: Fuso Horário

- Valor padrão: America/Sao_Paulo
- Opções: America/Sao_Paulo, America/New_York, Europe/London (para fundos com ativos no exterior)
- Select com busca

### RF-05: Cota Estimada

- Checkbox para habilitar/desabilitar
- Default: false
- Tooltip: "Permite divulgar cota estimada antes do fechamento oficial"

---

## Frontend

### Componente

**WizardStep4ParametrosCotaComponent**

### Layout Sugerido

```
┌─────────────────────────────────────────────────┐
│ PRECISÃO DE CÁLCULO                             │
│                                                 │
│ Casas decimais da cota     [6 ▼]               │
│ Preview: 1,234567                               │
│                                                 │
│ Casas decimais quantidade  [6 ▼]               │
│ Preview: 1.000,234567 cotas                     │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│ VALOR INICIAL                                   │
│                                                 │
│ Valor da cota inicial  [R$ 1,00        ]       │
│                                                 │
│ ℹ️ Valor usado como base no início do fundo     │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│ METODOLOGIA                                     │
│                                                 │
│ Tipo de cota           [Fechamento (D+0) ▼]    │
│ Frequência de cálculo  [Diária ▼]              │
│ Horário de corte       [14:00]                 │
└─────────────────────────────────────────────────┘
```

### Máscaras e Validação

| Campo | Máscara/Formato |
|-------|-----------------|
| `valor_cota_inicial` | Moeda BRL: R$ X.XXX,XXXXXXXX |
| `horario_corte` | HH:mm (24h) |

---

## Backend

### Entidade `ParametrosCota`

Esta entidade tem relação 1:1 com Fundo.

| Campo DB | Tipo | Constraints |
|----------|------|-------------|
| `id` | BIGINT | PK, auto-increment |
| `fundo_id` | UUID | FK, unique |
| `casas_decimais_cota` | SMALLINT | CHECK 4-8 |
| `casas_decimais_quantidade` | SMALLINT | CHECK 4-8 |
| `horario_corte` | TIME | NOT NULL |
| `valor_cota_inicial` | DECIMAL(18,8) | NOT NULL |
| `tipo_cota` | VARCHAR(20) | NOT NULL |
| `frequencia_calculo` | VARCHAR(20) | NOT NULL |

### Validações Backend

- Range de casas decimais: 4 a 8
- valor_cota_inicial > 0
- horario_corte formato válido

---

## Critérios de Aceite

- [ ] Todos os campos renderizam corretamente
- [ ] Selects de casas decimais têm opções 4-8
- [ ] Preview de formato atualiza em tempo real
- [ ] Input de hora funciona corretamente
- [ ] Valor monetário aceita decimais conforme precisão
- [ ] Valores padrão são aplicados por tipo/classificação
- [ ] Validações de range bloqueiam valores inválidos
- [ ] Dados persistem ao navegar

---

## Dependências

- Slice 01: Infraestrutura base
- Slice 03: Etapa 2 (classificacao_cvm para tipo_cota padrão)

## Próximo Slice

→ `06-SLICE-TAXAS.md`
