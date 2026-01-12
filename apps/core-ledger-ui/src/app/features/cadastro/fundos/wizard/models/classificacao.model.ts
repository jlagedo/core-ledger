import { TipoFundo } from './identificacao.model';

/**
 * Classificacao CVM conforme Instrucao CVM 175 e CVM 555.
 * Define a categoria regulatoria do fundo de investimento.
 */
export enum ClassificacaoCvm {
  RENDA_FIXA = 'RENDA_FIXA',
  ACOES = 'ACOES',
  MULTIMERCADO = 'MULTIMERCADO',
  CAMBIAL = 'CAMBIAL',
  FIDC = 'FIDC',
  FIP = 'FIP',
  FII = 'FII',
  FIAGRO = 'FIAGRO',
  FI_INFRA = 'FI_INFRA',
  ETF = 'ETF',
  PREVIDENCIA = 'PREVIDENCIA',
}

/**
 * Publico-alvo do fundo conforme regulacao CVM.
 * Define quais investidores podem aplicar no fundo.
 */
export enum PublicoAlvo {
  GERAL = 'GERAL',
  QUALIFICADO = 'QUALIFICADO',
  PROFISSIONAL = 'PROFISSIONAL',
}

/**
 * Regime de tributacao do fundo conforme legislacao da Receita Federal.
 */
export enum Tributacao {
  LONGO_PRAZO = 'LONGO_PRAZO',
  CURTO_PRAZO = 'CURTO_PRAZO',
  ACOES = 'ACOES',
  ISENTO = 'ISENTO',
  IMOBILIARIO = 'IMOBILIARIO',
}

/**
 * Option for select dropdowns
 */
export interface SelectOption<T = string> {
  value: T;
  label: string;
  description?: string;
}

/**
 * Classificacao ANBIMA option from API
 */
export interface ClassificacaoAnbimaOption {
  codigo: string;
  nome: string;
  nivel1: string;
  nivel2: string;
  nivel3?: string;
}

/**
 * Form data for Step 2: Classificacao
 */
export interface ClassificacaoFormData {
  classificacaoCvm: ClassificacaoCvm | null;
  classificacaoAnbima: string | null;
  codigoAnbima: string | null;
  publicoAlvo: PublicoAlvo | null;
  tributacao: Tributacao | null;
}

/**
 * Response from classificacoes-anbima API endpoint
 */
export interface ClassificacaoAnbimaResponse {
  items: ClassificacaoAnbimaOption[];
  total: number;
}

/**
 * Static list of Classificacao CVM options with metadata
 */
export const CLASSIFICACAO_CVM_OPTIONS: SelectOption<ClassificacaoCvm>[] = [
  {
    value: ClassificacaoCvm.RENDA_FIXA,
    label: 'Renda Fixa',
    description: 'Fundos que investem em ativos de renda fixa',
  },
  {
    value: ClassificacaoCvm.ACOES,
    label: 'Acoes',
    description: 'Fundos que investem em acoes',
  },
  {
    value: ClassificacaoCvm.MULTIMERCADO,
    label: 'Multimercado',
    description: 'Fundos com estrategias diversificadas',
  },
  {
    value: ClassificacaoCvm.CAMBIAL,
    label: 'Cambial',
    description: 'Fundos que investem em moeda estrangeira',
  },
  {
    value: ClassificacaoCvm.FIDC,
    label: 'Direitos Creditorios',
    description: 'Fundo de Investimento em Direitos Creditorios',
  },
  {
    value: ClassificacaoCvm.FIP,
    label: 'Participacoes',
    description: 'Fundo de Investimento em Participacoes',
  },
  {
    value: ClassificacaoCvm.FII,
    label: 'Imobiliario',
    description: 'Fundo de Investimento Imobiliario',
  },
  {
    value: ClassificacaoCvm.FIAGRO,
    label: 'Cadeias Agroindustriais',
    description: 'Fundo de Investimento em Cadeias Agroindustriais',
  },
  {
    value: ClassificacaoCvm.FI_INFRA,
    label: 'Infraestrutura',
    description: 'Fundo de Investimento em Infraestrutura',
  },
  {
    value: ClassificacaoCvm.ETF,
    label: 'ETF',
    description: 'Exchange Traded Fund (Fundo de Indice)',
  },
  {
    value: ClassificacaoCvm.PREVIDENCIA,
    label: 'Previdencia',
    description: 'Fundos de Previdencia',
  },
];

/**
 * Static list of Publico Alvo options
 */
export const PUBLICO_ALVO_OPTIONS: SelectOption<PublicoAlvo>[] = [
  {
    value: PublicoAlvo.GERAL,
    label: 'Investidores em Geral',
    description: 'Aberto a todos os investidores',
  },
  {
    value: PublicoAlvo.QUALIFICADO,
    label: 'Investidores Qualificados',
    description: 'Investimento minimo de R$ 1 milhao',
  },
  {
    value: PublicoAlvo.PROFISSIONAL,
    label: 'Investidores Profissionais',
    description: 'Investimento minimo de R$ 10 milhoes',
  },
];

/**
 * Static list of Tributacao options
 */
export const TRIBUTACAO_OPTIONS: SelectOption<Tributacao>[] = [
  {
    value: Tributacao.LONGO_PRAZO,
    label: 'Longo Prazo',
    description: 'Tabela regressiva: 22,5% a 15%',
  },
  {
    value: Tributacao.CURTO_PRAZO,
    label: 'Curto Prazo',
    description: 'Aliquotas: 22,5% ou 20%',
  },
  {
    value: Tributacao.ACOES,
    label: 'Tributacao de Acoes',
    description: 'Aliquota fixa de 15%',
  },
  {
    value: Tributacao.IMOBILIARIO,
    label: 'FII / FIAGRO',
    description: 'Isencao de IR para pessoa fisica (condicoes)',
  },
  {
    value: Tributacao.ISENTO,
    label: 'Isento de IR',
    description: 'Sem tributacao de imposto de renda',
  },
];

/**
 * Set of CVM classifications that have ANBIMA sub-classifications.
 * FIDC, FIP, FII, FIAGRO, FI_INFRA, ETF do NOT have ANBIMA options.
 */
export const CVM_HAS_ANBIMA_OPTIONS = new Set<ClassificacaoCvm>([
  ClassificacaoCvm.RENDA_FIXA,
  ClassificacaoCvm.ACOES,
  ClassificacaoCvm.MULTIMERCADO,
  ClassificacaoCvm.CAMBIAL,
  ClassificacaoCvm.PREVIDENCIA,
]);

/**
 * Mapping from TipoFundo to required PublicoAlvo restriction.
 * Null means no restriction (user can choose freely).
 */
export const TIPO_FUNDO_PUBLICO_ALVO_RESTRICTION: Record<string, PublicoAlvo | null> = {
  [TipoFundo.FIDC]: PublicoAlvo.QUALIFICADO,
  [TipoFundo.FIDC_NP]: PublicoAlvo.PROFISSIONAL,
  [TipoFundo.FIP]: PublicoAlvo.QUALIFICADO,
};

/**
 * Mapping from TipoFundo to suggested Tributacao.
 * Used to pre-select the tax regime based on fund type.
 */
export const TIPO_FUNDO_TRIBUTACAO_SUGGESTION: Record<string, Tributacao> = {
  [TipoFundo.FI]: Tributacao.LONGO_PRAZO,
  [TipoFundo.FIC]: Tributacao.LONGO_PRAZO,
  [TipoFundo.FIDC]: Tributacao.LONGO_PRAZO,
  [TipoFundo.FIDC_NP]: Tributacao.LONGO_PRAZO,
  [TipoFundo.FIP]: Tributacao.LONGO_PRAZO,
  [TipoFundo.FII]: Tributacao.IMOBILIARIO,
  [TipoFundo.FIAGRO]: Tributacao.IMOBILIARIO,
  [TipoFundo.FI_INFRA]: Tributacao.ISENTO,
  [TipoFundo.ETF]: Tributacao.ACOES,
  [TipoFundo.FMP_FGTS]: Tributacao.ACOES,
};
