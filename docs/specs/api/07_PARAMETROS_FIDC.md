# Etapa 07 - Parâmetros FIDC

## Objetivo
Implementar parâmetros específicos para FIDCs.

## Localização
- Entidade: `FundAccounting.Domain/Cadastros/Entities/FundoParametrosFIDC.cs`

## Modelo de Dados

### Tabela: cadastros.fundo_parametros_fidc

| Coluna | Tipo | Null | Descrição |
|--------|------|------|-----------|
| id | BIGINT | N | PK SERIAL |
| fundo_id | UUID | N | FK fundo, UNIQUE |
| tipo_fidc | VARCHAR(20) | N | PADRONIZADO/NAO_PADRONIZADO |
| tipos_recebiveis | VARCHAR(500) | N | JSON array |
| prazo_medio_carteira | INTEGER | Y | Dias |
| indice_subordinacao_alvo | DECIMAL(5,4) | Y | % |
| indice_subordinacao_minimo | DECIMAL(5,4) | Y | % |
| provisao_devedores_duvidosos | DECIMAL(5,4) | Y | % |
| limite_concentracao_cedente | DECIMAL(5,4) | Y | % |
| limite_concentracao_sacado | DECIMAL(5,4) | Y | % |
| possui_coobrigacao | BOOLEAN | N | Default: false |
| percentual_coobrigacao | DECIMAL(5,4) | Y | % |
| registradora_recebiveis | VARCHAR(20) | Y | LAQUS/CERC/TAG |
| integracao_registradora | BOOLEAN | N | Default: false |
| codigo_registradora | VARCHAR(50) | Y | Código no sistema registradora |
| created_at | TIMESTAMP | N | |
| updated_at | TIMESTAMP | Y | |

### Índices
```sql
CREATE UNIQUE INDEX ix_parametros_fidc_fundo ON cadastros.fundo_parametros_fidc(fundo_id);
```

## Regras de Negócio
1. Apenas para tipo_fundo IN (FIDC, FICFIDC)
2. Relacionamento 1:1 com Fundo
3. tipos_recebiveis: array de strings (CREDITO_CONSIGNADO, DUPLICATA, etc)
4. Se integracao_registradora = true, registradora_recebiveis obrigatória

## Enums Adicionais

### TipoFIDC
```
PADRONIZADO, NAO_PADRONIZADO
```

### TipoRecebiveis (para validação)
```
CREDITO_CONSIGNADO, DUPLICATA, CCB, CHEQUE, CARTAO_CREDITO, 
ENERGIA, TELECOM, PRECATORIOS, OUTROS
```

### Registradora
```
LAQUS, CERC, TAG
```

## DTOs
- ParametrosFIDCCreateDto
- ParametrosFIDCUpdateDto
- ParametrosFIDCResponseDto

## Critérios de Aceite
- [x] Migration para tabela parametros_fidc
- [x] Validação: somente para FIDC/FICFIDC
- [x] Campo JSON para tipos_recebiveis
- [x] Relacionamento 1:1 com Fundo
