# Especificação Funcional: Lista de Fundos (Grid de Consulta)
## Módulo de Cadastro - Sistema Fund Accounting

**Versão:** 1.0  
**Data:** Janeiro/2026  
**Módulo:** Cadastro de Fundos  
**Funcionalidade:** Lista/Grid de Consulta

---

## 1. Visão Geral

A Lista de Fundos é a tela principal do módulo de Cadastro de Fundos, permitindo aos usuários visualizar, pesquisar e filtrar fundos cadastrados no sistema. A interface segue o padrão moderno de grid baseado em AG Grid, com filtros avançados, presets e ações rápidas.

**Objetivo:** Proporcionar aos usuários uma forma eficiente de localizar fundos através de múltiplos critérios de busca e filtros combinados.

---

## 2. Estrutura da Tela

### 2.1 Layout Geral

A tela é dividida em 5 seções verticais principais:

```
┌─────────────────────────────────────────────────────────────────┐
│  1. CABEÇALHO DA PÁGINA                                         │
│     [Título: "Fundos"] [Botão Atualizar] [Botão Novo Fundo]    │
├─────────────────────────────────────────────────────────────────┤
│  2. BARRA DE PRESETS (Filtros Rápidos)                          │
│     [Todos] [Ativos] [Inativos] [Meus Fundos] [FIDCs] ...      │
├─────────────────────────────────────────────────────────────────┤
│  3. BARRA DE FILTROS                                            │
│     [Busca Rápida] [Tipo ▼] [Classe CVM ▼] [Situação ▼] ...    │
├─────────────────────────────────────────────────────────────────┤
│  4. FILTROS ATIVOS (Pills Removíveis)                           │
│     [Tipo: FIDC ×] [Situação: Ativo ×] [Admin: XYZ ×]          │
├─────────────────────────────────────────────────────────────────┤
│  5. GRADE DE DADOS (AG Grid)                                    │
│     [Tabela com colunas e dados dos fundos]                     │
├─────────────────────────────────────────────────────────────────┤
│  6. BARRA DE STATUS                                             │
│     ● READY | Total: 127 fundos | Filtrados: 45 | Sel: 2       │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 Dimensões e Responsividade

| Dispositivo | Largura Mínima | Comportamento |
|-------------|----------------|---------------|
| Desktop | 1280px | Layout completo |
| Tablet | 768px | Colunas adaptadas, scroll horizontal |
| Mobile | 375px | Lista simplificada (cards) |

---

## 3. Seção 1: Cabeçalho da Página

### 3.1 Elementos

| Elemento | Posição | Descrição | Interação |
|----------|---------|-----------|-----------|
| **Título** | Esquerda | "Fundos" (H1) | - |
| **Botão Atualizar** | Centro-direita | Ícone 🔄 + tooltip | Recarrega dados da grid |
| **Botão Novo Fundo** | Direita | "+ Novo Fundo" (primário) | Abre wizard de criação |

### 3.2 Comportamento
- Cabeçalho fixo no topo durante scroll
- Botão "Novo Fundo" sempre visível
- Tooltip no botão Atualizar: "Atualizar dados (Ctrl+R)"

---

## 4. Seção 2: Barra de Presets (Filtros Rápidos)

### 4.1 Presets Disponíveis

| Preset | Ícone | Filtros Aplicados | Cor |
|--------|-------|-------------------|-----|
| **Todos** | 📋 | Nenhum filtro | Neutro |
| **Ativos** | ✅ | Situação = ATIVO | Verde |
| **Inativos** | ❌ | Situação IN (INATIVO, LIQUIDADO) | Vermelho |
| **Meus Fundos** | 👤 | Gestor = usuário logado | Azul |
| **FIDCs** | 📊 | Tipo = FIDC | Verde escuro |
| **Renda Fixa** | 💵 | Classe CVM = Renda Fixa | Azul claro |
| **Multimercado** | 📈 | Classe CVM = Multimercado | Laranja |
| **Ações** | 📊 | Classe CVM = Ações | Roxo |
| **Abertos** | 🔓 | Regime = ABERTO | Azul |
| **Fechados** | 🔒 | Regime = FECHADO | Cinza |
| **Com Pendências** | ⚠️ | Tem alertas de cadastro | Amarelo |
| **Multiclasse** | 🔢 | Nº classes > 1 | Roxo claro |

### 4.2 Comportamento
- Ao clicar em um preset, os filtros são aplicados automaticamente
- Preset ativo fica destacado com borda colorida
- Pills dos filtros ativos aparecem na seção 4
- Apenas um preset pode estar ativo por vez
- Clicar novamente no preset ativo o desativa

### 4.3 Layout
```
[📋 Todos] [✅ Ativos] [❌ Inativos] [👤 Meus Fundos] [📊 FIDCs] 
[💵 RF] [📈 Multi] [📊 Ações] [🔓 Abertos] [🔒 Fechados] [⚠️ Pendências]
```

---

## 5. Seção 3: Barra de Filtros

### 5.1 Filtros Disponíveis

| Filtro | Tipo | Largura | Opções | Comportamento |
|--------|------|---------|--------|---------------|
| **Busca Rápida** | Text Input | 300px | - | Busca em: CNPJ, Razão Social, Nome Fantasia, Código ANBIMA |
| **Tipo de Fundo** | Dropdown Multi | 150px | FI, FIC, FIDC, FII, FIP, FIAGRO, FI-INFRA, ETF | Permite múltipla seleção |
| **Classe CVM** | Dropdown Multi | 180px | Renda Fixa, Ações, Cambial, Multimercado | Permite múltipla seleção |
| **Situação** | Dropdown Multi | 150px | ATIVO, INATIVO, EM_FUNCIONAMENTO_NORMAL, EM_LIQUIDACAO, LIQUIDADO | Permite múltipla seleção |
| **Regime** | Dropdown | 120px | ABERTO, FECHADO | Seleção única |
| **Administrador** | Autocomplete | 200px | - | Mínimo 3 caracteres |
| **Gestor** | Autocomplete | 200px | - | Mínimo 3 caracteres |
| **+ Filtros** | Button | 100px | - | Abre modal com filtros adicionais |

### 5.2 Layout Responsivo

**Desktop (> 1280px):**
```
[Busca Rápida____] [Tipo ▼] [Classe CVM ▼] [Situação ▼] [Regime ▼] 
[Administrador____] [Gestor____] [+ Filtros]
```

**Tablet (768px - 1280px):**
```
[Busca Rápida_________________] [Tipo ▼] [Classe CVM ▼]
[Situação ▼] [Regime ▼] [+ Filtros]
```

### 5.3 Busca Rápida - Especificação

**Campos pesquisados:**
1. CNPJ (com ou sem formatação)
2. Razão Social (busca parcial, case-insensitive)
3. Nome Fantasia (busca parcial, case-insensitive)
4. Código ANBIMA (busca exata)

**Comportamento:**
- Debounce de 300ms após parar de digitar
- Mínimo de 3 caracteres para iniciar busca
- Ícone de lupa no início do campo
- Ícone de "limpar" (×) quando há texto digitado
- Highlight dos termos encontrados nos resultados (opcional)

**Exemplos de busca:**
- `12.345.678` → Localiza fundos com CNPJ começando com esses dígitos
- `XP` → Localiza fundos com "XP" na Razão Social ou Nome Fantasia
- `FIDC` → Localiza fundos com "FIDC" no nome
- `300023` → Localiza por código ANBIMA exato

### 5.4 Filtros Adicionais (Modal "+ Filtros")

Ao clicar em "+ Filtros", abre modal com filtros avançados:

| Filtro | Tipo | Opções |
|--------|------|--------|
| **Tributação** | Dropdown | LONGO_PRAZO, CURTO_PRAZO, ISENTO |
| **Público Alvo** | Dropdown | GERAL, QUALIFICADO, PROFISSIONAL |
| **Condomínio** | Dropdown | ABERTO, FECHADO |
| **Alavancagem** | Checkbox | Sim / Não |
| **Permite Criptoativos** | Checkbox | Sim / Não |
| **Prazo de Duração** | Dropdown | INDETERMINADO, DETERMINADO |
| **Classe FIDC** | Dropdown | SENIOR, MEZANINO, SUBORDINADA |
| **Data Início (de)** | Date Picker | - |
| **Data Início (até)** | Date Picker | - |
| **PL Mínimo** | Number | Valor em R$ |
| **PL Máximo** | Number | Valor em R$ |
| **Custodiante** | Autocomplete | - |
| **Auditor** | Autocomplete | - |

**Layout do Modal:**
```
┌─────────────────────────────────────────┐
│  Filtros Avançados                  [×] │
├─────────────────────────────────────────┤
│                                         │
│  [Campos de filtros organizados         │
│   em 2 colunas]                         │
│                                         │
├─────────────────────────────────────────┤
│         [Limpar] [Aplicar Filtros]      │
└─────────────────────────────────────────┘
```

---

## 6. Seção 4: Filtros Ativos (Pills)

### 6.1 Comportamento

Pills removíveis que representam os filtros ativos:

```
[Tipo: FIDC ×] [Situação: Ativo ×] [Administrador: Órama ×] [Limpar Tudo]
```

**Características:**
- Cor de fundo suave (azul claro)
- Ícone × para remover
- Clicar no × remove o filtro específico
- Botão "Limpar Tudo" remove todos os filtros
- Seção oculta quando não há filtros ativos

### 6.2 Formato das Pills

| Tipo de Filtro | Formato de Exibição |
|----------------|---------------------|
| Busca Rápida | `Busca: "termo digitado" ×` |
| Dropdown Simples | `Nome do Campo: Valor ×` |
| Dropdown Multi | `Nome do Campo: Valor1, Valor2 (+3) ×` |
| Autocomplete | `Nome do Campo: Texto selecionado ×` |
| Data | `Data Início: 01/01/2024 até 31/12/2024 ×` |
| Numérico | `PL: R$ 1.000.000 até R$ 5.000.000 ×` |

---

## 7. Seção 5: Grade de Dados (AG Grid)

### 7.1 Colunas Principais (Sempre Visíveis)

| Coluna | Largura | Tipo | Fixada | Ordenável | Filtrável | Descrição |
|--------|---------|------|--------|-----------|-----------|-----------|
| **[ ]** | 40px | Checkbox | Esq | Não | Não | Seleção múltipla |
| **CNPJ** | 150px | Text | Esq | Sim | Sim | CNPJ formatado |
| **Nome do Fundo** | 300px | Text | Não | Sim | Sim | Razão Social |
| **Nome Fantasia** | 250px | Text | Não | Sim | Sim | Nome de divulgação |
| **Tipo** | 100px | Badge | Não | Sim | Sim | Tipo do fundo |
| **Classe CVM** | 150px | Text | Não | Sim | Sim | Classificação CVM |
| **Situação** | 120px | Badge | Não | Sim | Sim | Status regulatório |
| **Administrador** | 200px | Text | Não | Sim | Sim | Nome do administrador |
| **PL Estimado** | 150px | Numeric | Não | Sim | Sim | Patrimônio Líquido |
| **Nº Classes** | 100px | Numeric | Não | Sim | Sim | Quantidade de classes |
| **Data Início** | 120px | Date | Não | Sim | Sim | Data início funcionamento |
| **Ações** | 100px | Actions | Dir | Não | Não | Botões de ação |

### 7.2 Colunas Opcionais (Configuráveis)

Usuário pode escolher exibir/ocultar através do menu de configuração da grid:

| Coluna | Largura | Tipo | Descrição |
|--------|---------|------|-----------|
| **Código ANBIMA** | 120px | Text | Código ANBIMA |
| **Classificação ANBIMA** | 200px | Text | Classificação completa ANBIMA |
| **Gestor** | 200px | Text | Nome do gestor |
| **Regime** | 100px | Badge | ABERTO / FECHADO |
| **Público Alvo** | 120px | Text | GERAL, QUALIFICADO, PROFISSIONAL |
| **Tributação** | 120px | Text | Regime de tributação |
| **Condomínio** | 100px | Badge | Tipo de condomínio |
| **Prazo** | 150px | Text | Prazo de duração |
| **Alavancagem** | 100px | Boolean | Sim / Não |
| **Criptoativos** | 100px | Boolean | Permite criptoativos |
| **Custodiante** | 200px | Text | Nome do custodiante |
| **Auditor** | 200px | Text | Nome do auditor |
| **Taxa Adm (% a.a.)** | 120px | Numeric | Taxa de administração |
| **Taxa Perf (%)** | 120px | Numeric | Taxa de performance |
| **Benchmark** | 150px | Text | Indexador de referência |
| **D+ Aplicação** | 100px | Numeric | Prazo cotização aplicação |
| **D+ Resgate** | 100px | Numeric | Prazo cotização resgate |
| **D+ Liquidação** | 100px | Numeric | Prazo liquidação resgate |
| **Nº Cotistas** | 100px | Numeric | Quantidade de cotistas |
| **Última Cota** | 120px | Numeric | Última cota calculada |
| **Data Última Cota** | 120px | Date | Data da última cota |
| **Criado em** | 150px | DateTime | Data de cadastro no sistema |
| **Atualizado em** | 150px | DateTime | Última atualização |
| **Criado por** | 150px | Text | Usuário que criou |

### 7.3 Formatação de Valores

| Tipo de Dado | Formato | Exemplo |
|--------------|---------|---------|
| **CNPJ** | ##.###.###/####-## | 12.345.678/0001-90 |
| **Moeda (R$)** | R$ #.###.###,## | R$ 1.234.567,89 |
| **Percentual** | #,##% ou ##% | 2,50% ou 20% |
| **Data** | DD/MM/YYYY | 15/03/2024 |
| **Data/Hora** | DD/MM/YYYY HH:mm | 10/01/2026 14:30 |
| **Inteiro** | #.### | 1.234 |
| **Decimal** | #.###,#### | 1.234,5678 |
| **Boolean** | Sim / Não | Sim |

### 7.4 Badges Coloridos

#### 7.4.1 Tipo de Fundo

| Tipo | Cor de Fundo | Cor do Texto | Ícone |
|------|--------------|--------------|-------|
| **FI** | #3B82F6 (Azul) | Branco | - |
| **FIC** | #93C5FD (Azul claro) | #1E40AF | - |
| **FIDC** | #10B981 (Verde) | Branco | 📊 |
| **FII** | #8B5CF6 (Roxo) | Branco | 🏢 |
| **FIP** | #F59E0B (Laranja) | Branco | 💼 |
| **FIAGRO** | #059669 (Verde escuro) | Branco | 🌾 |
| **FI-INFRA** | #78350F (Marrom) | Branco | 🏗️ |
| **ETF** | #1E3A8A (Azul escuro) | Branco | 📈 |

#### 7.4.2 Situação

| Situação | Cor | Ícone |
|----------|-----|-------|
| **ATIVO** | Verde (#10B981) | ✓ |
| **INATIVO** | Cinza (#6B7280) | ○ |
| **EM_FUNCIONAMENTO_NORMAL** | Verde claro (#34D399) | ✓ |
| **EM_LIQUIDACAO** | Amarelo (#F59E0B) | ⚠ |
| **LIQUIDADO** | Vermelho (#EF4444) | × |

#### 7.4.3 Regime

| Regime | Cor | Ícone |
|--------|-----|-------|
| **ABERTO** | Azul (#3B82F6) | 🔓 |
| **FECHADO** | Cinza (#6B7280) | 🔒 |

### 7.5 Coluna de Ações

Botões disponíveis na última coluna (fixada à direita):

| Ação | Ícone | Tooltip | Comportamento |
|------|-------|---------|---------------|
| **Visualizar** | 👁️ | Ver detalhes | Abre modal/página com detalhes do fundo (somente leitura) |
| **Editar** | ✏️ | Editar fundo | Abre formulário de edição |
| **Menu** | ⋮ | Mais opções | Abre dropdown com ações adicionais |

**Dropdown do Menu (⋮):**

| Opção | Ícone | Descrição |
|-------|-------|-----------|
| Duplicar | 📋 | Cria cópia do fundo |
| Ver Classes | 🔢 | Lista de classes do fundo |
| Ver Taxas | 💰 | Taxas cadastradas |
| Ver Prazos | ⏱️ | Prazos operacionais |
| Ver Vínculos | 🤝 | Vínculos institucionais |
| Ver Documentos | 📄 | Documentos anexados |
| Histórico | 📜 | Histórico de alterações |
| Exportar | 💾 | Exportar dados do fundo |
| --- | --- | --- |
| Inativar / Reativar | ○ / ✓ | Altera situação |
| Excluir | 🗑️ | Exclusão lógica (soft delete) |

---

## 8. Funcionalidades da Grid

### 8.1 Ordenação

**Colunas ordenáveis:**
- CNPJ
- Nome do Fundo
- Nome Fantasia
- Tipo
- Classe CVM
- Situação
- Administrador
- Gestor
- PL Estimado
- Nº Classes
- Data Início
- Criado em

**Comportamento:**
- **1º clique:** Ordem crescente (↑)
- **2º clique:** Ordem decrescente (↓)
- **3º clique:** Remove ordenação
- Indicador visual no cabeçalho da coluna
- Pode ordenar por múltiplas colunas (Shift + clique)

### 8.2 Filtros por Coluna

Cada coluna possui um ícone de filtro no cabeçalho que abre opções específicas:

| Tipo de Coluna | Opções de Filtro |
|----------------|------------------|
| **Text** | Contém, Não contém, Igual a, Começa com, Termina com |
| **Numeric** | Igual, Diferente, Maior que, Menor que, Entre |
| **Date** | Igual, Antes de, Depois de, Entre |
| **Badge/Enum** | Lista de checkboxes com valores únicos |
| **Boolean** | Sim / Não / Todos |

### 8.3 Agrupamento de Linhas

Usuário pode agrupar registros por colunas específicas:

**Colunas agrupáveis:**
- Tipo de Fundo
- Classe CVM
- Situação
- Administrador
- Gestor
- Regime
- Público Alvo
- Tributação

**Comportamento:**
- Arrastar cabeçalho da coluna para área de agrupamento
- Permite múltiplos níveis de agrupamento
- Exibe totalizadores por grupo (quando aplicável)
- Expandir/colapsar grupos

### 8.4 Seleção Múltipla

**Comportamento:**
- Checkbox na primeira coluna
- Checkbox no cabeçalho seleciona/desseleciona todos
- Shift + clique para selecionar intervalo
- Ctrl + clique para seleção individual
- Contador de selecionados na barra de status

**Ações disponíveis com seleção múltipla:**
- Exportar selecionados
- Inativar em lote
- Alterar gestor em lote
- Alterar administrador em lote
- Adicionar tags/marcadores
- Gerar relatório consolidado

### 8.5 Configuração de Colunas

Menu de configuração (ícone ⚙️ ao lado dos filtros):

**Opções:**
- Mostrar/Ocultar colunas (lista com checkboxes)
- Reordenar colunas (drag and drop)
- Redimensionar colunas
- Auto-ajustar largura das colunas
- Resetar para padrão
- Salvar configuração personalizada
- Carregar configuração salva

### 8.6 Totalizadores (Footer)

Linha de totalizadores no rodapé da grid (fixada):

| Coluna | Totalizador |
|--------|-------------|
| **CNPJ** | Total de fundos |
| **PL Estimado** | Soma total |
| **Nº Classes** | Média |
| **Nº Cotistas** | Soma total |

---

## 9. Alertas e Indicadores Visuais

### 9.1 Fundos com Pendências

Fundos com alertas de cadastro exibem:
- **Ícone:** ⚠️ na primeira coluna (antes do checkbox)
- **Cor de fundo:** Amarelo muito claro (#FEF3C7)
- **Tooltip:** Ao passar o mouse, exibe lista de pendências

**Tipos de alerta:**

| Código | Tipo de Pendência | Ícone |
|--------|-------------------|-------|
| PEND-01 | Cadastro incompleto (campos obrigatórios faltando) | ⚠️ |
| PEND-02 | Documentos vencidos ou faltando | 📄 |
| PEND-03 | Vínculos sem prestador ativo | 🤝 |
| PEND-04 | Classes sem parâmetros definidos | 🔢 |
| PEND-05 | Taxas sem benchmark quando obrigatório | 💰 |
| PEND-06 | Prazos não configurados | ⏱️ |
| PEND-07 | Sem cotistas cadastrados (para abertos) | 👥 |

**Exemplo de tooltip:**
```
⚠️ Pendências de Cadastro:
• Cadastro incompleto: Falta campo "Data Vencimento"
• Documentos: Regulamento vencido desde 15/12/2025
• Classes: Classe "A" sem parâmetros de cota
```

### 9.2 FIDCs - Indicadores Especiais

Para fundos do tipo FIDC, exibir informações adicionais na linha:

**Subrow expandível (clique na seta ▶):**

```
▶ FIDC XYZ (Linha principal)
  ├─ Classes: [SENIOR] [MEZANINO] [SUBORDINADA]
  ├─ Séries Ativas: 5 séries
  ├─ DC Não-Padronizados: Sim
  └─ Público Alvo: Investidores Qualificados
```

**Badges inline (na linha principal):**
- Classe Sênior: Badge verde claro
- Classe Mezanino: Badge laranja
- Classe Subordinada: Badge vermelho claro

### 9.3 Destaque de Fundos Novos

Fundos cadastrados há menos de 7 dias:
- **Badge:** "NOVO" em azul
- **Posição:** Ao lado do nome do fundo
- **Duração:** Exibido por 7 dias após cadastro

---

## 10. Exportação de Dados

### 10.1 Formatos Disponíveis

| Formato | Extensão | Descrição | Colunas | Formatação |
|---------|----------|-----------|---------|------------|
| **Excel** | .xlsx | Dados completos com formatação | Todas visíveis | Formatação de valores, cores, bordas |
| **CSV** | .csv | Dados brutos | Todas visíveis | Sem formatação |
| **PDF** | .pdf | Relatório formatado | Selecionáveis | Layout profissional |
| **JSON** | .json | Dados estruturados (API) | Todas disponíveis | Estrutura completa |

### 10.2 Opções de Exportação

Modal de exportação com opções:

```
┌─────────────────────────────────────────┐
│  Exportar Fundos                    [×] │
├─────────────────────────────────────────┤
│                                         │
│  Formato:                               │
│  ○ Excel (.xlsx)                        │
│  ○ CSV (.csv)                           │
│  ○ PDF (.pdf)                           │
│                                         │
│  Registros:                             │
│  ○ Todos os registros (127)             │
│  ○ Apenas filtrados (45)                │
│  ○ Apenas selecionados (2)              │
│                                         │
│  Colunas:                               │
│  ○ Todas as colunas disponíveis         │
│  ○ Apenas colunas visíveis              │
│  ○ Personalizar... [Selecionar]         │
│                                         │
│  Opções Adicionais:                     │
│  ☑ Incluir totalizadores                │
│  ☑ Incluir cabeçalho                    │
│  ☑ Incluir filtros aplicados            │
│  ☐ Incluir alertas e pendências         │
│                                         │
├─────────────────────────────────────────┤
│         [Cancelar] [Exportar]           │
└─────────────────────────────────────────┘
```

### 10.3 Nome do Arquivo Gerado

**Padrão:** `Fundos_[Data]_[Hora].xlsx`

**Exemplos:**
- `Fundos_20260115_143022.xlsx`
- `Fundos_Filtrados_FIDC_20260115.csv`
- `Fundos_Selecionados_20260115.pdf`

---

## 11. Paginação e Performance

### 11.1 Configuração de Paginação

| Parâmetro | Valor Padrão | Opções Disponíveis |
|-----------|--------------|-------------------|
| **Modo** | Server-side | Server-side, Client-side |
| **Itens por página** | 50 | 25, 50, 100, 200, Todos |
| **Scroll virtual** | Habilitado | Para mais de 200 registros |
| **Cache** | 5 minutos | Configurable |

### 11.2 Controles de Paginação

Posicionados no rodapé da grid:

```
┌─────────────────────────────────────────────────────────────┐
│  Mostrando 1-50 de 127 fundos                               │
│  [◀◀ Primeira] [◀ Anterior] [▶ Próxima] [▶▶ Última]         │
│  Página: [1 ▼] de 3      Itens por página: [50 ▼]          │
└─────────────────────────────────────────────────────────────┘
```

### 11.3 Indicadores de Performance

Durante carregamento:

| Indicador | Posição | Descrição |
|-----------|---------|-----------|
| **Barra de progresso** | Topo da grid | Animação de carregamento |
| **Spinner** | Centro da grid | Durante primeira carga |
| **Contador** | Barra de status | "Carregando... 45/127 (35%)" |
| **Tempo** | Barra de status | "Última atualização: há 2 minutos" |

**Mensagens de estado:**
- `● READY` - Dados carregados e prontos
- `● LOADING...` - Carregando dados
- `● ERROR` - Erro ao carregar dados
- `● FILTERING...` - Aplicando filtros

---

## 12. Contextos Especiais

### 12.1 Visualização para Gestoras (Meus Fundos)

Quando o usuário representa uma gestora:

**Ajustes automáticos:**
- Preset "Meus Fundos" ativado por padrão
- Filtro de Gestor pré-selecionado com a gestora do usuário
- Destaque visual para fundos da própria gestora
- Atalho rápido "Ver todos os fundos" no cabeçalho

**Indicador visual:**
```
👤 Mostrando apenas fundos da sua gestora (XYZ Gestora)
[× Remover filtro] [Ver todos os fundos]
```

### 12.2 Visualização para Administradores

Quando o usuário representa uma administradora:

**Ajustes automáticos:**
- Filtro de Administrador pré-selecionado
- Coluna "Situação Regulatória" mais destacada
- Alertas de pendências documentais mais visíveis
- Acesso rápido a relatórios regulatórios

### 12.3 Visualização para Auditores

Quando o usuário é auditor:

**Ajustes automáticos:**
- Colunas de auditoria visíveis por padrão
- Filtro de "Fundos auditados por mim"
- Destaque para campos "Criado por" e "Atualizado em"
- Acesso ao histórico completo de alterações

### 12.4 Modo de Visualização para FIDCs

Filtro específico "FIDCs" ativa visualização especializada:

**Colunas adicionais automáticas:**
- Classe FIDC (Sênior/Mezanino/Subordinada)
- Séries Ativas
- DC Não-Padronizados
- Patrimônio Separado
- Cedentes Principais
- Taxa de Inadimplência

**Agrupamento sugerido:**
- Por Classe FIDC
- Por Série

---

## 13. Ações em Lote

### 13.1 Barra de Ações em Lote

Quando um ou mais fundos estão selecionados, aparece barra flutuante no topo:

```
┌─────────────────────────────────────────────────────────────┐
│ ☑️ 3 fundos selecionados                                    │
│ [📊 Exportar] [❌ Inativar] [🤝 Alterar Gestor]              │
│ [📄 Relatório] [🏷️ Tags] [× Limpar Seleção]                │
└─────────────────────────────────────────────────────────────┘
```

### 13.2 Ações Disponíveis

| Ação | Ícone | Modal | Descrição |
|------|-------|-------|-----------|
| **Exportar** | 📊 | Sim | Exporta fundos selecionados |
| **Inativar** | ❌ | Sim (confirmação) | Inativa múltiplos fundos |
| **Alterar Gestor** | 🤝 | Sim | Altera gestor em lote |
| **Alterar Administrador** | 🏢 | Sim | Altera administrador em lote |
| **Gerar Relatório** | 📄 | Sim | Relatório consolidado |
| **Adicionar Tags** | 🏷️ | Sim | Adiciona marcadores |
| **Enviar Email** | ✉️ | Sim | Envia notificação |

### 13.3 Modal de Alteração em Lote

**Exemplo: Alterar Gestor**

```
┌─────────────────────────────────────────┐
│  Alterar Gestor em Lote             [×] │
├─────────────────────────────────────────┤
│                                         │
│  Fundos selecionados: 3                 │
│  • FIDC XYZ (12.345.678/0001-90)        │
│  • FIC ABC (98.765.432/0001-11)         │
│  • FII DEF (11.222.333/0001-44)         │
│                                         │
│  Novo Gestor:                           │
│  [Buscar gestor____________] [🔍]       │
│                                         │
│  Data de vigência:                      │
│  [15/01/2026]                           │
│                                         │
│  ☐ Notificar prestadores por email      │
│  ☑ Registrar no histórico               │
│                                         │
├─────────────────────────────────────────┤
│         [Cancelar] [Confirmar]          │
└─────────────────────────────────────────┘
```

---

## 14. Barra de Status (Rodapé)

### 14.1 Elementos da Barra

| Elemento | Posição | Exemplo | Descrição |
|----------|---------|---------|-----------|
| **Status** | Esquerda | `● READY` | Estado atual da grid |
| **Total** | Centro-esq | `Total: 127 fundos` | Total de registros |
| **Filtrados** | Centro | `Filtrados: 45` | Registros após filtros |
| **Selecionados** | Centro-dir | `Selecionados: 2` | Registros selecionados |
| **Última Atualização** | Direita | `Atualizado às 14:35` | Timestamp da última carga |

### 14.2 Estados Possíveis

| Status | Cor | Descrição |
|--------|-----|-----------|
| `● READY` | Verde | Dados carregados e prontos |
| `● LOADING...` | Azul | Carregando dados do servidor |
| `● FILTERING...` | Azul | Aplicando filtros |
| `● ERROR` | Vermelho | Erro ao carregar dados |
| `● UPDATING...` | Laranja | Atualizando dados |

**Layout completo:**
```
● READY | Total: 127 fundos | Filtrados: 45 | Selecionados: 2 | Atualizado às 14:35
```

---

## 15. Atalhos de Teclado

| Atalho | Ação |
|--------|------|
| **Ctrl + N** | Novo Fundo |
| **Ctrl + F** | Foco no campo de busca |
| **Ctrl + R** | Atualizar dados |
| **Ctrl + E** | Exportar |
| **Ctrl + A** | Selecionar todos (visíveis) |
| **Ctrl + D** | Desselecionar todos |
| **Enter** | Abrir detalhes do fundo selecionado |
| **Esc** | Limpar filtros/seleção |
| **Setas ↑↓** | Navegar entre linhas |
| **Shift + Setas** | Seleção múltipla |
| **Tab** | Navegar entre filtros |

---

## 16. Responsividade

### 16.1 Desktop (> 1280px)

- Layout completo com todas as colunas
- Filtros em linha
- Presets visíveis
- Sidebar de configuração

### 16.2 Tablet (768px - 1280px)

- Scroll horizontal na grid
- Colunas menos importantes ocultas por padrão
- Filtros em 2 linhas
- Menu hambúrguer para ações

### 16.3 Mobile (< 768px)

- Grid substituída por lista de cards
- Busca rápida mantida
- Filtros em modal fullscreen
- Ações em menu inferior

**Exemplo de Card Mobile:**
```
┌────────────────────────────────────┐
│ FIDC XYZ                       [⋮] │
│ 12.345.678/0001-90                 │
│                                    │
│ Tipo: FIDC | Situação: ✓ Ativo    │
│ Administrador: Órama DTVM          │
│ PL: R$ 125.000.000,00              │
│                                    │
│ [👁️ Ver] [✏️ Editar]               │
└────────────────────────────────────┘
```

---

## 17. Acessibilidade (A11y)

### 17.1 Requisitos WCAG 2.1 AA

| Requisito | Implementação |
|-----------|---------------|
| **Navegação por teclado** | Todos os elementos interativos acessíveis via Tab |
| **Leitores de tela** | ARIA labels em todos os controles |
| **Contraste** | Mínimo 4.5:1 para texto |
| **Foco visível** | Outline destacado em elementos focados |
| **Labels** | Todos os inputs com labels associados |
| **Alternativas de texto** | Alt text em ícones e badges |

### 17.2 Suporte a Leitores de Tela

**Anúncios importantes:**
- "127 fundos carregados"
- "45 fundos após aplicar filtros"
- "Fundo XYZ selecionado"
- "Ordenando por Nome do Fundo em ordem crescente"

---

## 18. Performance e Otimização

### 18.1 Estratégias de Carregamento

| Cenário | Estratégia |
|---------|-----------|
| **Até 100 registros** | Client-side (carrega tudo) |
| **101-1000 registros** | Server-side com paginação |
| **1000+ registros** | Server-side + virtual scroll |
| **Filtros** | Debounce de 300ms |
| **Ordenação** | Server-side para grandes volumes |

### 18.2 Cache e Atualização

| Elemento | Tempo de Cache | Atualização |
|----------|----------------|-------------|
| **Lista de fundos** | 5 minutos | Manual ou automática |
| **Filtros (dropdowns)** | 15 minutos | Manual |
| **Totalizadores** | 5 minutos | Junto com dados |
| **Presets** | Sessão | Não expira |

### 18.3 Métricas de Performance

**Metas:**
- Primeira carga: < 2 segundos
- Aplicação de filtros: < 500ms
- Ordenação: < 300ms
- Exportação (até 100 registros): < 3 segundos

---

## 19. Segurança e Permissões

### 19.1 Controle de Acesso

| Ação | Perfil Necessário |
|------|-------------------|
| **Visualizar lista** | Todos os usuários autenticados |
| **Ver detalhes** | Todos os usuários |
| **Criar fundo** | Administrador, Gestor |
| **Editar fundo** | Administrador, Gestor (próprios fundos) |
| **Inativar fundo** | Administrador |
| **Excluir fundo** | Administrador (super admin) |
| **Ações em lote** | Administrador |
| **Exportar dados** | Todos os usuários |

### 19.2 Filtros por Permissão

**Usuários Gestores:**
- Veem apenas fundos da própria gestora (exceto se admin)
- Podem editar apenas fundos da própria gestora
- Não podem inativar ou excluir

**Usuários Administradores (de fundos):**
- Veem apenas fundos que administram
- Podem editar fundos administrados
- Podem inativar fundos administrados

**Super Administradores:**
- Acesso total a todos os fundos
- Podem realizar todas as ações

---

## 20. Estados de Erro e Mensagens

### 20.1 Mensagens de Sucesso

| Ação | Mensagem |
|------|----------|
| **Criar fundo** | "✓ Fundo criado com sucesso!" |
| **Editar fundo** | "✓ Fundo atualizado com sucesso!" |
| **Inativar fundo** | "✓ Fundo inativado com sucesso!" |
| **Ação em lote** | "✓ 3 fundos atualizados com sucesso!" |
| **Exportar** | "✓ Exportação concluída! Arquivo baixado." |

### 20.2 Mensagens de Erro

| Cenário | Mensagem |
|---------|----------|
| **Erro de conexão** | "✗ Erro ao carregar dados. Verifique sua conexão." |
| **Sem permissão** | "✗ Você não tem permissão para realizar esta ação." |
| **Validação** | "✗ Preencha todos os campos obrigatórios." |
| **Duplicidade** | "✗ Já existe um fundo com este CNPJ." |
| **Timeout** | "✗ A operação demorou muito. Tente novamente." |

### 20.3 Estados Vazios

**Nenhum fundo cadastrado:**
```
┌─────────────────────────────────────┐
│          📋                         │
│   Nenhum fundo cadastrado           │
│                                     │
│   Comece cadastrando seu            │
│   primeiro fundo.                   │
│                                     │
│   [+ Novo Fundo]                    │
└─────────────────────────────────────┘
```

**Nenhum resultado na busca:**
```
┌─────────────────────────────────────┐
│          🔍                         │
│   Nenhum fundo encontrado           │
│                                     │
│   Tente ajustar seus filtros ou     │
│   realizar uma nova busca.          │
│                                     │
│   [Limpar Filtros]                  │
└─────────────────────────────────────┘
```

---

## 21. Integrações e APIs

### 21.1 Endpoint Principal

```
GET /api/v1/fundos
```

**Query Parameters:**

| Parâmetro | Tipo | Descrição | Exemplo |
|-----------|------|-----------|---------|
| `page` | Integer | Página (inicia em 1) | `page=1` |
| `size` | Integer | Itens por página | `size=50` |
| `sort` | String | Campo e direção | `sort=nome,asc` |
| `cnpj` | String | Filtro por CNPJ | `cnpj=12.345.678/0001-90` |
| `nome` | String | Busca no nome | `nome=XYZ` |
| `tipo` | String[] | Filtro por tipo | `tipo=FIDC,FII` |
| `classeCVM` | String[] | Filtro por classe | `classeCVM=Renda Fixa` |
| `situacao` | String[] | Filtro por situação | `situacao=ATIVO` |
| `administradorId` | UUID | Filtro por administrador | `administradorId=uuid` |
| `gestorId` | UUID | Filtro por gestor | `gestorId=uuid` |

**Exemplo de Request:**
```
GET /api/v1/fundos?page=1&size=50&tipo=FIDC&situacao=ATIVO&sort=nome,asc
```

**Exemplo de Response:**
```json
{
  "content": [
    {
      "id": "uuid",
      "cnpj": "12.345.678/0001-90",
      "razaoSocial": "FIDC XYZ",
      "nomeFantasia": "XYZ FIDC",
      "tipo": "FIDC",
      "classeCVM": "Renda Fixa",
      "situacao": "ATIVO",
      "regime": "FECHADO",
      "administrador": {
        "id": "uuid",
        "nome": "Órama DTVM"
      },
      "gestor": {
        "id": "uuid",
        "nome": "XYZ Gestora"
      },
      "plEstimado": 125000000.00,
      "numeroClasses": 3,
      "dataInicio": "2024-01-15",
      "criadoEm": "2026-01-10T14:30:00Z",
      "alertas": [
        {
          "tipo": "PEND-02",
          "mensagem": "Regulamento vencido"
        }
      ]
    }
  ],
  "pageable": {
    "pageNumber": 0,
    "pageSize": 50,
    "sort": {
      "sorted": true,
      "orders": [
        {
          "property": "nome",
          "direction": "ASC"
        }
      ]
    }
  },
  "totalElements": 127,
  "totalPages": 3,
  "last": false,
  "first": true,
  "numberOfElements": 50
}
```

---

## 22. Observações Finais

### 22.1 Prioridades de Implementação

| Fase | Funcionalidades |
|------|----------------|
| **MVP (Fase 1)** | Grid básica, busca, filtros principais, CRUD básico |
| **Fase 2** | Presets, filtros avançados, exportação, ações em lote |
| **Fase 3** | Agrupamento, configuração de colunas, dashboards |
| **Fase 4** | Visualizações especializadas (FIDCs), mobile |

### 22.2 Referências de Design

- AG Grid Documentation: https://www.ag-grid.com/
- Material Design Guidelines: https://material.io/
- WCAG 2.1: https://www.w3.org/WAI/WCAG21/quickref/

### 22.3 Dependências Técnicas

**Frontend:**
- Angular 17+
- AG Grid (Enterprise)
- Angular Material
- RxJS

**Backend:**
- API REST conforme especificação no documento de APIs
- PostgreSQL para consultas otimizadas
- Cache Redis para performance

---

**Documento criado em:** 15/01/2026  
**Versão:** 1.0  
**Autor:** Claude (Anthropic)  
**Status:** Especificação Completa - Pronto para Implementação
