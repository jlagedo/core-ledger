# Etapa 01 - Entidades Base e Enums

## Objetivo
Criar enums e value objects compartilhados pelo módulo.

## Localização
`FundAccounting.Domain/Cadastros/Enums/`
`FundAccounting.Domain/Cadastros/ValueObjects/`

## Enums a Criar

### TipoFundo
```
FI, FIC, FIDC, FIP, FII, FIAGRO, FICFIDC
```

### ClassificacaoCVM
```
RENDA_FIXA, ACOES, CAMBIAL, MULTIMERCADO, FIDC, FIP, FII, FIAGRO
```

### SituacaoFundo
```
EM_CONSTITUICAO, ATIVO, SUSPENSO, EM_LIQUIDACAO, LIQUIDADO
```

### PublicoAlvo
```
GERAL, QUALIFICADO, PROFISSIONAL
```

### TipoCondominio
```
ABERTO, FECHADO
```

### PrazoFundo
```
DETERMINADO, INDETERMINADO
```

### TributacaoFundo
```
CURTO_PRAZO, LONGO_PRAZO, ACOES, ISENTO
```

### TipoClasseFIDC
```
SENIOR, MEZANINO, SUBORDINADA
```

### TipoPrazoOperacional
```
APLICACAO, RESGATE, CARENCIA
```

### TipoTaxa
```
ADMINISTRACAO, GESTAO, PERFORMANCE, CUSTODIA, ENTRADA, SAIDA
```

### BaseCalculoTaxa
```
PL_MEDIO, PL_FINAL, COTAS_EMITIDAS
```

### TipoVinculoInstitucional
```
ADMINISTRADOR, GESTOR, CUSTODIANTE, DISTRIBUIDOR, AUDITOR
```

## Value Objects

### CNPJ
- Validação: 14 dígitos + dígitos verificadores
- Formatação: XX.XXX.XXX/XXXX-XX
- Imutável

### CodigoANBIMA
- Validação: 6 dígitos numéricos
- Opcional

## Critérios de Aceite
- [x] Todos enums criados com descrições XML
- [x] Value object CNPJ com validação
- [x] Testes unitários para CNPJ
