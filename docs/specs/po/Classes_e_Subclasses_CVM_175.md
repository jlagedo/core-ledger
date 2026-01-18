# Classes e Subclasses em Fundos de Investimento - CVM 175

## 1. Conceitos Fundamentais

### 1.1 O que são Classes de Cotas?

Classes de cotas são divisões do patrimônio de um fundo de investimento que permitem a segmentação de investidores com diferentes perfis, objetivos e condições de investimento. Cada classe opera dentro do mesmo fundo, mas com características próprias.

**Base Legal:** Artigo 5º da Resolução CVM 175/2022

### 1.2 O que são Subclasses?

Subclasses são subdivisões dentro de uma classe de cotas, permitindo diferenciações mais específicas entre grupos de cotistas. Uma classe pode conter múltiplas subclasses, mas uma subclasse não pode ter outra subclasse (limite de 2 níveis hierárquicos).

**Base Legal:** Artigos 5º §3º, §4º, §5º e §6º da CVM 175

---

## 2. Estrutura Hierárquica

```
FUNDO DE INVESTIMENTO
├── Classe 1
│   ├── Subclasse A
│   ├── Subclasse B
│   └── Subclasse C
├── Classe 2 (Classe Única)
│   └── (sem subclasses)
└── Classe 3
    ├── Subclasse Senior
    ├── Subclasse Mezanino
    └── Subclasse Subordinada
```

### 2.1 Regras da Hierarquia

- **Máximo 2 níveis:** Classe → Subclasse (não há sub-subclasses)
- **Classe Única:** Fundo sem diferentes classes emite cotas em classe única, podendo ter subclasses
- **Segregação Patrimonial:** Vedada a afetação de parcela do patrimônio de uma classe a qualquer subclasse específica

---

## 3. Denominação e Identificação

### 3.1 Regras de Nomenclatura

**Para Classes:**
- Cada classe deve ter denominação própria
- Deve incluir referência à categoria do fundo
- Se houver responsabilidade limitada: adicionar sufixo "Responsabilidade Limitada"

**Exemplo:**
```
Fundo XYZ de Investimento em Ações - Classe A
Fundo XYZ de Investimento em Ações - Classe B Responsabilidade Limitada
```

**Base Legal:** Artigo 6º §1º e §3º da CVM 175

### 3.2 Vedações na Nomenclatura

Não podem ser incluídos termos que:
- Induzam interpretação indevida sobre objetivos
- Sugiram tratamento tributário inexistente
- Confundam quanto ao público-alvo
- Distorçam as políticas de investimento

**Base Legal:** Artigo 6º §2º da CVM 175

---

## 4. Critérios de Diferenciação

### 4.1 Diferenciação entre Classes

Classes podem ser diferenciadas por praticamente qualquer aspecto, incluindo:
- Política de investimento
- Público-alvo (geral, qualificado, profissional)
- Regime (aberta ou fechada)
- Prazo de duração
- Responsabilidade dos cotistas
- Categoria de investimento
- Taxas e encargos
- Direitos econômicos e políticos (em classes restritas)

**Base Legal:** Artigo 48 §2º da CVM 175

### 4.2 Diferenciação entre Subclasses

Subclasses podem ser diferenciadas **EXCLUSIVAMENTE** por:

1. **Público-alvo**
   - Investidor geral
   - Investidor qualificado
   - Investidor profissional

2. **Prazos e condições**
   - Prazos de aplicação
   - Prazos de amortização
   - Prazos de resgate
   - Condições operacionais

3. **Taxas**
   - Taxa de administração
   - Taxa de gestão
   - Taxa máxima de distribuição
   - Taxa de ingresso
   - Taxa de saída

**EXCEÇÃO:** Subclasses de **classes restritas** podem ter diferenciação adicional de:
- Direitos econômicos
- Direitos políticos

**Base Legal:** Artigo 5º §5º e §6º da CVM 175

---

## 5. Tipos de Regime

### 5.1 Classe Aberta

Classe cujo regulamento **admite** o resgate de cotas pelos cotistas.

**Características:**
- Cotistas podem solicitar resgate
- Liquidez conforme condições do regulamento
- Prazo de cotização e liquidação definidos

### 5.2 Classe Fechada

Classe cujo regulamento **NÃO admite** o resgate de cotas.

**Características:**
- Cotistas não podem resgatar cotas antes do vencimento
- Possibilidade de amortização conforme regulamento
- Saída via venda no mercado secundário ou liquidação do fundo

**Base Legal:** Artigo 5º §7º da CVM 175

---

## 6. Casos Especiais: FIDCs (Fundos de Investimento em Direitos Creditórios)

### 6.1 Tipos de Subclasses em FIDCs

Os FIDCs possuem estrutura específica de subordinação:

#### 6.1.1 Cota Sênior
- **Definição:** Não se subordina a nenhuma outra para fins de amortização e resgate
- **Prioridade:** Primeira a receber
- **Risco:** Menor
- **Emissão:** Única subclasse sênior por classe
- **Séries:** Pode ter séries com índices e prazos diferentes (se classe fechada)

#### 6.1.2 Cota Subordinada Mezanino
- **Definição:** Subordina-se à(s) sênior(es) E possui subordinada(s) abaixo dela
- **Prioridade:** Intermediária
- **Risco:** Médio
- **Quantidade:** Pode haver múltiplas mezaninos
- **Séries:** Pode ter séries com índices e prazos diferentes (se classe fechada)

#### 6.1.3 Cota Subordinada (Junior)
- **Definição:** Subordina-se a todas as demais para amortização e resgate
- **Prioridade:** Última a receber
- **Risco:** Maior (absorve perdas primeiro)
- **Função:** "Colchão de proteção" para cotas sênior

**Base Legal:** Artigo 2º VIII, IX, X e Artigo 8º do Anexo II da CVM 175

### 6.2 Vedações em FIDCs

- **Vedada** subordinação entre diferentes subclasses de cotas subordinadas
- Cotas seniores devem ser emitidas em **única subclasse**

**Base Legal:** Artigo 8º e §2º do Anexo II da CVM 175

### 6.3 Exemplo de Estrutura FIDC

```
FIDC - Classe Fechada
│
├── Subclasse Sênior (80% do patrimônio)
│   ├── Série 1 (IPCA + 5% a.a., vencimento 2026)
│   └── Série 2 (CDI + 2% a.a., vencimento 2027)
│
├── Subclasse Mezanino 1 (15% do patrimônio)
│   └── Série Única (IPCA + 8% a.a., vencimento 2026)
│
└── Subclasse Subordinada (5% do patrimônio)
    └── Série Única (sem rentabilidade garantida)

Ordem de pagamento: Sênior → Mezanino 1 → Subordinada
```

---

## 7. Patrimônio e Segregação

### 7.1 Princípio da Não-Afetação

**REGRA CRÍTICA:** É **VEDADA** a afetação ou vinculação de parcela do patrimônio de uma classe a qualquer subclasse.

**O que isso significa:**
- O patrimônio da classe é único e indivisível entre subclasses
- Todas as subclasses têm direito proporcional ao patrimônio da classe
- Não pode haver "reserva patrimonial" exclusiva de uma subclasse

**Exceção:** Em FIDCs, a subordinação define **ordem de recebimento**, não **segregação patrimonial**

**Base Legal:** Artigo 5º §4º da CVM 175

---

## 8. Assembleia e Deliberações

### 8.1 Assembleia de Classe

Cada classe possui sua própria assembleia, que delibera sobre:
- Demonstrações contábeis da classe
- Alterações que afetem especificamente aquela classe
- Matérias específicas previstas no regulamento

**Base Legal:** Artigo 71 da Resolução CVM 187/2023 (alteração da CVM 175)

### 8.2 Prazos

- Assembleia deve ocorrer em até **60 dias** após encaminhamento das demonstrações à CVM
- Assembleia só pode ser realizada **no mínimo 15 dias** após disponibilização das demonstrações aos cotistas

---

## 9. Informações e Transparência

### 9.1 Dados Obrigatórios por Subclasse

Os fundos devem divulgar, separadamente para cada subclasse:

- Número de cotistas subscritores
- Quantidade de cotas subscritas
- Quantidade de cotas integralizadas
- Valor da cota
- Direitos políticos especiais (se houver)
- Direitos econômico-financeiros distintos (se houver)
- Rentabilidade efetiva mensal

**Base Legal:** Suplementos G, L e O da CVM 175

---

## 10. Casos Práticos

### 10.1 Exemplo 1: Fundo Multimercado com Classes por Público

```
Fundo ABC Multimercado
│
├── Classe Varejo (público geral)
│   ├── Subclasse A (aplicação mín: R$ 1.000)
│   ├── Subclasse B (aplicação mín: R$ 10.000)
│   └── Subclasse C (aplicação mín: R$ 50.000)
│
└── Classe Institucional (qualificados)
    ├── Subclasse Institucional 1 (taxa admin: 1,5% a.a.)
    └── Subclasse Institucional 2 (taxa admin: 1,0% a.a.)

Diferenciação entre subclasses: valor mínimo e taxas
```

### 10.2 Exemplo 2: Fundo de Ações com Classes Restritas

```
Fundo XYZ Ações - Classe Restrita
│
├── Subclasse Fundador
│   - Direito político: veto em decisões de investimento
│   - Direito econômico: 20% dos lucros acima do benchmark
│
└── Subclasse Investidor
    - Direitos econômicos e políticos padrão

Base legal: Art. 5º §6º - classes restritas permitem 
diferenciação em direitos econômicos e políticos
```

### 10.3 Exemplo 3: FIDC com Estrutura Completa

```
FIDC Direitos Creditórios ABC
│
Classe Única
│
├── Subclasse Sênior (subordinação: 0)
│   - 85% do patrimônio alvo
│   - Série A: CDI + 3% a.a.
│   - Série B: IPCA + 5% a.a.
│
├── Subclasse Mezanino 1 (subordinação: 1)
│   - 10% do patrimônio alvo
│   - Taxa: CDI + 5% a.a.
│
└── Subclasse Subordinada (subordinação: 2)
    - 5% do patrimônio alvo
    - Rentabilidade residual

Índice de Subordinação Mínimo: 15%
(Mezanino + Subordinada protegem Sênior)
```

---

## 11. Requisitos Regulatórios

### 11.1 Regulamento - Anexo Descritivo de Classe

Cada classe deve ter anexo descritivo contendo:

1. Público-alvo
2. Responsabilidade dos cotistas
3. Regime (aberta ou fechada)
4. Prazo de duração
5. Categoria
6. Política de investimento
7. Taxas (administração, gestão, performance, distribuição)
8. Condições de aplicação e resgate
9. Valores mínimos
10. Condições para barreiras aos resgates

**Base Legal:** Artigo 48 §2º da CVM 175

### 11.2 Apêndice de Subclasse

Quando aplicável, subclasses devem ter apêndice com:
- Taxas específicas de ingresso e saída
- Prazos e condições específicas
- Público-alvo (se diferente)

**Base Legal:** Artigo 48 §3º da CVM 175

---

## 12. Vantagens e Desvantagens

### 12.1 Vantagens da Estrutura de Classes/Subclasses

**Para Gestores:**
- Economia de escala (gestão unificada)
- Atendimento a múltiplos perfis de investidor
- Flexibilidade comercial

**Para Investidores:**
- Acesso a estratégias adequadas ao perfil
- Possibilidade de escolher taxas vs. serviços
- Personalização de prazos e liquidez

### 12.2 Complexidades

- Gestão administrativa mais complexa
- Necessidade de controles segregados
- Comunicação e transparência multiplicadas
- Risco de conflitos de interesse entre classes
- Custo operacional mais elevado

---

## 13. Pontos de Atenção

### 13.1 Para Gestores e Administradores

✓ Garantir que diferenciação entre subclasses respeita limites legais
✓ Manter controles segregados por classe/subclasse
✓ Documentar adequadamente no regulamento
✓ Assegurar tratamento equitativo dentro da mesma classe
✓ Evitar conflitos de interesse entre classes

### 13.2 Para Investidores

✓ Entender a qual classe/subclasse pertence
✓ Verificar direitos econômicos e políticos
✓ Conhecer a ordem de subordinação (se FIDC)
✓ Comparar taxas entre subclasses
✓ Avaliar prazos e condições de resgate

---

## 14. Resumo das Principais Regras

| Aspecto | Regra CVM 175 |
|---------|---------------|
| **Níveis hierárquicos** | Máximo 2 (Classe → Subclasse) |
| **Diferenciação Subclasses** | Público-alvo, prazos/condições, taxas |
| **Classes Restritas** | Podem diferenciar direitos econômicos/políticos |
| **Segregação Patrimonial** | VEDADA entre subclasses da mesma classe |
| **FIDC - Sênior** | Única subclasse, não subordinada |
| **FIDC - Mezanino** | Subordinada a sênior, tem subordinadas |
| **FIDC - Subordinada** | Subordinada a todas, absorve perdas |
| **Denominação** | Deve identificar classe e categoria |
| **Regulamento** | Anexo descritivo por classe obrigatório |
| **Assembleia** | Separada por classe quando aplicável |

---

## 15. Base Legal Principal

- **Resolução CVM 175/2022:** Artigos 5º, 6º, 48
- **Anexo Normativo II (FIDCs):** Artigos 2º, 8º
- **Resolução CVM 187/2023:** Artigo 71 (assembleias)
- **Suplementos G, L, O:** Informações por subclasse

---

## 16. Conclusão

A estrutura de classes e subclasses da CVM 175 permite flexibilidade na estruturação de fundos de investimento, atendendo diferentes perfis de investidores dentro de um mesmo veículo. Contudo, essa flexibilidade vem acompanhada de regras específicas que garantem:

- **Transparência:** Obrigações de divulgação segregadas
- **Equidade:** Vedação à segregação patrimonial entre subclasses
- **Proteção:** Ordem clara de subordinação em FIDCs
- **Governança:** Assembleias e deliberações organizadas

O conhecimento profundo dessas regras é essencial para gestores, administradores, distribuidores e investidores que atuam no mercado de fundos de investimento brasileiro.

---

**Documento elaborado com base na Resolução CVM 175/2022 consolidada e suas alterações.**

*Última atualização: Janeiro/2026*
