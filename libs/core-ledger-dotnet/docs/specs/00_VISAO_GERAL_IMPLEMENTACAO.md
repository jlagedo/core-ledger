# Implementação API - Módulo Cadastro de Fundos

## Contexto
Calendário e Indexadores já implementados. Este documento guia a implementação do cadastro de fundos.

## Estrutura de Etapas

| Etapa | Documento | Dependências |
|-------|-----------|--------------|
| 01 | Entidades Base | Nenhuma |
| 02 | Fundo Core | Etapa 01 |
| 03 | Classes e Subclasses | Etapa 02 |
| 04 | Taxas | Etapa 02 |
| 05 | Prazos Operacionais | Etapa 02 |
| 06 | Vínculos Institucionais | Etapa 02 |
| 07 | Parâmetros FIDC | Etapa 03 |
| 08 | API Endpoints | Etapas 02-07 |
| 09 | Validações e Regras | Etapa 08 |
| 10 | Testes | Todas |

## Stack (já definido)
- Backend: .NET 10
- ORM: EF Core + Dapper (relatórios)
- DB: PostgreSQL 16+
- Validação: FluentValidation
- Mapeamento: AutoMapper
- CQRS: MediatR

## Schema SQL
```sql
CREATE SCHEMA IF NOT EXISTS cadastros;
```

## Convenções
- IDs expostos em API: UUID
- IDs internos/alto volume: BIGINT
- Soft delete: `deleted_at TIMESTAMP NULL`
- Audit: `created_at`, `updated_at`, `created_by`, `updated_by`
- Nomenclatura: snake_case (DB), PascalCase (.NET)
