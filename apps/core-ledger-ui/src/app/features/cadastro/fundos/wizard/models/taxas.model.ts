/**
 * Modelo de Taxas do Fundo - Etapa 5 do Wizard
 * Baseado na especificacao 06-SLICE-TAXAS.md
 */

/**
 * Tipo da taxa cobrada pelo fundo
 */
export enum TipoTaxa {
  ADMINISTRACAO = 'ADMINISTRACAO',
  GESTAO = 'GESTAO',
  CUSTODIA = 'CUSTODIA',
  PERFORMANCE = 'PERFORMANCE',
  ENTRADA = 'ENTRADA',
  SAIDA = 'SAIDA',
  DISTRIBUICAO = 'DISTRIBUICAO',
  CONSULTORIA = 'CONSULTORIA',
  ESCRITURACAO = 'ESCRITURACAO',
  ESTRUTURACAO = 'ESTRUTURACAO',
}

/**
 * Base de calculo da taxa
 */
export enum BaseCalculo {
  PL_MEDIO = 'PL_MEDIO',
  PL_FINAL = 'PL_FINAL',
  RENDIMENTO = 'RENDIMENTO',
  RENDIMENTO_ACIMA_BENCHMARK = 'RENDIMENTO_ACIMA_BENCHMARK',
}

/**
 * Forma de cobranca da taxa
 */
export enum FormaCobranca {
  DIARIA = 'DIARIA',
  MENSAL = 'MENSAL',
  SEMESTRAL = 'SEMESTRAL',
  ANUAL = 'ANUAL',
  EVENTO = 'EVENTO',
}

/**
 * Interface para representar uma taxa do fundo
 */
export interface FundoTaxa {
  id?: number;
  tipoTaxa: TipoTaxa | null;
  percentual: number | null;
  percentualMinimo: number | null;
  percentualMaximo: number | null;
  baseCalculo: BaseCalculo | null;
  formaCobranca: FormaCobranca | null;
  dataInicioVigencia: string | null;
  dataFimVigencia: string | null;
  benchmarkId: number | null;
  percentualBenchmark: number | null;
  possuiHurdle: boolean;
  possuiHighWaterMark: boolean;
  linhaDAguaGlobal: boolean;
  classeId: string | null;
  ativo: boolean;
}

/**
 * Form data for Step 5: Taxas
 */
export interface TaxasFormData {
  taxas: FundoTaxa[];
}

/**
 * Opcoes de tipo de taxa para dropdown
 */
export interface TipoTaxaOption {
  value: TipoTaxa;
  label: string;
  descricao: string;
}

/**
 * Opcoes de base de calculo para dropdown
 */
export interface BaseCalculoOption {
  value: BaseCalculo;
  label: string;
}

/**
 * Opcoes de forma de cobranca para dropdown
 */
export interface FormaCobrancaOption {
  value: FormaCobranca;
  label: string;
}

// Re-export Indexador from the main model for convenience
export type { Indexador } from '../../../../../models/indexador.model';

/**
 * Opcoes de tipo de taxa
 */
export const TIPO_TAXA_OPTIONS: TipoTaxaOption[] = [
  {
    value: TipoTaxa.ADMINISTRACAO,
    label: 'Administracao',
    descricao: 'Taxa de Administracao',
  },
  { value: TipoTaxa.GESTAO, label: 'Gestao', descricao: 'Taxa de Gestao' },
  { value: TipoTaxa.CUSTODIA, label: 'Custodia', descricao: 'Taxa de Custodia' },
  {
    value: TipoTaxa.PERFORMANCE,
    label: 'Performance',
    descricao: 'Taxa de Performance',
  },
  { value: TipoTaxa.ENTRADA, label: 'Entrada', descricao: 'Taxa de Entrada (aplicacao)' },
  { value: TipoTaxa.SAIDA, label: 'Saida', descricao: 'Taxa de Saida (resgate)' },
  {
    value: TipoTaxa.DISTRIBUICAO,
    label: 'Distribuicao',
    descricao: 'Taxa de Distribuicao',
  },
  {
    value: TipoTaxa.CONSULTORIA,
    label: 'Consultoria',
    descricao: 'Taxa de Consultoria (FIDCs)',
  },
  {
    value: TipoTaxa.ESCRITURACAO,
    label: 'Escrituracao',
    descricao: 'Taxa de Escrituracao',
  },
  {
    value: TipoTaxa.ESTRUTURACAO,
    label: 'Estruturacao',
    descricao: 'Taxa de Estruturacao (FIDCs/FIPs)',
  },
];

/**
 * Opcoes de base de calculo
 */
export const BASE_CALCULO_OPTIONS: BaseCalculoOption[] = [
  { value: BaseCalculo.PL_MEDIO, label: 'PL Medio' },
  { value: BaseCalculo.PL_FINAL, label: 'PL Final' },
  { value: BaseCalculo.RENDIMENTO, label: 'Rendimento do Periodo' },
  { value: BaseCalculo.RENDIMENTO_ACIMA_BENCHMARK, label: 'Rendimento acima do Benchmark' },
];

/**
 * Opcoes de forma de cobranca
 */
export const FORMA_COBRANCA_OPTIONS: FormaCobrancaOption[] = [
  { value: FormaCobranca.DIARIA, label: 'Diaria' },
  { value: FormaCobranca.MENSAL, label: 'Mensal' },
  { value: FormaCobranca.SEMESTRAL, label: 'Semestral' },
  { value: FormaCobranca.ANUAL, label: 'Anual' },
  { value: FormaCobranca.EVENTO, label: 'Por Evento' },
];

/**
 * Limites de percentual por tipo de taxa (RF-05)
 */
export const LIMITES_PERCENTUAL: Record<TipoTaxa, number> = {
  [TipoTaxa.ADMINISTRACAO]: 10.0, // maximo 10% a.a.
  [TipoTaxa.GESTAO]: 5.0, // maximo 5% a.a.
  [TipoTaxa.CUSTODIA]: 2.0, // maximo 2% a.a.
  [TipoTaxa.PERFORMANCE]: 50.0, // maximo 50%
  [TipoTaxa.ENTRADA]: 5.0, // maximo 5%
  [TipoTaxa.SAIDA]: 5.0, // maximo 5%
  [TipoTaxa.DISTRIBUICAO]: 5.0, // maximo 5%
  [TipoTaxa.CONSULTORIA]: 5.0, // maximo 5%
  [TipoTaxa.ESCRITURACAO]: 2.0, // maximo 2%
  [TipoTaxa.ESTRUTURACAO]: 5.0, // maximo 5%
};

/**
 * Valores default para taxa de administracao (RF-01)
 */
export const TAXA_ADMINISTRACAO_DEFAULT: FundoTaxa = {
  tipoTaxa: TipoTaxa.ADMINISTRACAO,
  percentual: 1.5,
  percentualMinimo: null,
  percentualMaximo: null,
  baseCalculo: BaseCalculo.PL_MEDIO,
  formaCobranca: FormaCobranca.DIARIA,
  dataInicioVigencia: null,
  dataFimVigencia: null,
  benchmarkId: null,
  percentualBenchmark: null,
  possuiHurdle: false,
  possuiHighWaterMark: false,
  linhaDAguaGlobal: false,
  classeId: null,
  ativo: true,
};

/**
 * Constante para maximo de taxas permitidas (RF-02)
 */
export const MAX_TAXAS = 10;
