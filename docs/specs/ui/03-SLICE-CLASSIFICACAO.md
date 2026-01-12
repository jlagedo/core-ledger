# SLICE 03: Etapa 2 - Classificação

## Objetivo

Implementar a segunda etapa do wizard para definição das classificações regulatórias e tributárias do fundo.

## Escopo

### Campos do Formulário

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `classificacao_cvm` | enum | Sim | Classificação conforme CVM 175 |
| `classificacao_anbima` | string(50) | Não | Classificação ANBIMA |
| `codigo_anbima` | string(20) | Não | Código ANBIMA do fundo |
| `publico_alvo` | enum | Sim | Público alvo do fundo |
| `tributacao` | enum | Sim | Regime de tributação |

---

### Valores do Enum `classificacao_cvm`

| Valor | Descrição |
|-------|-----------|
| `RENDA_FIXA` | Renda Fixa |
| `ACOES` | Ações |
| `MULTIMERCADO` | Multimercado |
| `CAMBIAL` | Cambial |
| `FIDC` | Direitos Creditórios |
| `FIP` | Participações |
| `FII` | Imobiliário |
| `FIAGRO` | Cadeias Agroindustriais |
| `INFRA` | Infraestrutura |
| `ETF_RF` | ETF Renda Fixa |
| `ETF_RV` | ETF Renda Variável |

### Valores do Enum `publico_alvo`

| Valor | Descrição |
|-------|-----------|
| `GERAL` | Investidores em Geral |
| `QUALIFICADO` | Investidores Qualificados |
| `PROFISSIONAL` | Investidores Profissionais |

### Valores do Enum `tributacao`

| Valor | Descrição |
|-------|-----------|
| `LONGO_PRAZO` | Longo Prazo (tabela regressiva 15%-22.5%) |
| `CURTO_PRAZO` | Curto Prazo (22.5% / 20%) |
| `ACOES` | Tributação de Ações (15%) |
| `IMOBILIARIO` | FII (isenção PF em condições específicas) |
| `ISENTO` | Isento de IR |
| `PREVIDENCIA` | Regime de previdência (PGBL/VGBL) |

---

## Requisitos Funcionais

### RF-01: Classificação ANBIMA Dependente

- A lista de classificações ANBIMA deve ser filtrada com base na classificação CVM selecionada
- Ao trocar a classificação CVM, limpar a classificação ANBIMA se incompatível
- Endpoint para obter classificações ANBIMA:
  ```
  GET /api/v1/parametros/classificacoes-anbima?classificacao_cvm={valor}
  ```

### RF-02: Regras de Tributação por Tipo de Fundo

A tributação deve ser pré-selecionada/sugerida com base no tipo de fundo (definido na etapa 1):

| Tipo Fundo | Tributação Padrão |
|------------|-------------------|
| FI (RF) | LONGO_PRAZO |
| FI (Ações) | ACOES |
| FII | FII |
| FIAGRO | FIAGRO |
| ETF | Depende do tipo |

- Permitir alteração manual pelo usuário
- Exibir aviso se tributação selecionada for atípica para o tipo

### RF-03: Público Alvo e Validação

- Se tipo é FIDC ou FIP, público alvo mínimo é `QUALIFICADO`
- Se FIDC_NP, público alvo obrigatório é `PROFISSIONAL`
- Bloquear opções inválidas no select

### RF-04: Código ANBIMA

- Campo opcional
- Se preenchido, validar formato (6 dígitos)
- Permitir busca/verificação futura (não implementar neste slice)

---

## Backend

### Endpoint de Classificações ANBIMA

```
GET /api/v1/parametros/classificacoes-anbima
```

**Query Parameters:**
- `classificacao_cvm` (opcional): Filtra por classificação CVM

**Response:**
```json
[
  {
    "codigo": "RF_DL_CP",
    "nome": "Renda Fixa Duração Livre Crédito Privado",
    "classificacao_cvm": "RENDA_FIXA"
  },
  {
    "codigo": "RF_DL_SB",
    "nome": "Renda Fixa Duração Livre Soberano",
    "classificacao_cvm": "RENDA_FIXA"
  }
]
```

### Validação de Regras de Negócio

O backend deve validar as regras de público alvo conforme tipo do fundo.

---

## Frontend

### Componente

**WizardStep2ClassificacaoComponent**

### Comportamentos

1. **Carregamento inicial:**
   - Carregar lista de classificações ANBIMA
   - Pré-selecionar tributação baseada no tipo_fundo da etapa 1

2. **Ao mudar classificação CVM:**
   - Filtrar lista de classificações ANBIMA
   - Limpar seleção ANBIMA anterior se não pertencer ao novo filtro

3. **Validação de público alvo:**
   - Desabilitar opções inválidas baseado no tipo de fundo
   - Exibir tooltip explicando restrição

---

## Critérios de Aceite

- [ ] Select de classificação CVM exibe todas as opções
- [ ] Classificações ANBIMA são filtradas por classificação CVM
- [ ] Público alvo respeita restrições por tipo de fundo
- [ ] Tributação é pré-selecionada corretamente
- [ ] Validação impede combinações inválidas
- [ ] Dados da etapa 1 (tipo_fundo) são acessíveis
- [ ] Dados persistem ao navegar entre etapas

---

## Dependências

- Slice 01: Infraestrutura base
- Slice 02: Etapa 1 completa (tipo_fundo necessário)

## Próximo Slice

→ `04-SLICE-CARACTERISTICAS.md`
