import { CnpjVerificationResponse } from '../../features/cadastro/fundos/wizard/models/identificacao.model';

/**
 * Mock registered CNPJs for testing duplicate detection.
 * @internal
 */
export const MOCK_REGISTERED_CNPJS: Map<string, { fundoId: number; nomeFantasia: string }> =
  new Map([
    ['11222333000181', { fundoId: 1, nomeFantasia: 'FI Valor Acoes Master' }],
    ['22333444000192', { fundoId: 2, nomeFantasia: 'FIC Renda Fixa Premium' }],
    ['33444555000103', { fundoId: 3, nomeFantasia: 'FII Shopping Center Sul' }],
  ]);

/**
 * Mock response factory for CNPJ verification.
 * ALWAYS returns disponivel: true for development/testing purposes.
 * @internal
 */
export function mockVerificarCnpj(cnpj: string): CnpjVerificationResponse {
  // Always return available for development
  return { disponivel: true };
}
