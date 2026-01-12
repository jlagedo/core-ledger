# SLICE 01: Infraestrutura Base do Wizard

## Objetivo

Criar a estrutura fundamental do wizard que será utilizada por todas as etapas subsequentes. Este slice estabelece a navegação, gerenciamento de estado e endpoint base da API.

## Escopo

### Backend (API)

**Endpoint principal do wizard:**

```
POST /api/v1/fundos/wizard
```

Este endpoint recebe o payload completo do wizard e cria o fundo com todas as entidades relacionadas em uma transação única.

**Request DTO esperado:**

```json
{
  "identificacao": { },
  "classificacao": { },
  "caracteristicas": { },
  "parametrosCota": { },
  "taxas": [ ],
  "prazos": [ ],
  "classes": [ ],
  "vinculos": [ ],
  "documentos": [ ]
}
```

**Responses:**

| Status | Descrição |
|--------|-----------|
| 201 | Fundo criado com sucesso |
| 400 | Erro de validação |
| 409 | CNPJ já cadastrado |
| 500 | Erro interno |

**Endpoint de progresso:**

```
GET /api/v1/fundos/{id}/progresso
```

Retorna o percentual de preenchimento do cadastro.

---

### Frontend (Angular)

**Componentes necessários:**

1. **WizardContainerComponent** - Container principal do wizard
2. **WizardStepperComponent** - Barra de progresso/navegação entre etapas
3. **WizardNavigationComponent** - Botões Anterior/Próximo/Salvar

**Estado NgRx:**

O estado do wizard deve armazenar:
- Etapa atual (número 1-10)
- Dados de cada etapa
- Status de validação por etapa
- Flag de submissão em andamento

**Rota:**

```
/cadastros/fundos/novo
```

---

## Requisitos Funcionais

### RF-01: Navegação entre Etapas

- O usuário deve poder avançar para a próxima etapa ao clicar em "Próximo"
- O usuário deve poder voltar para a etapa anterior ao clicar em "Anterior"
- A navegação para a próxima etapa só é permitida se a etapa atual estiver válida
- O usuário pode navegar livremente para etapas anteriores já preenchidas

### RF-02: Barra de Progresso Visual

- Exibir indicador visual das 10 etapas
- Marcar etapas completas com check (✓)
- Destacar etapa atual
- Mostrar etapas pendentes como círculos vazios

**Referência visual:**
```
[✓] Identificação  [✓] Classificação  [●] Características  [○] Cota
[○] Taxas  [○] Prazos  [○] Classes  [○] Vínculos  [○] Docs  [○] Revisão
```

### RF-03: Persistência do Estado

- Os dados preenchidos devem persistir ao navegar entre etapas
- Ao sair do wizard sem salvar, exibir confirmação
- O estado é mantido em memória (NgRx) durante a sessão

### RF-04: Indicador de Progresso Percentual

- Calcular e exibir o percentual de preenchimento (0-100%)
- Fórmula: `(etapas_completas / total_etapas) * 100`

---

## Requisitos Não-Funcionais

- Tempo de transição entre etapas < 100ms
- Layout responsivo (desktop, tablet, mobile)
- Suporte a navegação por teclado (Tab, Enter)

---

## Critérios de Aceite

- [ ] Container do wizard renderiza corretamente
- [ ] Stepper mostra as 10 etapas
- [ ] Navegação Próximo/Anterior funciona
- [ ] Estado persiste entre etapas
- [ ] Endpoint POST /api/v1/fundos/wizard responde (pode retornar 501 Not Implemented por enquanto)
- [ ] Validação bloqueia avanço quando etapa está inválida

---

## Dependências

- Nenhuma (este é o primeiro slice)

## Próximo Slice

→ `02-SLICE-IDENTIFICACAO-FUNDO.md`
