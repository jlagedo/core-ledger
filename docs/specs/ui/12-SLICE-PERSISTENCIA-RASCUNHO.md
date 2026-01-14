# SLICE 12: Persistência de Rascunho

## Objetivo

Implementar funcionalidade de salvamento automático de rascunho do wizard, permitindo que o usuário retome um cadastro incompleto em sessão futura.

## Escopo

### Funcionalidades

1. **Auto-save**: Salvar automaticamente a cada mudança de etapa
2. **Salvamento manual**: Botão "Salvar Rascunho" disponível
3. **Retomada**: Listar e carregar rascunhos pendentes
4. **Expiração**: Rascunhos expiram após 30 dias
5. **Exclusão**: Permitir excluir rascunhos manualmente

---

## Requisitos Funcionais

### RF-01: Auto-save

- Salvar estado do wizard automaticamente:
  - Ao avançar para próxima etapa
  - Ao voltar para etapa anterior
  - A cada 60 segundos de inatividade com mudanças
- Exibir indicador discreto: "Rascunho salvo às HH:mm"
- Não bloquear navegação durante salvamento

### RF-02: Salvamento Manual

- Botão "Salvar Rascunho" sempre visível no wizard
- Ao clicar, salvar imediatamente
- Exibir confirmação: "Rascunho salvo com sucesso"

### RF-03: Lista de Rascunhos

- Ao acessar /cadastros/fundos/novo, verificar se há rascunhos
- Se existir, exibir modal:
  ```
  Você possui um cadastro em andamento:
  - FI RF CREDITO PRIVADO (70% concluído)
  - Última edição: 20/01/2024 às 15:30
  
  [Continuar Cadastro]  [Iniciar Novo]  [Ver Todos]
  ```

### RF-04: Múltiplos Rascunhos

- Permitir até 5 rascunhos por usuário
- Se limite atingido, solicitar exclusão de um existente
- Listar todos com opção de retomar ou excluir

### RF-05: Identificação do Rascunho

Identificar rascunho por:
- CNPJ (se informado)
- Nome fantasia (se informado)
- Ou "Rascunho sem nome #1, #2..."

### RF-06: Expiração

- Rascunhos expiram em 30 dias
- Exibir alerta se rascunho próximo de expirar (< 5 dias)
- Job de limpeza remove rascunhos expirados

### RF-07: Confirmação de Exclusão

- Ao excluir rascunho, solicitar confirmação
- Exclusão é permanente

### RF-08: Conflito de CNPJ

- Se ao carregar rascunho o CNPJ já foi cadastrado por outro fluxo:
  - Exibir aviso
  - Permitir alterar CNPJ ou descartar rascunho

---

## Modelo de Dados - Rascunho

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `id` | UUID | Identificador do rascunho |
| `usuario_id` | UUID | Usuário proprietário |
| `nome_identificacao` | string | CNPJ ou nome para exibição |
| `etapa_atual` | integer | Última etapa acessada |
| `progresso` | integer | % de completude |
| `dados_json` | jsonb | Estado completo do wizard |
| `created_at` | timestamp | Criação |
| `updated_at` | timestamp | Última atualização |
| `expires_at` | timestamp | Data de expiração |

---

## Frontend

### Componente

**WizardRascunhoService** - Serviço para gerenciar rascunhos

### Integração com Wizard

1. **Ao iniciar wizard:**
   ```typescript
   // Verificar rascunhos existentes
   const rascunhos = await rascunhoService.listar();
   if (rascunhos.length > 0) {
     // Exibir modal de seleção
   }
   ```

2. **Ao mudar de etapa:**
   ```typescript
   // Auto-save
   await rascunhoService.salvar(estadoAtual);
   ```

3. **Ao submeter com sucesso:**
   ```typescript
   // Excluir rascunho
   await rascunhoService.excluir(rascunhoId);
   ```

### Modal de Rascunhos

```
┌─────────────────────────────────────────────────────────────────┐
│ CADASTROS EM ANDAMENTO                                     [X] │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ FI RF CREDITO PRIVADO                                      │ │
│ │ CNPJ: 12.345.678/0001-99                                   │ │
│ │ Progresso: ████████████░░░░░░░░ 60%                        │ │
│ │ Última edição: 20/01/2024 15:30                            │ │
│ │                                                             │ │
│ │ [Continuar]                                          [🗑]  │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ Rascunho sem nome #1                                       │ │
│ │ Progresso: ████░░░░░░░░░░░░░░░░ 20%                        │ │
│ │ Última edição: 18/01/2024 10:15                            │ │
│ │ ⚠️ Expira em 3 dias                                        │ │
│ │                                                             │ │
│ │ [Continuar]                                          [🗑]  │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ──────────────────────────────────────────────────────────────  │
│                                                                 │
│                    [Iniciar Novo Cadastro]                     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Indicador de Auto-save

```
┌────────────────────────────────────────────┐
│ 💾 Rascunho salvo às 15:32                │
└────────────────────────────────────────────┘
```

- Exibir por 3 segundos após salvamento
- Posicionar no canto inferior ou junto ao stepper

---

## Backend

### Endpoints

**Listar rascunhos:**
```
GET /api/v1/wizard/rascunhos
```

**Response:**
```json
{
  "items": [
    {
      "id": "uuid",
      "nome_identificacao": "FI RF CREDITO PRIVADO",
      "cnpj": "12345678000199",
      "etapa_atual": 6,
      "progresso": 60,
      "updated_at": "2024-01-20T15:30:00Z",
      "expires_at": "2024-02-19T15:30:00Z",
      "dias_para_expirar": 28
    }
  ],
  "total": 2,
  "limite": 5
}
```

**Obter rascunho:**
```
GET /api/v1/wizard/rascunhos/{id}
```

**Response:**
```json
{
  "id": "uuid",
  "etapa_atual": 6,
  "progresso": 60,
  "dados": {
    "identificacao": { ... },
    "classificacao": { ... },
    ...
  },
  "updated_at": "2024-01-20T15:30:00Z"
}
```

**Salvar rascunho:**
```
POST /api/v1/wizard/rascunhos
PUT /api/v1/wizard/rascunhos/{id}
```

**Request:**
```json
{
  "etapa_atual": 6,
  "dados": {
    "identificacao": { ... },
    "classificacao": { ... },
    ...
  }
}
```

**Excluir rascunho:**
```
DELETE /api/v1/wizard/rascunhos/{id}
```

### Entidade `WizardRascunho`

| Campo DB | Tipo | Constraints |
|----------|------|-------------|
| `id` | UUID | PK |
| `usuario_id` | UUID | FK, NOT NULL |
| `nome_identificacao` | VARCHAR(200) | NULL |
| `etapa_atual` | SMALLINT | NOT NULL |
| `progresso` | SMALLINT | NOT NULL |
| `dados_json` | JSONB | NOT NULL |
| `created_at` | TIMESTAMP | NOT NULL |
| `updated_at` | TIMESTAMP | NOT NULL |
| `expires_at` | TIMESTAMP | NOT NULL |

### Job de Limpeza

Criar job/worker que executa diariamente:
1. Buscar rascunhos com `expires_at < NOW()`
2. Excluir documentos temporários associados
3. Excluir rascunhos expirados
4. Log de rascunhos removidos

---

## Critérios de Aceite

- [ ] Auto-save executa ao mudar de etapa
- [ ] Indicador de salvamento aparece
- [ ] Botão "Salvar Rascunho" funciona
- [ ] Modal de rascunhos existentes aparece
- [ ] Carregar rascunho restaura estado completo
- [ ] Navegação restaura para etapa correta
- [ ] Múltiplos rascunhos são listados
- [ ] Limite de 5 rascunhos é respeitado
- [ ] Exclusão de rascunho funciona
- [ ] Ao criar fundo, rascunho é excluído
- [ ] Alerta de expiração é exibido

---

## Dependências

- Slice 01-11 completos
- Sistema de autenticação (para usuario_id)

## Considerações Finais

Este slice é considerado uma **melhoria de UX** e pode ser implementado após os slices 01-11 estarem funcionais. O wizard pode operar sem persistência de rascunho na primeira versão.

---

*Fim da documentação de slices do Wizard de Cadastro de Fundos*
