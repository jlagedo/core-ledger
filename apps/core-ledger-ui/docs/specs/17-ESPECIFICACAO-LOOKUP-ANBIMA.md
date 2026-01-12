# Especificação: Lookup de Classificações ANBIMA

## Objetivo

Implementar entidade de lookup e endpoint para classificações ANBIMA de fundos de investimento, com relacionamento para classificações CVM.

---

## 1. Modelo de Dados

### 1.1 Entidade `ClassificacaoAnbima`

```csharp
public class ClassificacaoAnbima
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;           // Ex: "RF_DL_SOB"
    public string Nome { get; set; } = null!;             // Ex: "Renda Fixa Duração Baixa Soberano"
    public string Nivel1 { get; set; } = null!;           // Categoria: Renda Fixa
    public string Nivel2 { get; set; } = null!;           // Tipo: Duração Baixa
    public string? Nivel3 { get; set; }                   // Subtipo: Soberano (opcional)
    public string ClassificacaoCvm { get; set; } = null!; // FK lógica para enum CVM
    public string? Descricao { get; set; }                // Descrição detalhada
    public bool Ativo { get; set; } = true;
    public int OrdemExibicao { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### 1.2 Migration

```sql
CREATE TABLE classificacao_anbima (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(20) NOT NULL UNIQUE,
    nome VARCHAR(100) NOT NULL,
    nivel1 VARCHAR(50) NOT NULL,
    nivel2 VARCHAR(50) NOT NULL,
    nivel3 VARCHAR(50),
    classificacao_cvm VARCHAR(30) NOT NULL,
    descricao TEXT,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    ordem_exibicao SMALLINT NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Índices
CREATE INDEX idx_classificacao_anbima_cvm ON classificacao_anbima(classificacao_cvm);
CREATE INDEX idx_classificacao_anbima_nivel1 ON classificacao_anbima(nivel1);
CREATE INDEX idx_classificacao_anbima_ativo ON classificacao_anbima(ativo) WHERE ativo = TRUE;
```

---

## 2. Seed Data - Classificações ANBIMA

### 2.1 Renda Fixa (classificacao_cvm = 'RENDA_FIXA')

| Código | Nome Completo | Nível 1 | Nível 2 | Nível 3 |
|--------|---------------|---------|---------|---------|
| `RF_DB_SOB` | Renda Fixa Duração Baixa Soberano | Renda Fixa | Duração Baixa | Soberano |
| `RF_DB_GRD` | Renda Fixa Duração Baixa Grau de Investimento | Renda Fixa | Duração Baixa | Grau de Investimento |
| `RF_DB_CL` | Renda Fixa Duração Baixa Crédito Livre | Renda Fixa | Duração Baixa | Crédito Livre |
| `RF_DM_SOB` | Renda Fixa Duração Média Soberano | Renda Fixa | Duração Média | Soberano |
| `RF_DM_GRD` | Renda Fixa Duração Média Grau de Investimento | Renda Fixa | Duração Média | Grau de Investimento |
| `RF_DM_CL` | Renda Fixa Duração Média Crédito Livre | Renda Fixa | Duração Média | Crédito Livre |
| `RF_DA_SOB` | Renda Fixa Duração Alta Soberano | Renda Fixa | Duração Alta | Soberano |
| `RF_DA_GRD` | Renda Fixa Duração Alta Grau de Investimento | Renda Fixa | Duração Alta | Grau de Investimento |
| `RF_DA_CL` | Renda Fixa Duração Alta Crédito Livre | Renda Fixa | Duração Alta | Crédito Livre |
| `RF_DL_SOB` | Renda Fixa Duração Livre Soberano | Renda Fixa | Duração Livre | Soberano |
| `RF_DL_GRD` | Renda Fixa Duração Livre Grau de Investimento | Renda Fixa | Duração Livre | Grau de Investimento |
| `RF_DL_CL` | Renda Fixa Duração Livre Crédito Livre | Renda Fixa | Duração Livre | Crédito Livre |
| `RF_IDX_IPCA` | Renda Fixa Indexados Índices | Renda Fixa | Indexados | - |
| `RF_INV_EXT` | Renda Fixa Investimento no Exterior | Renda Fixa | Investimento no Exterior | - |
| `RF_SIMPLES` | Renda Fixa Simples | Renda Fixa | Simples | - |

### 2.2 Ações (classificacao_cvm = 'ACOES')

| Código | Nome Completo | Nível 1 | Nível 2 | Nível 3 |
|--------|---------------|---------|---------|---------|
| `AE_IDX_ATIVO` | Ações Indexados Índice Ativo | Ações | Indexados | Índice Ativo |
| `AE_IDX_IBOV` | Ações Indexados Ibovespa Ativo | Ações | Indexados | Ibovespa Ativo |
| `AE_IDX_IBRX` | Ações Indexados IBrX Ativo | Ações | Indexados | IBrX Ativo |
| `AE_ATI_VAL` | Ações Ativos Valor/Crescimento | Ações | Ativos | Valor/Crescimento |
| `AE_ATI_SET` | Ações Ativos Setoriais | Ações | Ativos | Setoriais |
| `AE_ATI_DIV` | Ações Ativos Dividendos | Ações | Ativos | Dividendos |
| `AE_ATI_SC` | Ações Ativos Small Caps | Ações | Ativos | Small Caps |
| `AE_ATI_SUS` | Ações Ativos Sustentabilidade/Governança | Ações | Ativos | Sustentabilidade/Governança |
| `AE_ATI_LIV` | Ações Ativos Livre | Ações | Ativos | Livre |
| `AE_ESP_FMP` | Ações Específicos FMP-FGTS | Ações | Específicos | FMP-FGTS |
| `AE_ESP_FEC` | Ações Específicos Fechados de Ações | Ações | Específicos | Fechados de Ações |
| `AE_ESP_MON` | Ações Específicos Mono Ação | Ações | Específicos | Mono Ação |
| `AE_INV_EXT` | Ações Investimento no Exterior | Ações | Investimento no Exterior | - |

### 2.3 Multimercado (classificacao_cvm = 'MULTIMERCADO')

| Código | Nome Completo | Nível 1 | Nível 2 | Nível 3 |
|--------|---------------|---------|---------|---------|
| `MM_MACRO` | Multimercado Macro | Multimercado | Alocação | Macro |
| `MM_DINAMIC` | Multimercado Dinâmico | Multimercado | Alocação | Dinâmico |
| `MM_TRADING` | Multimercado Trading | Multimercado | Estratégia | Trading |
| `MM_LS_DIR` | Multimercado Long and Short Direcional | Multimercado | Estratégia | Long and Short Direcional |
| `MM_LS_NEU` | Multimercado Long and Short Neutro | Multimercado | Estratégia | Long and Short Neutro |
| `MM_JRS_EXT` | Multimercado Juros e Moedas | Multimercado | Estratégia | Juros e Moedas |
| `MM_LIVRE` | Multimercado Livre | Multimercado | Estratégia | Livre |
| `MM_CAP_PROT` | Multimercado Capital Protegido | Multimercado | Estratégia | Capital Protegido |
| `MM_EST_ESP` | Multimercado Estratégia Específica | Multimercado | Estratégia | Específica |
| `MM_BAL` | Multimercado Balanceados | Multimercado | Alocação | Balanceados |
| `MM_INV_EXT` | Multimercado Investimento no Exterior | Multimercado | Investimento no Exterior | - |

### 2.4 Cambial (classificacao_cvm = 'CAMBIAL')

| Código | Nome Completo | Nível 1 | Nível 2 | Nível 3 |
|--------|---------------|---------|---------|---------|
| `CB_DOLAR` | Cambial Dólar | Cambial | Moeda | Dólar |
| `CB_EURO` | Cambial Euro | Cambial | Moeda | Euro |
| `CB_OUTRAS` | Cambial Outras Moedas | Cambial | Moeda | Outras |

### 2.5 Previdência (se aplicável)

| Código | Nome Completo | Nível 1 | Nível 2 | Nível 3 |
|--------|---------------|---------|---------|---------|
| `PREV_RF` | Previdência Renda Fixa | Previdência | Renda Fixa | - |
| `PREV_BAL` | Previdência Balanceados | Previdência | Balanceados | - |
| `PREV_MM` | Previdência Multimercados | Previdência | Multimercados | - |
| `PREV_AE` | Previdência Ações | Previdência | Ações | - |
| `PREV_DT` | Previdência Data Alvo | Previdência | Data Alvo | - |

### 2.6 Sem Classificação ANBIMA (CVM apenas)

| classificacao_cvm | Observação |
|-------------------|------------|
| `FIDC` | ANBIMA não classifica FIDCs no mesmo framework |
| `FIP` | Classificação própria de Private Equity |
| `FII` | Classificação própria de FIIs |
| `FIAGRO` | Novo, classificação em definição |
| `FI_INFRA` | Sem classificação ANBIMA específica |
| `ETF_RF` | ETF Renda Fixa (classificação específica) |
| `ETF_RV` | ETF Renda Variável (classificação específica) |

---

## 3. Mapeamento CVM → ANBIMA

### 3.1 Regras de Filtragem

```csharp
public static class ClassificacaoCvmAnbimaMap
{
    public static readonly Dictionary<string, string[]> Map = new()
    {
        ["RENDA_FIXA"] = new[] 
        { 
            "RF_DB_SOB", "RF_DB_GRD", "RF_DB_CL",
            "RF_DM_SOB", "RF_DM_GRD", "RF_DM_CL",
            "RF_DA_SOB", "RF_DA_GRD", "RF_DA_CL",
            "RF_DL_SOB", "RF_DL_GRD", "RF_DL_CL",
            "RF_IDX_IPCA", "RF_INV_EXT", "RF_SIMPLES"
        },
        ["ACOES"] = new[] 
        { 
            "AE_IDX_ATIVO", "AE_IDX_IBOV", "AE_IDX_IBRX",
            "AE_ATI_VAL", "AE_ATI_SET", "AE_ATI_DIV", 
            "AE_ATI_SC", "AE_ATI_SUS", "AE_ATI_LIV",
            "AE_ESP_FMP", "AE_ESP_FEC", "AE_ESP_MON",
            "AE_INV_EXT"
        },
        ["MULTIMERCADO"] = new[] 
        { 
            "MM_MACRO", "MM_DINAMIC", "MM_TRADING",
            "MM_LS_DIR", "MM_LS_NEU", "MM_JRS_EXT",
            "MM_LIVRE", "MM_CAP_PROT", "MM_EST_ESP",
            "MM_BAL", "MM_INV_EXT"
        },
        ["CAMBIAL"] = new[] 
        { 
            "CB_DOLAR", "CB_EURO", "CB_OUTRAS"
        },
        
        // Classificações CVM sem ANBIMA correspondente
        ["FIDC"] = Array.Empty<string>(),
        ["FIP"] = Array.Empty<string>(),
        ["FII"] = Array.Empty<string>(),
        ["FIAGRO"] = Array.Empty<string>(),
        ["FI_INFRA"] = Array.Empty<string>(),
        ["ETF_RF"] = Array.Empty<string>(),
        ["ETF_RV"] = Array.Empty<string>()
    };
}
```

---

## 4. API Endpoints

### 4.1 Listar Classificações ANBIMA

```http
GET /api/v1/parametros/classificacoes-anbima
```

**Query Parameters:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `classificacao_cvm` | string | Não | Filtra por classificação CVM |
| `nivel1` | string | Não | Filtra por categoria (Nível 1) |
| `ativo` | boolean | Não | Filtra por status (default: true) |

**Response 200:**

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
      "descricao": "Fundos que buscam retorno por meio de investimentos em ativos de renda fixa, com duration livre e sem restrição de crédito.",
      "nomeCompleto": "Renda Fixa > Duração Livre > Crédito Livre"
    },
    {
      "id": 2,
      "codigo": "RF_DL_SOB",
      "nome": "Renda Fixa Duração Livre Soberano",
      "nivel1": "Renda Fixa",
      "nivel2": "Duração Livre",
      "nivel3": "Soberano",
      "classificacaoCvm": "RENDA_FIXA",
      "descricao": "Fundos que investem exclusivamente em títulos públicos federais.",
      "nomeCompleto": "Renda Fixa > Duração Livre > Soberano"
    }
  ],
  "total": 15,
  "classificacaoCvmFiltrada": "RENDA_FIXA"
}
```

**Response 200 (filtro sem resultados):**

```json
{
  "items": [],
  "total": 0,
  "classificacaoCvmFiltrada": "FIDC",
  "mensagem": "Classificação CVM 'FIDC' não possui classificações ANBIMA correspondentes"
}
```

### 4.2 Obter Classificação por Código

```http
GET /api/v1/parametros/classificacoes-anbima/{codigo}
```

**Response 200:**

```json
{
  "id": 1,
  "codigo": "RF_DL_CL",
  "nome": "Renda Fixa Duração Livre Crédito Livre",
  "nivel1": "Renda Fixa",
  "nivel2": "Duração Livre",
  "nivel3": "Crédito Livre",
  "classificacaoCvm": "RENDA_FIXA",
  "descricao": "Fundos que buscam retorno por meio de investimentos em ativos de renda fixa, com duration livre e sem restrição de crédito.",
  "caracteristicas": {
    "duration": "Livre",
    "restricaoCredito": "Nenhuma",
    "publicoAlvoMinimo": "GERAL"
  }
}
```

**Response 404:**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Classificação ANBIMA não encontrada",
  "status": 404,
  "detail": "Código 'XYZ_INVALIDO' não encontrado"
}
```

### 4.3 Listar Níveis (para filtros hierárquicos)

```http
GET /api/v1/parametros/classificacoes-anbima/niveis
```

**Query Parameters:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `classificacao_cvm` | string | Não | Filtra por classificação CVM |

**Response 200:**

```json
{
  "nivel1": [
    { "valor": "Renda Fixa", "quantidade": 15 },
    { "valor": "Ações", "quantidade": 13 },
    { "valor": "Multimercado", "quantidade": 11 },
    { "valor": "Cambial", "quantidade": 3 }
  ],
  "nivel2PorNivel1": {
    "Renda Fixa": [
      { "valor": "Duração Baixa", "quantidade": 3 },
      { "valor": "Duração Média", "quantidade": 3 },
      { "valor": "Duração Alta", "quantidade": 3 },
      { "valor": "Duração Livre", "quantidade": 3 },
      { "valor": "Indexados", "quantidade": 1 },
      { "valor": "Investimento no Exterior", "quantidade": 1 },
      { "valor": "Simples", "quantidade": 1 }
    ],
    "Ações": [
      { "valor": "Indexados", "quantidade": 3 },
      { "valor": "Ativos", "quantidade": 6 },
      { "valor": "Específicos", "quantidade": 3 },
      { "valor": "Investimento no Exterior", "quantidade": 1 }
    ]
  }
}
```

### 4.4 Verificar Compatibilidade

```http
GET /api/v1/parametros/classificacoes-anbima/verificar
```

**Query Parameters:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `codigo_anbima` | string | Sim | Código ANBIMA a verificar |
| `classificacao_cvm` | string | Sim | Classificação CVM |

**Response 200 (compatível):**

```json
{
  "compativel": true,
  "codigoAnbima": "RF_DL_CL",
  "classificacaoCvm": "RENDA_FIXA"
}
```

**Response 200 (incompatível):**

```json
{
  "compativel": false,
  "codigoAnbima": "AE_ATI_LIV",
  "classificacaoCvm": "RENDA_FIXA",
  "mensagem": "Classificação ANBIMA 'Ações Ativos Livre' não é compatível com CVM 'RENDA_FIXA'. Esperado: ACOES"
}
```

---

## 5. Implementação Backend

### 5.1 Controller

```csharp
[ApiController]
[Route("api/v1/parametros/classificacoes-anbima")]
public class ClassificacoesAnbimaController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet]
    [ProducesResponseType(typeof(ListarClassificacoesAnbimaResponse), 200)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? classificacaoCvm,
        [FromQuery] string? nivel1,
        [FromQuery] bool ativo = true,
        CancellationToken ct = default)
    {
        var query = new ListarClassificacoesAnbimaQuery(classificacaoCvm, nivel1, ativo);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{codigo}")]
    [ProducesResponseType(typeof(ClassificacaoAnbimaDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorCodigo(
        string codigo,
        CancellationToken ct = default)
    {
        var query = new ObterClassificacaoAnbimaQuery(codigo);
        var result = await _mediator.Send(query, ct);
        
        if (result is null)
            return NotFound(new { detail = $"Código '{codigo}' não encontrado" });
        
        return Ok(result);
    }

    [HttpGet("niveis")]
    [ProducesResponseType(typeof(NiveisClassificacaoAnbimaResponse), 200)]
    public async Task<IActionResult> ListarNiveis(
        [FromQuery] string? classificacaoCvm,
        CancellationToken ct = default)
    {
        var query = new ListarNiveisAnbimaQuery(classificacaoCvm);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("verificar")]
    [ProducesResponseType(typeof(VerificarCompatibilidadeResponse), 200)]
    public async Task<IActionResult> VerificarCompatibilidade(
        [FromQuery] string codigoAnbima,
        [FromQuery] string classificacaoCvm,
        CancellationToken ct = default)
    {
        var query = new VerificarCompatibilidadeAnbimaQuery(codigoAnbima, classificacaoCvm);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}
```

### 5.2 Query Handler

```csharp
public class ListarClassificacoesAnbimaQueryHandler 
    : IRequestHandler<ListarClassificacoesAnbimaQuery, ListarClassificacoesAnbimaResponse>
{
    private readonly FundAccountingDbContext _db;

    public async Task<ListarClassificacoesAnbimaResponse> Handle(
        ListarClassificacoesAnbimaQuery request,
        CancellationToken ct)
    {
        var query = _db.ClassificacoesAnbima.AsQueryable();

        if (request.Ativo)
            query = query.Where(c => c.Ativo);

        if (!string.IsNullOrEmpty(request.ClassificacaoCvm))
            query = query.Where(c => c.ClassificacaoCvm == request.ClassificacaoCvm);

        if (!string.IsNullOrEmpty(request.Nivel1))
            query = query.Where(c => c.Nivel1 == request.Nivel1);

        var items = await query
            .OrderBy(c => c.OrdemExibicao)
            .ThenBy(c => c.Nome)
            .Select(c => new ClassificacaoAnbimaDto
            {
                Id = c.Id,
                Codigo = c.Codigo,
                Nome = c.Nome,
                Nivel1 = c.Nivel1,
                Nivel2 = c.Nivel2,
                Nivel3 = c.Nivel3,
                ClassificacaoCvm = c.ClassificacaoCvm,
                Descricao = c.Descricao,
                NomeCompleto = c.Nivel3 != null 
                    ? $"{c.Nivel1} > {c.Nivel2} > {c.Nivel3}"
                    : $"{c.Nivel1} > {c.Nivel2}"
            })
            .ToListAsync(ct);

        return new ListarClassificacoesAnbimaResponse
        {
            Items = items,
            Total = items.Count,
            ClassificacaoCvmFiltrada = request.ClassificacaoCvm,
            Mensagem = items.Count == 0 && !string.IsNullOrEmpty(request.ClassificacaoCvm)
                ? $"Classificação CVM '{request.ClassificacaoCvm}' não possui classificações ANBIMA correspondentes"
                : null
        };
    }
}
```

### 5.3 DTOs

```csharp
public class ClassificacaoAnbimaDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nome { get; set; } = null!;
    public string Nivel1 { get; set; } = null!;
    public string Nivel2 { get; set; } = null!;
    public string? Nivel3 { get; set; }
    public string ClassificacaoCvm { get; set; } = null!;
    public string? Descricao { get; set; }
    public string NomeCompleto { get; set; } = null!;
}

public class ListarClassificacoesAnbimaResponse
{
    public List<ClassificacaoAnbimaDto> Items { get; set; } = new();
    public int Total { get; set; }
    public string? ClassificacaoCvmFiltrada { get; set; }
    public string? Mensagem { get; set; }
}

public class NiveisClassificacaoAnbimaResponse
{
    public List<NivelContagem> Nivel1 { get; set; } = new();
    public Dictionary<string, List<NivelContagem>> Nivel2PorNivel1 { get; set; } = new();
}

public class NivelContagem
{
    public string Valor { get; set; } = null!;
    public int Quantidade { get; set; }
}

public class VerificarCompatibilidadeResponse
{
    public bool Compativel { get; set; }
    public string CodigoAnbima { get; set; } = null!;
    public string ClassificacaoCvm { get; set; } = null!;
    public string? Mensagem { get; set; }
}
```

---

## 6. Seed Data SQL

```sql
-- Renda Fixa
INSERT INTO classificacao_anbima (codigo, nome, nivel1, nivel2, nivel3, classificacao_cvm, ordem_exibicao, descricao) VALUES
('RF_DB_SOB', 'Renda Fixa Duração Baixa Soberano', 'Renda Fixa', 'Duração Baixa', 'Soberano', 'RENDA_FIXA', 1, 'Fundos que investem em títulos públicos federais com duration média até 21 dias úteis'),
('RF_DB_GRD', 'Renda Fixa Duração Baixa Grau de Investimento', 'Renda Fixa', 'Duração Baixa', 'Grau de Investimento', 'RENDA_FIXA', 2, 'Fundos com duration baixa e foco em crédito de alta qualidade'),
('RF_DB_CL', 'Renda Fixa Duração Baixa Crédito Livre', 'Renda Fixa', 'Duração Baixa', 'Crédito Livre', 'RENDA_FIXA', 3, 'Fundos com duration baixa sem restrição de crédito'),
('RF_DM_SOB', 'Renda Fixa Duração Média Soberano', 'Renda Fixa', 'Duração Média', 'Soberano', 'RENDA_FIXA', 4, 'Fundos soberanos com duration média entre 21 e 126 dias úteis'),
('RF_DM_GRD', 'Renda Fixa Duração Média Grau de Investimento', 'Renda Fixa', 'Duração Média', 'Grau de Investimento', 'RENDA_FIXA', 5, 'Fundos com duration média e crédito de alta qualidade'),
('RF_DM_CL', 'Renda Fixa Duração Média Crédito Livre', 'Renda Fixa', 'Duração Média', 'Crédito Livre', 'RENDA_FIXA', 6, 'Fundos com duration média sem restrição de crédito'),
('RF_DA_SOB', 'Renda Fixa Duração Alta Soberano', 'Renda Fixa', 'Duração Alta', 'Soberano', 'RENDA_FIXA', 7, 'Fundos soberanos com duration acima de 126 dias úteis'),
('RF_DA_GRD', 'Renda Fixa Duração Alta Grau de Investimento', 'Renda Fixa', 'Duração Alta', 'Grau de Investimento', 'RENDA_FIXA', 8, 'Fundos com duration alta e crédito de alta qualidade'),
('RF_DA_CL', 'Renda Fixa Duração Alta Crédito Livre', 'Renda Fixa', 'Duração Alta', 'Crédito Livre', 'RENDA_FIXA', 9, 'Fundos com duration alta sem restrição de crédito'),
('RF_DL_SOB', 'Renda Fixa Duração Livre Soberano', 'Renda Fixa', 'Duração Livre', 'Soberano', 'RENDA_FIXA', 10, 'Fundos soberanos sem compromisso de duration'),
('RF_DL_GRD', 'Renda Fixa Duração Livre Grau de Investimento', 'Renda Fixa', 'Duração Livre', 'Grau de Investimento', 'RENDA_FIXA', 11, 'Fundos com duration livre e crédito de alta qualidade'),
('RF_DL_CL', 'Renda Fixa Duração Livre Crédito Livre', 'Renda Fixa', 'Duração Livre', 'Crédito Livre', 'RENDA_FIXA', 12, 'Fundos com duration livre sem restrição de crédito'),
('RF_IDX', 'Renda Fixa Indexados', 'Renda Fixa', 'Indexados', NULL, 'RENDA_FIXA', 13, 'Fundos que buscam seguir índices de renda fixa como IMA-B'),
('RF_INV_EXT', 'Renda Fixa Investimento no Exterior', 'Renda Fixa', 'Investimento no Exterior', NULL, 'RENDA_FIXA', 14, 'Fundos de renda fixa com investimentos no exterior'),
('RF_SIMPLES', 'Renda Fixa Simples', 'Renda Fixa', 'Simples', NULL, 'RENDA_FIXA', 15, 'Fundos destinados a investidores de varejo com política de investimento simplificada');

-- Ações
INSERT INTO classificacao_anbima (codigo, nome, nivel1, nivel2, nivel3, classificacao_cvm, ordem_exibicao, descricao) VALUES
('AE_IDX_ATIVO', 'Ações Indexados Índice Ativo', 'Ações', 'Indexados', 'Índice Ativo', 'ACOES', 20, 'Fundos que buscam superar índices de ações'),
('AE_IDX_IBOV', 'Ações Indexados Ibovespa Ativo', 'Ações', 'Indexados', 'Ibovespa Ativo', 'ACOES', 21, 'Fundos que buscam superar o Ibovespa'),
('AE_IDX_IBRX', 'Ações Indexados IBrX Ativo', 'Ações', 'Indexados', 'IBrX Ativo', 'ACOES', 22, 'Fundos que buscam superar o IBrX'),
('AE_ATI_VAL', 'Ações Ativos Valor/Crescimento', 'Ações', 'Ativos', 'Valor/Crescimento', 'ACOES', 23, 'Fundos com estratégia value ou growth'),
('AE_ATI_SET', 'Ações Ativos Setoriais', 'Ações', 'Ativos', 'Setoriais', 'ACOES', 24, 'Fundos focados em setores específicos'),
('AE_ATI_DIV', 'Ações Ativos Dividendos', 'Ações', 'Ativos', 'Dividendos', 'ACOES', 25, 'Fundos focados em empresas pagadoras de dividendos'),
('AE_ATI_SC', 'Ações Ativos Small Caps', 'Ações', 'Ativos', 'Small Caps', 'ACOES', 26, 'Fundos focados em empresas de menor capitalização'),
('AE_ATI_SUS', 'Ações Ativos Sustentabilidade/Governança', 'Ações', 'Ativos', 'Sustentabilidade/Governança', 'ACOES', 27, 'Fundos com critérios ESG'),
('AE_ATI_LIV', 'Ações Ativos Livre', 'Ações', 'Ativos', 'Livre', 'ACOES', 28, 'Fundos de ações sem estratégia específica'),
('AE_ESP_FMP', 'Ações Específicos FMP-FGTS', 'Ações', 'Específicos', 'FMP-FGTS', 'ACOES', 29, 'Fundos de privatização com recursos do FGTS'),
('AE_ESP_FEC', 'Ações Específicos Fechados de Ações', 'Ações', 'Específicos', 'Fechados de Ações', 'ACOES', 30, 'Fundos fechados de ações'),
('AE_ESP_MON', 'Ações Específicos Mono Ação', 'Ações', 'Específicos', 'Mono Ação', 'ACOES', 31, 'Fundos que investem em uma única ação'),
('AE_INV_EXT', 'Ações Investimento no Exterior', 'Ações', 'Investimento no Exterior', NULL, 'ACOES', 32, 'Fundos de ações com investimentos no exterior');

-- Multimercado
INSERT INTO classificacao_anbima (codigo, nome, nivel1, nivel2, nivel3, classificacao_cvm, ordem_exibicao, descricao) VALUES
('MM_MACRO', 'Multimercado Macro', 'Multimercado', 'Alocação', 'Macro', 'MULTIMERCADO', 40, 'Fundos com estratégia baseada em cenários macroeconômicos'),
('MM_DINAMIC', 'Multimercado Dinâmico', 'Multimercado', 'Alocação', 'Dinâmico', 'MULTIMERCADO', 41, 'Fundos com alocação dinâmica entre classes de ativos'),
('MM_TRADING', 'Multimercado Trading', 'Multimercado', 'Estratégia', 'Trading', 'MULTIMERCADO', 42, 'Fundos com estratégia de curto prazo'),
('MM_LS_DIR', 'Multimercado Long and Short Direcional', 'Multimercado', 'Estratégia', 'Long and Short Direcional', 'MULTIMERCADO', 43, 'Fundos long/short com exposição direcional'),
('MM_LS_NEU', 'Multimercado Long and Short Neutro', 'Multimercado', 'Estratégia', 'Long and Short Neutro', 'MULTIMERCADO', 44, 'Fundos long/short com exposição neutra'),
('MM_JRS', 'Multimercado Juros e Moedas', 'Multimercado', 'Estratégia', 'Juros e Moedas', 'MULTIMERCADO', 45, 'Fundos focados em juros e câmbio'),
('MM_LIVRE', 'Multimercado Livre', 'Multimercado', 'Estratégia', 'Livre', 'MULTIMERCADO', 46, 'Fundos sem estratégia específica definida'),
('MM_CAP_PROT', 'Multimercado Capital Protegido', 'Multimercado', 'Estratégia', 'Capital Protegido', 'MULTIMERCADO', 47, 'Fundos com proteção de capital'),
('MM_EST_ESP', 'Multimercado Estratégia Específica', 'Multimercado', 'Estratégia', 'Específica', 'MULTIMERCADO', 48, 'Fundos com estratégias específicas não enquadradas'),
('MM_BAL', 'Multimercado Balanceados', 'Multimercado', 'Alocação', 'Balanceados', 'MULTIMERCADO', 49, 'Fundos com alocação balanceada entre classes'),
('MM_INV_EXT', 'Multimercado Investimento no Exterior', 'Multimercado', 'Investimento no Exterior', NULL, 'MULTIMERCADO', 50, 'Fundos multimercado com investimentos no exterior');

-- Cambial
INSERT INTO classificacao_anbima (codigo, nome, nivel1, nivel2, nivel3, classificacao_cvm, ordem_exibicao, descricao) VALUES
('CB_DOLAR', 'Cambial Dólar', 'Cambial', 'Moeda', 'Dólar', 'CAMBIAL', 60, 'Fundos com exposição ao dólar americano'),
('CB_EURO', 'Cambial Euro', 'Cambial', 'Moeda', 'Euro', 'CAMBIAL', 61, 'Fundos com exposição ao euro'),
('CB_OUTRAS', 'Cambial Outras Moedas', 'Cambial', 'Moeda', 'Outras', 'CAMBIAL', 62, 'Fundos com exposição a outras moedas');
```

---

## 7. Integração com Wizard (Slice 03)

### 7.1 Fluxo no Frontend

```typescript
// Ao selecionar classificação CVM
onClassificacaoCvmChange(cvm: string): void {
  this.classificacaoAnbimaService
    .listar({ classificacaoCvm: cvm })
    .subscribe(response => {
      this.classificacoesAnbimaDisponiveis = response.items;
      
      // Limpar seleção se não compatível
      if (this.form.value.classificacaoAnbima) {
        const atual = response.items.find(
          c => c.codigo === this.form.value.classificacaoAnbima
        );
        if (!atual) {
          this.form.patchValue({ classificacaoAnbima: null });
        }
      }
      
      // Se não há classificações ANBIMA, ocultar campo
      this.mostrarCampoAnbima = response.items.length > 0;
    });
}
```

### 7.2 Validação Backend

```csharp
// No validador do wizard
RuleFor(x => x.Classificacao.ClassificacaoAnbima)
    .MustAsync(async (command, codigoAnbima, ct) =>
    {
        if (string.IsNullOrEmpty(codigoAnbima))
            return true; // Campo opcional
        
        var classificacao = await _db.ClassificacoesAnbima
            .FirstOrDefaultAsync(c => c.Codigo == codigoAnbima, ct);
        
        if (classificacao is null)
            return false;
        
        return classificacao.ClassificacaoCvm == command.Classificacao.ClassificacaoCvm;
    })
    .WithMessage("Classificação ANBIMA incompatível com classificação CVM selecionada");
```

---

## 8. Critérios de Aceite

- [ ] Entidade `ClassificacaoAnbima` criada com migration
- [ ] Seed data inserido com todas as classificações
- [ ] `GET /parametros/classificacoes-anbima` retorna lista filtrada
- [ ] `GET /parametros/classificacoes-anbima/{codigo}` retorna item específico
- [ ] `GET /parametros/classificacoes-anbima/niveis` retorna hierarquia
- [ ] `GET /parametros/classificacoes-anbima/verificar` valida compatibilidade
- [ ] Filtro por `classificacao_cvm` funciona corretamente
- [ ] Classificações CVM sem ANBIMA retornam lista vazia com mensagem
- [ ] Integração com wizard valida compatibilidade

---

## 9. Referências

- [Classificação ANBIMA de Fundos](https://www.anbima.com.br/pt_br/representar/fundos-de-investimento/classificacao-de-fundos.htm)
- [Resolução CVM 175/2022](https://www.gov.br/cvm/pt-br/assuntos/regulados/fundos-de-investimento)
- Diretriz ANBIMA de Classificação de Fundos (atualização 2023)

---

*Especificação: Lookup de Classificações ANBIMA*
*Sistema Fund Accounting - Janeiro/2026*
