# Etapa 09 - Validações e Regras de Negócio

## Objetivo
Implementar validadores FluentValidation e regras de domínio.

## Localização
- Validators: `FundAccounting.Application/Cadastros/Validators/`
- Domain Services: `FundAccounting.Domain/Cadastros/Services/`

## Validadores FluentValidation

### CreateFundoValidator
```
- CNPJ: obrigatório, formato válido, único
- RazaoSocial: obrigatório, 5-200 caracteres
- TipoFundo: enum válido
- ClassificacaoCVM: enum válido
- PublicoAlvo: enum válido
- Tributacao: enum válido
- Condominio: enum válido
- DataConstituicao: <= hoje
- PercentualExterior: 0-100
```

### CreateClasseValidator
```
- FundoId: existe e não deletado
- CodigoClasse: obrigatório, único por fundo
- TipoClasseFIDC: obrigatório se fundo é FIDC
- OrdemSubordinacao: obrigatório se FIDC
- ValorMinimoAplicacao: >= 0
```

### CreateTaxaValidator
```
- FundoId: existe
- TipoTaxa: enum válido
- Percentual: > 0 e <= 100
- DataInicioVigencia: obrigatória
- Se PERFORMANCE: benchmark (indexador_id) obrigatório
- Não pode haver taxa ativa duplicada do mesmo tipo
```

### CreatePrazoValidator
```
- FundoId: existe
- TipoPrazo: enum válido
- DiasCotizacao: >= 0
- DiasLiquidacao: >= DiasCotizacao
- HorarioLimite: formato válido
- CalendarioId: existe (se informado)
```

### CreateVinculoValidator
```
- FundoId: existe
- InstituicaoId: existe
- TipoVinculo: enum válido
- DataInicio: <= hoje
- Se Principal=true: não pode haver outro principal vigente do tipo
```

## Regras de Domínio

### FundoDomainService

```csharp
public class FundoDomainService
{
    // Valida se pode ativar fundo
    bool PodeAtivar(Fundo fundo);
    
    // Valida se pode liquidar fundo
    bool PodeLiquidar(Fundo fundo);
    
    // Calcula progresso do cadastro (0-100)
    int CalcularProgressoCadastro(Fundo fundo);
    
    // Valida vínculos obrigatórios
    ValidationResult ValidarVinculosObrigatorios(Fundo fundo);
}
```

### Regras de Progresso
| Campo/Relacionamento | Peso |
|---------------------|------|
| Dados básicos | 20% |
| Classificação | 15% |
| Parâmetros cota | 10% |
| Taxas configuradas | 15% |
| Prazos configurados | 15% |
| Vínculos obrigatórios | 20% |
| Documentos (futuro) | 5% |

## Códigos de Erro

| Código | Descrição |
|--------|-----------|
| FUNDO_NOT_FOUND | Fundo não encontrado |
| FUNDO_CNPJ_EXISTS | CNPJ já cadastrado |
| FUNDO_CANNOT_DELETE | Fundo com movimentações |
| CLASSE_NOT_FOUND | Classe não encontrada |
| CLASSE_CODIGO_EXISTS | Código já existe no fundo |
| TAXA_DUPLICADA | Taxa do tipo já ativa |
| TAXA_BENCHMARK_REQUIRED | Performance requer benchmark |
| VINCULO_OBRIGATORIO | Vínculo obrigatório faltando |
| VINCULO_PRINCIPAL_EXISTS | Já existe vínculo principal |

## Critérios de Aceite
- [x] Validators para todos Commands
- [x] Pipeline behavior de validação (MediatR)
- [x] Domain service implementado
- [x] Mensagens de erro em português
- [x] Códigos de erro padronizados
