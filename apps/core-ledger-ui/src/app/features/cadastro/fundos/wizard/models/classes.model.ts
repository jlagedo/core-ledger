/**
 * Modelo de Classes CVM 175 - Etapa 7 do Wizard
 * Baseado na especificacao 08-SLICE-CLASSES-CVM175.md
 */

/**
 * Tipo de classe para FIDCs conforme CVM 175
 */
export enum TipoClasseFidc {
  SENIOR = 'SENIOR',
  MEZANINO = 'MEZANINO',
  SUBORDINADA = 'SUBORDINADA',
  SUBORDINADA_JUNIOR = 'SUBORDINADA_JUNIOR',
  UNICA = 'UNICA',
}

/**
 * Publico alvo da classe
 */
export enum PublicoAlvo {
  GERAL = 'GERAL',
  QUALIFICADO = 'QUALIFICADO',
  PROFISSIONAL = 'PROFISSIONAL',
}

/**
 * Interface para representar uma classe do fundo
 */
export interface FundoClasse {
  id?: string;
  codigoClasse: string;
  nomeClasse: string;
  cnpjClasse: string | null;
  classePaiId: string | null;
  nivel: number;
  publicoAlvo: PublicoAlvo | null;
  tipoClasseFidc: TipoClasseFidc | null;
  ordemSubordinacao: number | null;
  rentabilidadeAlvo: number | null;
  indiceSubordinacaoMinimo: number | null;
  valorMinimoAplicacao: number | null;
  valorMinimoPermanencia: number | null;
  responsabilidadeLimitada: boolean;
  segregacaoPatrimonial: boolean;
  taxaAdministracao: number | null;
  taxaGestao: number | null;
  taxaPerformance: number | null;
  benchmarkId: number | null;
  permiteResgateAntecipado: boolean;
  dataInicio: string | null;
  dataEncerramento: string | null;
  motivoEncerramento: string | null;
  ativo: boolean;
}

/**
 * Form data for Step 7: Classes
 */
export interface ClassesFormData {
  multiclasse: boolean;
  classes: FundoClasse[];
}

/**
 * Option interface for tipo classe FIDC dropdown
 */
export interface TipoClasseFidcOption {
  value: TipoClasseFidc;
  label: string;
  descricao: string;
  ordemPadrao: number;
  responsabilidadeLimitadaDefault: boolean;
}

/**
 * Option interface for publico alvo dropdown
 */
export interface PublicoAlvoOption {
  value: PublicoAlvo;
  label: string;
  descricao: string;
}

/**
 * Opcoes de tipo de classe FIDC
 */
export const TIPO_CLASSE_FIDC_OPTIONS: TipoClasseFidcOption[] = [
  {
    value: TipoClasseFidc.SENIOR,
    label: 'Senior',
    descricao: 'Classe Senior - Prioridade de recebimento',
    ordemPadrao: 1,
    responsabilidadeLimitadaDefault: true,
  },
  {
    value: TipoClasseFidc.MEZANINO,
    label: 'Mezanino',
    descricao: 'Classe Mezanino - Prioridade intermediaria',
    ordemPadrao: 2,
    responsabilidadeLimitadaDefault: true,
  },
  {
    value: TipoClasseFidc.SUBORDINADA,
    label: 'Subordinada',
    descricao: 'Classe Subordinada - Absorve perdas primeiro',
    ordemPadrao: 3,
    responsabilidadeLimitadaDefault: false,
  },
  {
    value: TipoClasseFidc.SUBORDINADA_JUNIOR,
    label: 'Subordinada Junior',
    descricao: 'Classe Subordinada Junior - Maior risco',
    ordemPadrao: 4,
    responsabilidadeLimitadaDefault: false,
  },
  {
    value: TipoClasseFidc.UNICA,
    label: 'Unica',
    descricao: 'Classe Unica - Sem subordinacao',
    ordemPadrao: 1,
    responsabilidadeLimitadaDefault: true,
  },
];

/**
 * Opcoes de publico alvo
 */
export const PUBLICO_ALVO_OPTIONS: PublicoAlvoOption[] = [
  {
    value: PublicoAlvo.GERAL,
    label: 'Geral',
    descricao: 'Investidores em geral',
  },
  {
    value: PublicoAlvo.QUALIFICADO,
    label: 'Qualificado',
    descricao: 'Investidores qualificados (R$ 1MM+)',
  },
  {
    value: PublicoAlvo.PROFISSIONAL,
    label: 'Profissional',
    descricao: 'Investidores profissionais (R$ 10MM+)',
  },
];

/**
 * Constante para maximo de classes permitidas (RF-03)
 */
export const MAX_CLASSES = 10;

/**
 * Valores default para classe Senior (RF-02, RF-06)
 */
export const CLASSE_SENIOR_DEFAULT: FundoClasse = {
  codigoClasse: 'SR',
  nomeClasse: 'Classe Senior',
  cnpjClasse: null,
  classePaiId: null,
  nivel: 1,
  publicoAlvo: PublicoAlvo.QUALIFICADO,
  tipoClasseFidc: TipoClasseFidc.SENIOR,
  ordemSubordinacao: 1,
  rentabilidadeAlvo: null,
  indiceSubordinacaoMinimo: null,
  valorMinimoAplicacao: null,
  valorMinimoPermanencia: null,
  responsabilidadeLimitada: true,
  segregacaoPatrimonial: true,
  taxaAdministracao: null,
  taxaGestao: null,
  taxaPerformance: null,
  benchmarkId: null,
  permiteResgateAntecipado: false,
  dataInicio: null,
  dataEncerramento: null,
  motivoEncerramento: null,
  ativo: true,
};

/**
 * Valores default para nova classe generica
 */
export const CLASSE_DEFAULT: FundoClasse = {
  codigoClasse: '',
  nomeClasse: '',
  cnpjClasse: null,
  classePaiId: null,
  nivel: 1,
  publicoAlvo: null,
  tipoClasseFidc: null,
  ordemSubordinacao: null,
  rentabilidadeAlvo: null,
  indiceSubordinacaoMinimo: null,
  valorMinimoAplicacao: null,
  valorMinimoPermanencia: null,
  responsabilidadeLimitada: true,
  segregacaoPatrimonial: true,
  taxaAdministracao: null,
  taxaGestao: null,
  taxaPerformance: null,
  benchmarkId: null,
  permiteResgateAntecipado: false,
  dataInicio: null,
  dataEncerramento: null,
  motivoEncerramento: null,
  ativo: true,
};

/**
 * Get default responsabilidade limitada based on tipo classe (RF-06)
 */
export function getDefaultResponsabilidadeLimitada(tipoClasse: TipoClasseFidc | null): boolean {
  if (!tipoClasse) return true;
  const option = TIPO_CLASSE_FIDC_OPTIONS.find((opt) => opt.value === tipoClasse);
  return option?.responsabilidadeLimitadaDefault ?? true;
}

/**
 * Get default ordem subordinacao based on tipo classe
 */
export function getDefaultOrdemSubordinacao(tipoClasse: TipoClasseFidc | null): number | null {
  if (!tipoClasse) return null;
  const option = TIPO_CLASSE_FIDC_OPTIONS.find((opt) => opt.value === tipoClasse);
  return option?.ordemPadrao ?? null;
}

/**
 * Limites de percentual para taxas especificas (RF-07)
 */
export const LIMITES_TAXA_CLASSE = {
  taxaAdministracao: 10.0,
  taxaGestao: 5.0,
  taxaPerformance: 50.0,
};
