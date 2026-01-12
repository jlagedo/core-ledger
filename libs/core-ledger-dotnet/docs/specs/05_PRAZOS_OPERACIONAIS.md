# Etapa 05 - Prazos Operacionais

## Objetivo
Implementar prazos de aplicação, resgate e carência.

## Localização
- Entidade: `FundAccounting.Domain/Cadastros/Entities/FundoPrazo.cs`

## Modelo de Dados

### Tabela: cadastros.fundo_prazo

| Coluna | Tipo | Null | Descrição |
|--------|------|------|-----------|
| id | BIGINT | N | PK SERIAL |
| fundo_id | UUID | N | FK fundo |
| classe_id | UUID | Y | FK classe (prazo por classe) |
| tipo_prazo | VARCHAR(20) | N | Enum TipoPrazoOperacional |
| dias_cotizacao | INTEGER | N | D+X cotização |
| dias_liquidacao | INTEGER | N | D+X liquidação financeira |
| dias_carencia | INTEGER | Y | Carência inicial |
| horario_limite | TIME | N | Horário de corte |
| dias_uteis | BOOLEAN | N | true=úteis, false=corridos |
| calendario_id | INTEGER | Y | FK calendário específico |
| permite_parcial | BOOLEAN | N | Resgate parcial |
| percentual_minimo | DECIMAL(5,2) | Y | % mínimo resgate |
| valor_minimo | DECIMAL(18,2) | Y | Valor mínimo |
| ativo | BOOLEAN | N | Default: true |
| created_at | TIMESTAMP | N | |
| updated_at | TIMESTAMP | Y | |

### Tabela: cadastros.fundo_prazo_excecao

| Coluna | Tipo | Null | Descrição |
|--------|------|------|-----------|
| id | BIGINT | N | PK SERIAL |
| prazo_id | BIGINT | N | FK prazo |
| data_inicio | DATE | N | |
| data_fim | DATE | N | |
| dias_cotizacao | INTEGER | N | Override D+X |
| dias_liquidacao | INTEGER | N | Override D+X |
| motivo | VARCHAR(200) | N | |
| created_at | TIMESTAMP | N | |

### Índices
```sql
CREATE INDEX ix_prazo_fundo ON cadastros.fundo_prazo(fundo_id);
CREATE INDEX ix_prazo_classe ON cadastros.fundo_prazo(classe_id);
CREATE UNIQUE INDEX ix_prazo_tipo ON cadastros.fundo_prazo(fundo_id, classe_id, tipo_prazo) 
  WHERE ativo = true;
```

## Regras de Negócio
1. Aplicação e Resgate: obrigatórios
2. FK para calendário (módulo existente)
3. Horário limite típico: 14:00
4. Exceções: para períodos especiais (fechamento balanço, etc)

## DTOs
- PrazoCreateDto
- PrazoUpdateDto
- PrazoResponseDto
- PrazoExcecaoDto

## Critérios de Aceite
- [x] Migration para tabelas de prazo
- [x] FK para calendário existente
- [x] Validação: dias não negativos
- [ ] Cálculo D+X usando calendário (implementação futura - requer lógica de negócio)
