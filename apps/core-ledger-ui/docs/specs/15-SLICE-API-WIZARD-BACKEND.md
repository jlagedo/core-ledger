# SLICE 15: API do Wizard - Especificação para Backend

## Objetivo

Especificar a implementação dos endpoints da API REST para o Wizard de Cadastro de Fundos no backend .NET 10.

---

## Estado Atual da API

### ✅ Entidades Já Existentes (Reutilizar)

| Entidade | Ação |
|----------|------|
| `Fundo` | Reutilizar |
| `FundoClasse` | Reutilizar |
| `FundoSubclasse` | Reutilizar |
| `FundoTaxa` | Reutilizar + adicionar campos |
| `FundoPrazo` | Reutilizar + adicionar campos |
| `FundoVinculo` | Reutilizar (usa FK para Instituicao) |
| `FundoParametrosFIDC` | Reutilizar |
| `Instituicao` | Reutilizar |

### ❌ Entidades a Criar

| Entidade | Prioridade |
|----------|------------|
| `FundoParametrosCota` | 🔴 BLOQUEANTE |
| `WizardRascunho` | 🟡 MÉDIA |
| `DocumentoTemporario` | 🟡 MÉDIA |

---

## 1. Criar Entidade FundoParametrosCota

### Modelo

```csharp
public class FundoParametrosCota
{
    public long Id { get; set; }
    public Guid FundoId { get; set; }
    
    public int CasasDecimaisCota { get; set; } = 8;
    public int CasasDecimaisQuantidade { get; set; } = 6;
    public int CasasDecimaisPl { get; set; } = 2;
    public TipoCota TipoCota { get; set; }
    public TimeOnly HorarioCorte { get; set; }
    public decimal CotaInicial { get; set; }
    public DateOnly DataCotaInicial { get; set; }
    public string FusoHorario { get; set; } = "America/Sao_Paulo";
    public bool PermiteCotaEstimada { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public Fundo Fundo { get; set; } = null!;
}

public enum TipoCota
{
    Fechamento,
    Abertura
}
```

### Migration

```sql
CREATE TABLE fundo_parametros_cota (
    id BIGSERIAL PRIMARY KEY,
    fundo_id UUID NOT NULL UNIQUE REFERENCES fundo(id) ON DELETE CASCADE,
    casas_decimais_cota SMALLINT NOT NULL DEFAULT 8,
    casas_decimais_quantidade SMALLINT NOT NULL DEFAULT 6,
    casas_decimais_pl SMALLINT NOT NULL DEFAULT 2,
    tipo_cota VARCHAR(20) NOT NULL,
    horario_corte TIME NOT NULL,
    cota_inicial DECIMAL(20,8) NOT NULL,
    data_cota_inicial DATE NOT NULL,
    fuso_horario VARCHAR(50) NOT NULL DEFAULT 'America/Sao_Paulo',
    permite_cota_estimada BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX idx_fundo_parametros_cota_fundo ON fundo_parametros_cota(fundo_id);
```

---

## 2. Alterações em Entidades Existentes

### 2.1 FundoTaxa - Adicionar Campo

```csharp
// Adicionar à entidade existente
public bool? LinhaDaguaGlobal { get; set; }
```

```sql
ALTER TABLE fundo_taxa ADD COLUMN linha_dagua_global BOOLEAN;
```

### 2.2 FundoPrazo - Adicionar Campos

```csharp
// Adicionar à entidade existente
public string TipoCalendario { get; set; } = "NACIONAL";
public bool PermiteResgateProgramado { get; set; }
public int? PrazoMaximoProgramacao { get; set; }
```

```sql
ALTER TABLE fundo_prazo 
    ADD COLUMN tipo_calendario VARCHAR(20) DEFAULT 'NACIONAL',
    ADD COLUMN permite_resgate_programado BOOLEAN DEFAULT FALSE,
    ADD COLUMN prazo_maximo_programacao SMALLINT;
```

### 2.3 FundoClasse - Adicionar Campos

```csharp
// Adicionar à entidade existente
public bool PermiteResgateAntecipado { get; set; } = true;
public DateOnly? DataEncerramento { get; set; }
public string? MotivoEncerramento { get; set; }
```

```sql
ALTER TABLE fundo_classe
    ADD COLUMN permite_resgate_antecipado BOOLEAN DEFAULT TRUE,
    ADD COLUMN data_encerramento DATE,
    ADD COLUMN motivo_encerramento VARCHAR(200);
```

### 2.4 Enum RegimeTributacao - Adicionar Valores

```csharp
public enum RegimeTributacao
{
    Ordinario,           // Existente
    FundosFechados,      // Existente
    Isento,              // NOVO
    Previdencia          // NOVO
}
```

---

## 3. Mapeamento de Nomenclatura (Wizard → API)

O wizard usa nomenclatura diferente da API existente. O AutoMapper deve fazer a conversão:

| Campo Wizard | Campo API Existente |
|--------------|---------------------|
| `tipo_operacao` | `TipoPrazoOperacional` |
| `prazo_cotizacao` | `DiasCotizacao` |
| `prazo_liquidacao` | `DiasLiquidacao` |
| `forma_cobranca` | `PeriodicidadePagamento` |
| `cnpj_instituicao` | Resolver para `InstituicaoId` |

### AutoMapper Profile

```csharp
public class WizardMappingProfile : Profile
{
    public WizardMappingProfile()
    {
        // Prazo: Wizard → Entidade existente
        CreateMap<PrazoWizardDto, FundoPrazo>()
            .ForMember(d => d.TipoPrazoOperacional,
                opt => opt.MapFrom(s => ParseEnum<TipoPrazoOperacional>(s.TipoOperacao)))
            .ForMember(d => d.DiasCotizacao,
                opt => opt.MapFrom(s => s.PrazoCotizacao))
            .ForMember(d => d.DiasLiquidacao,
                opt => opt.MapFrom(s => s.PrazoLiquidacao));
        
        // Taxa: Wizard → Entidade existente
        CreateMap<TaxaWizardDto, FundoTaxa>()
            .ForMember(d => d.PeriodicidadePagamento,
                opt => opt.MapFrom(s => MapFormaCobranca(s.FormaCobranca)));
        
        // ParametrosCota: Wizard → Nova entidade
        CreateMap<ParametrosCotaDto, FundoParametrosCota>();
        
        // Classe: Wizard → Entidade existente
        CreateMap<ClasseWizardDto, FundoClasse>();
        
        // ParametrosFIDC: Wizard → Entidade existente
        CreateMap<ParametrosFidcDto, FundoParametrosFIDC>();
    }
}
```

---

## 4. Endpoints a Implementar

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/v1/fundos/wizard` | Criação completa do fundo |
| POST | `/api/v1/fundos/wizard/validar` | Validação prévia sem criar |
| GET | `/api/v1/fundos/verificar-cnpj/{cnpj}` | Verificar disponibilidade CNPJ |
| POST | `/api/v1/wizard/rascunhos` | Salvar rascunho |
| GET | `/api/v1/wizard/rascunhos` | Listar rascunhos do usuário |
| GET | `/api/v1/wizard/rascunhos/{id}` | Obter rascunho específico |
| PUT | `/api/v1/wizard/rascunhos/{id}` | Atualizar rascunho |
| DELETE | `/api/v1/wizard/rascunhos/{id}` | Excluir rascunho |
| POST | `/api/v1/wizard/documentos/temp` | Upload temporário de documento |

---

## 1. POST /api/v1/fundos/wizard

### Descrição

Cria o fundo completo com todas as entidades relacionadas em uma única transação atômica.

### Request

```http
POST /api/v1/fundos/wizard
Content-Type: application/json
Authorization: Bearer {token}
```

### Request Body (DTO)

```csharp
public class CriarFundoWizardCommand
{
    public Guid? RascunhoId { get; set; }
    public IdentificacaoDto Identificacao { get; set; } = null!;
    public ClassificacaoDto Classificacao { get; set; } = null!;
    public CaracteristicasDto Caracteristicas { get; set; } = null!;
    public ParametrosCotaDto ParametrosCota { get; set; } = null!;
    public List<TaxaDto> Taxas { get; set; } = new();
    public List<PrazoDto> Prazos { get; set; } = new();
    public List<ClasseDto> Classes { get; set; } = new();
    public List<VinculoDto> Vinculos { get; set; } = new();
    public ParametrosFidcDto? ParametrosFidc { get; set; }
    public List<Guid> DocumentosTempIds { get; set; } = new();
}
```

### DTOs Detalhados

```csharp
public class IdentificacaoDto
{
    public string Cnpj { get; set; } = null!;              // 14 dígitos
    public string RazaoSocial { get; set; } = null!;       // max 200
    public string NomeFantasia { get; set; } = null!;      // max 100
    public string? NomeCurto { get; set; }                 // max 50
    public string TipoFundo { get; set; } = null!;         // enum
    public DateOnly DataConstituicao { get; set; }
    public DateOnly DataInicioAtividade { get; set; }
}

public class ClassificacaoDto
{
    public string ClassificacaoCvm { get; set; } = null!;  // enum
    public string? ClassificacaoAnbima { get; set; }       // max 50
    public string? CodigoAnbima { get; set; }              // max 20
    public string PublicoAlvo { get; set; } = null!;       // enum
    public string Tributacao { get; set; } = null!;        // enum
}

public class CaracteristicasDto
{
    public string Condominio { get; set; } = null!;        // enum
    public string Prazo { get; set; } = null!;             // enum
    public DateOnly? DataEncerramento { get; set; }
    public bool Exclusivo { get; set; }
    public bool Reservado { get; set; }
    public bool PermiteAlavancagem { get; set; }
    public decimal? LimiteAlavancagem { get; set; }
    public bool AceitaCripto { get; set; }
    public decimal? PercentualExterior { get; set; }
}

public class ParametrosCotaDto
{
    public int CasasDecimaisCota { get; set; }             // 4-10, default 8
    public int CasasDecimaisQuantidade { get; set; }       // 4-8, default 6
    public int CasasDecimaisPl { get; set; }               // 2-4, default 2
    public string TipoCota { get; set; } = null!;          // enum
    public TimeOnly HorarioCorte { get; set; }
    public decimal CotaInicial { get; set; }
    public DateOnly DataCotaInicial { get; set; }
    public string FusoHorario { get; set; } = "America/Sao_Paulo";
    public bool PermiteCotaEstimada { get; set; }
}

public class TaxaDto
{
    public string TipoTaxa { get; set; } = null!;          // enum
    public decimal Percentual { get; set; }                // 8,6
    public decimal? PercentualMinimo { get; set; }
    public decimal? PercentualMaximo { get; set; }
    public string BaseCalculo { get; set; } = null!;       // enum
    public string FormaCobranca { get; set; } = null!;     // enum
    public DateOnly DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public int? BenchmarkId { get; set; }                  // FK Indexador
    public decimal? PercentualBenchmark { get; set; }
    public bool PossuiHurdle { get; set; }
    public bool PossuiHighWaterMark { get; set; }
    public bool? LinhaDaguaGlobal { get; set; }
    public Guid? ClasseId { get; set; }                    // FK Classe
}

public class PrazoDto
{
    public string TipoOperacao { get; set; } = null!;      // enum
    public int PrazoCotizacao { get; set; }                // D+X
    public int PrazoLiquidacao { get; set; }               // D+X
    public string TipoCalendario { get; set; } = "NACIONAL";
    public decimal? ValorMinimoInicial { get; set; }
    public decimal? ValorMinimoAdicional { get; set; }
    public decimal? ValorMinimoResgate { get; set; }
    public decimal? ValorMinimoPermanencia { get; set; }
    public int? PrazoCarenciaDias { get; set; }
    public bool PermiteResgateTotal { get; set; } = true;
    public bool PermiteResgateProgramado { get; set; }
    public int? PrazoMaximoProgramacao { get; set; }
    public Guid? ClasseId { get; set; }                    // FK Classe
}

public class ClasseDto
{
    public string CodigoClasse { get; set; } = null!;      // max 20
    public string NomeClasse { get; set; } = null!;        // max 100
    public string? CnpjClasse { get; set; }                // 14 dígitos
    public Guid? ClassePaiId { get; set; }                 // auto-ref subclasses
    public int Nivel { get; set; } = 1;                    // 1=classe, 2=subclasse
    public string PublicoAlvo { get; set; } = null!;       // enum
    public string? TipoClasseFidc { get; set; }            // enum
    public int? OrdemSubordinacao { get; set; }
    public decimal? RentabilidadeAlvo { get; set; }
    public decimal? IndiceSubordinacaoMinimo { get; set; }
    public decimal? ValorMinimoAplicacao { get; set; }
    public decimal? ValorMinimoPermanencia { get; set; }
    public bool ResponsabilidadeLimitada { get; set; } = true;
    public bool SegregacaoPatrimonial { get; set; } = true;
    public decimal? TaxaAdministracao { get; set; }
    public decimal? TaxaGestao { get; set; }
    public decimal? TaxaPerformance { get; set; }
    public int? BenchmarkId { get; set; }
    public bool PermiteResgateAntecipado { get; set; } = true;
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataEncerramento { get; set; }
}

public class VinculoDto
{
    public string TipoVinculo { get; set; } = null!;       // enum
    public string CnpjInstituicao { get; set; } = null!;   // 14 dígitos
    public string NomeInstituicao { get; set; } = null!;   // max 200
    public string? CodigoCvm { get; set; }                 // max 20
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public string? MotivoFim { get; set; }
    public string? ResponsavelNome { get; set; }
    public string? ResponsavelEmail { get; set; }
    public string? ResponsavelTelefone { get; set; }
}

public class ParametrosFidcDto
{
    public string TipoFidc { get; set; } = null!;          // enum
    public List<string> TipoRecebiveis { get; set; } = new();
    public int? PrazoMedioCarteira { get; set; }
    public decimal? IndiceSubordinacaoAlvo { get; set; }
    public decimal? ProvisaoDevedoresDuvidosos { get; set; }
    public decimal? LimiteConcentracaoCedente { get; set; }
    public decimal? LimiteConcentracaoSacado { get; set; }
    public bool PossuiCoobrigacao { get; set; }
    public decimal? PercentualCoobrigacao { get; set; }
    public bool PermiteCessaoParcial { get; set; } = true;
    public string? RatingMinimo { get; set; }
    public string? AgenciaRating { get; set; }
    public string? RegistradoraRecebiveis { get; set; }    // enum
    public string? ContaRegistradora { get; set; }
    public bool IntegracaoRegistradora { get; set; }
}
```

### Responses

**201 Created:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "cnpj": "12345678000199",
  "razaoSocial": "FUNDO DE INVESTIMENTO RENDA FIXA CREDITO PRIVADO",
  "nomeFantasia": "FI RF CREDITO PRIVADO",
  "tipoFundo": "FI",
  "situacao": "EM_CONSTITUICAO",
  "createdAt": "2024-01-20T10:30:00Z"
}
```

**400 Bad Request:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Erro de Validação",
  "status": 400,
  "errors": {
    "identificacao.cnpj": ["CNPJ inválido"],
    "taxas": ["Taxa de administração é obrigatória"],
    "vinculos": ["Vínculo de CUSTODIANTE é obrigatório"]
  }
}
```

**409 Conflict:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "CNPJ já cadastrado",
  "status": 409,
  "fundoExistente": {
    "id": "uuid-existente",
    "nomeFantasia": "OUTRO FUNDO RF"
  }
}
```

---

## 2. POST /api/v1/fundos/wizard/validar

### Descrição

Valida o payload do wizard sem criar o fundo. Útil para validação na etapa de revisão.

### Request Body

Mesmo DTO de `CriarFundoWizardCommand`.

### Responses

**200 OK (válido):**
```json
{
  "valido": true,
  "avisos": [
    "Regulamento não anexado (recomendado)",
    "Índice de subordinação alvo não definido para FIDC"
  ]
}
```

**200 OK (inválido):**
```json
{
  "valido": false,
  "erros": {
    "vinculos": ["CUSTODIANTE é obrigatório"]
  },
  "avisos": []
}
```

---

## 3. GET /api/v1/fundos/verificar-cnpj/{cnpj}

### Descrição

Verifica se um CNPJ já está cadastrado para algum fundo.

### Response

**200 OK (disponível):**
```json
{
  "disponivel": true
}
```

**200 OK (já existe):**
```json
{
  "disponivel": false,
  "fundoId": "550e8400-e29b-41d4-a716-446655440001",
  "nomeFantasia": "FUNDO EXISTENTE RF"
}
```

---

## 4. Endpoints de Rascunho

### POST /api/v1/wizard/rascunhos

```json
// Request
{
  "etapaAtual": 6,
  "dados": { /* JSON do estado completo */ }
}

// Response 201
{
  "id": "uuid",
  "nomeIdentificacao": "FI RF CREDITO PRIVADO",
  "etapaAtual": 6,
  "progresso": 60,
  "createdAt": "2024-01-20T10:30:00Z",
  "expiresAt": "2024-02-19T10:30:00Z"
}
```

### GET /api/v1/wizard/rascunhos

```json
// Response 200
{
  "items": [
    {
      "id": "uuid",
      "nomeIdentificacao": "FI RF CREDITO PRIVADO",
      "cnpj": "12345678000199",
      "etapaAtual": 6,
      "progresso": 60,
      "updatedAt": "2024-01-20T15:30:00Z",
      "expiresAt": "2024-02-19T15:30:00Z",
      "diasParaExpirar": 28
    }
  ],
  "total": 2,
  "limite": 5
}
```

### GET /api/v1/wizard/rascunhos/{id}

```json
// Response 200
{
  "id": "uuid",
  "etapaAtual": 6,
  "progresso": 60,
  "dados": {
    "identificacao": { /* ... */ },
    "classificacao": { /* ... */ },
    "caracteristicas": { /* ... */ },
    "parametrosCota": { /* ... */ },
    "taxas": [ /* ... */ ],
    "prazos": [ /* ... */ ],
    "classes": [ /* ... */ ],
    "vinculos": [ /* ... */ ],
    "parametrosFidc": { /* ... */ }
  },
  "updatedAt": "2024-01-20T15:30:00Z"
}
```

---

## 5. POST /api/v1/wizard/documentos/temp

### Descrição

Upload temporário de documento antes da criação do fundo.

### Request

```http
POST /api/v1/wizard/documentos/temp
Content-Type: multipart/form-data
```

Form fields:
- `arquivo`: File (PDF, DOC, DOCX)
- `tipoDocumento`: string (enum)
- `dataVigencia`: date (ISO 8601)

### Response 201

```json
{
  "tempId": "uuid",
  "arquivoNome": "regulamento_fundo_rf.pdf",
  "arquivoTamanho": 2450000,
  "arquivoHash": "sha256:abc123...",
  "expiresAt": "2024-01-21T10:30:00Z"
}
```

---

## 6. Validações (FluentValidation)

### CriarFundoWizardCommandValidator

```csharp
public class CriarFundoWizardCommandValidator : AbstractValidator<CriarFundoWizardCommand>
{
    public CriarFundoWizardCommandValidator()
    {
        // Identificação
        RuleFor(x => x.Identificacao).NotNull();
        RuleFor(x => x.Identificacao.Cnpj)
            .NotEmpty()
            .Length(14)
            .Must(BeValidCnpj).WithMessage("CNPJ inválido");
        RuleFor(x => x.Identificacao.RazaoSocial)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(200);
        RuleFor(x => x.Identificacao.DataInicioAtividade)
            .GreaterThanOrEqualTo(x => x.Identificacao.DataConstituicao)
            .WithMessage("Data início deve ser >= data constituição");
        
        // Classificação
        RuleFor(x => x.Classificacao).NotNull();
        RuleFor(x => x.Classificacao.ClassificacaoCvm)
            .NotEmpty()
            .Must(BeValidClassificacaoCvm);
        
        // Taxas
        RuleFor(x => x.Taxas)
            .Must(t => t.Any(tx => tx.TipoTaxa == "ADMINISTRACAO"))
            .WithMessage("Taxa de administração é obrigatória");
        RuleForEach(x => x.Taxas).SetValidator(new TaxaDtoValidator());
        
        // Prazos
        RuleFor(x => x.Prazos)
            .Must(p => p.Any(pr => pr.TipoOperacao == "APLICACAO"))
            .WithMessage("Prazo de aplicação é obrigatório");
        RuleFor(x => x.Prazos)
            .Must(p => p.Any(pr => pr.TipoOperacao == "RESGATE"))
            .WithMessage("Prazo de resgate é obrigatório");
        RuleForEach(x => x.Prazos).SetValidator(new PrazoDtoValidator());
        
        // Vínculos obrigatórios
        RuleFor(x => x.Vinculos)
            .Must(v => v.Any(vc => vc.TipoVinculo == "ADMINISTRADOR"))
            .WithMessage("Vínculo de ADMINISTRADOR é obrigatório");
        RuleFor(x => x.Vinculos)
            .Must(v => v.Any(vc => vc.TipoVinculo == "GESTOR"))
            .WithMessage("Vínculo de GESTOR é obrigatório");
        RuleFor(x => x.Vinculos)
            .Must(v => v.Any(vc => vc.TipoVinculo == "CUSTODIANTE"))
            .WithMessage("Vínculo de CUSTODIANTE é obrigatório");
        
        // FIDC: Parâmetros obrigatórios
        When(x => x.Identificacao.TipoFundo == "FIDC" || 
                  x.Identificacao.TipoFundo == "FIDC_NP", () =>
        {
            RuleFor(x => x.ParametrosFidc)
                .NotNull()
                .WithMessage("Parâmetros FIDC são obrigatórios para fundos FIDC");
            RuleFor(x => x.Classes)
                .Must(c => c.Count > 0)
                .WithMessage("FIDC deve ter pelo menos uma classe");
        });
        
        // Classes
        RuleForEach(x => x.Classes).SetValidator(new ClasseDtoValidator());
    }
}
```

---

## 7. Fluxo de Transação

```csharp
public async Task<FundoCriadoResponse> Handle(
    CriarFundoWizardCommand command, 
    CancellationToken ct)
{
    // 1. Verificar CNPJ
    if (await _fundoRepo.ExistsByCnpjAsync(command.Identificacao.Cnpj, ct))
        throw new CnpjJaCadastradoException(...);
    
    // 2. Iniciar transação
    await using var tx = await _db.Database.BeginTransactionAsync(ct);
    
    try
    {
        // 3. Criar Fundo
        var fundo = _mapper.Map<Fundo>(command);
        fundo.Id = Guid.NewGuid();
        fundo.Situacao = SituacaoFundo.EmConstituicao;
        _db.Fundos.Add(fundo);
        
        // 4. Criar Parâmetros de Cota
        var paramCota = _mapper.Map<FundoParametrosCota>(command.ParametrosCota);
        paramCota.FundoId = fundo.Id;
        _db.FundoParametrosCota.Add(paramCota);
        
        // 5. Criar Taxas
        foreach (var taxaDto in command.Taxas)
        {
            var taxa = _mapper.Map<FundoTaxa>(taxaDto);
            taxa.FundoId = fundo.Id;
            taxa.Ativo = true;
            _db.FundoTaxas.Add(taxa);
        }
        
        // 6. Criar Prazos
        foreach (var prazoDto in command.Prazos)
        {
            var prazo = _mapper.Map<FundoPrazo>(prazoDto);
            prazo.FundoId = fundo.Id;
            prazo.Ativo = true;
            _db.FundoPrazos.Add(prazo);
        }
        
        // 7. Criar Classes (guardar mapa de IDs temporários)
        var classeIdMap = new Dictionary<string, Guid>();
        foreach (var classeDto in command.Classes)
        {
            var classe = _mapper.Map<FundoClasse>(classeDto);
            classe.Id = Guid.NewGuid();
            classe.FundoId = fundo.Id;
            classe.Ativo = true;
            classeIdMap[classeDto.CodigoClasse] = classe.Id;
            _db.FundoClasses.Add(classe);
        }
        
        // 8. Criar Vínculos
        foreach (var vinculoDto in command.Vinculos)
        {
            var vinculo = _mapper.Map<FundoVinculo>(vinculoDto);
            vinculo.FundoId = fundo.Id;
            vinculo.Ativo = true;
            _db.FundoVinculos.Add(vinculo);
        }
        
        // 9. Criar Parâmetros FIDC (se aplicável)
        if (command.ParametrosFidc != null)
        {
            var paramFidc = _mapper.Map<FidcParametros>(command.ParametrosFidc);
            paramFidc.FundoId = fundo.Id;
            _db.FidcParametros.Add(paramFidc);
        }
        
        // 10. Salvar para gerar IDs
        await _db.SaveChangesAsync(ct);
        
        // 11. Vincular documentos temporários
        if (command.DocumentosTempIds.Any())
        {
            await _docService.VincularTempAsync(fundo.Id, command.DocumentosTempIds, ct);
        }
        
        // 12. Commit
        await tx.CommitAsync(ct);
        
        // 13. Excluir rascunho (se existir)
        if (command.RascunhoId.HasValue)
        {
            await _rascunhoService.ExcluirAsync(command.RascunhoId.Value, ct);
        }
        
        // 14. Publicar evento
        await _mediator.Publish(new FundoCriadoEvent(fundo.Id), ct);
        
        return _mapper.Map<FundoCriadoResponse>(fundo);
    }
    catch
    {
        // Rollback automático pelo using
        throw;
    }
}
```

---

## 8. Estrutura de Projeto

```
FundAccounting.API/
├── Controllers/
│   ├── FundosWizardController.cs
│   └── WizardRascunhosController.cs
├── Commands/
│   ├── CriarFundoWizard/
│   │   ├── CriarFundoWizardCommand.cs
│   │   ├── CriarFundoWizardCommandHandler.cs
│   │   └── CriarFundoWizardCommandValidator.cs
│   └── ValidarFundoWizard/
│       └── ...
├── DTOs/
│   ├── IdentificacaoDto.cs
│   ├── ClassificacaoDto.cs
│   ├── CaracteristicasDto.cs
│   ├── ParametrosCotaDto.cs
│   ├── TaxaDto.cs
│   ├── PrazoDto.cs
│   ├── ClasseDto.cs
│   ├── VinculoDto.cs
│   └── ParametrosFidcDto.cs
└── Validators/
    ├── CnpjValidator.cs
    ├── TaxaDtoValidator.cs
    ├── PrazoDtoValidator.cs
    └── ClasseDtoValidator.cs
```

---

## Critérios de Aceite

- [ ] POST /api/v1/fundos/wizard cria fundo completo
- [ ] Transação é atômica (rollback em caso de erro)
- [ ] Todas as validações funcionam
- [ ] CNPJ duplicado retorna 409
- [ ] Erros de validação retornam 400 com detalhes
- [ ] Documentos temporários são vinculados
- [ ] Rascunho é excluído após criação bem-sucedida
- [ ] Endpoint de validação retorna avisos
- [ ] Endpoints de rascunho funcionam
- [ ] Upload temporário de documentos funciona

---

## Dependências

- Entidades de domínio implementadas
- DbContext configurado
- FluentValidation configurado
- AutoMapper configurado
- Serviço de documentos implementado

---

*Especificação da API do Wizard*
*Sistema Fund Accounting - Janeiro/2026*
