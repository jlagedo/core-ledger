# SLICE 10: Etapa 9 - Upload de Documentos

## Objetivo

Implementar a nona etapa do wizard para upload de documentos do fundo (regulamento, lâmina, etc.). Esta etapa é **opcional** no fluxo do wizard.

## Escopo

### Modelo de Dados - Documento

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `tipo_documento` | enum | Sim | Tipo do documento |
| `versao` | integer | Sim | Versão do documento |
| `data_vigencia` | date | Sim | Data de início da vigência |
| `data_fim_vigencia` | date | Não | Data de fim da vigência |
| `arquivo_nome` | string(200) | Sim | Nome do arquivo |
| `arquivo_path` | string(500) | Sim | Caminho do arquivo |
| `arquivo_hash` | string(64) | Sim | Hash SHA-256 |
| `arquivo_tamanho` | bigint | Sim | Tamanho em bytes |
| `observacoes` | text | Não | Observações |

---

### Valores do Enum `tipo_documento`

| Valor | Descrição | Obrigatório |
|-------|-----------|-------------|
| `REGULAMENTO` | Regulamento do Fundo | Recomendado |
| `LAMINA` | Lâmina de Informações | Não |
| `FIC` | Formulário Info. Complementares | Não |
| `PROSPECTO` | Prospecto | Não |
| `TERMO_ADESAO` | Termo de Adesão | Não |
| `POLITICA_INVESTIMENTO` | Política de Investimento | Não |
| `MANUAL_COMPLIANCE` | Manual de Compliance | Não |
| `CONTRATO_CUSTODIA` | Contrato de Custódia | Não |
| `CONTRATO_GESTAO` | Contrato de Gestão | Não |
| `ATA_ASSEMBLEIA` | Ata de Assembleia | Não |
| `PARECER_AUDITOR` | Parecer do Auditor | Não |

---

## Requisitos Funcionais

### RF-01: Etapa Opcional

- Nenhum documento é obrigatório para concluir o wizard
- Exibir mensagem: "Você pode adicionar documentos agora ou posteriormente"
- Permitir avançar mesmo sem uploads

### RF-02: Recomendação de Regulamento

- Exibir aviso se não houver REGULAMENTO anexado
- Texto: "Recomendamos anexar o regulamento do fundo"
- Não bloquear avanço

### RF-03: Upload de Arquivo

- Formatos aceitos: PDF, DOC, DOCX
- Tamanho máximo: 10MB por arquivo
- Validar tipo MIME do arquivo
- Exibir progresso do upload

### RF-04: Drag and Drop

- Área de drop para arrastar arquivos
- Suporte a múltiplos arquivos de uma vez
- Feedback visual durante hover

### RF-05: Versionamento

- Cada tipo de documento pode ter múltiplas versões
- Ao fazer upload de documento já existente, incrementar versão
- Manter histórico de versões anteriores

### RF-06: Hash e Integridade

- Calcular SHA-256 do arquivo no backend
- Armazenar para verificação de integridade futura

### RF-07: Preview de PDF

- Para arquivos PDF, exibir preview thumbnail
- Link para download do arquivo

### RF-08: Lista de Documentos

- Exibir lista de documentos já anexados
- Permitir remover documentos
- Exibir: tipo, versão, data vigência, tamanho

---

## Frontend

### Componente

**WizardStep9DocumentosComponent**

### Layout Sugerido

```
┌─────────────────────────────────────────────────────────────────┐
│ DOCUMENTOS DO FUNDO (opcional)                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│ ⚠️ Recomendamos anexar o regulamento do fundo                   │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │                                                             │ │
│ │     📁 Arraste arquivos aqui ou clique para selecionar     │ │
│ │                                                             │ │
│ │     Formatos: PDF, DOC, DOCX | Máximo: 10MB                │ │
│ │                                                             │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ OU                                                              │
│                                                                 │
│ Tipo de documento: [Regulamento ▼]                             │
│ Data de vigência: [01/02/2024]                                 │
│ [Selecionar arquivo...]                                        │
│                                                                 │
│ ──────────────────────────────────────────────────────────────  │
│                                                                 │
│ DOCUMENTOS ANEXADOS                                             │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ 📄 Regulamento v1                            [👁] [🗑]      │ │
│ │    regulamento_fundo_rf.pdf                                │ │
│ │    Vigência: 01/02/2024 | 2.3 MB                          │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ 📄 Lâmina v1                                 [👁] [🗑]      │ │
│ │    lamina_informacoes.pdf                                  │ │
│ │    Vigência: 01/02/2024 | 0.5 MB                          │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Componentes de Upload

Usar componente de drag-and-drop do Angular CDK ou biblioteca como `ngx-file-drop`.

---

## Backend

### Endpoint de Upload

```
POST /api/v1/fundos/{fundo_id}/documentos
Content-Type: multipart/form-data
```

**Form Data:**
- `arquivo`: File
- `tipo_documento`: string
- `data_vigencia`: date (ISO 8601)
- `observacoes`: string (opcional)

**Response (201):**
```json
{
  "id": 123,
  "tipo_documento": "REGULAMENTO",
  "versao": 1,
  "arquivo_nome": "regulamento_fundo_rf.pdf",
  "arquivo_tamanho": 2450000,
  "arquivo_hash": "sha256:abc123...",
  "data_vigencia": "2024-02-01",
  "created_at": "2024-01-20T10:30:00Z"
}
```

### Endpoint de Upload Temporário (Wizard)

Como o fundo ainda não foi criado, usar endpoint temporário:

```
POST /api/v1/wizard/documentos/temp
Content-Type: multipart/form-data
```

**Response:**
```json
{
  "temp_id": "uuid",
  "arquivo_nome": "regulamento_fundo_rf.pdf",
  "arquivo_tamanho": 2450000,
  "arquivo_hash": "sha256:abc123...",
  "expires_at": "2024-01-21T10:30:00Z"
}
```

O `temp_id` será enviado no payload final do wizard para vinculação.

### Endpoint de Download

```
GET /api/v1/documentos/{id}/download
```

### Entidade `FundoDocumento`

| Campo DB | Tipo | Constraints |
|----------|------|-------------|
| `id` | BIGINT | PK, auto-increment |
| `fundo_id` | UUID | FK |
| `tipo_documento` | VARCHAR(30) | NOT NULL |
| `versao` | SMALLINT | NOT NULL |
| `data_vigencia` | DATE | NOT NULL |
| `data_fim_vigencia` | DATE | NULL |
| `arquivo_nome` | VARCHAR(200) | NOT NULL |
| `arquivo_path` | VARCHAR(500) | NOT NULL |
| `arquivo_hash` | VARCHAR(64) | NOT NULL |
| `arquivo_tamanho` | BIGINT | NOT NULL |
| `observacoes` | TEXT | NULL |
| `aprovado` | BOOLEAN | DEFAULT false |
| `aprovado_por` | UUID | NULL |
| `aprovado_em` | TIMESTAMP | NULL |
| `created_at` | TIMESTAMP | NOT NULL |

### Validações Backend

- Tipo MIME permitido (application/pdf, application/msword, etc.)
- Tamanho máximo 10MB
- Hash SHA-256 calculado no servidor

---

## Storage

Armazenamento recomendado:
- Azure Blob Storage ou AWS S3 para produção
- Sistema de arquivos local para desenvolvimento

Estrutura de diretórios:
```
/storage/fundos/{fundo_id}/documentos/{tipo}/{versao}/{arquivo}
```

---

## Critérios de Aceite

- [ ] Área de drop renderiza corretamente
- [ ] Upload via botão funciona
- [ ] Upload via drag-and-drop funciona
- [ ] Validação de tipo de arquivo
- [ ] Validação de tamanho máximo
- [ ] Progresso de upload exibido
- [ ] Lista de documentos anexados renderiza
- [ ] Preview de PDF funciona
- [ ] Download de documento funciona
- [ ] Remoção de documento funciona
- [ ] Permite avançar sem documentos

---

## Dependências

- Slice 01: Infraestrutura base
- Configuração de storage (local ou cloud)

## Próximo Slice

→ `11-SLICE-REVISAO-SUBMISSAO.md`
