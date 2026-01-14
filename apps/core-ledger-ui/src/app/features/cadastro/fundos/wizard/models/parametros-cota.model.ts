/**
 * Tipo de cota - Metodologia de calculo
 */
export enum TipoCota {
  FECHAMENTO = 'FECHAMENTO',
  ABERTURA = 'ABERTURA',
}

/**
 * Fuso horario disponivel
 */
export enum FusoHorario {
  SAO_PAULO = 'America/Sao_Paulo',
  NEW_YORK = 'America/New_York',
  LONDON = 'Europe/London',
}

/**
 * Form data for Step 4: Parametros de Cota
 */
export interface ParametrosCotaFormData {
  casasDecimaisCota: number;
  casasDecimaisQuantidade: number;
  casasDecimaisPl: number;
  tipoCota: TipoCota | null;
  horarioCorte: string | null; // HH:mm format
  cotaInicial: number | null; // decimal(20,8)
  dataCotaInicial: string | null; // ISO date string
  fusoHorario: FusoHorario;
  permiteCotaEstimada: boolean;
}

/**
 * Option for tipo cota select dropdown
 */
export interface TipoCotaOption {
  value: TipoCota;
  label: string;
  description: string;
}

/**
 * Option for fuso horario select dropdown
 */
export interface FusoHorarioOption {
  value: FusoHorario;
  label: string;
}

/**
 * Option for casas decimais select dropdown
 */
export interface CasasDecimaisOption {
  value: number;
  label: string;
}

/**
 * Opcoes de tipo de cota
 */
export const TIPO_COTA_OPTIONS: TipoCotaOption[] = [
  {
    value: TipoCota.FECHAMENTO,
    label: 'Fechamento (D+0)',
    description: 'Cota calculada no fechamento do dia',
  },
  {
    value: TipoCota.ABERTURA,
    label: 'Abertura (Estimada)',
    description: 'Cota de abertura (estimada)',
  },
];

/**
 * Opcoes de fuso horario
 */
export const FUSO_HORARIO_OPTIONS: FusoHorarioOption[] = [
  { value: FusoHorario.SAO_PAULO, label: 'America/Sao_Paulo (BRT)' },
  { value: FusoHorario.NEW_YORK, label: 'America/New_York (EST/EDT)' },
  { value: FusoHorario.LONDON, label: 'Europe/London (GMT/BST)' },
];

/**
 * Opcoes de casas decimais para cota (4-10)
 */
export const CASAS_DECIMAIS_COTA_OPTIONS: CasasDecimaisOption[] = [
  { value: 4, label: '4 casas' },
  { value: 5, label: '5 casas' },
  { value: 6, label: '6 casas' },
  { value: 7, label: '7 casas' },
  { value: 8, label: '8 casas (padrao)' },
  { value: 9, label: '9 casas' },
  { value: 10, label: '10 casas' },
];

/**
 * Opcoes de casas decimais para quantidade (4-8)
 */
export const CASAS_DECIMAIS_QUANTIDADE_OPTIONS: CasasDecimaisOption[] = [
  { value: 4, label: '4 casas' },
  { value: 5, label: '5 casas' },
  { value: 6, label: '6 casas (padrao)' },
  { value: 7, label: '7 casas' },
  { value: 8, label: '8 casas' },
];

/**
 * Opcoes de casas decimais para PL (2-4)
 */
export const CASAS_DECIMAIS_PL_OPTIONS: CasasDecimaisOption[] = [
  { value: 2, label: '2 casas (padrao)' },
  { value: 3, label: '3 casas' },
  { value: 4, label: '4 casas' },
];

/**
 * Gera preview de formato com casas decimais
 */
export function formatarPreviewCota(casas: number): string {
  const inteiro = '1';
  const decimais = '234567890'.substring(0, casas);
  return `${inteiro},${decimais}`;
}

/**
 * Gera preview de formato de quantidade
 */
export function formatarPreviewQuantidade(casas: number): string {
  const decimais = '234567890'.substring(0, casas);
  return `1.000,${decimais} cotas`;
}

/**
 * Valores padroes para parametros de cota
 */
export const PARAMETROS_COTA_DEFAULTS: Omit<ParametrosCotaFormData, 'dataCotaInicial'> = {
  casasDecimaisCota: 8,
  casasDecimaisQuantidade: 6,
  casasDecimaisPl: 2,
  tipoCota: TipoCota.FECHAMENTO,
  horarioCorte: '14:00',
  cotaInicial: 1.0,
  fusoHorario: FusoHorario.SAO_PAULO,
  permiteCotaEstimada: false,
};
