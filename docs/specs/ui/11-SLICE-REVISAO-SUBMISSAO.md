# SLICE 11: Etapa 10 - Revisão e Submissão

## Objetivo

Implementar a décima e última etapa do wizard onde o usuário revisa todos os dados e submete o cadastro para criação do fundo.

## Escopo

### Funcionalidade Principal

- Exibir resumo consolidado de todas as etapas
- Permitir edição rápida (voltar à etapa específica)
- Validar completude do cadastro
- Submeter para criação via API
- Exibir feedback de sucesso ou erro

---

## Requisitos Funcionais

### RF-01: Resumo Consolidado

Exibir em formato de accordion ou cards:

1. **Identificação**: CNPJ, Razão Social, Nome Fantasia, Tipo
2. **Classificação**: CVM, ANBIMA, Público Alvo, Tributação
3. **Características**: Condomínio, Prazo, Alavancagem, Cripto
4. **Parâmetros de Cota**: Casas decimais, Horário corte, Valor inicial
5. **Taxas**: Lista de taxas com percentuais
6. **Prazos**: D+X de aplicação e resgate
7. **Classes**: Lista de classes (se multiclasse)
8. **Vínculos**: Administrador, Gestor, Custodiante, etc.
9. **Documentos**: Lista de documentos anexados

### RF-02: Navegação para Edição

- Cada seção do resumo tem botão "Editar"
- Ao clicar, navegar diretamente para a etapa correspondente
- Ao voltar da edição, retornar à revisão

### RF-03: Indicador de Completude

Exibir checklist visual:

```
[✓] Identificação - Completo
[✓] Classificação - Completo
[✓] Características - Completo
[✓] Parâmetros de Cota - Completo
[✓] Taxas - Completo
[✓] Prazos - Completo
[○] Classes - Não configurado (opcional)
[✓] Vínculos - Completo
[!] Documentos - Regulamento não anexado (recomendado)
```

### RF-04: Validação Final

Antes de habilitar submissão, validar:
- Todos os campos obrigatórios preenchidos
- CNPJ não duplicado (revalidar)
- Vínculos obrigatórios presentes
- Datas consistentes

### RF-05: Submissão

Ao clicar em "Criar Fundo":
1. Exibir loading/spinner
2. Desabilitar botão de submissão
3. Chamar endpoint POST /api/v1/fundos/wizard
4. Aguardar resposta

### RF-06: Feedback de Sucesso

Se criação bem-sucedida:
- Exibir mensagem de sucesso com animação
- Mostrar ID/CNPJ do fundo criado
- Botões de ação:
  - "Ver Fundo" → navegar para /cadastros/fundos/{id}
  - "Cadastrar Outro" → reiniciar wizard
  - "Ir para Dashboard" → navegar para /dashboard

### RF-07: Tratamento de Erros

Se falha na criação:
- Exibir mensagem de erro clara
- Se erro de validação (400), mostrar campos com problema
- Se erro de duplicidade (409), informar e oferecer busca
- Se erro de servidor (500), sugerir tentar novamente
- Manter dados preenchidos para correção

### RF-08: Confirmação de Saída

Se usuário tentar sair da página antes de submeter:
- Exibir modal de confirmação
- Opções: "Sair sem salvar" / "Continuar editando"

---

## Frontend

### Componente

**WizardStep10RevisaoComponent**

### Layout Sugerido

```
┌─────────────────────────────────────────────────────────────────┐
│ REVISÃO DO CADASTRO                                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│ Progresso: ████████████████████████████░░░░ 90%                │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ ✓ IDENTIFICAÇÃO                                    [Editar] │ │
│ ├─────────────────────────────────────────────────────────────┤ │
│ │ CNPJ: 12.345.678/0001-99                                   │ │
│ │ Razão Social: FUNDO DE INVESTIMENTO RENDA FIXA CP          │ │
│ │ Nome Fantasia: FI RF CREDITO PRIVADO                       │ │
│ │ Tipo: FI - Fundo de Investimento                           │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ ✓ CLASSIFICAÇÃO                                    [Editar] │ │
│ ├─────────────────────────────────────────────────────────────┤ │
│ │ CVM: Renda Fixa                                            │ │
│ │ ANBIMA: Renda Fixa Duração Livre Crédito Privado           │ │
│ │ Público Alvo: Investidores Qualificados                    │ │
│ │ Tributação: Longo Prazo                                    │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ ✓ TAXAS                                            [Editar] │ │
│ ├─────────────────────────────────────────────────────────────┤ │
│ │ • Administração: 1.50% a.a. (PL Médio, Diária)             │ │
│ │ • Performance: 20.00% (Acima CDI, Semestral)               │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ ✓ VÍNCULOS                                         [Editar] │ │
│ ├─────────────────────────────────────────────────────────────┤ │
│ │ • Administrador: BTG PACTUAL SERVICOS FINANCEIROS          │ │
│ │ • Gestor: KINEA INVESTIMENTOS LTDA                         │ │
│ │ • Custodiante: BANCO BTG PACTUAL SA                        │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ ⚠ DOCUMENTOS                                       [Editar] │ │
│ ├─────────────────────────────────────────────────────────────┤ │
│ │ Nenhum documento anexado.                                  │ │
│ │ Recomendamos anexar o regulamento do fundo.               │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ──────────────────────────────────────────────────────────────  │
│                                                                 │
│              [Voltar]              [Criar Fundo]               │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Modal de Sucesso

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│                           ✓                                    │
│                                                                 │
│               Fundo criado com sucesso!                        │
│                                                                 │
│               CNPJ: 12.345.678/0001-99                         │
│               FI RF CREDITO PRIVADO                            │
│                                                                 │
│    [Ver Fundo]    [Cadastrar Outro]    [Dashboard]             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Modal de Erro

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│                           ✗                                    │
│                                                                 │
│               Erro ao criar fundo                              │
│                                                                 │
│    O CNPJ informado já está cadastrado para outro fundo.       │
│                                                                 │
│               [Corrigir CNPJ]    [Cancelar]                    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Backend

### Endpoint de Criação via Wizard

```
POST /api/v1/fundos/wizard
```

**Request (payload completo):**
```json
{
  "identificacao": {
    "cnpj": "12345678000199",
    "razao_social": "FUNDO DE INVESTIMENTO RENDA FIXA CP",
    "nome_fantasia": "FI RF CREDITO PRIVADO",
    "nome_curto": "RF CP",
    "tipo_fundo": "FI",
    "data_constituicao": "2024-01-15",
    "data_inicio_atividade": "2024-02-01"
  },
  "classificacao": {
    "classificacao_cvm": "RENDA_FIXA",
    "classificacao_anbima": "RF_DL_CP",
    "codigo_anbima": "123456",
    "publico_alvo": "QUALIFICADO",
    "tributacao": "LONGO_PRAZO"
  },
  "caracteristicas": {
    "condominio": "ABERTO",
    "prazo": "INDETERMINADO",
    "exclusivo": false,
    "reservado": false,
    "permite_alavancagem": false,
    "aceita_cripto": false,
    "percentual_exterior": 0
  },
  "parametros_cota": {
    "casas_decimais_cota": 6,
    "casas_decimais_quantidade": 6,
    "horario_corte": "14:00",
    "valor_cota_inicial": 1.00,
    "tipo_cota": "FECHAMENTO",
    "frequencia_calculo": "DIARIA"
  },
  "taxas": [
    {
      "tipo_taxa": "ADMINISTRACAO",
      "percentual": 0.015,
      "base_calculo": "PL_MEDIO",
      "forma_cobranca": "DIARIA",
      "data_inicio_vigencia": "2024-02-01"
    }
  ],
  "prazos": [
    {
      "tipo_prazo": "APLICACAO",
      "dias_cotizacao": 0,
      "dias_liquidacao": 0,
      "tipo_dia": "UTIL"
    },
    {
      "tipo_prazo": "RESGATE",
      "dias_cotizacao": 1,
      "dias_liquidacao": 2,
      "tipo_dia": "UTIL"
    }
  ],
  "classes": [],
  "vinculos": [
    {
      "tipo_vinculo": "ADMINISTRADOR",
      "instituicao_id": "uuid",
      "data_inicio": "2024-02-01"
    }
  ],
  "documentos_temp_ids": ["uuid1", "uuid2"]
}
```

**Response (201):**
```json
{
  "id": "uuid-do-fundo",
  "cnpj": "12345678000199",
  "nome_fantasia": "FI RF CREDITO PRIVADO",
  "progresso_cadastro": 100,
  "wizard_completo": true,
  "created_at": "2024-01-20T10:30:00Z"
}
```

**Response (400 - Validação):**
```json
{
  "type": "ValidationError",
  "title": "Erro de validação",
  "status": 400,
  "errors": [
    {
      "field": "identificacao.cnpj",
      "message": "CNPJ inválido"
    }
  ]
}
```

**Response (409 - Duplicidade):**
```json
{
  "type": "ConflictError",
  "title": "CNPJ já cadastrado",
  "status": 409,
  "detail": "O CNPJ 12.345.678/0001-99 já está cadastrado para o fundo 'OUTRO FUNDO'",
  "fundo_existente_id": "uuid"
}
```

### Transação

A criação via wizard deve ser executada em **transação única**:
1. Criar fundo
2. Criar parâmetros de cota
3. Criar taxas
4. Criar prazos
5. Criar classes
6. Criar vínculos
7. Vincular documentos temporários

Se qualquer etapa falhar, fazer rollback completo.

---

## Critérios de Aceite

- [ ] Resumo exibe dados de todas as etapas
- [ ] Botão "Editar" navega para etapa correta
- [ ] Indicador de completude funciona
- [ ] Validação final bloqueia submissão se inválido
- [ ] Loading exibido durante submissão
- [ ] Modal de sucesso exibe dados do fundo
- [ ] Modal de erro exibe mensagem clara
- [ ] Erros de validação indicam campo problemático
- [ ] Botões pós-sucesso funcionam
- [ ] Confirmação de saída funciona

---

## Dependências

- Todos os slices anteriores (01-10)
- Endpoint POST /api/v1/fundos/wizard implementado

## Próximo Slice

→ `12-SLICE-PERSISTENCIA-RASCUNHO.md`
