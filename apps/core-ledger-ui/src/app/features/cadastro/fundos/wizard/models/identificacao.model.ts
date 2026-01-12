/**
 * Tipos de fundo conforme regulamentacao brasileira
 */
export enum TipoFundo {
  FI = 'FI',
  FIC = 'FIC',
  FIDC = 'FIDC',
  FIDC_NP = 'FIDC_NP',
  FIP = 'FIP',
  FII = 'FII',
  FIAGRO = 'FIAGRO',
  FI_INFRA = 'FI_INFRA',
  ETF = 'ETF',
  FMP_FGTS = 'FMP_FGTS',
}

/**
 * Option for tipo_fundo select dropdown
 */
export interface TipoFundoOption {
  value: TipoFundo;
  name: string;
  description: string;
  prefixoNomeCurto: string;
}

/**
 * Form data for Step 1: Identificacao
 */
export interface IdentificacaoFormData {
  cnpj: string;
  razaoSocial: string;
  nomeFantasia: string;
  nomeCurto: string | null;
  tipoFundo: TipoFundo | null;
  dataConstituicao: string;
  dataInicioAtividade: string;
}

/**
 * Response from CNPJ verification endpoint
 */
export interface CnpjVerificationResponse {
  disponivel: boolean;
  fundoId?: number;
  nomeFantasia?: string;
}

/**
 * Static list of tipo_fundo options with metadata
 */
export const TIPO_FUNDO_OPTIONS: TipoFundoOption[] = [
  {
    value: TipoFundo.FI,
    name: 'Fundo de Investimento',
    description: 'FI',
    prefixoNomeCurto: 'FI',
  },
  {
    value: TipoFundo.FIC,
    name: 'Fundo de Investimento em Cotas',
    description: 'FIC',
    prefixoNomeCurto: 'FIC',
  },
  {
    value: TipoFundo.FIDC,
    name: 'Fundo de Investimento em Direitos Creditorios',
    description: 'FIDC',
    prefixoNomeCurto: 'FIDC',
  },
  {
    value: TipoFundo.FIDC_NP,
    name: 'FIDC Nao Padronizado',
    description: 'FIDC-NP',
    prefixoNomeCurto: 'FIDC-NP',
  },
  {
    value: TipoFundo.FIP,
    name: 'Fundo de Investimento em Participacoes',
    description: 'FIP',
    prefixoNomeCurto: 'FIP',
  },
  {
    value: TipoFundo.FII,
    name: 'Fundo de Investimento Imobiliario',
    description: 'FII',
    prefixoNomeCurto: 'FII',
  },
  {
    value: TipoFundo.FIAGRO,
    name: 'Fundo Investimento Cadeias Agroindustriais',
    description: 'FIAGRO',
    prefixoNomeCurto: 'FIAGRO',
  },
  {
    value: TipoFundo.FI_INFRA,
    name: 'Fundo de Investimento em Infraestrutura',
    description: 'FI-Infra',
    prefixoNomeCurto: 'FI-INFRA',
  },
  {
    value: TipoFundo.ETF,
    name: 'Fundo de Indice',
    description: 'ETF',
    prefixoNomeCurto: 'ETF',
  },
  {
    value: TipoFundo.FMP_FGTS,
    name: 'Fundo Mutuo de Privatizacao',
    description: 'FMP-FGTS',
    prefixoNomeCurto: 'FMP-FGTS',
  },
];
