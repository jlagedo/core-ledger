/**
 * Prazos Operacionais - Etapa 6 do Wizard
 * Configuracao de prazos de aplicacao e resgate do fundo
 */

// ============================================================
// ENUMS
// ============================================================

/**
 * Tipo de operacao do prazo
 */
export enum TipoOperacao {
  APLICACAO = 'APLICACAO',
  RESGATE = 'RESGATE',
  RESGATE_CRISE = 'RESGATE_CRISE',
}

/**
 * Tipo de calendario para calculo de D+X
 */
export enum TipoCalendario {
  NACIONAL = 'NACIONAL',
  SAO_PAULO = 'SAO_PAULO',
  RIO_DE_JANEIRO = 'RIO_DE_JANEIRO',
  EXTERIOR_EUA = 'EXTERIOR_EUA',
  EXTERIOR_EUR = 'EXTERIOR_EUR',
}

// ============================================================
// OPTIONS ARRAYS
// ============================================================

export interface TipoOperacaoOption {
  value: TipoOperacao;
  label: string;
  description: string;
}

export const TIPO_OPERACAO_OPTIONS: TipoOperacaoOption[] = [
  {
    value: TipoOperacao.APLICACAO,
    label: 'Aplicacao',
    description: 'Prazos para aplicacao de recursos',
  },
  {
    value: TipoOperacao.RESGATE,
    label: 'Resgate',
    description: 'Prazos para resgate normal',
  },
  {
    value: TipoOperacao.RESGATE_CRISE,
    label: 'Resgate em Crise',
    description: 'Prazos para resgate em cenario de crise',
  },
];

export interface TipoCalendarioOption {
  value: TipoCalendario;
  label: string;
  description: string;
}

export const TIPO_CALENDARIO_OPTIONS: TipoCalendarioOption[] = [
  {
    value: TipoCalendario.NACIONAL,
    label: 'Nacional (B3/ANBIMA)',
    description: 'Calendario nacional brasileiro',
  },
  {
    value: TipoCalendario.SAO_PAULO,
    label: 'Sao Paulo',
    description: 'Praca de Sao Paulo',
  },
  {
    value: TipoCalendario.RIO_DE_JANEIRO,
    label: 'Rio de Janeiro',
    description: 'Praca do Rio de Janeiro',
  },
  {
    value: TipoCalendario.EXTERIOR_EUA,
    label: 'Exterior - EUA',
    description: 'Calendario dos Estados Unidos',
  },
  {
    value: TipoCalendario.EXTERIOR_EUR,
    label: 'Exterior - Europa',
    description: 'Calendario europeu',
  },
];

// ============================================================
// INTERFACES
// ============================================================

/**
 * Dados de um prazo individual
 */
export interface FundoPrazo {
  tipoOperacao: TipoOperacao | null;
  prazoCotizacao: number;
  prazoLiquidacao: number;
  tipoCalendario: TipoCalendario;
  valorMinimoInicial: number | null;
  valorMinimoAdicional: number | null;
  valorMinimoResgate: number | null;
  valorMinimoPermanencia: number | null;
  prazoCarenciaDias: number | null;
  permiteResgateTotal: boolean;
  permiteResgateProgramado: boolean;
  prazoMaximoProgramacao: number | null;
  classeId: string | null;
  ativo: boolean;
}

/**
 * Form data para etapa de prazos
 */
export interface PrazosFormData {
  prazos: FundoPrazo[];
}

// ============================================================
// DEFAULTS POR TIPO DE FUNDO (RF-02)
// ============================================================

export interface PrazosDefault {
  cotizacaoAplicacao: number;
  liquidacaoAplicacao: number;
  cotizacaoResgate: number;
  liquidacaoResgate: number;
}

/**
 * Defaults por classificacao CVM do fundo
 * RF = Renda Fixa
 * Acoes = Acoes
 * Multimercado = Multimercado
 * FII = Fundo Imobiliario
 * FIDC = Direitos Creditorios
 */
export const PRAZOS_DEFAULT_POR_TIPO: Record<string, PrazosDefault> = {
  RF: {
    cotizacaoAplicacao: 0,
    liquidacaoAplicacao: 0,
    cotizacaoResgate: 0,
    liquidacaoResgate: 1,
  },
  ACOES: {
    cotizacaoAplicacao: 0,
    liquidacaoAplicacao: 0,
    cotizacaoResgate: 1,
    liquidacaoResgate: 2,
  },
  MULTIMERCADO: {
    cotizacaoAplicacao: 0,
    liquidacaoAplicacao: 0,
    cotizacaoResgate: 1,
    liquidacaoResgate: 1,
  },
  FII: {
    cotizacaoAplicacao: 0,
    liquidacaoAplicacao: 0,
    cotizacaoResgate: 30,
    liquidacaoResgate: 2,
  },
  FIDC: {
    cotizacaoAplicacao: 1,
    liquidacaoAplicacao: 1,
    cotizacaoResgate: 30,
    liquidacaoResgate: 30,
  },
  // Default generico para outros tipos
  DEFAULT: {
    cotizacaoAplicacao: 0,
    liquidacaoAplicacao: 0,
    cotizacaoResgate: 1,
    liquidacaoResgate: 1,
  },
};

// ============================================================
// DEFAULTS GERAIS
// ============================================================

export const PRAZO_APLICACAO_DEFAULT: FundoPrazo = {
  tipoOperacao: TipoOperacao.APLICACAO,
  prazoCotizacao: 0,
  prazoLiquidacao: 0,
  tipoCalendario: TipoCalendario.NACIONAL,
  valorMinimoInicial: null,
  valorMinimoAdicional: null,
  valorMinimoResgate: null,
  valorMinimoPermanencia: null,
  prazoCarenciaDias: null,
  permiteResgateTotal: true,
  permiteResgateProgramado: false,
  prazoMaximoProgramacao: null,
  classeId: null,
  ativo: true,
};

export const PRAZO_RESGATE_DEFAULT: FundoPrazo = {
  tipoOperacao: TipoOperacao.RESGATE,
  prazoCotizacao: 1,
  prazoLiquidacao: 1,
  tipoCalendario: TipoCalendario.NACIONAL,
  valorMinimoInicial: null,
  valorMinimoAdicional: null,
  valorMinimoResgate: null,
  valorMinimoPermanencia: null,
  prazoCarenciaDias: null,
  permiteResgateTotal: true,
  permiteResgateProgramado: false,
  prazoMaximoProgramacao: null,
  classeId: null,
  ativo: true,
};

// ============================================================
// CONSTANTES
// ============================================================

export const MAX_PRAZOS = 5;
export const MAX_PRAZO_DIAS = 365;
export const MAX_VALOR_MINIMO = 1000000000; // 1 bilhao

// ============================================================
// HELPERS
// ============================================================

/**
 * Formata prazo para notacao D+X
 */
export function formatarPrazoDX(dias: number): string {
  return `D+${dias}`;
}

/**
 * Obtem defaults de prazo baseado no tipo de fundo
 */
export function getPrazosDefaultPorTipo(tipoFundo: string | null): PrazosDefault {
  if (!tipoFundo) return PRAZOS_DEFAULT_POR_TIPO['DEFAULT'];

  // Mapear tipo de fundo para categoria de prazos
  const mapping: Record<string, string> = {
    FI: 'MULTIMERCADO',
    FIC: 'MULTIMERCADO',
    FIDC: 'FIDC',
    FIDC_NP: 'FIDC',
    FIP: 'FIDC',
    FII: 'FII',
    FIAGRO: 'FII',
    FI_INFRA: 'FIDC',
    ETF: 'ACOES',
    FMP_FGTS: 'ACOES',
  };

  const categoria = mapping[tipoFundo] ?? 'DEFAULT';
  return PRAZOS_DEFAULT_POR_TIPO[categoria] ?? PRAZOS_DEFAULT_POR_TIPO['DEFAULT'];
}

/**
 * Cria prazo de aplicacao com defaults para tipo de fundo
 */
export function createPrazoAplicacao(tipoFundo: string | null): FundoPrazo {
  const defaults = getPrazosDefaultPorTipo(tipoFundo);
  return {
    ...PRAZO_APLICACAO_DEFAULT,
    prazoCotizacao: defaults.cotizacaoAplicacao,
    prazoLiquidacao: defaults.liquidacaoAplicacao,
  };
}

/**
 * Cria prazo de resgate com defaults para tipo de fundo
 */
export function createPrazoResgate(tipoFundo: string | null): FundoPrazo {
  const defaults = getPrazosDefaultPorTipo(tipoFundo);
  return {
    ...PRAZO_RESGATE_DEFAULT,
    prazoCotizacao: defaults.cotizacaoResgate,
    prazoLiquidacao: defaults.liquidacaoResgate,
  };
}
