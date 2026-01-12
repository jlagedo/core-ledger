# SLICE 08: Etapa 7 - Classes CVM 175

## Objetivo

Implementar a sétima etapa do wizard para configuração de classes e subclasses conforme CVM 175 (estrutura multiclasse). Esta etapa é **opcional** para fundos que não utilizam estrutura multiclasse.

## Escopo

### Modelo de Dados - Classe

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `codigo_classe` | string(20) | Sim | Código identificador |
| `nome_classe` | string(100) | Sim | Nome da classe |
| `cnpj_classe` | string(14) | Não | CNPJ específico da classe |
| `classe_pai_id` | uuid | Não | FK para classe pai (subclasses) |
| `nivel` | integer | Sim | Nível hierarquia (1=classe, 2=subclasse) |
| `publico_alvo` | enum | Sim | Público alvo da classe |
| `tipo_classe_fidc` | enum | Condicional | Tipo para FIDCs |
| `ordem_subordinacao` | integer | Não | Ordem de subordinação |
| `rentabilidade_alvo` | decimal(8,6) | Não | Rentabilidade alvo % |
| `indice_subordinacao_minimo` | decimal(8,4) | Não | Índice subordinação mínimo |
| `valor_minimo_aplicacao` | decimal(18,2) | Não | Valor mínimo aplicação |
| `valor_minimo_permanencia` | decimal(18,2) | Não | Valor mínimo permanência |
| `responsabilidade_limitada` | boolean | Sim | Responsabilidade limitada |
| `segregacao_patrimonial` | boolean | Sim | Segregação patrimonial |
| `taxa_administracao` | decimal(8,6) | Não | Taxa específica (se diferente do fundo) |
| `taxa_gestao` | decimal(8,6) | Não | Taxa gestão específica |
| `taxa_performance` | decimal(8,6) | Não | Taxa performance específica |
| `benchmark_id` | integer | Não | FK para Indexador (benchmark específico) |
| `permite_resgate_antecipado` | boolean | Sim | Permite resgate antes vencimento (FIDC) |
| `data_inicio` | date | Sim | Data de início da classe |
| `data_encerramento` | date | Não | Data de encerramento |
| `motivo_encerramento` | string(200) | Não | Motivo do encerramento |
| `ativo` | boolean | Sim | Indica se classe está ativa |

---

### Valores do Enum `tipo_classe_fidc`

| Valor | Descrição |
|-------|-----------|
| `SENIOR` | Classe Sênior |
| `MEZANINO` | Classe Mezanino |
| `SUBORDINADA` | Classe Subordinada |
| `SUBORDINADA_JUNIOR` | Classe Subordinada Júnior |
| `UNICA` | Classe Única (sem subordinação) |

---

## Requisitos Funcionais

### RF-01: Etapa Opcional

- Exibir opção: "Este fundo possui estrutura multiclasse?"
- Se NÃO: mostrar mensagem e permitir avançar
- Se SIM: exibir formulário de classes

### RF-02: Obrigatoriedade para FIDCs

- Se tipo_fundo = FIDC ou FIDC_NP, a criação de classes é **obrigatória**
- Pré-criar classe SENIOR como default

### RF-03: Lista de Classes

- Interface de lista dinâmica (similar a taxas)
- Botão "Adicionar Classe"
- Mínimo 1 classe se estrutura multiclasse ativada
- Máximo 10 classes por fundo

### RF-04: Campos Específicos FIDC

Quando tipo_fundo é FIDC:
- Campo `tipo_classe_fidc` obrigatório
- Campo `ordem_subordinacao` obrigatório
- Ordem: SENIOR = 1, MEZANINO = 2, SUBORDINADA = 3

### RF-05: Subordinação

- Classes com menor ordem têm prioridade de recebimento
- Validar que não há gaps na ordem (1, 2, 3... não 1, 3)
- Validar que SUBORDINADA sempre tem maior ordem

### RF-06: Responsabilidade Limitada

- Default: true para classes SENIOR e MEZANINO
- Default: false para classe SUBORDINADA
- Permitir alteração com aviso

### RF-07: Taxa Específica por Classe

- Campo opcional
- Se informado, sobrepõe taxa do fundo para esta classe
- Validar que não excede taxa geral do fundo

### RF-08: Índice de Subordinação (Preview)

Se houver múltiplas classes, calcular e exibir preview:

```
Índice de Subordinação = PL(Subordinada + Mezanino) / PL(Total)
```

Neste slice, apenas exibir fórmula explicativa (cálculo real será feito após cotizações).

---

## Frontend

### Componente

**WizardStep7ClassesComponent**

### Layout Sugerido

```
┌─────────────────────────────────────────────────────────────────┐
│ ESTRUTURA DE CLASSES (CVM 175)                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│ Este fundo possui estrutura multiclasse?                       │
│ ( ) Não - Classe única                                         │
│ (●) Sim - Múltiplas classes                                    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ CLASSES DO FUNDO                                  [+ Adicionar] │
├─────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ Código: [SR     ]  Nome: [Classe Sênior              ]     │ │
│ │ Tipo: [Sênior ▼]   Ordem: [1]                              │ │
│ │ Rentabilidade alvo: [12.00]% a.a.                          │ │
│ │ Valor mínimo: R$ [25.000,00   ]                            │ │
│ │ [✓] Responsabilidade Limitada  [✓] Segregação Patrimonial  │ │
│ │ Público alvo: [Qualificado ▼]                          [🗑] │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ Código: [SUB    ]  Nome: [Classe Subordinada         ]     │ │
│ │ Tipo: [Subordinada ▼]  Ordem: [2]                          │ │
│ │ [✗] Responsabilidade Limitada  [✓] Segregação Patrimonial  │ │
│ │ Público alvo: [Profissional ▼]                         [🗑] │ │
│ └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ ℹ️ Índice de Subordinação                                       │
│ O índice será calculado após cotizações das classes:           │
│ IS = (PL Subordinada + PL Mezanino) / PL Total                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Backend

### Entidade `FundoClasse`

| Campo DB | Tipo | Constraints |
|----------|------|-------------|
| `id` | UUID | PK |
| `fundo_id` | UUID | FK |
| `codigo_classe` | VARCHAR(20) | NOT NULL, unique per fund |
| `nome_classe` | VARCHAR(100) | NOT NULL |
| `tipo_classe_fidc` | VARCHAR(20) | NULL |
| `ordem_subordinacao` | SMALLINT | NULL |
| `rentabilidade_alvo` | DECIMAL(8,4) | NULL |
| `valor_minimo_aplicacao` | DECIMAL(18,2) | NULL |
| `responsabilidade_limitada` | BOOLEAN | DEFAULT true |
| `segregacao_patrimonial` | BOOLEAN | DEFAULT true |
| `publico_alvo_classe` | VARCHAR(20) | NULL |
| `taxa_administracao_classe` | DECIMAL(8,6) | NULL |
| `ativo` | BOOLEAN | DEFAULT true |

### Validações Backend

- codigo_classe único por fundo
- Se FIDC: tipo_classe_fidc obrigatório
- ordem_subordinacao sem gaps
- SUBORDINADA deve ter maior ordem

---

## Critérios de Aceite

- [ ] Toggle multiclasse funciona
- [ ] Lista de classes renderiza
- [ ] Adicionar/remover classes funciona
- [ ] Para FIDC, classe é obrigatória
- [ ] Campos específicos FIDC aparecem
- [ ] Validação de ordem de subordinação
- [ ] Defaults aplicados por tipo de classe
- [ ] Código único por fundo é validado
- [ ] Dados persistem ao navegar

---

## Dependências

- Slice 01: Infraestrutura base
- Slice 02: Etapa 1 (tipo_fundo para determinar obrigatoriedade)

## Próximo Slice

→ `09-SLICE-VINCULOS.md`
