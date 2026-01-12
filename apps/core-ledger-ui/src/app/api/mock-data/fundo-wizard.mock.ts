import {
  FundoWizardResponse,
  FundoWizardRequest,
  FundoProgressoResponse,
} from '../../features/cadastro/fundos/wizard/models/wizard.model';

/**
 * Mock response para criação de fundo via wizard
 * @internal
 */
export const MOCK_WIZARD_RESPONSE: FundoWizardResponse = {
  id: 1001,
  status: 'rascunho',
  progresso: 100,
  mensagem: 'Fundo cadastrado com sucesso',
  createdAt: new Date().toISOString(),
};

/**
 * Mock request template para wizard
 * @internal
 */
export const MOCK_WIZARD_REQUEST: FundoWizardRequest = {
  identificacao: {},
  classificacao: {},
  caracteristicas: {},
  parametrosCota: {},
  taxas: [],
  prazos: [],
  classes: [],
  vinculos: [],
  documentos: [],
};

/**
 * Mock response para endpoint de progresso
 * @internal
 */
export const MOCK_PROGRESSO_RESPONSE: FundoProgressoResponse = {
  fundoId: 1001,
  progresso: 100,
  stepsConcluidos: 10,
  totalSteps: 10,
};

/**
 * Mensagens de erro em português
 * @internal
 */
export const MOCK_WIZARD_ERRORS = {
  cnpjDuplicado: 'CNPJ já cadastrado no sistema',
  validacaoFalhou: 'Erro de validação nos dados informados',
  erroInterno: 'Erro interno do servidor. Tente novamente mais tarde.',
};
