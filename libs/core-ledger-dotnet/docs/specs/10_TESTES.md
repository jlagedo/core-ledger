# Etapa 10 - Testes

## Objetivo
Implementar testes unitários e de integração.

## Localização
- Unit: `FundAccounting.UnitTests/Cadastros/`
- Integration: `FundAccounting.IntegrationTests/Cadastros/`

## Testes Unitários

### Entidades
```
FundoTests
├── Deve_criar_fundo_valido
├── Deve_rejeitar_cnpj_invalido
├── Deve_calcular_progresso_cadastro
└── Deve_aplicar_soft_delete

ClasseTests
├── Deve_criar_classe_valida
├── Deve_validar_subordinacao_fidc
└── Deve_herdar_parametros_fundo

TaxaTests
├── Deve_criar_taxa_valida
├── Deve_validar_performance_com_benchmark
└── Deve_impedir_taxa_duplicada
```

### Validators
```
CreateFundoValidatorTests
├── Deve_passar_dados_validos
├── Deve_falhar_cnpj_invalido
├── Deve_falhar_tipo_fundo_invalido
└── Deve_falhar_percentual_exterior_negativo

CreateClasseValidatorTests
├── Deve_exigir_tipo_classe_para_fidc
└── Deve_exigir_ordem_subordinacao_fidc
```

### Domain Services
```
FundoDomainServiceTests
├── Deve_calcular_progresso_corretamente
├── Deve_validar_vinculos_obrigatorios
├── Deve_permitir_ativacao_fundo_completo
└── Deve_impedir_ativacao_sem_vinculos
```

## Testes de Integração

### Setup
- Testcontainers com PostgreSQL
- Database seeding por teste
- Cleanup automático

### API Tests
```
FundosControllerTests
├── GET_fundos_deve_retornar_lista_paginada
├── GET_fundo_por_id_deve_retornar_fundo
├── GET_fundo_inexistente_deve_retornar_404
├── POST_fundo_valido_deve_criar
├── POST_fundo_cnpj_duplicado_deve_retornar_400
├── PUT_fundo_deve_atualizar
├── DELETE_fundo_deve_soft_delete

ClassesControllerTests
├── POST_classe_para_fidc_sem_tipo_deve_falhar
├── GET_classes_fundo_deve_retornar_lista

TaxasControllerTests
├── POST_taxa_duplicada_deve_falhar
├── POST_performance_sem_benchmark_deve_falhar
```

### Repository Tests
```
FundoRepositoryTests
├── Deve_buscar_por_cnpj
├── Deve_ignorar_soft_deleted
├── Deve_incluir_relacionamentos
└── Deve_paginar_corretamente
```

## Cobertura Mínima
- Entidades: 90%
- Validators: 100%
- Handlers: 80%
- Controllers: 70%

## Ferramentas
- xUnit
- FluentAssertions
- Moq
- Testcontainers
- Bogus (geração de dados)

## Critérios de Aceite
- [ ] Testes unitários para entidades principais
- [ ] Testes unitários para validators
- [ ] Testes integração com banco real (container)
- [ ] Pipeline CI executando testes
- [ ] Cobertura mínima atingida
