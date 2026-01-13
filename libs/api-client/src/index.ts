/**
 * Core Ledger API Client
 *
 * Auto-generated TypeScript client for Core Ledger API.
 * Generated using Microsoft Kiota from OpenAPI specification.
 *
 * To regenerate:
 * 1. Ensure API is running: nx serve core-ledger-api
 * 2. Run: nx run api-client:generate
 *
 * @example
 * ```typescript
 * // In Angular service
 * import { inject, Injectable } from '@angular/core';
 * import { ApiClientService } from '../api/api-client.service';
 * import { type CnpjDisponibilidadeResponseDto } from '@core-ledger/api-client';
 *
 * @Injectable({ providedIn: 'root' })
 * export class MyService {
 *   private readonly apiClient = inject(ApiClientService);
 *
 *   async checkCnpj(cnpj: string) {
 *     return await this.apiClient.fundos.verificarCnpj.byCnpj(cnpj).get();
 *   }
 * }
 * ```
 */

// Export main client factory and type
export {
  type CoreLedgerApiClient,
  createCoreLedgerApiClient,
} from '../generated/coreLedgerApiClient';

// Export commonly used models
export type { CnpjDisponibilidadeResponseDto, FundoCreateDto, FundoUpdateDto } from '../generated/models/application/dTOs/fundo';

// Export all generated code (for advanced usage)
export * from '../generated/coreLedgerApiClient';
// Note: Models are re-exported selectively above due to Kiota's nested structure
export * from '../generated/api';
