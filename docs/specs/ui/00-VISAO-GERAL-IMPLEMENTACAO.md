# Wizard de Cadastro de Fundos - Guia de Implementação

## Visão Geral

Este conjunto de documentos guia a implementação do **Wizard de Cadastro de Fundos** do sistema Fund Accounting. Cada documento representa um **vertical slice** independente que pode ser implementado, testado e entregue de forma iterativa.

## Contexto do Projeto

- **Backend**: .NET 10 (API REST)
- **Frontend**: Angular 21 + Bootstrap 5 + NgRx
- **Banco de Dados**: PostgreSQL 16+
- **Referência**: Especificação do Módulo de Cadastro de Fundos v2.0

## Etapas do Wizard (Especificação)

| Etapa | Nome | Campos Principais | Obrigatório |
|-------|------|-------------------|-------------|
| 1 | Identificação | CNPJ, Razão Social, Nome Fantasia, Tipo Fundo | Sim |
| 2 | Classificação | CVM, ANBIMA, Público Alvo, Tributação | Sim |
| 3 | Características | Condomínio, Prazo, Alavancagem, Cripto | Sim |
| 4 | Parâmetros de Cota | Casas decimais, Horário corte, Cota inicial | Sim |
| 5 | Taxas | Administração, Gestão, Performance | Parcial |
| 6 | Prazos | Aplicação, Resgate | Sim |
| 7 | Classes (CVM 175) | Criar classes e subclasses | Opcional |
| 8 | Vínculos | Administrador, Gestor, Custodiante | Sim |
| 9 | Documentos | Upload de regulamento | Opcional |
| 10 | Revisão | Resumo de todos os dados | Sim |

## Ordem de Implementação dos Slices

```
┌─────────────────────────────────────────────────────────────────┐
│  SLICE 01: Infraestrutura Base do Wizard                        │
│  - Navegação entre etapas                                       │
│  - Estado compartilhado (NgRx)                                  │
│  - Endpoint POST /api/v1/fundos/wizard                          │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  SLICE 02: Etapa 1 - Identificação do Fundo                     │
│  - Formulário de identificação                                  │
│  - Validação de CNPJ                                            │
│  - Verificação de duplicidade                                   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  SLICE 03: Etapa 2 - Classificação                              │
│  - Selects dependentes (CVM → ANBIMA)                           │
│  - Regras de tributação por tipo                                │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  SLICE 04: Etapa 3 - Características                            │
│  - Campos condicionais                                          │
│  - Validações de limites                                        │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  SLICE 05: Etapa 4 - Parâmetros de Cota                         │
│  - Precisão decimal                                             │
│  - Horários e valores iniciais                                  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  SLICE 06: Etapa 5 - Taxas do Fundo                             │
│  - Múltiplas taxas (lista dinâmica)                             │
│  - Taxa de performance com benchmark                            │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  SLICE 07: Etapa 6 - Prazos Operacionais                        │
│  - Configuração D+X                                             │
│  - Horários de corte                                            │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  SLICE 08: Etapa 7 - Classes CVM 175                            │
│  - Lista de classes                                             │
│  - Subclasses                                                   │
│  - Parâmetros específicos FIDC                                  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  SLICE 09: Etapa 8 - Vínculos Institucionais                    │
│  - Busca de instituições                                        │
│  - Tipos de vínculo                                             │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  SLICE 10: Etapa 9 - Upload de Documentos                       │
│  - Upload de arquivos                                           │
│  - Validação de tipos                                           │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  SLICE 11: Etapa 10 - Revisão e Submissão                       │
│  - Resumo consolidado                                           │
│  - Submissão final                                              │
│  - Tratamento de erros                                          │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  SLICE 12: Persistência de Rascunho                             │
│  - Salvamento automático                                        │
│  - Retomada de cadastro                                         │
└─────────────────────────────────────────────────────────────────┘
```

## Critérios de Conclusão de Cada Slice

Cada slice está **completo** quando:

1. ✅ Backend implementado e testado (unitário)
2. ✅ Frontend implementado e conectado à API
3. ✅ Validações funcionando
4. ✅ Navegação entre etapas operacional
5. ✅ Teste de integração passando

## Métricas de Qualidade (Referência)

| Métrica | Especificação |
|---------|---------------|
| Tempo de resposta API | < 200ms (p95) |
| Cadastro completo via Wizard | < 10 minutos |
| Taxa de conclusão Wizard | > 80% |
| Erros de validação | < 5% por sessão |

## Como Usar Esta Documentação

1. **Leia o slice atual** antes de iniciar
2. **Implemente** seguindo os requisitos funcionais
3. **Teste** a integração com a API
4. **Valide** os critérios de aceite
5. **Prossiga** para o próximo slice

## Arquivos do Guia

| Arquivo | Descrição |
|---------|-----------|
| `01-SLICE-INFRAESTRUTURA-BASE.md` | Estrutura base do wizard |
| `02-SLICE-IDENTIFICACAO-FUNDO.md` | Etapa 1 do wizard |
| `03-SLICE-CLASSIFICACAO.md` | Etapa 2 do wizard |
| `04-SLICE-CARACTERISTICAS.md` | Etapa 3 do wizard |
| `05-SLICE-PARAMETROS-COTA.md` | Etapa 4 do wizard |
| `06-SLICE-TAXAS.md` | Etapa 5 do wizard |
| `07-SLICE-PRAZOS.md` | Etapa 6 do wizard |
| `08-SLICE-CLASSES-CVM175.md` | Etapa 7 do wizard |
| `08-B-SLICE-PARAMETROS-FIDC.md` | Etapa 7.1 (condicional para FIDCs) |
| `09-SLICE-VINCULOS.md` | Etapa 8 do wizard |
| `10-SLICE-DOCUMENTOS.md` | Etapa 9 do wizard |
| `11-SLICE-REVISAO-SUBMISSAO.md` | Etapa 10 do wizard |
| `12-SLICE-PERSISTENCIA-RASCUNHO.md` | Auto-save e retomada |
| `13-ESPECIFICACAO-ENDPOINT-WIZARD.md` | Estratégia de persistência |
| `14-ANALISE-GAPS-ESPECIFICACAO.md` | Análise de desvios |
| `15-SLICE-API-WIZARD-BACKEND.md` | Especificação API Backend |
| `16-PLANO-IMPLEMENTACAO-API-EXISTENTE.md` | **NOVO** - Plano considerando API atual |

---

## Estado Atual da API

### ✅ Entidades Já Implementadas

| Entidade | Status |
|----------|--------|
| `Fundo` | ✅ Completa |
| `FundoClasse` + `FundoSubclasse` | ✅ Completa |
| `FundoTaxa` | ✅ Completa (adicionar 1 campo) |
| `FundoPrazo` | ✅ Completa (adicionar 3 campos) |
| `FundoVinculo` | ✅ Completa (usa FK para Instituicao) |
| `FundoParametrosFIDC` | ✅ Completa |
| `Instituicao` | ✅ Completa |

### ❌ A Criar

| Entidade | Prioridade |
|----------|------------|
| `FundoParametrosCota` | 🔴 BLOQUEANTE |
| `WizardRascunho` | 🟡 Médio |
| Endpoint `POST /api/v1/fundos/wizard` | 🔴 BLOQUEANTE |

---

## Ordem de Implementação Recomendada

### Fase 1: Backend - Itens Bloqueantes
1. Criar entidade `FundoParametrosCota` + migration
2. Adicionar campos faltantes às entidades existentes
3. Adicionar valores ao enum `RegimeTributacao`
4. Criar `CriarFundoWizardCommand` + Handler
5. Implementar `POST /api/v1/fundos/wizard`

### Fase 2: Frontend Base
6. `01-SLICE-INFRAESTRUTURA-BASE.md` - Shell do wizard
7. `02-SLICE-IDENTIFICACAO-FUNDO.md` - Primeira etapa funcional

### Fase 3: Etapas do Wizard
8. `03-SLICE-CLASSIFICACAO.md`
9. `04-SLICE-CARACTERISTICAS.md`
10. `05-SLICE-PARAMETROS-COTA.md`
11. `06-SLICE-TAXAS.md`
12. `07-SLICE-PRAZOS.md`
13. `08-SLICE-CLASSES-CVM175.md`
14. `08-B-SLICE-PARAMETROS-FIDC.md`
15. `09-SLICE-VINCULOS.md`
16. `10-SLICE-DOCUMENTOS.md`

### Fase 4: Finalização
17. `11-SLICE-REVISAO-SUBMISSAO.md`
18. `12-SLICE-PERSISTENCIA-RASCUNHO.md` (opcional)

---

*Documentação de Implementação - Wizard de Cadastro de Fundos*  
*Sistema Fund Accounting - Janeiro/2026*
