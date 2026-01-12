# Plano de Implementação do Wizard - Considerando API Existente

## Status Atual da API

### ✅ Entidades Já Implementadas

| Entidade | Status | Observação |
|----------|--------|------------|
| `Fundo` | ✅ Completa | Todos os campos base |
| `FundoClasse` | ✅ Completa | Com subclasses |
| `FundoSubclasse` | ✅ Completa | Hierarquia funcional |
| `FundoTaxa` | ✅ Completa | Com TaxaPerformance |
| `FundoPrazo` | ✅ Completa | Com exceções |
| `FundoVinculo` | ✅ Completa | FK para Instituição |
| `FundoParametrosFIDC` | ✅ Completa | Todos os campos |
| `Instituicao` | ✅ Completa | Cadastro separado |

### ❌ Entidades Faltantes

| Entidade | Prioridade | Descrição |
|----------|------------|-----------|
| `FundoParametrosCota` | 🔴 BLOQUEANTE | Etapa 4 do wizard |
| `WizardRascunho` | 🟡 MÉDIA | Auto-save |
| `DocumentoTemporario` | 🟡 MÉDIA | Upload antes de criar fundo |

### ❌ Endpoints Faltantes

| Endpoint | Prioridade |
|----------|------------|
| `POST /api/v1/fundos/wizard` | 🔴 BLOQUEANTE |
| `POST /api/v1/fundos/wizard/validar` | 🟡 MÉDIA |
| `GET /api/v1/fundos/verificar-cnpj/{cnpj}` | 🟢 BAIXA |
| `CRUD /api/v1/wizard/rascunhos` | 🟡 MÉDIA |
| `POST /api/v1/wizard/documentos/temp` | 🟡 MÉDIA |

---

## Mapeamento de Nomenclatura

### Campos que Precisam de Mapeamento no DTO

| Especificação Wizard | API Existente | Ação |
|---------------------|---------------|------|
| `tipo_operacao` | `TipoPrazoOperacional` | Mapear no DTO |
| `prazo_cotizacao` | `DiasCotizacao` | Mapear no DTO |
| `prazo_liquidacao` | `DiasLiquidacao` | Mapear no DTO |
| `forma_cobranca` | `PeriodicidadePagamento` | Mapear no DTO |
| `cnpj_instituicao` | `InstituicaoId` (FK) | API usa FK, wizard envia CNPJ → resolver instituição |

### Enums que Precisam de Adição/Mapeamento

**Tributação:**

| Especificação | API Atual | Ação |
|---------------|-----------|------|
| `LONGO_PRAZO` | `Ordinario` | Mapear |
| `CURTO_PRAZO` | `Ordinario` | Mapear |
| `ACOES` | `Ordinario` | Mapear |
| `IMOBILIARIO` | `FundosFechados` | Mapear |
| `ISENTO` | ❌ Não existe | **Adicionar ao enum** |
| `PREVIDENCIA` | ❌ Não existe | **Adicionar ao enum** |

---

## Plano de Implementação Revisado

### Fase 1: Backend - Itens Bloqueantes

#### 1.1 Criar Entidade `FundoParametrosCota`

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

**Migration:**
```sql
CREATE TABLE fundo_parametros_cota (
    id BIGSERIAL PRIMARY KEY,
    fundo_id UUID NOT NULL UNIQUE REFERENCES fundo(id),
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
```

#### 1.2 Adicionar Valores ao Enum Tributação

```csharp
public enum RegimeTributacao
{
    Ordinario,           // Mapeia LONGO_PRAZO, CURTO_PRAZO, ACOES
    FundosFechados,      // Mapeia IMOBILIARIO
    Isento,              // NOVO
    Previdencia          // NOVO
}
```

#### 1.3 Adicionar Campos Faltantes às Entidades Existentes

**FundoTaxa:**
```csharp
public bool? LinhaDaguaGlobal { get; set; }
```

**FundoPrazo:**
```csharp
public string TipoCalendario { get; set; } = "NACIONAL";
public bool PermiteResgateProgramado { get; set; }
public int? PrazoMaximoProgramacao { get; set; }
```

**FundoClasse:**
```csharp
public bool PermiteResgateAntecipado { get; set; } = true;
public DateOnly? DataEncerramento { get; set; }
public string? MotivoEncerramento { get; set; }
```

#### 1.4 Criar Command `CriarFundoWizardCommand`

```csharp
public class CriarFundoWizardCommand : IRequest<FundoCriadoResponse>
{
    public Guid? RascunhoId { get; set; }
    public IdentificacaoDto Identificacao { get; set; } = null!;
    public ClassificacaoDto Classificacao { get; set; } = null!;
    public CaracteristicasDto Caracteristicas { get; set; } = null!;
    public ParametrosCotaDto ParametrosCota { get; set; } = null!;
    public List<TaxaWizardDto> Taxas { get; set; } = new();
    public List<PrazoWizardDto> Prazos { get; set; } = new();
    public List<ClasseWizardDto> Classes { get; set; } = new();
    public List<VinculoWizardDto> Vinculos { get; set; } = new();
    public ParametrosFidcDto? ParametrosFidc { get; set; }
    public List<Guid> DocumentosTempIds { get; set; } = new();
}
```

#### 1.5 Criar Handler com Transação Atômica

```csharp
public class CriarFundoWizardCommandHandler 
    : IRequestHandler<CriarFundoWizardCommand, FundoCriadoResponse>
{
    private readonly FundAccountingDbContext _db;
    private readonly IMapper _mapper;
    private readonly IInstituicaoRepository _instituicaoRepo;
    
    public async Task<FundoCriadoResponse> Handle(
        CriarFundoWizardCommand request, 
        CancellationToken ct)
    {
        // 1. Verificar CNPJ
        var cnpjExiste = await _db.Fundos
            .AnyAsync(f => f.Cnpj == request.Identificacao.Cnpj, ct);
        if (cnpjExiste)
            throw new CnpjJaCadastradoException(request.Identificacao.Cnpj);
        
        // 2. Resolver IDs de Instituições pelos CNPJs
        var instituicoesMap = await ResolverInstituicoesAsync(
            request.Vinculos.Select(v => v.CnpjInstituicao), ct);
        
        // 3. Iniciar transação
        await using var transaction = await _db.Database
            .BeginTransactionAsync(ct);
        
        try
        {
            // 4. Criar Fundo
            var fundo = MapearFundo(request);
            _db.Fundos.Add(fundo);
            
            // 5. Criar ParametrosCota (NOVA ENTIDADE)
            var paramCota = _mapper.Map<FundoParametrosCota>(request.ParametrosCota);
            paramCota.FundoId = fundo.Id;
            _db.FundoParametrosCota.Add(paramCota);
            
            // 6. Criar Taxas (reutiliza entidade existente)
            foreach (var taxaDto in request.Taxas)
            {
                var taxa = MapearTaxa(taxaDto, fundo.Id);
                _db.FundoTaxas.Add(taxa);
            }
            
            // 7. Criar Prazos (reutiliza entidade existente)
            foreach (var prazoDto in request.Prazos)
            {
                var prazo = MapearPrazo(prazoDto, fundo.Id);
                _db.FundoPrazos.Add(prazo);
            }
            
            // 8. Criar Classes (reutiliza entidade existente)
            foreach (var classeDto in request.Classes)
            {
                var classe = MapearClasse(classeDto, fundo.Id);
                _db.FundoClasses.Add(classe);
            }
            
            // 9. Criar Vínculos (usa FK para Instituição)
            foreach (var vinculoDto in request.Vinculos)
            {
                var instituicaoId = instituicoesMap[vinculoDto.CnpjInstituicao];
                var vinculo = MapearVinculo(vinculoDto, fundo.Id, instituicaoId);
                _db.FundoVinculos.Add(vinculo);
            }
            
            // 10. Criar Parâmetros FIDC (reutiliza entidade existente)
            if (request.ParametrosFidc != null)
            {
                var paramFidc = _mapper.Map<FundoParametrosFIDC>(request.ParametrosFidc);
                paramFidc.FundoId = fundo.Id;
                _db.FundoParametrosFIDC.Add(paramFidc);
            }
            
            // 11. Salvar
            await _db.SaveChangesAsync(ct);
            
            // 12. Commit
            await transaction.CommitAsync(ct);
            
            return _mapper.Map<FundoCriadoResponse>(fundo);
        }
        catch
        {
            // Rollback automático
            throw;
        }
    }
    
    private async Task<Dictionary<string, Guid>> ResolverInstituicoesAsync(
        IEnumerable<string> cnpjs, CancellationToken ct)
    {
        var cnpjsList = cnpjs.Distinct().ToList();
        var instituicoes = await _db.Instituicoes
            .Where(i => cnpjsList.Contains(i.Cnpj))
            .ToDictionaryAsync(i => i.Cnpj, i => i.Id, ct);
        
        var faltantes = cnpjsList.Except(instituicoes.Keys).ToList();
        if (faltantes.Any())
            throw new InstituicaoNaoEncontradaException(faltantes);
        
        return instituicoes;
    }
}
```

### Fase 2: Backend - DTOs do Wizard

#### 2.1 DTOs de Entrada (Request)

Os DTOs do wizard devem mapear para as entidades existentes:

```csharp
// DTO do Wizard → Entidade Existente

public class TaxaWizardDto
{
    // Campos do wizard
    public string TipoTaxa { get; set; }          // Mapeia para TipoTaxa enum existente
    public decimal Percentual { get; set; }
    public string BaseCalculo { get; set; }        // Mapeia para BaseCalculoTaxa enum
    public string FormaCobranca { get; set; }      // → PeriodicidadePagamento
    public DateOnly DataInicioVigencia { get; set; }
    // ... outros campos
}

public class PrazoWizardDto
{
    public string TipoOperacao { get; set; }       // → TipoPrazoOperacional
    public int PrazoCotizacao { get; set; }        // → DiasCotizacao
    public int PrazoLiquidacao { get; set; }       // → DiasLiquidacao
    public string TipoCalendario { get; set; }     // NOVO campo
    // ... outros campos
}

public class VinculoWizardDto
{
    public string TipoVinculo { get; set; }
    public string CnpjInstituicao { get; set; }    // Wizard envia CNPJ
    // API resolve → InstituicaoId
    public DateOnly DataInicio { get; set; }
    // ... outros campos
}
```

#### 2.2 AutoMapper Profile

```csharp
public class WizardMappingProfile : Profile
{
    public WizardMappingProfile()
    {
        // Taxa: Wizard → Entidade
        CreateMap<TaxaWizardDto, FundoTaxa>()
            .ForMember(d => d.PeriodicidadePagamento, 
                opt => opt.MapFrom(s => MapFormaCobranca(s.FormaCobranca)));
        
        // Prazo: Wizard → Entidade
        CreateMap<PrazoWizardDto, FundoPrazo>()
            .ForMember(d => d.TipoPrazoOperacional,
                opt => opt.MapFrom(s => MapTipoOperacao(s.TipoOperacao)))
            .ForMember(d => d.DiasCotizacao,
                opt => opt.MapFrom(s => s.PrazoCotizacao))
            .ForMember(d => d.DiasLiquidacao,
                opt => opt.MapFrom(s => s.PrazoLiquidacao));
        
        // ParametrosCota: Wizard → Nova Entidade
        CreateMap<ParametrosCotaDto, FundoParametrosCota>();
    }
    
    private static PeriodicidadePagamento MapFormaCobranca(string valor)
    {
        return valor switch
        {
            "DIARIA" => PeriodicidadePagamento.Diaria,
            "MENSAL" => PeriodicidadePagamento.Mensal,
            "SEMESTRAL" => PeriodicidadePagamento.Semestral,
            "ANUAL" => PeriodicidadePagamento.Anual,
            _ => throw new ArgumentException($"Forma cobrança inválida: {valor}")
        };
    }
}
```

### Fase 3: Backend - Endpoints

```csharp
[ApiController]
[Route("api/v1/fundos")]
public class FundosWizardController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost("wizard")]
    [ProducesResponseType(typeof(FundoCriadoResponse), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> CriarFundoWizard(
        [FromBody] CriarFundoWizardCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(
            nameof(FundosController.GetById),
            "Fundos",
            new { id = result.Id },
            result);
    }
    
    [HttpPost("wizard/validar")]
    [ProducesResponseType(typeof(ValidacaoWizardResponse), 200)]
    public async Task<IActionResult> ValidarWizard(
        [FromBody] CriarFundoWizardCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ValidarFundoWizardQuery(command), ct);
        return Ok(result);
    }
    
    [HttpGet("verificar-cnpj/{cnpj}")]
    [ProducesResponseType(typeof(VerificacaoCnpjResponse), 200)]
    public async Task<IActionResult> VerificarCnpj(
        string cnpj,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new VerificarCnpjQuery(cnpj), ct);
        return Ok(result);
    }
}
```

### Fase 4: Frontend

Com o backend ajustado, o frontend pode ser implementado conforme os slices existentes, apenas garantindo que os DTOs enviados correspondam ao formato esperado pela API.

---

## Checklist de Implementação

### Backend - Fase 1 (Bloqueante)

- [ ] Criar entidade `FundoParametrosCota`
- [ ] Criar migration para `fundo_parametros_cota`
- [ ] Adicionar `DbSet<FundoParametrosCota>` ao DbContext
- [ ] Adicionar valores `Isento` e `Previdencia` ao enum `RegimeTributacao`
- [ ] Adicionar campo `LinhaDaguaGlobal` em `FundoTaxa`
- [ ] Adicionar campos de calendário/programação em `FundoPrazo`
- [ ] Adicionar campos de encerramento em `FundoClasse`
- [ ] Criar `CriarFundoWizardCommand`
- [ ] Criar `CriarFundoWizardCommandHandler`
- [ ] Criar `CriarFundoWizardCommandValidator`
- [ ] Criar DTOs do wizard
- [ ] Criar `WizardMappingProfile` para AutoMapper
- [ ] Implementar `POST /api/v1/fundos/wizard`

### Backend - Fase 2 (Média)

- [ ] Criar entidade `WizardRascunho`
- [ ] Criar migration para `wizard_rascunho`
- [ ] Implementar CRUD de rascunhos
- [ ] Implementar `POST /api/v1/fundos/wizard/validar`
- [ ] Implementar `GET /api/v1/fundos/verificar-cnpj/{cnpj}`

### Backend - Fase 3 (Baixa)

- [ ] Criar entidade `DocumentoTemporario`
- [ ] Implementar upload temporário

### Frontend

- [ ] Implementar conforme slices 01-12

---

## Estimativa de Esforço

| Item | Complexidade | Esforço |
|------|--------------|---------|
| Entidade ParametrosCota + migration | Baixa | 2h |
| Alterações em entidades existentes | Baixa | 1h |
| CriarFundoWizardCommand + Handler | Alta | 8h |
| DTOs e Mapeamentos | Média | 4h |
| Endpoint POST /wizard | Média | 2h |
| Validadores | Média | 4h |
| Testes unitários | Média | 6h |
| **Total Backend Fase 1** | | **~27h** |

---

*Plano de Implementação Revisado*
*Considerando Estado Atual da API*
*Sistema Fund Accounting - Janeiro/2026*
