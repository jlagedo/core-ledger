import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { type CoreLedgerApiClient, createCoreLedgerApiClient } from '@core-ledger/api-client';
import { AngularRequestAdapter } from './angular-request-adapter';

/**
 * Angular service wrapper for the Kiota-generated Core Ledger API client.
 *
 * Provides type-safe access to all API endpoints through the generated client.
 * Integrates with Angular's HttpClient to ensure:
 * - Authentication via HTTP interceptors
 * - Mock API support in development
 * - Consistent error handling
 *
 * @example
 * ```typescript
 * // Inject in a component or service
 * private readonly apiClient = inject(ApiClientService);
 *
 * // Access API endpoints
 * const funds = await this.apiClient.client.api.v1.fundos.get();
 * const fund = await this.apiClient.client.api.v1.fundos.item('123').get();
 * ```
 *
 * @example
 * ```typescript
 * // Using convenience accessors
 * const funds = await this.apiClient.fundos.get();
 * const accounts = await this.apiClient.accounts.get();
 * ```
 */
@Injectable({ providedIn: 'root' })
export class ApiClientService {
  private readonly httpClient = inject(HttpClient);

  /**
   * Kiota-generated API client instance.
   * Use this to access all API endpoints with full type safety.
   */
  readonly client: CoreLedgerApiClient;

  constructor() {
    // Create Angular-compatible request adapter
    const adapter = new AngularRequestAdapter(this.httpClient);

    // Initialize Kiota client
    // Note: The adapter's getRequestUrl() uses empty baseUrl for relative URLs,
    // allowing Angular's proxy to route '/api/**' to the backend.
    this.client = createCoreLedgerApiClient(adapter);
  }

  // Convenience accessors for common API endpoints

  /**
   * Access to Fundos (Funds) API endpoints
   * @example await this.apiClient.fundos.get()
   */
  get fundos() {
    return this.client.api.v1.fundos;
  }

  /**
   * Access to Accounts API endpoints
   * @example await this.apiClient.accounts.get()
   */
  get accounts() {
    return this.client.api.accounts;
  }

  /**
   * Access to Securities API endpoints
   * @example await this.apiClient.securities.get()
   */
  get securities() {
    return this.client.api.securities;
  }

  /**
   * Access to Transactions API endpoints
   * @example await this.apiClient.transactions.get()
   */
  get transactions() {
    return this.client.api.transactions;
  }

  /**
   * Access to Indexadores (Indices) API endpoints
   * @example await this.apiClient.indexadores.get()
   */
  get indexadores() {
    return this.client.api.indexadores;
  }

  /**
   * Access to Users API endpoints
   * @example await this.apiClient.users.me.get()
   */
  get users() {
    return this.client.api.users;
  }

  /**
   * Access to Parametros (Parameters) API endpoints
   * @example await this.apiClient.parametros.classificacoesAnbima.get()
   */
  get parametros() {
    return this.client.api.v1.parametros;
  }

  /**
   * Access to Calendario (Calendar) API endpoints
   * @example await this.apiClient.calendario.get()
   */
  get calendario() {
    return this.client.api.v1.calendario;
  }
}
