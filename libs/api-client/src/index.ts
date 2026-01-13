/**
 * Core Ledger API Client
 *
 * Auto-generated TypeScript clients for Core Ledger API.
 * Generated using NSwag from OpenAPI specification.
 *
 * To regenerate:
 * 1. Ensure API is running: nx serve core-ledger-api
 * 2. Export OpenAPI spec: nx run core-ledger-api:export-openapi
 * 3. Run: nx run api-client:generate
 *
 * @example
 * ```typescript
 * // In Angular service
 * import { inject, Injectable } from '@angular/core';
 * import { FundosClient, CnpjDisponibilidadeResponseDto } from '@core-ledger/api-client';
 *
 * @Injectable({ providedIn: 'root' })
 * export class MyService {
 *   private readonly fundosClient = inject(FundosClient);
 *
 *   checkCnpj(cnpj: string): Observable<CnpjDisponibilidadeResponseDto> {
 *     return this.fundosClient.verificarCnpjDisponivel(cnpj);
 *   }
 * }
 * ```
 */

// Export all generated clients and DTOs
export * from '../generated/api-clients';
