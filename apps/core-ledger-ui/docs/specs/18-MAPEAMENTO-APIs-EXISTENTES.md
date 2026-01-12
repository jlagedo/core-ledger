# Mapeamento de APIs Existentes no Core Ledger

## Objetivo

Documentar as APIs do backend (.NET) que já existem no projeto `core-ledger-api` e que serão utilizadas pelo wizard de cadastro de fundos.

---

## APIs Verificadas e Confirmadas

### ✅ 1. Fundos - Endpoint do Wizard

**Especificado em:** `01-SLICE-INFRAESTRUTURA-BASE.md`, `15-SLICE-API-WIZARD-BACKEND.md`

```
POST /api/v1/fundos/wizard
```

**Arquivo:** `CoreLedger.API/Endpoints/Cadastros/FundosEndpoints.cs`

**Request Body:**
```json
{
  "identificacao": { },
  "classificacao": { },
  "caracteristicas": { },
  "parametrosCota": { },
  "taxas": [ ],
  "prazos": [ ],
  "classes": [ ],
  "vinculos": [ ],
  "parametrosFidc": { },
  "documentosTempIds": [ ]
}
```

**Responses:**
- `201 Created` - Fundo criado com sucesso
- `400 Bad Request` - Erro de validação
- `409 Conflict` - CNPJ já cadastrado
- `500 Internal Server Error` - Erro interno

**Handler:** Usa MediatR com `CriarFundoWizardCommand` e transação atômica para criar fundo + todas entidades relacionadas.

---

### ✅ 2. Fundos - Verificação de CNPJ

**Especificado em:** `02-SLICE-IDENTIFICACAO-FUNDO.md`, `15-SLICE-API-WIZARD-BACKEND.md`

```
GET /api/v1/fundos/verificar-cnpj/{cnpj}
```

**Arquivo:** `CoreLedger.API/Endpoints/Cadastros/FundosEndpoints.cs`

**Parâmetros:**
- `cnpj` (path parameter) - CNPJ com 14 dígitos (sem formatação)

**Response (CNPJ disponível):**
```json
{
  "disponivel": true
}
```

**Response (CNPJ já existe):**
```json
{
  "disponivel": false,
  "fundoId": "550e8400-e29b-41d4-a716-446655440001",
  "nomeFantasia": "Nome do Fundo Existente"
}
```

---

### ✅ 3. Classificações ANBIMA

**Especificado em:** `03-SLICE-CLASSIFICACAO.md`, `17-ESPECIFICACAO-LOOKUP-ANBIMA.md`

```
GET /api/v1/parametros/classificacoes-anbima
```

**Arquivo:** `CoreLedger.API/Endpoints/Cadastros/ClassificacoesAnbimaEndpoints.cs`

**Query Parameters:**
- `classificacao_cvm` (opcional) - Filtra por classificação CVM (ex: "RENDA_FIXA", "ACOES", "MULTIMERCADO")
- `nivel1` (opcional) - Filtra por categoria (Nível 1)
- `ativo` (opcional, default: true) - Filtra por status ativo

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "codigo": "RF_DL_CL",
      "nome": "Renda Fixa Duração Livre Crédito Livre",
      "nivel1": "Renda Fixa",
      "nivel2": "Duração Livre",
      "nivel3": "Crédito Livre",
      "classificacaoCvm": "RENDA_FIXA",
      "descricao": "Fundos que buscam retorno por meio de investimentos...",
      "nomeCompleto": "Renda Fixa > Duração Livre > Crédito Livre"
    }
  ],
  "total": 15,
  "classificacaoCvmFiltrada": "RENDA_FIXA"
}
```

**Endpoints Relacionados:**
```
GET /api/v1/parametros/classificacoes-anbima/{codigo}
GET /api/v1/parametros/classificacoes-anbima/niveis
GET /api/v1/parametros/classificacoes-anbima/verificar
```

---

### ✅ 4. Indexadores (Benchmarks)

**Especificado em:** `06-SLICE-TAXAS.md`

**⚠️ ATENÇÃO:** A rota real é diferente da especificada nos documentos do wizard.

**Rota especificada no wizard:**
```
GET /api/v1/indexadores?busca={termo}
```

**Rota real no projeto:**
```
GET /api/indexadores
```

**Arquivo:** `CoreLedger.API/Endpoints/IndexadoresEndpoints.cs`

**Query Parameters:**
- Não há parâmetro `busca` implementado
- Parâmetros disponíveis:
  - `tipo` (opcional) - Filtro por tipo de indexador
  - `periodicidade` (opcional) - Filtro por periodicidade
  - `fonte` (opcional) - Filtro por fonte
  - `ativo` (opcional) - Filtro por status ativo
  - `importacaoAutomatica` (opcional) - Filtro por importação automática

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "codigo": "CDI",
      "nome": "CDI - Certificado de Depósito Interbancário",
      "tipo": "TAXA_JUROS",
      "periodicidade": "DIARIA",
      "fonte": "B3",
      "ativo": true
    }
  ],
  "total": 10
}
```

**⚠️ Ajuste Necessário:**
O frontend do wizard precisa:
1. Usar a rota `/api/indexadores` (sem `/v1`)
2. Implementar busca client-side ou solicitar adicionar parâmetro `busca` na API

---

### ✅ 5. Instituições

**Especificado em:** `09-SLICE-VINCULOS.md`

```
GET /api/v1/instituicoes
```

**Arquivo:** `CoreLedger.API/Endpoints/Cadastros/InstituicoesEndpoints.cs`

**Query Parameters:**
- `busca` (opcional) - Termo de busca (nome ou CNPJ)
- `ativo` (opcional) - Filtro por status ativo
- `limit` (opcional) - Limite de resultados (paginação)
- `offset` (opcional) - Deslocamento para paginação

**Response:**
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "cnpj": "59281253000123",
      "razaoSocial": "BTG PACTUAL SERVICOS FINANCEIROS DTVM",
      "nomeFantasia": "BTG DTVM",
      "tiposHabilitados": ["ADMINISTRADOR", "CUSTODIANTE", "DISTRIBUIDOR"]
    }
  ],
  "total": 150,
  "limit": 10,
  "offset": 0
}
```

**Criação de Instituição:**
```
POST /api/v1/instituicoes
```

**Request:**
```json
{
  "cnpj": "12345678000199",
  "razaoSocial": "NOVA INSTITUICAO LTDA",
  "nomeFantasia": "Nova Instituição",
  "tiposHabilitados": ["ADMINISTRADOR"]
}
```

**Response 201:**
```json
{
  "id": "uuid",
  "cnpj": "12345678000199",
  "razaoSocial": "NOVA INSTITUICAO LTDA",
  "nomeFantasia": "Nova Instituição"
}
```

---

## APIs por Slice do Wizard

### Slice 01 - Infraestrutura Base
- ✅ `POST /api/v1/fundos/wizard`

### Slice 02 - Identificação do Fundo
- ✅ `GET /api/v1/fundos/verificar-cnpj/{cnpj}`

### Slice 03 - Classificação
- ✅ `GET /api/v1/parametros/classificacoes-anbima?classificacao_cvm={valor}`
- ✅ `GET /api/v1/parametros/classificacoes-anbima/{codigo}`
- ✅ `GET /api/v1/parametros/classificacoes-anbima/niveis`
- ✅ `GET /api/v1/parametros/classificacoes-anbima/verificar`

### Slice 04 - Características
- Nenhuma API específica necessária

### Slice 05 - Parâmetros de Cota
- Nenhuma API específica necessária (dados enviados no wizard)

### Slice 06 - Taxas do Fundo
- ⚠️ `GET /api/indexadores` (rota diferente da especificada)
  - **Especificado:** `/api/v1/indexadores?busca={termo}`
  - **Real:** `/api/indexadores` (sem parâmetro `busca`)

### Slice 07 - Prazos Operacionais
- Nenhuma API específica necessária

### Slice 08 - Classes CVM 175
- Nenhuma API específica necessária

### Slice 08-B - Parâmetros FIDC
- Nenhuma API específica necessária

### Slice 09 - Vínculos Institucionais
- ✅ `GET /api/v1/instituicoes?busca={termo}`
- ✅ `POST /api/v1/instituicoes`

### Slice 10 - Upload de Documentos
- ❌ `POST /api/v1/wizard/documentos/temp` - **NÃO IMPLEMENTADO**
  - Esta API precisa ser criada no backend

### Slice 11 - Revisão e Submissão
- ✅ `POST /api/v1/fundos/wizard` (mesmo endpoint da infraestrutura base)
- ❌ `POST /api/v1/fundos/wizard/validar` - **NÃO IMPLEMENTADO**
  - API opcional para validação prévia sem criação

### Slice 12 - Persistência de Rascunho
- ❌ `POST /api/v1/wizard/rascunhos` - **NÃO IMPLEMENTADO**
- ❌ `GET /api/v1/wizard/rascunhos` - **NÃO IMPLEMENTADO**
- ❌ `GET /api/v1/wizard/rascunhos/{id}` - **NÃO IMPLEMENTADO**
- ❌ `PUT /api/v1/wizard/rascunhos/{id}` - **NÃO IMPLEMENTADO**
- ❌ `DELETE /api/v1/wizard/rascunhos/{id}` - **NÃO IMPLEMENTADO**

---

## Resumo do Status

| Status | Quantidade | APIs |
|--------|------------|------|
| ✅ Implementadas | 9 | Fundos wizard, verificar CNPJ, classificações ANBIMA (4 endpoints), instituições (2) |
| ⚠️ Implementadas com diferença | 1 | Indexadores (rota e parâmetros diferentes) |
| ❌ Não implementadas | 7 | Upload temporário, validação wizard, CRUD rascunhos (5) |

---

## Próximos Passos

### Prioridade Alta (Bloqueantes)
1. **Ajustar chamada de indexadores no frontend:**
   - Usar `/api/indexadores` em vez de `/api/v1/indexadores`
   - Implementar filtro client-side ou solicitar adição do parâmetro `busca`

### Prioridade Média (Melhorias UX)
2. **Implementar validação prévia do wizard:**
   - `POST /api/v1/fundos/wizard/validar`
   - Permite validar dados antes de submeter

3. **Implementar upload temporário de documentos:**
   - `POST /api/v1/wizard/documentos/temp`
   - Necessário para o Slice 10

### Prioridade Baixa (Features Nice-to-Have)
4. **Implementar CRUD de rascunhos:**
   - `POST /api/v1/wizard/rascunhos`
   - `GET /api/v1/wizard/rascunhos`
   - `GET /api/v1/wizard/rascunhos/{id}`
   - `PUT /api/v1/wizard/rascunhos/{id}`
   - `DELETE /api/v1/wizard/rascunhos/{id}`
   - Permite auto-save e retomada de cadastros

---

## Arquitetura da API

### Padrões Utilizados
- **Minimal APIs** (não Controllers tradicionais)
- **MediatR** para CQRS (Commands/Queries)
- **FluentValidation** para validação de requests
- **Entity Framework Core** para acesso a dados
- **Auth0** para autenticação JWT
- **PostgreSQL** como banco de dados

### Estrutura de Pastas
```
CoreLedger.API/
├── Endpoints/
│   ├── Cadastros/
│   │   ├── FundosEndpoints.cs
│   │   ├── InstituicoesEndpoints.cs
│   │   ├── ClassificacoesAnbimaEndpoints.cs
│   │   ├── ClassesEndpoints.cs
│   │   └── TaxasEndpoints.cs
│   └── IndexadoresEndpoints.cs
├── Application/
│   ├── Commands/
│   │   └── CriarFundoWizard/
│   │       ├── CriarFundoWizardCommand.cs
│   │       ├── CriarFundoWizardCommandHandler.cs
│   │       └── CriarFundoWizardCommandValidator.cs
│   └── Queries/
└── Domain/
    └── Entities/
```

### Handlers de MediatR
Todos os endpoints delegam para handlers que implementam a lógica de negócio:
- `CriarFundoWizardCommand` → `CriarFundoWizardCommandHandler`
- `VerificarCnpjQuery` → `VerificarCnpjQueryHandler`
- `ListarClassificacoesAnbimaQuery` → `ListarClassificacoesAnbimaQueryHandler`

---

## Exemplo de Chamada - Frontend

### Verificar CNPJ (Slice 02)

```typescript
// src/app/services/fund.service.ts
verificarCnpj(cnpj: string): Observable<{ disponivel: boolean; fundoId?: string; nomeFantasia?: string }> {
  return this.http.get<any>(`${this.apiUrl}/fundos/verificar-cnpj/${cnpj}`);
}
```

### Buscar Classificações ANBIMA (Slice 03)

```typescript
// src/app/services/classificacao-anbima.service.ts
listarClassificacoes(classificacaoCvm?: string): Observable<ClassificacaoAnbimaResponse> {
  let params = new HttpParams();
  if (classificacaoCvm) {
    params = params.set('classificacao_cvm', classificacaoCvm);
  }
  return this.http.get<ClassificacaoAnbimaResponse>(
    `${this.apiUrl}/parametros/classificacoes-anbima`,
    { params }
  );
}
```

### Buscar Instituições (Slice 09)

```typescript
// src/app/services/instituicao.service.ts
buscar(termo: string): Observable<InstituicaoResponse> {
  const params = new HttpParams()
    .set('busca', termo)
    .set('limit', '10');
  return this.http.get<InstituicaoResponse>(`${this.apiUrl}/instituicoes`, { params });
}
```

### Criar Fundo via Wizard (Slice 01)

```typescript
// src/app/services/fund.service.ts
criarFundoWizard(dados: CriarFundoWizardRequest): Observable<FundoCriadoResponse> {
  return this.http.post<FundoCriadoResponse>(`${this.apiUrl}/fundos/wizard`, dados);
}
```

---

*Mapeamento de APIs Existentes - Core Ledger*
*Sistema Fund Accounting - Janeiro/2026*
