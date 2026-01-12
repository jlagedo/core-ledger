# Etapa 06 - Vínculos Institucionais

## Objetivo
Implementar vínculos com administrador, gestor, custodiante, distribuidor.

## Localização
- Entidades: `FundAccounting.Domain/Cadastros/Entities/`

## Modelo de Dados

### Tabela: cadastros.instituicao

| Coluna | Tipo | Null | Descrição |
|--------|------|------|-----------|
| id | INTEGER | N | PK SERIAL |
| cnpj | VARCHAR(14) | N | Único |
| razao_social | VARCHAR(200) | N | |
| nome_fantasia | VARCHAR(100) | Y | |
| ativo | BOOLEAN | N | Default: true |
| created_at | TIMESTAMP | N | |
| updated_at | TIMESTAMP | Y | |

### Tabela: cadastros.fundo_vinculo

| Coluna | Tipo | Null | Descrição |
|--------|------|------|-----------|
| id | BIGINT | N | PK SERIAL |
| fundo_id | UUID | N | FK fundo |
| instituicao_id | INTEGER | N | FK instituição |
| tipo_vinculo | VARCHAR(30) | N | Enum TipoVinculoInstitucional |
| data_inicio | DATE | N | |
| data_fim | DATE | Y | NULL = vigente |
| contrato_numero | VARCHAR(50) | Y | |
| observacao | VARCHAR(500) | Y | |
| principal | BOOLEAN | N | Marca vínculo principal do tipo |
| created_at | TIMESTAMP | N | |
| updated_at | TIMESTAMP | Y | |

### Índices
```sql
CREATE UNIQUE INDEX ix_instituicao_cnpj ON cadastros.instituicao(cnpj);
CREATE INDEX ix_vinculo_fundo ON cadastros.fundo_vinculo(fundo_id);
CREATE INDEX ix_vinculo_tipo ON cadastros.fundo_vinculo(tipo_vinculo);
CREATE INDEX ix_vinculo_vigente ON cadastros.fundo_vinculo(fundo_id, tipo_vinculo) 
  WHERE data_fim IS NULL;
```

## Regras de Negócio
1. Obrigatórios: ADMINISTRADOR, GESTOR, CUSTODIANTE
2. Apenas um principal por tipo vigente
3. Encerrar vínculo = preencher data_fim
4. Não permite deletar, apenas encerrar

## DTOs
- InstituicaoCreateDto
- InstituicaoResponseDto
- VinculoCreateDto
- VinculoUpdateDto
- VinculoResponseDto

## Critérios de Aceite
- [x] Migration para instituição e vínculo
- [x] Entidades Instituicao e FundoVinculo criadas com validações
- [x] Configurações EF Core (InstituicaoConfiguration e FundoVinculoConfiguration)
- [x] DTOs criados (Create, Update, Response)
- [x] Validators FluentValidation implementados
- [x] Testes unitários básicos
- [ ] Seed de instituições principais (B3, principais admins)
- [ ] Validação: vínculos obrigatórios
- [ ] Endpoint de encerramento (PATCH)
