# Etapa 02 - Entidade Fundo (Core)

## Objetivo
Implementar entidade principal `Fundo` e configuração EF Core.

## Localização
- Entidade: `FundAccounting.Domain/Cadastros/Entities/Fundo.cs`
- Config: `FundAccounting.Infrastructure/Data/Configurations/FundoConfiguration.cs`

## Modelo de Dados

### Tabela: cadastros.fundo

| Coluna | Tipo | Null | Descrição |
|--------|------|------|-----------|
| id | UUID | N | PK, gen_random_uuid() |
| cnpj | VARCHAR(14) | N | Único, sem formatação |
| razao_social | VARCHAR(200) | N | |
| nome_fantasia | VARCHAR(100) | Y | |
| nome_curto | VARCHAR(30) | Y | Para exibição |
| data_constituicao | DATE | Y | |
| data_inicio_atividade | DATE | Y | |
| tipo_fundo | VARCHAR(20) | N | Enum |
| classificacao_cvm | VARCHAR(30) | N | Enum |
| classificacao_anbima | VARCHAR(100) | Y | |
| codigo_anbima | VARCHAR(6) | Y | |
| situacao | VARCHAR(20) | N | Default: EM_CONSTITUICAO |
| prazo | VARCHAR(20) | N | Enum |
| publico_alvo | VARCHAR(20) | N | Enum |
| tributacao | VARCHAR(20) | N | Enum |
| condominio | VARCHAR(10) | N | Enum |
| exclusivo | BOOLEAN | N | Default: false |
| reservado | BOOLEAN | N | Default: false |
| permite_alavancagem | BOOLEAN | N | Default: false |
| aceita_cripto | BOOLEAN | N | Default: false |
| percentual_exterior | DECIMAL(5,2) | N | Default: 0 |
| wizard_completo | BOOLEAN | N | Default: false |
| progresso_cadastro | INTEGER | N | Default: 0 |
| created_at | TIMESTAMP | N | |
| updated_at | TIMESTAMP | Y | |
| deleted_at | TIMESTAMP | Y | Soft delete |
| created_by | VARCHAR(100) | Y | |
| updated_by | VARCHAR(100) | Y | |

### Índices
```sql
CREATE UNIQUE INDEX ix_fundo_cnpj ON cadastros.fundo(cnpj) WHERE deleted_at IS NULL;
CREATE INDEX ix_fundo_situacao ON cadastros.fundo(situacao);
CREATE INDEX ix_fundo_tipo ON cadastros.fundo(tipo_fundo);
```

## Entidade .NET

```csharp
public class Fundo : BaseEntity<Guid>
{
    public CNPJ Cnpj { get; private set; }
    public string RazaoSocial { get; private set; }
    // ... demais propriedades
    
    // Navegações
    public ICollection<FundoClasse> Classes { get; }
    public ICollection<FundoTaxa> Taxas { get; }
    public ICollection<FundoPrazo> Prazos { get; }
    public ICollection<FundoVinculo> Vinculos { get; }
    public FundoParametrosFIDC? ParametrosFIDC { get; }
}
```

## DTOs

### FundoCreateDto
Campos obrigatórios para criação inicial.

### FundoUpdateDto
Campos editáveis.

### FundoResponseDto
Resposta completa com relacionamentos.

### FundoListDto
Versão resumida para listagens.

## Critérios de Aceite
- [x] Migration criada para tabela fundo
- [x] Entidade com encapsulamento (setters privados)
- [x] Configuration do EF Core completa
- [x] DTOs criados
- [x] AutoMapper profiles configurados
