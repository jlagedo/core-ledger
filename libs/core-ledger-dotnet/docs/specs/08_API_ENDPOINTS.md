# Etapa 08 - API Endpoints

## Objetivo
Implementar endpoints Minimal API e handlers MediatR.

## Localização
- Endpoints: `CoreLedger.API/Endpoints/Cadastros/`
- Handlers: `CoreLedger.Application/UseCases/Cadastros/`
- QueryServices: `CoreLedger.Application/Interfaces/QueryServices/` + `CoreLedger.Infrastructure/Services/QueryServices/`

## Estrutura CQRS

```
Application/Cadastros/
├── Fundos/
│   ├── Commands/
│   │   ├── CreateFundo/
│   │   ├── UpdateFundo/
│   │   └── DeleteFundo/
│   └── Queries/
│       ├── GetFundo/
│       ├── GetFundos/
│       └── SearchFundos/
├── Classes/
├── Taxas/
├── Prazos/
└── Vinculos/
```

## Endpoints

### Fundos
| Método | Rota | Handler | Descrição |
|--------|------|---------|-----------|
| GET | /api/v1/fundos | GetFundosQuery | Lista paginada |
| GET | /api/v1/fundos/{id} | GetFundoQuery | Por ID |
| GET | /api/v1/fundos/cnpj/{cnpj} | GetFundoByCnpjQuery | Por CNPJ |
| POST | /api/v1/fundos | CreateFundoCommand | Criar |
| PUT | /api/v1/fundos/{id} | UpdateFundoCommand | Atualizar |
| DELETE | /api/v1/fundos/{id} | DeleteFundoCommand | Soft delete |
| GET | /api/v1/fundos/busca | SearchFundosQuery | Busca texto |

### Classes
| Método | Rota | Handler |
|--------|------|---------|
| GET | /api/v1/fundos/{fundoId}/classes | GetClassesQuery |
| POST | /api/v1/fundos/{fundoId}/classes | CreateClasseCommand |
| GET | /api/v1/classes/{id} | GetClasseQuery |
| PUT | /api/v1/classes/{id} | UpdateClasseCommand |
| DELETE | /api/v1/classes/{id} | DeleteClasseCommand |

### Taxas
| Método | Rota | Handler |
|--------|------|---------|
| GET | /api/v1/fundos/{fundoId}/taxas | GetTaxasQuery |
| POST | /api/v1/fundos/{fundoId}/taxas | CreateTaxaCommand |
| PUT | /api/v1/taxas/{id} | UpdateTaxaCommand |
| DELETE | /api/v1/taxas/{id} | DeleteTaxaCommand |

### Prazos
| Método | Rota | Handler |
|--------|------|---------|
| GET | /api/v1/fundos/{fundoId}/prazos | GetPrazosQuery |
| POST | /api/v1/fundos/{fundoId}/prazos | CreatePrazoCommand |
| PUT | /api/v1/prazos/{id} | UpdatePrazoCommand |

### Vínculos
| Método | Rota | Handler |
|--------|------|---------|
| GET | /api/v1/fundos/{fundoId}/vinculos | GetVinculosQuery |
| POST | /api/v1/fundos/{fundoId}/vinculos | CreateVinculoCommand |
| PATCH | /api/v1/vinculos/{id}/encerrar | EncerrarVinculoCommand |

### Instituições
| Método | Rota | Handler |
|--------|------|---------|
| GET | /api/v1/instituicoes | GetInstituicoesQuery |
| POST | /api/v1/instituicoes | CreateInstituicaoCommand |

## Padrões de Resposta

### Sucesso
```json
{
  "data": { ... },
  "meta": { "timestamp": "..." }
}
```

### Lista Paginada
```json
{
  "data": [ ... ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 100,
    "totalPages": 5
  }
}
```

### Erro
```json
{
  "error": {
    "code": "FUNDO_NOT_FOUND",
    "message": "...",
    "details": [ ... ]
  }
}
```

## Critérios de Aceite
- [x] Endpoints Minimal API criados com rotas corretas
- [x] Handlers MediatR implementados (Commands + Queries)
- [x] Swagger/OpenAPI documentado via WithTags e WithName
- [x] Respostas padronizadas (PagedResult para listas, DTOs para detalhes)
- [x] Build e testes passando (277 testes, 0 falhas)
