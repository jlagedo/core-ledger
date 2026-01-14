/**
 * Wizard de Cadastro de Fundo - Modelos de Vínculos Institucionais
 * Etapa 9: Configuração de vínculos com instituições
 */

// ============================================================
// TIPOS DE VÍNCULO
// ============================================================

/**
 * Enum com os tipos de vínculo institucional
 * RF-01: ADMINISTRADOR, GESTOR, CUSTODIANTE são obrigatórios
 * RF-02: Para FIDC, AGENTE_COBRANCA é recomendado
 */
export enum TipoVinculo {
  ADMINISTRADOR = 'ADMINISTRADOR',
  GESTOR = 'GESTOR',
  CUSTODIANTE = 'CUSTODIANTE',
  DISTRIBUIDOR = 'DISTRIBUIDOR',
  AUDITOR = 'AUDITOR',
  ESCRITURADOR = 'ESCRITURADOR',
  CONTROLADOR = 'CONTROLADOR',
  CONSULTORIA_CREDITO = 'CONSULTORIA_CREDITO',
  AGENTE_COBRANCA = 'AGENTE_COBRANCA',
  CEDENTE = 'CEDENTE',
  FORMADOR_MERCADO = 'FORMADOR_MERCADO',
}

/**
 * Opções de tipo de vínculo para select
 */
export interface TipoVinculoOption {
  value: TipoVinculo;
  label: string;
  descricao: string;
  obrigatorio: boolean;
  fidcRecomendado?: boolean;
}

/**
 * Lista de opções de tipos de vínculo
 */
export const TIPO_VINCULO_OPTIONS: TipoVinculoOption[] = [
  {
    value: TipoVinculo.ADMINISTRADOR,
    label: 'Administrador Fiduciário',
    descricao: 'Responsável pela administração fiduciária do fundo',
    obrigatorio: true,
  },
  {
    value: TipoVinculo.GESTOR,
    label: 'Gestor de Recursos',
    descricao: 'Responsável pela gestão da carteira de investimentos',
    obrigatorio: true,
  },
  {
    value: TipoVinculo.CUSTODIANTE,
    label: 'Custodiante',
    descricao: 'Responsável pela custódia dos ativos do fundo',
    obrigatorio: true,
  },
  {
    value: TipoVinculo.DISTRIBUIDOR,
    label: 'Distribuidor',
    descricao: 'Responsável pela distribuição de cotas do fundo',
    obrigatorio: false,
  },
  {
    value: TipoVinculo.AUDITOR,
    label: 'Auditor Independente',
    descricao: 'Responsável pela auditoria independente',
    obrigatorio: false,
  },
  {
    value: TipoVinculo.ESCRITURADOR,
    label: 'Escriturador',
    descricao: 'Responsável pela escrituração de cotas',
    obrigatorio: false,
  },
  {
    value: TipoVinculo.CONTROLADOR,
    label: 'Controlador',
    descricao: 'Responsável pelo controle (se terceirizado)',
    obrigatorio: false,
  },
  {
    value: TipoVinculo.CONSULTORIA_CREDITO,
    label: 'Consultoria de Crédito',
    descricao: 'Responsável pela consultoria de crédito (FIDC)',
    obrigatorio: false,
    fidcRecomendado: true,
  },
  {
    value: TipoVinculo.AGENTE_COBRANCA,
    label: 'Agente de Cobrança',
    descricao: 'Responsável pela cobrança de recebíveis (FIDC)',
    obrigatorio: false,
    fidcRecomendado: true,
  },
  {
    value: TipoVinculo.CEDENTE,
    label: 'Cedente',
    descricao: 'Cedente de recebíveis (FIDC)',
    obrigatorio: false,
    fidcRecomendado: true,
  },
  {
    value: TipoVinculo.FORMADOR_MERCADO,
    label: 'Formador de Mercado',
    descricao: 'Formador de mercado (ETF/FII)',
    obrigatorio: false,
  },
];

/**
 * Vínculos obrigatórios para todos os fundos (RF-01)
 */
export const VINCULOS_OBRIGATORIOS: TipoVinculo[] = [
  TipoVinculo.ADMINISTRADOR,
  TipoVinculo.GESTOR,
  TipoVinculo.CUSTODIANTE,
];

/**
 * Vínculos recomendados para FIDC (RF-02)
 */
export const VINCULOS_FIDC_RECOMENDADOS: TipoVinculo[] = [
  TipoVinculo.AGENTE_COBRANCA,
  TipoVinculo.CONSULTORIA_CREDITO,
  TipoVinculo.CEDENTE,
];

// ============================================================
// INSTITUIÇÃO
// ============================================================

/**
 * Representação de uma instituição financeira
 */
export interface Instituicao {
  id: string;
  cnpj: string;
  razaoSocial: string;
  nomeFantasia?: string;
  codigoCvm?: string;
  tiposHabilitados?: TipoVinculo[];
}

/**
 * Item de autocomplete de instituição
 */
export interface InstituicaoAutocompleteItem {
  id: string;
  cnpj: string;
  cnpjFormatado: string;
  razaoSocial: string;
  nomeFantasia?: string;
  displayName: string;
}

// ============================================================
// VÍNCULO
// ============================================================

/**
 * Dados de um vínculo institucional no formulário
 */
export interface VinculoFormData {
  tipoVinculo: TipoVinculo;
  instituicaoId: string | null;
  cnpjInstituicao: string;
  nomeInstituicao: string;
  dataInicio: string; // ISO date string YYYY-MM-DD
  dataFim: string | null;
  motivoFim: string | null;
  responsavelNome: string | null;
  responsavelEmail: string | null;
  responsavelTelefone: string | null;
}

/**
 * Dados completos da etapa de vínculos
 */
export interface VinculosFormData {
  vinculos: VinculoFormData[];
}

/**
 * Vínculo padrão para inicialização
 */
export const VINCULO_DEFAULT: Partial<VinculoFormData> = {
  instituicaoId: null,
  cnpjInstituicao: '',
  nomeInstituicao: '',
  dataInicio: '',
  dataFim: null,
  motivoFim: null,
  responsavelNome: null,
  responsavelEmail: null,
  responsavelTelefone: null,
};

// ============================================================
// FUNÇÕES UTILITÁRIAS
// ============================================================

/**
 * Verifica se um tipo de vínculo é obrigatório
 */
export function isVinculoObrigatorio(tipo: TipoVinculo): boolean {
  return VINCULOS_OBRIGATORIOS.includes(tipo);
}

/**
 * Verifica se um tipo de vínculo é recomendado para FIDC
 */
export function isVinculoFidcRecomendado(tipo: TipoVinculo): boolean {
  return VINCULOS_FIDC_RECOMENDADOS.includes(tipo);
}

/**
 * Obtém a opção de tipo de vínculo pelo valor
 */
export function getTipoVinculoOption(tipo: TipoVinculo): TipoVinculoOption | undefined {
  return TIPO_VINCULO_OPTIONS.find((opt) => opt.value === tipo);
}

/**
 * Obtém o label de um tipo de vínculo
 */
export function getTipoVinculoLabel(tipo: TipoVinculo): string {
  const option = getTipoVinculoOption(tipo);
  return option?.label ?? tipo;
}

/**
 * Formata CNPJ para exibição (XX.XXX.XXX/XXXX-XX)
 */
export function formatCnpj(cnpj: string): string {
  const cleaned = cnpj.replace(/\D/g, '');
  if (cleaned.length !== 14) return cnpj;
  return cleaned.replace(
    /^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/,
    '$1.$2.$3/$4-$5'
  );
}

/**
 * Remove formatação do CNPJ
 */
export function unformatCnpj(cnpj: string): string {
  return cnpj.replace(/\D/g, '');
}

/**
 * Obtém tipos de vínculo disponíveis para adicionar
 * (excluindo os já presentes)
 */
export function getAvailableTipoVinculos(
  vinculosExistentes: TipoVinculo[],
  incluirObrigatorios = false
): TipoVinculoOption[] {
  return TIPO_VINCULO_OPTIONS.filter((opt) => {
    // Se já existe um vínculo ativo deste tipo, não permite adicionar outro (RF-05)
    if (vinculosExistentes.includes(opt.value)) {
      return false;
    }
    // Se não incluir obrigatórios, filtra apenas os opcionais
    if (!incluirObrigatorios && opt.obrigatorio) {
      return false;
    }
    return true;
  });
}

/**
 * Verifica se todos os vínculos obrigatórios estão preenchidos
 */
export function hasAllRequiredVinculos(vinculos: VinculoFormData[]): boolean {
  const tiposPreenchidos = vinculos
    .filter((v) => v.instituicaoId || v.cnpjInstituicao)
    .map((v) => v.tipoVinculo);

  return VINCULOS_OBRIGATORIOS.every((tipo) => tiposPreenchidos.includes(tipo));
}

/**
 * Obtém vínculos obrigatórios que estão faltando
 */
export function getMissingRequiredVinculos(vinculos: VinculoFormData[]): TipoVinculo[] {
  const tiposPreenchidos = vinculos
    .filter((v) => v.instituicaoId || v.cnpjInstituicao)
    .map((v) => v.tipoVinculo);

  return VINCULOS_OBRIGATORIOS.filter((tipo) => !tiposPreenchidos.includes(tipo));
}

/**
 * Verifica se um FIDC tem o vínculo de agente de cobrança recomendado
 */
export function hasFidcRecommendedVinculos(vinculos: VinculoFormData[]): boolean {
  const tiposPreenchidos = vinculos
    .filter((v) => v.instituicaoId || v.cnpjInstituicao)
    .map((v) => v.tipoVinculo);

  return tiposPreenchidos.includes(TipoVinculo.AGENTE_COBRANCA);
}
