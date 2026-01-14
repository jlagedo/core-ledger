/**
 * FIDC Parameters Step - Slice 08-B
 * Configuração de parâmetros específicos de FIDCs
 */

// ============================================================
// ENUMS
// ============================================================

/**
 * Tipo do FIDC conforme CVM 175
 */
export enum TipoFidc {
  PADRONIZADO = 'PADRONIZADO',
  NAO_PADRONIZADO = 'NAO_PADRONIZADO',
}

export const TIPO_FIDC_OPTIONS: { value: TipoFidc; label: string }[] = [
  { value: TipoFidc.PADRONIZADO, label: 'FIDC Padronizado' },
  { value: TipoFidc.NAO_PADRONIZADO, label: 'FIDC Não Padronizado' },
];

/**
 * Tipos de recebíveis aceitos pelo FIDC
 */
export enum TipoRecebiveis {
  DUPLICATAS = 'DUPLICATAS',
  CCB = 'CCB',
  CCI = 'CCI',
  CARTAO_CREDITO = 'CARTAO_CREDITO',
  CHEQUES = 'CHEQUES',
  CONTRATOS_ALUGUEL = 'CONTRATOS_ALUGUEL',
  ENERGIA = 'ENERGIA',
  FINANCIAMENTO_VEICULOS = 'FINANCIAMENTO_VEICULOS',
  CREDITO_CONSIGNADO = 'CREDITO_CONSIGNADO',
  PRECATORIOS = 'PRECATORIOS',
  CREDITOS_JUDICIAIS = 'CREDITOS_JUDICIAIS',
  OUTROS = 'OUTROS',
}

export const TIPO_RECEBIVEIS_OPTIONS: { value: TipoRecebiveis; label: string }[] = [
  { value: TipoRecebiveis.DUPLICATAS, label: 'Duplicatas mercantis' },
  { value: TipoRecebiveis.CCB, label: 'Cédula de Crédito Bancário (CCB)' },
  { value: TipoRecebiveis.CCI, label: 'Cédula de Crédito Imobiliário (CCI)' },
  { value: TipoRecebiveis.CARTAO_CREDITO, label: 'Recebíveis de cartão de crédito' },
  { value: TipoRecebiveis.CHEQUES, label: 'Cheques' },
  { value: TipoRecebiveis.CONTRATOS_ALUGUEL, label: 'Contratos de aluguel' },
  { value: TipoRecebiveis.ENERGIA, label: 'Recebíveis de energia' },
  { value: TipoRecebiveis.FINANCIAMENTO_VEICULOS, label: 'Financiamento de veículos' },
  { value: TipoRecebiveis.CREDITO_CONSIGNADO, label: 'Crédito consignado' },
  { value: TipoRecebiveis.PRECATORIOS, label: 'Precatórios' },
  { value: TipoRecebiveis.CREDITOS_JUDICIAIS, label: 'Créditos judiciais' },
  { value: TipoRecebiveis.OUTROS, label: 'Outros tipos' },
];

/**
 * Registradoras de recebíveis autorizadas
 */
export enum RegistradoraRecebiveis {
  LAQUS = 'LAQUS',
  CERC = 'CERC',
  TAG = 'TAG',
  B3 = 'B3',
}

export const REGISTRADORA_OPTIONS: { value: RegistradoraRecebiveis; label: string }[] = [
  { value: RegistradoraRecebiveis.LAQUS, label: 'Laqus Registradora' },
  { value: RegistradoraRecebiveis.CERC, label: 'CERC' },
  { value: RegistradoraRecebiveis.TAG, label: 'TAG Registradora' },
  { value: RegistradoraRecebiveis.B3, label: 'B3 Registradora' },
];

/**
 * Agências de rating sugeridas (RF-06)
 */
export const AGENCIA_RATING_OPTIONS: { value: string; label: string }[] = [
  { value: 'MOODYS', label: "Moody's" },
  { value: 'SP', label: 'S&P Global Ratings' },
  { value: 'FITCH', label: 'Fitch Ratings' },
  { value: 'AUSTIN', label: 'Austin Rating' },
  { value: 'SR_RATING', label: 'SR Rating' },
];

// ============================================================
// INTERFACES
// ============================================================

/**
 * Parâmetros específicos de FIDC
 */
export interface FidcParametros {
  tipoFidc: TipoFidc | null;
  tipoRecebiveis: TipoRecebiveis[];
  prazoMedioCarteira: number | null;
  indiceSubordinacaoAlvo: number | null;
  provisaoDevedoresDuvidosos: number | null;
  limiteConcentracaoCedente: number | null;
  limiteConcentracaoSacado: number | null;
  possuiCoobrigacao: boolean;
  percentualCoobrigacao: number | null;
  permiteCessaoParcial: boolean;
  ratingMinimo: string | null;
  agenciaRating: string | null;
  registradoraRecebiveis: RegistradoraRecebiveis | null;
  contaRegistradora: string | null;
  integracaoRegistradora: boolean;
}

/**
 * Form data para step de parâmetros FIDC
 */
export interface ParametrosFidcFormData {
  tipoFidc: TipoFidc | null;
  tipoRecebiveis: TipoRecebiveis[];
  prazoMedioCarteira: number | null;
  indiceSubordinacaoAlvo: number | null;
  provisaoDevedoresDuvidosos: number | null;
  limiteConcentracaoCedente: number | null;
  limiteConcentracaoSacado: number | null;
  possuiCoobrigacao: boolean;
  percentualCoobrigacao: number | null;
  permiteCessaoParcial: boolean;
  ratingMinimo: string | null;
  agenciaRating: string | null;
  registradoraRecebiveis: RegistradoraRecebiveis | null;
  contaRegistradora: string | null;
  integracaoRegistradora: boolean;
}

// ============================================================
// DEFAULTS & CONSTANTS
// ============================================================

/**
 * Valores padrão para parâmetros FIDC
 * RF-05: Sugerir valores padrão de mercado
 */
export const DEFAULT_LIMITE_CEDENTE = 20; // 20%
export const DEFAULT_LIMITE_SACADO = 5; // 5%

/**
 * Limites de validação
 */
export const MAX_PRAZO_DIAS = 3650; // 10 anos
export const MAX_PERCENTUAL = 100;

/**
 * Cria parâmetros FIDC default baseado no tipo do fundo
 * RF-02: Tipo é pré-selecionado baseado em tipo_fundo
 */
export function createDefaultFidcParametros(isFidcNp: boolean): FidcParametros {
  return {
    tipoFidc: isFidcNp ? TipoFidc.NAO_PADRONIZADO : TipoFidc.PADRONIZADO,
    tipoRecebiveis: [],
    prazoMedioCarteira: null,
    indiceSubordinacaoAlvo: null,
    provisaoDevedoresDuvidosos: null,
    limiteConcentracaoCedente: DEFAULT_LIMITE_CEDENTE,
    limiteConcentracaoSacado: DEFAULT_LIMITE_SACADO,
    possuiCoobrigacao: false,
    percentualCoobrigacao: null,
    permiteCessaoParcial: true,
    ratingMinimo: null,
    agenciaRating: null,
    registradoraRecebiveis: null,
    contaRegistradora: null,
    integracaoRegistradora: false,
  };
}
