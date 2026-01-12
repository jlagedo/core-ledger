# SLICE 04: Etapa 3 - Características

## Objetivo

Implementar a terceira etapa do wizard para definição das características operacionais do fundo.

## Escopo

### Campos do Formulário

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `condominio` | enum | Sim | Tipo de condomínio |
| `prazo` | enum | Sim | Prazo do fundo |
| `data_encerramento` | date | Condicional | Obrigatório se prazo DETERMINADO |
| `exclusivo` | boolean | Sim | Indica se é fundo exclusivo |
| `reservado` | boolean | Sim | Indica se é fundo reservado |
| `permite_alavancagem` | boolean | Sim | Indica se permite alavancagem |
| `limite_alavancagem` | decimal(8,4) | Condicional | Obrigatório se permite_alavancagem |
| `aceita_cripto` | boolean | Sim | Indica se aceita criptoativos |
| `percentual_exterior` | decimal(5,2) | Não | % permitido em ativos exterior |

---

### Valores do Enum `condominio`

| Valor | Descrição |
|-------|-----------|
| `ABERTO` | Condomínio Aberto |
| `FECHADO` | Condomínio Fechado |

### Valores do Enum `prazo`

| Valor | Descrição |
|-------|-----------|
| `INDETERMINADO` | Prazo Indeterminado |
| `DETERMINADO` | Prazo Determinado |

---

## Requisitos Funcionais

### RF-01: Campos Condicionais

**Data de encerramento:**
- Visível apenas quando `prazo = DETERMINADO`
- Obrigatório neste caso
- Deve ser data futura

**Limite de alavancagem:**
- Visível apenas quando `permite_alavancagem = true`
- Obrigatório neste caso
- Valor mínimo: 1.01 (101%)
- Valor máximo: 10.00 (1000%)
- Exibir como percentual (ex: 200% = 2.0)

### RF-02: Regras por Tipo de Fundo

| Tipo Fundo | Condomínio Padrão | Alavancagem Permitida |
|------------|-------------------|----------------------|
| FIDC | FECHADO | Não |
| FIP | FECHADO | Não |
| FII | FECHADO | Não |
| FI/FIC | Qualquer | Sim |

- Pré-selecionar condomínio baseado no tipo
- Alertar se seleção divergir do padrão

### RF-03: Validação de Criptoativos

- Se `aceita_cripto = true`, exibir informativo sobre CVM 175
- Se classificação CVM não permitir cripto (ex: Renda Fixa pura), alertar

### RF-04: Percentual no Exterior

- Valor entre 0 e 100
- Se classificação é "Cambial", valor mínimo 80%
- Se tipo não permite exterior (ex: FII), desabilitar campo

### RF-05: Fundos Exclusivos e Reservados

- Se `exclusivo = true`, campo `reservado` fica `true` automaticamente
- Exibir nota: "Fundos exclusivos são automaticamente reservados"

---

## Frontend

### Componente

**WizardStep3CaracteristicasComponent**

### Layout Sugerido

Organizar em grupos visuais:

```
┌─────────────────────────────────────────┐
│ ESTRUTURA DO FUNDO                      │
│ [Condomínio ▼]  [Prazo ▼]              │
│ [Data Encerramento] (condicional)       │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ TIPO DE INVESTIDOR                      │
│ [✓] Fundo Exclusivo                     │
│ [✓] Fundo Reservado                     │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ ESTRATÉGIA DE INVESTIMENTO              │
│ [✓] Permite Alavancagem                 │
│ [Limite: ___% ] (condicional)           │
│ [✓] Aceita Criptoativos                 │
│ [% Exterior: ___]                       │
└─────────────────────────────────────────┘
```

### Máscaras e Formatação

| Campo | Formato |
|-------|---------|
| `limite_alavancagem` | Percentual com 2 decimais (ex: 200.00%) |
| `percentual_exterior` | Percentual com 2 decimais (ex: 20.00%) |
| `data_encerramento` | dd/MM/yyyy |

---

## Backend

### Validação de Regras de Negócio

O backend deve validar:

1. `data_encerramento` obrigatório se `prazo = DETERMINADO`
2. `limite_alavancagem` obrigatório se `permite_alavancagem = true`
3. Consistência entre tipo de fundo e condomínio
4. Ranges de valores percentuais

---

## Critérios de Aceite

- [ ] Todos os campos renderizam corretamente
- [ ] Campo data_encerramento aparece/some conforme prazo
- [ ] Campo limite_alavancagem aparece/some conforme checkbox
- [ ] Checkbox exclusivo marca automaticamente reservado
- [ ] Validações de range funcionam
- [ ] Valores default são aplicados por tipo de fundo
- [ ] Alertas de divergência são exibidos quando aplicável
- [ ] Dados persistem ao navegar

---

## Dependências

- Slice 01: Infraestrutura base
- Slice 02: Etapa 1 (tipo_fundo)
- Slice 03: Etapa 2 (classificacao_cvm para validação de cripto)

## Próximo Slice

→ `05-SLICE-PARAMETROS-COTA.md`
