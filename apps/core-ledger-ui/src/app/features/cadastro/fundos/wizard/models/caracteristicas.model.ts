import { TipoFundo } from './identificacao.model';

/**
 * Tipo de condominio do fundo
 */
export enum Condominio {
  ABERTO = 'ABERTO',
  FECHADO = 'FECHADO',
}

/**
 * Prazo do fundo
 */
export enum Prazo {
  INDETERMINADO = 'INDETERMINADO',
  DETERMINADO = 'DETERMINADO',
}

/**
 * Form data for Step 3: Caracteristicas
 */
export interface CaracteristicasFormData {
  condominio: Condominio | null;
  prazo: Prazo | null;
  dataEncerramento: string | null; // ISO date string, condicional (prazo = DETERMINADO)
  exclusivo: boolean;
  reservado: boolean;
  permiteAlavancagem: boolean;
  limiteAlavancagem: number | null; // decimal(8,4), condicional (permiteAlavancagem = true)
  aceitaCripto: boolean;
  percentualExterior: number | null; // decimal(5,2)
}

/**
 * Option for condominio select dropdown
 */
export interface CondominioOption {
  value: Condominio;
  label: string;
}

/**
 * Option for prazo select dropdown
 */
export interface PrazoOption {
  value: Prazo;
  label: string;
}

/**
 * Opcoes de condominio
 */
export const CONDOMINIO_OPTIONS: CondominioOption[] = [
  { value: Condominio.ABERTO, label: 'Condominio Aberto' },
  { value: Condominio.FECHADO, label: 'Condominio Fechado' },
];

/**
 * Opcoes de prazo
 */
export const PRAZO_OPTIONS: PrazoOption[] = [
  { value: Prazo.INDETERMINADO, label: 'Prazo Indeterminado' },
  { value: Prazo.DETERMINADO, label: 'Prazo Determinado' },
];

/**
 * Condominio padrao por tipo de fundo
 */
export const TIPO_FUNDO_CONDOMINIO_PADRAO: Record<TipoFundo, Condominio> = {
  [TipoFundo.FI]: Condominio.ABERTO,
  [TipoFundo.FIC]: Condominio.ABERTO,
  [TipoFundo.FIDC]: Condominio.FECHADO,
  [TipoFundo.FIDC_NP]: Condominio.FECHADO,
  [TipoFundo.FIP]: Condominio.FECHADO,
  [TipoFundo.FII]: Condominio.FECHADO,
  [TipoFundo.FIAGRO]: Condominio.FECHADO,
  [TipoFundo.FI_INFRA]: Condominio.FECHADO,
  [TipoFundo.ETF]: Condominio.ABERTO,
  [TipoFundo.FMP_FGTS]: Condominio.ABERTO,
};

/**
 * Permite alavancagem por tipo de fundo
 */
export const TIPO_FUNDO_PERMITE_ALAVANCAGEM: Record<TipoFundo, boolean> = {
  [TipoFundo.FI]: true,
  [TipoFundo.FIC]: true,
  [TipoFundo.FIDC]: false,
  [TipoFundo.FIDC_NP]: false,
  [TipoFundo.FIP]: false,
  [TipoFundo.FII]: false,
  [TipoFundo.FIAGRO]: false,
  [TipoFundo.FI_INFRA]: false,
  [TipoFundo.ETF]: false,
  [TipoFundo.FMP_FGTS]: false,
};

/**
 * Permite investimento no exterior por tipo de fundo
 */
export const TIPO_FUNDO_PERMITE_EXTERIOR: Record<TipoFundo, boolean> = {
  [TipoFundo.FI]: true,
  [TipoFundo.FIC]: true,
  [TipoFundo.FIDC]: false,
  [TipoFundo.FIDC_NP]: false,
  [TipoFundo.FIP]: true,
  [TipoFundo.FII]: false,
  [TipoFundo.FIAGRO]: true,
  [TipoFundo.FI_INFRA]: true,
  [TipoFundo.ETF]: true,
  [TipoFundo.FMP_FGTS]: true,
};
