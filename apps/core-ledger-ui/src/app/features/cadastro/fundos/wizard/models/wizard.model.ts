/**
 * Wizard de Cadastro de Fundo - Modelos TypeScript
 */

// IDs dos passos do wizard (1-10)
export type WizardStepId = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10;

// Estados visuais do passo
export type StepStatus = 'pending' | 'current' | 'completed' | 'error';

/**
 * Configuração de um passo do wizard
 */
export interface WizardStepConfig {
  id: WizardStepId;
  key: string;
  label: string;
  shortLabel: string;  // 6 caracteres max para display terminal
  icon: string;        // Bootstrap icon class
  descricao: string;   // Texto de ajuda/tooltip
}

/**
 * Estado de validação de um passo
 */
export interface StepValidation {
  isValid: boolean;
  isDirty: boolean;
  errors: string[];
}

/**
 * Estado completo do wizard
 */
export interface WizardState {
  currentStep: WizardStepId;
  stepData: Record<string, unknown>;
  stepValidation: Record<WizardStepId, StepValidation>;
  completedSteps: Set<WizardStepId>;
  isSubmitting: boolean;
  submitError: string | null;
  isDirty: boolean;
}

/**
 * Configuração dos 10 passos do wizard (PT-BR)
 */
export const WIZARD_STEPS: WizardStepConfig[] = [
  {
    id: 1,
    key: 'identificacao',
    label: 'Identificação',
    shortLabel: 'IDENT',
    icon: 'bi-card-heading',
    descricao: 'Dados básicos do fundo (CNPJ, nome, administrador)',
  },
  {
    id: 2,
    key: 'classificacao',
    label: 'Classificação',
    shortLabel: 'CLASS',
    icon: 'bi-tags',
    descricao: 'Classificação ANBIMA e CVM',
  },
  {
    id: 3,
    key: 'caracteristicas',
    label: 'Características',
    shortLabel: 'CARAC',
    icon: 'bi-sliders',
    descricao: 'Características operacionais do fundo',
  },
  {
    id: 4,
    key: 'parametrosCota',
    label: 'Parâmetros de Cota',
    shortLabel: 'COTA',
    icon: 'bi-calculator',
    descricao: 'Configuração de cálculo de cota',
  },
  {
    id: 5,
    key: 'taxas',
    label: 'Taxas',
    shortLabel: 'TAXAS',
    icon: 'bi-percent',
    descricao: 'Taxas de administração, performance e custódia',
  },
  {
    id: 6,
    key: 'prazos',
    label: 'Prazos',
    shortLabel: 'PRAZO',
    icon: 'bi-calendar-event',
    descricao: 'Prazos de cotização, liquidação e carência',
  },
  {
    id: 7,
    key: 'classes',
    label: 'Classes',
    shortLabel: 'CLASSE',
    icon: 'bi-layers',
    descricao: 'Classes de cotas e subclasses',
  },
  {
    id: 8,
    key: 'vinculos',
    label: 'Vínculos',
    shortLabel: 'VINC',
    icon: 'bi-link-45deg',
    descricao: 'Vínculos com outros fundos (master/feeder)',
  },
  {
    id: 9,
    key: 'documentos',
    label: 'Documentos',
    shortLabel: 'DOCS',
    icon: 'bi-file-earmark-text',
    descricao: 'Upload de regulamento e documentos',
  },
  {
    id: 10,
    key: 'revisao',
    label: 'Revisão',
    shortLabel: 'REVIS',
    icon: 'bi-check2-all',
    descricao: 'Revisão final e submissão',
  },
];

/**
 * DTOs para API do Wizard
 */

// Request para criação de fundo via wizard
export interface FundoWizardRequest {
  identificacao: Record<string, unknown>;
  classificacao: Record<string, unknown>;
  caracteristicas: Record<string, unknown>;
  parametrosCota: Record<string, unknown>;
  taxas: unknown[];
  prazos: unknown[];
  classes: unknown[];
  vinculos: unknown[];
  documentos: unknown[];
}

// Response da criação do fundo
export interface FundoWizardResponse {
  id: number;
  status: string;
  progresso: number;
  mensagem?: string;
  createdAt: string;
}

// Response do endpoint de progresso
export interface FundoProgressoResponse {
  fundoId: number;
  progresso: number;
  stepsConcluidos: number;
  totalSteps: number;
}
