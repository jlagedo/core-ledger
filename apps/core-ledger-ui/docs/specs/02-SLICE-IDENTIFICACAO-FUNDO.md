# SLICE 02: Etapa 1 - Identificação do Fundo

## Objetivo

Implementar a primeira etapa do wizard onde o usuário informa os dados básicos de identificação do fundo.

## Escopo

### Campos do Formulário

| Campo | Tipo | Obrigatório | Validação |
|-------|------|-------------|-----------|
| `cnpj` | string(14) | Sim | Formato CNPJ válido, dígitos verificadores |
| `razao_social` | string(200) | Sim | Mínimo 10 caracteres |
| `nome_fantasia` | string(100) | Sim | Mínimo 3 caracteres |
| `nome_curto` | string(50) | Não | Máximo 50 caracteres |
| `tipo_fundo` | enum | Sim | Valor válido do enum |
| `data_constituicao` | date | Sim | Data válida, não futura |
| `data_inicio_atividade` | date | Sim | >= data_constituicao |

### Valores do Enum `tipo_fundo`

| Valor | Descrição |
|-------|-----------|
| `FI` | Fundo de Investimento |
| `FIC` | Fundo de Investimento em Cotas |
| `FIDC` | Fundo de Investimento em Direitos Creditórios |
| `FIDC_NP` | FIDC Não Padronizado |
| `FIP` | Fundo de Investimento em Participações |
| `FII` | Fundo de Investimento Imobiliário |
| `FIAGRO` | Fundo Investimento Cadeias Agroindustriais |
| `FI_INFRA` | Fundo de Investimento em Infraestrutura |
| `ETF` | Fundo de Índice |
| `FMP_FGTS` | Fundo Mútuo de Privatização |

---

## Requisitos Funcionais

### RF-01: Validação de CNPJ

- Validar formato: 14 dígitos numéricos
- Validar dígitos verificadores (algoritmo padrão Receita Federal)
- Exibir mensagem de erro específica: "CNPJ inválido"
- Aplicar máscara de exibição: `XX.XXX.XXX/XXXX-XX`

### RF-02: Verificação de Duplicidade

- Ao perder foco do campo CNPJ, verificar se já existe cadastro
- Endpoint de verificação:
  ```
  GET /api/v1/fundos/verificar-cnpj/{cnpj}
  ```
- Se existir, exibir: "CNPJ já cadastrado para o fundo [nome]"
- Bloquear avanço se CNPJ duplicado

### RF-03: Validação de Datas

- `data_constituicao` não pode ser data futura
- `data_inicio_atividade` deve ser >= `data_constituicao`
- Exibir mensagem clara se regra violada

### RF-04: Preenchimento Automático

- Ao selecionar `tipo_fundo`, sugerir prefixo para `nome_curto` baseado no tipo
- Exemplo: FIDC → sugerir "FIDC "

---

## Backend

### Endpoint de Verificação de CNPJ

```
GET /api/v1/fundos/verificar-cnpj/{cnpj}
```

**Response (CNPJ não existe):**
```json
{
  "disponivel": true
}
```

**Response (CNPJ já existe):**
```json
{
  "disponivel": false,
  "fundo_id": "uuid",
  "nome_fantasia": "Nome do Fundo Existente"
}
```

### Validação no DTO

O backend deve validar:
- CNPJ com dígitos verificadores válidos
- Campos obrigatórios preenchidos
- Tamanhos máximos respeitados

---

## Frontend

### Componente

**WizardStep1IdentificacaoComponent**

- Formulário reativo (ReactiveFormsModule)
- Validadores síncronos para formato
- Validador assíncrono para duplicidade de CNPJ
- Feedback visual de validação em tempo real

### Máscaras

| Campo | Máscara |
|-------|---------|
| CNPJ | `00.000.000/0000-00` |
| Datas | `dd/MM/yyyy` |

### UX

- Loading indicator durante verificação de CNPJ
- Destaque visual em campos com erro
- Tooltips explicativos para cada campo

---

## Critérios de Aceite

- [ ] Formulário renderiza todos os campos especificados
- [ ] CNPJ é validado com dígitos verificadores
- [ ] Máscara de CNPJ funciona corretamente
- [ ] Verificação de duplicidade executa ao blur do campo
- [ ] Mensagens de erro são exibidas inline
- [ ] Select de tipo_fundo mostra todas as opções
- [ ] Validação de datas funciona
- [ ] Botão "Próximo" só habilita quando formulário válido
- [ ] Dados persistem ao navegar para próxima etapa e voltar

---

## Dependências

- Slice 01 concluído (infraestrutura base)

## Próximo Slice

→ `03-SLICE-CLASSIFICACAO.md`
