# Análise de Gaps - Wizard vs Especificação Original

## Status: ✅ CORRIGIDO

Esta análise identificou os desvios entre os slices originais e a especificação. **Todos os gaps foram corrigidos.**

## Resumo das Correções Aplicadas

| Categoria | Status |
|-----------|--------|
| Campos faltantes | ✅ **Adicionados** |
| Nomes de campos | ✅ **Corrigidos** |
| Valores de enum faltantes | ✅ **Adicionados** |
| Entidade Parâmetros FIDC | ✅ **Slice 08-B criado** |
| API Backend | ✅ **Slice 15 criado** |
| Desvio intencional (wizard_completo) | ✅ **Documentado** |

---

## 1. Desvio Intencional (Já Documentado)

### 1.1 Campos `wizard_completo` e `progresso_cadastro`

| Item | Especificação Original | Wizard |
|------|------------------------|--------|
| `wizard_completo` | Sim (linha 294) | ❌ Removido |
| `progresso_cadastro` | Sim (linha 295) | ❌ Removido |

**Justificativa**: Com criação atômica, esses campos são sempre `true` e `100`. Movidos para tabela `wizard_rascunho`.

**Ação**: ✅ Já documentado no arquivo `13-ESPECIFICACAO-ENDPOINT-WIZARD.md`

---

## 2. Parâmetros de Cota (Slice 05)

### 2.1 Campos FALTANTES no Wizard

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `casas_decimais_pl` | INTEGER | Casas decimais do PL (padrão: 2) |
| `data_cota_inicial` | DATE | Data da primeira cota |
| `fuso_horario` | VARCHAR(50) | Fuso horário (padrão: America/Sao_Paulo) |
| `permite_cota_estimada` | BOOLEAN | Permite divulgação de cota estimada |

### 2.2 Campos EXTRAS no Wizard (não especificados)

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `frequencia_calculo` | ENUM | DIARIA/MENSAL |

**Ação Necessária**: Atualizar Slice 05 para incluir campos faltantes e remover/avaliar `frequencia_calculo`.

---

## 3. Taxas do Fundo (Slice 06)

### 3.1 Campos FALTANTES no Wizard

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `classe_id` | UUID | FK para Classe (taxa específica da classe) |
| `percentual_minimo` | DECIMAL(8,6) | Taxa mínima (taxas escalonadas) |
| `percentual_maximo` | DECIMAL(8,6) | Taxa máxima (taxas escalonadas) |
| `linha_dagua_global` | BOOLEAN | Linha d'água global ou por cotista |
| `ativo` | BOOLEAN | Indica se taxa está ativa |

### 3.2 Valores de Enum FALTANTES - `tipo_taxa`

| Valor | Descrição |
|-------|-----------|
| `DISTRIBUICAO` | Taxa de distribuição |
| `CONSULTORIA` | Taxa de consultoria (FIDCs) |
| `ESCRITURACAO` | Taxa de escrituração |
| `ESTRUTURACAO` | Taxa de estruturação (FIDCs/FIPs) |

### 3.3 Valores de Enum FALTANTES - `forma_cobranca`

| Valor | Descrição |
|-------|-----------|
| `EVENTO` | Cobrança por evento (ex: estruturação) |

**Ação Necessária**: Atualizar Slice 06 com campos e enums faltantes.

---

## 4. Prazos Operacionais (Slice 07)

### 4.1 Nomes de Campos DIFERENTES

| Wizard | Especificação Original | Ação |
|--------|------------------------|------|
| `tipo_prazo` | `tipo_operacao` | Renomear |
| `dias_cotizacao` | `prazo_cotizacao` | Renomear |
| `dias_liquidacao` | `prazo_liquidacao` | Renomear |
| `valor_minimo` | `valor_minimo_inicial` | Renomear |
| `permite_parcial` | `permite_resgate_total` (invertido) | Ajustar |

### 4.2 Campos FALTANTES no Wizard

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `classe_id` | UUID | FK para Classe (prazo específico) |
| `valor_minimo_adicional` | DECIMAL(18,2) | Mínimo aplicação adicional |
| `valor_minimo_permanencia` | DECIMAL(18,2) | Mínimo de permanência |
| `permite_resgate_programado` | BOOLEAN | Permite agendamento |
| `prazo_maximo_programacao` | INTEGER | Máximo dias úteis para agendar |
| `tipo_calendario` | VARCHAR(20) | Praça do calendário para D+X |
| `ativo` | BOOLEAN | Indica se prazo está ativo |

### 4.3 Valores de Enum FALTANTES - `tipo_operacao`

| Valor | Descrição |
|-------|-----------|
| `RESGATE_CRISE` | Prazos para resgate em cenário de crise |

### 4.4 Campo EXTRA no Wizard (não especificado)

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `tipo_dia` | ENUM | UTIL/CORRIDO |

**Nota**: `tipo_dia` faz sentido mas não está na especificação original. Avaliar se deve ser adicionado à especificação.

**Ação Necessária**: Renomear campos e adicionar faltantes no Slice 07.

---

## 5. Classes CVM 175 (Slice 08)

### 5.1 Campos FALTANTES no Wizard

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `cnpj_classe` | VARCHAR(14) | CNPJ específico da classe |
| `classe_pai_id` | UUID | FK para classe pai (subclasses) |
| `nivel` | INTEGER | Nível hierarquia (1=classe, 2=subclasse) |
| `taxa_administracao` | DECIMAL(8,6) | Taxa específica (se diferente do fundo) |
| `taxa_gestao` | DECIMAL(8,6) | Taxa de gestão específica |
| `taxa_performance` | DECIMAL(8,6) | Taxa performance específica |
| `benchmark_id` | INTEGER | FK para Indexador (benchmark específico) |
| `valor_minimo_permanencia` | DECIMAL(18,2) | Mínimo de permanência da classe |
| `permite_resgate_antecipado` | BOOLEAN | Permite resgate antes vencimento (FIDC) |
| `data_inicio` | DATE | Data de início da classe |
| `data_encerramento` | DATE | Data de encerramento |
| `motivo_encerramento` | VARCHAR(200) | Motivo do encerramento |
| `ativo` | BOOLEAN | Indica se classe está ativa |

### 5.2 Valores de Enum FALTANTES - `tipo_classe_fidc`

| Valor | Descrição |
|-------|-----------|
| `SUBORDINADA_JUNIOR` | Classe subordinada júnior |

**Ação Necessária**: Atualizar Slice 08 com campos faltantes.

---

## 6. Vínculos Institucionais (Slice 09)

### 6.1 Diferença Estrutural CRÍTICA

| Wizard | Especificação Original |
|--------|------------------------|
| `instituicao_id` (FK) | `cnpj_instituicao` + `nome_instituicao` (inline) |

O wizard assume existência de entidade `Instituicao` separada. A especificação original armazena dados inline.

**Decisão Necessária**: Manter FK (melhor normalização) ou usar dados inline (conforme especificação)?

### 6.2 Campos FALTANTES no Wizard

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `codigo_cvm` | VARCHAR(20) | Código CVM da instituição |
| `responsavel_telefone` | VARCHAR(20) | Telefone do responsável |
| `motivo_fim` | VARCHAR(200) | Motivo do término |
| `ativo` | BOOLEAN | Indica se vínculo está ativo |

### 6.3 Valores de Enum FALTANTES - `tipo_vinculo`

| Valor | Descrição |
|-------|-----------|
| `ESCRITURADOR` | Escriturador de cotas |
| `CONTROLADOR` | Controlador (se terceirizado) |
| `CONSULTORIA_CREDITO` | Consultoria de crédito (FIDC) |
| `CEDENTE` | Cedente de recebíveis (FIDC) |
| `FORMADOR_MERCADO` | Formador de mercado (ETF/FII) |

**Ação Necessária**: Decidir sobre estrutura e adicionar campos/enums faltantes.

---

## 7. Parâmetros FIDC - ENTIDADE NÃO CONTEMPLADA 🔴

### 7.1 Situação

A especificação original define entidade `Parâmetros FIDC` (linhas 556-612) com 18 campos específicos para FIDCs.

**O wizard NÃO possui etapa para Parâmetros FIDC.**

### 7.2 Campos da Entidade

| Campo | Tipo | Obrigatório |
|-------|------|-------------|
| `tipo_fidc` | ENUM | Sim |
| `tipo_recebiveis` | ENUM[] | Sim |
| `prazo_medio_carteira` | INTEGER | Não |
| `indice_subordinacao_alvo` | DECIMAL(8,4) | Não |
| `provisao_devedores_duvidosos` | DECIMAL(8,4) | Não |
| `limite_concentracao_cedente` | DECIMAL(8,4) | Não |
| `limite_concentracao_sacado` | DECIMAL(8,4) | Não |
| `possui_coobrigacao` | BOOLEAN | Sim |
| `percentual_coobrigacao` | DECIMAL(8,4) | Não |
| `permite_cessao_parcial` | BOOLEAN | Sim |
| `rating_minimo` | VARCHAR(10) | Não |
| `agencia_rating` | VARCHAR(50) | Não |
| `registradora_recebiveis` | ENUM | Não |
| `conta_registradora` | VARCHAR(50) | Não |
| `integracao_registradora` | BOOLEAN | Sim |

### 7.3 Enums da Entidade

**tipo_fidc:**
- `PADRONIZADO`
- `NAO_PADRONIZADO`

**tipo_recebiveis:**
- `DUPLICATAS`, `CCB`, `CCI`, `CARTAO_CREDITO`, `CHEQUES`, `CONTRATOS_ALUGUEL`, `ENERGIA`, `FINANCIAMENTO_VEICULOS`, `CREDITO_CONSIGNADO`, `PRECATORIOS`, `CREDITOS_JUDICIAIS`, `OUTROS`

**registradora_recebiveis:**
- `LAQUS`, `CERC`, `TAG`, `B3`

**Ação Necessária**: Criar novo slice (08-B ou renumerar) para Parâmetros FIDC, condicional ao tipo de fundo.

---

## 8. Classificação/Tributação (Slice 03)

### 8.1 Valores de Enum DIFERENTES - `tributacao`

| Wizard | Especificação Original |
|--------|------------------------|
| `FII` | `IMOBILIARIO` |
| - | `PREVIDENCIA` (faltando) |
| `FIAGRO` | - (não existe na especificação) |

**Ação Necessária**: Alinhar valores de enum com especificação.

---

## 9. Documentos (Slice 10)

### 9.1 Verificação

Campos parecem alinhados. Verificar detalhes menores.

---

## 10. Resumo de Ações Necessárias

### 10.1 Ações Críticas (Bloqueiam Implementação)

| # | Ação | Slice Afetado |
|---|------|---------------|
| 1 | Criar etapa Parâmetros FIDC | Novo slice 08-B |
| 2 | Decidir estrutura de Vínculos (FK vs inline) | Slice 09 |

### 10.2 Ações de Alinhamento (Campos)

| # | Ação | Slice Afetado |
|---|------|---------------|
| 3 | Adicionar campos de Parâmetros de Cota | Slice 05 |
| 4 | Adicionar campos de Taxas | Slice 06 |
| 5 | Renomear e adicionar campos de Prazos | Slice 07 |
| 6 | Adicionar campos de Classes | Slice 08 |
| 7 | Adicionar campos de Vínculos | Slice 09 |

### 10.3 Ações de Alinhamento (Enums)

| # | Ação | Slice Afetado |
|---|------|---------------|
| 8 | Adicionar tipos de taxa faltantes | Slice 06 |
| 9 | Adicionar tipos de vínculo faltantes | Slice 09 |
| 10 | Adicionar tipo classe FIDC faltante | Slice 08 |
| 11 | Alinhar valores de tributação | Slice 03 |

---

## 11. Recomendação

### Opção A: Atualizar Slices (Recomendado)
Corrigir todos os desvios nos slices existentes antes de iniciar implementação.

### Opção B: Atualizar Especificação Original
Se alguns campos/estruturas do wizard forem melhores, atualizar a especificação.

### Opção C: Híbrido
- Campos faltantes → Adicionar aos slices
- Campos extras (ex: `tipo_dia`) → Adicionar à especificação se fizerem sentido
- Estrutura de Vínculos → Decidir e documentar

---

*Análise gerada em Janeiro/2026*
*Comparação: Slices Wizard v1 vs Especificacao_Modulo_Cadastro_Fundos.md v2.0*
