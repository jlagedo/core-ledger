import { Injectable, inject } from '@angular/core';
import {
  FundosClient,
  CalendárioClient,
  IndexadoresClient,
  UsersClient,
  AccountsClient,
  SecuritiesClient,
  TransactionsClient,
  ANBIMA_ClassificationsClient,
  HistoricosIndexadoresClient,
} from '@core-ledger/api-client';

/**
 * Facade service providing unified access to all NSwag-generated API clients.
 *
 * This service simplifies API access by providing named accessors for each client.
 * Each client is already registered as a singleton (`providedIn: 'root'`),
 * so this facade is optional - you can inject clients directly if preferred.
 *
 * @example
 * ```typescript
 * // Using facade service
 * private readonly apiClient = inject(ApiClientService);
 * this.apiClient.fundos.getAllFundos()
 *
 * // Or inject clients directly
 * private readonly fundosClient = inject(FundosClient);
 * this.fundosClient.getAllFundos()
 * ```
 */
@Injectable({ providedIn: 'root' })
export class ApiClientService {
  /** Fundos (Funds) API - CVM 175 compliant fund management */
  readonly fundos = inject(FundosClient);

  /** Calendário (Calendar) API - Business days and holidays */
  readonly calendario = inject(CalendárioClient);

  /** Indexadores (Indices) API - Financial indices (CDI, IPCA, etc.) */
  readonly indexadores = inject(IndexadoresClient);

  /** Históricos de Indexadores API - Index history data */
  readonly historicosIndexadores = inject(HistoricosIndexadoresClient);

  /** Users API - User management */
  readonly users = inject(UsersClient);

  /** Accounts API - Chart of accounts */
  readonly accounts = inject(AccountsClient);

  /** Securities API - Financial instruments */
  readonly securities = inject(SecuritiesClient);

  /** Transactions API - Transaction management */
  readonly transactions = inject(TransactionsClient);

  /** ANBIMA Classifications API - Fund classification parameters */
  readonly parametros = inject(ANBIMA_ClassificationsClient);
}
