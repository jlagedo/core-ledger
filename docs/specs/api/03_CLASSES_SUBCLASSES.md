# Etapa 03 - Classes e Subclasses (CVM 175)

## Objetivo
Implementar estrutura multiclasse conforme Resolução CVM 175/2022.

## Localização
- Entidades: `FundAccounting.Domain/Cadastros/Entities/`
- Config: `FundAccounting.Infrastructure/Data/Configurations/`

## Modelo de Dados

### Tabela: cadastros.fundo_classe

| Coluna | Tipo | Null | Descrição |
|--------|------|------|-----------|
| id | UUID | N | PK |
| fundo_id | UUID | N | FK fundo |
| cnpj_classe | VARCHAR(14) | Y | CNPJ próprio (se aplicável) |
| codigo_classe | VARCHAR(10) | N | Ex: SR, MEZ, SUB |
| nome_classe | VARCHAR(100) | N | |
| tipo_classe_fidc | VARCHAR(20) | Y | SENIOR/MEZANINO/SUBORDINADA |
| ordem_subordinacao | INTEGER | Y | Para FIDCs |
| rentabilidade_alvo | DECIMAL(8,4) | Y | % a.a. |
| responsabilidade_limitada | BOOLEAN | N | Default: false |
| segregacao_patrimonial | BOOLEAN | N | Default: false |
| valor_minimo_aplicacao | DECIMAL(18,2) | Y | |
| ativa | BOOLEAN | N | Default: true |
| created_at | TIMESTAMP | N | |
| updated_at | TIMESTAMP | Y | |
| deleted_at | TIMESTAMP | Y | |

### Tabela: cadastros.fundo_subclasse

| Coluna | Tipo | Null | Descrição |
|--------|------|------|-----------|
| id | BIGINT | N | PK SERIAL |
| classe_id | UUID | N | FK classe |
| codigo_subclasse | VARCHAR(10) | N | |
| nome_subclasse | VARCHAR(100) | N | |
| serie | INTEGER | Y | Número da série |
| valor_minimo_aplicacao | DECIMAL(18,2) | Y | |
| taxa_administracao_diferenciada | DECIMAL(8,4) | Y | |
| ativa | BOOLEAN | N | Default: true |
| created_at | TIMESTAMP | N | |
| deleted_at | TIMESTAMP | Y | |

### Índices
```sql
CREATE INDEX ix_classe_fundo ON cadastros.fundo_classe(fundo_id);
CREATE INDEX ix_subclasse_classe ON cadastros.fundo_subclasse(classe_id);
CREATE UNIQUE INDEX ix_classe_codigo ON cadastros.fundo_classe(fundo_id, codigo_classe) 
  WHERE deleted_at IS NULL;
```

## Regras de Negócio
1. Classe só pode ser criada se fundo permite (tipo FI, FIC, FIDC)
2. Subclasse herda parâmetros da classe se não especificados
3. FIDC: ordem_subordinacao obrigatória
4. Exclusão lógica: não pode excluir classe com movimentações

## DTOs
- ClasseCreateDto
- ClasseUpdateDto
- ClasseResponseDto
- SubclasseCreateDto
- SubclasseResponseDto

## Critérios de Aceite
- [x] Migration para tabelas classe e subclasse
- [x] Relacionamento 1:N Fundo→Classe
- [x] Relacionamento 1:N Classe→Subclasse
- [x] Validação de tipo_classe_fidc para FIDCs
