# SLICE 09: Etapa 8 - Vínculos Institucionais

## Objetivo

Implementar a oitava etapa do wizard para configuração dos vínculos com instituições (administrador, gestor, custodiante, distribuidor, auditor).

## Escopo

### Modelo de Dados - Vínculo

> **IMPORTANTE**: A API existente usa `InstituicaoId` (FK) em vez de dados inline. O wizard envia `cnpj_instituicao` e o backend resolve para o ID da instituição cadastrada.

| Campo Wizard | Campo API | Descrição |
|--------------|-----------|-----------|
| `tipo_vinculo` | `TipoVinculo` | Tipo do vínculo |
| `cnpj_instituicao` | → resolver para `InstituicaoId` | Backend busca instituição pelo CNPJ |
| `nome_instituicao` | (ignorado) | API já tem na entidade Instituicao |
| `codigo_cvm` | (ignorado) | API já tem na entidade Instituicao |
| `data_inicio` | `DataInicio` | Início do vínculo |
| `data_fim` | `DataFim` | Fim do vínculo |
| `motivo_fim` | `MotivoFim` | Motivo do término |
| `responsavel_nome` | `ResponsavelNome` | Nome do responsável |
| `responsavel_email` | `ResponsavelEmail` | Email do responsável |
| `responsavel_telefone` | `ResponsavelTelefone` | Telefone do responsável |
| - | `Ativo` | Sempre `true` na criação |

### DTO do Wizard (Entrada)

```csharp
public class VinculoWizardDto
{
    public string TipoVinculo { get; set; } = null!;
    public string CnpjInstituicao { get; set; } = null!;  // Wizard envia CNPJ
    public string? NomeInstituicao { get; set; }          // Opcional, para exibição
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public string? MotivoFim { get; set; }
    public string? ResponsavelNome { get; set; }
    public string? ResponsavelEmail { get; set; }
    public string? ResponsavelTelefone { get; set; }
}
```

### Resolução de Instituição no Backend

```csharp
// No handler, antes de criar vínculos:
var cnpjs = request.Vinculos.Select(v => v.CnpjInstituicao).Distinct();
var instituicoes = await _db.Instituicoes
    .Where(i => cnpjs.Contains(i.Cnpj))
    .ToDictionaryAsync(i => i.Cnpj, i => i.Id, ct);

// Validar que todas existem
var faltantes = cnpjs.Except(instituicoes.Keys).ToList();
if (faltantes.Any())
    throw new InstituicaoNaoEncontradaException(faltantes);

// Criar vínculo com FK
var vinculo = new FundoVinculo
{
    FundoId = fundo.Id,
    InstituicaoId = instituicoes[dto.CnpjInstituicao],  // FK resolvido
    TipoVinculo = ParseEnum<TipoVinculo>(dto.TipoVinculo),
    DataInicio = dto.DataInicio,
    // ... outros campos
};
```

---

### Valores do Enum `tipo_vinculo`

| Valor | Obrigatório | Descrição |
|-------|-------------|-----------|
| `ADMINISTRADOR` | Sim | Administrador Fiduciário |
| `GESTOR` | Sim | Gestor de Recursos |
| `CUSTODIANTE` | Sim | Custodiante |
| `DISTRIBUIDOR` | Não | Distribuidor |
| `AUDITOR` | Não | Auditor Independente |
| `ESCRITURADOR` | Não | Escriturador de cotas |
| `CONTROLADOR` | Não | Controlador (se terceirizado) |
| `CONSULTORIA_CREDITO` | Não | Consultoria de crédito (FIDC) |
| `AGENTE_COBRANCA` | Não | Agente de Cobrança (FIDC) |
| `CEDENTE` | Não | Cedente de recebíveis (FIDC) |
| `FORMADOR_MERCADO` | Não | Formador de mercado (ETF/FII) |

---

## Requisitos Funcionais

### RF-01: Vínculos Obrigatórios

Os seguintes vínculos são obrigatórios para todos os fundos:
- ADMINISTRADOR
- GESTOR
- CUSTODIANTE

### RF-02: Vínculos Adicionais para FIDC

Se tipo_fundo = FIDC ou FIDC_NP:
- AGENTE_COBRANCA é recomendado (warning se não informado)

### RF-03: Busca de Instituições

- Implementar autocomplete para busca de instituições **já cadastradas**
- Endpoint existente: `GET /api/v1/instituicoes?busca={termo}`
- A instituição **DEVE existir** previamente no sistema
- Exibir: Nome + CNPJ formatado

### RF-04: Instituição Não Encontrada

Se a instituição não existe na base:
- Exibir mensagem: "Instituição não encontrada no sistema."
- Duas opções:
  1. Link para cadastro completo: `/cadastros/instituicoes/novo`
  2. Modal de cadastro rápido (campos mínimos):
     - CNPJ (obrigatório)
     - Razão Social (obrigatório)
     - Nome Fantasia (opcional)
- Endpoint: `POST /api/v1/instituicoes`
- Após criar, selecionar automaticamente

### RF-05: Um Vínculo por Tipo (Ativo)

- Apenas um vínculo ativo por tipo por vez
- Se adicionar novo GESTOR, o anterior deve ter data_fim

### RF-06: Mesmo CNPJ para Múltiplos Tipos

- Permitir que a mesma instituição seja ADMINISTRADOR e CUSTODIANTE
- Comum em fundos de bancos (gestão/administração integrada)

### RF-07: Validação de Datas

- data_inicio não pode ser futura
- data_fim deve ser >= data_inicio (se informada)

---

## Frontend

### Componente

**WizardStep8VinculosComponent**

### Layout Sugerido

```
┌─────────────────────────────────────────────────────────────────┐
│ VÍNCULOS INSTITUCIONAIS                                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│ ADMINISTRADOR FIDUCIÁRIO (obrigatório)                         │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ Instituição: [Buscar instituição...    ▼] [+ Nova]         │ │
│ │ → BTG PACTUAL SERVICOS FINANCEIROS DTVM                    │ │
│ │ CNPJ: 59.281.253/0001-23                                   │ │
│ │                                                             │ │
│ │ Data início: [01/02/2024]  Data fim: [________]            │ │
│ │ Contrato: [ADM-2024-001    ]                               │ │
│ │                                                             │ │
│ │ Responsável: [João Silva        ] [joao@btg.com          ] │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ GESTOR DE RECURSOS (obrigatório)                               │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ Instituição: [Buscar instituição...    ▼] [+ Nova]         │ │
│ │ → KINEA INVESTIMENTOS LTDA                                 │ │
│ │ CNPJ: 08.604.465/0001-33                                   │ │
│ │                                                             │ │
│ │ Data início: [01/02/2024]  Data fim: [________]            │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ CUSTODIANTE (obrigatório)                                      │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ [Preencher...]                                              │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ──────────────────────────────────────────────────────────────  │
│                                                                 │
│ VÍNCULOS OPCIONAIS                                [+ Adicionar] │
│                                                                 │
│ DISTRIBUIDOR                                                   │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ [Preencher...]                                          [🗑] │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Autocomplete de Instituições

- Debounce de 300ms na busca
- Mínimo 3 caracteres para buscar
- Exibir: Nome + CNPJ formatado
- Cache local das últimas buscas

---

## Backend

### Endpoint de Busca de Instituições

```
GET /api/v1/instituicoes
```

**Query Parameters:**
- `busca`: Termo de busca (nome ou CNPJ)
- `tipo`: Filtro por tipo de instituição (opcional)
- `page`: Página (default 1)
- `pageSize`: Tamanho (default 10)

**Response:**
```json
{
  "items": [
    {
      "id": "uuid",
      "cnpj": "59281253000123",
      "razao_social": "BTG PACTUAL SERVICOS FINANCEIROS DTVM",
      "nome_fantasia": "BTG DTVM",
      "tipos_habilitados": ["ADMINISTRADOR", "CUSTODIANTE", "DISTRIBUIDOR"]
    }
  ],
  "total": 150,
  "page": 1,
  "pageSize": 10
}
```

### Endpoint de Criação Rápida

```
POST /api/v1/instituicoes
```

**Request:**
```json
{
  "cnpj": "12345678000199",
  "razao_social": "NOVA INSTITUICAO LTDA",
  "nome_fantasia": "Nova Instituição"
}
```

### Entidade `FundoVinculo`

| Campo DB | Tipo | Constraints |
|----------|------|-------------|
| `id` | BIGINT | PK, auto-increment |
| `fundo_id` | UUID | FK |
| `tipo_vinculo` | VARCHAR(20) | NOT NULL |
| `instituicao_id` | UUID | FK |
| `data_inicio` | DATE | NOT NULL |
| `data_fim` | DATE | NULL |
| `contrato_numero` | VARCHAR(50) | NULL |
| `responsavel_nome` | VARCHAR(100) | NULL |
| `responsavel_email` | VARCHAR(100) | NULL |
| `observacoes` | TEXT | NULL |
| `ativo` | BOOLEAN | DEFAULT true |

### Validações Backend

- ADMINISTRADOR, GESTOR, CUSTODIANTE obrigatórios
- Apenas um vínculo ativo por tipo
- CNPJ da instituição válido
- Datas consistentes

---

## Critérios de Aceite

- [ ] Seções para vínculos obrigatórios renderizam
- [ ] Autocomplete de instituições funciona
- [ ] Modal de cadastro rápido funciona
- [ ] Validação de vínculos obrigatórios
- [ ] Campos opcionais renderizam
- [ ] Botão adicionar vínculo opcional funciona
- [ ] Mesma instituição pode ter múltiplos vínculos
- [ ] Dados persistem ao navegar

---

## Dependências

- Slice 01: Infraestrutura base
- Slice 02: Etapa 1 (tipo_fundo para vínculos de FIDC)
- Entidade Instituição deve existir no sistema

## Próximo Slice

→ `10-SLICE-DOCUMENTOS.md`
