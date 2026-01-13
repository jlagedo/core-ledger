import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { from, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_URL } from '../config/api.config';
import { ApiClientService } from '../api/api-client.service';
import {
  Indexador,
  CreateIndexador,
  UpdateIndexador,
  HistoricoIndexador,
  CreateHistoricoIndexador,
  ImportHistoricoResult,
  PaginatedResponse,
  TipoIndexadorOption,
  PeriodicidadeOption,
  FonteOption,
  TipoIndexador,
  Periodicidade,
} from '../models/indexador.model';

@Injectable({ providedIn: 'root' })
export class IndexadorService {
  private readonly apiClient = inject(ApiClientService);

  // HttpClient retained for file operations (export/import)
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(API_URL);

  // Static enum options for dropdowns with Bloomberg-inspired colors
  readonly tipoIndexadorOptions: TipoIndexadorOption[] = [
    {
      value: TipoIndexador.Juros,
      name: 'Juros',
      description: 'Taxa de juros (CDI, SELIC)',
      icon: 'bi-percent',
      color: '#0068ff',
    },
    {
      value: TipoIndexador.Inflacao,
      name: 'Inflacao',
      description: 'Indice de inflacao (IPCA, IGP-M)',
      icon: 'bi-graph-up-arrow',
      color: '#f97316',
    },
    {
      value: TipoIndexador.Cambio,
      name: 'Cambio',
      description: 'Taxa de cambio (PTAX)',
      icon: 'bi-currency-exchange',
      color: '#22c55e',
    },
    {
      value: TipoIndexador.IndiceBolsa,
      name: 'Indice Bolsa',
      description: 'Indice de bolsa (IBOVESPA, IBRX)',
      icon: 'bi-bar-chart-line',
      color: '#9333ea',
    },
    {
      value: TipoIndexador.IndiceRendaFixa,
      name: 'Indice Renda Fixa',
      description: 'Indice de renda fixa (IMA-B, IMA-S)',
      icon: 'bi-graph-down',
      color: '#06b6d4',
    },
    {
      value: TipoIndexador.Crypto,
      name: 'Crypto',
      description: 'Criptomoeda (BTC, ETH)',
      icon: 'bi-currency-bitcoin',
      color: '#eab308',
    },
    {
      value: TipoIndexador.Outro,
      name: 'Outro',
      description: 'Outro tipo de indexador',
      icon: 'bi-question-circle',
      color: '#6b7280',
    },
  ];

  readonly periodicidadeOptions: PeriodicidadeOption[] = [
    { value: Periodicidade.Diaria, name: 'Diaria', description: 'Atualizado diariamente' },
    { value: Periodicidade.Mensal, name: 'Mensal', description: 'Atualizado mensalmente' },
    { value: Periodicidade.Anual, name: 'Anual', description: 'Atualizado anualmente' },
  ];

  readonly fonteOptions: FonteOption[] = [
    {
      value: 'ANBIMA',
      name: 'ANBIMA',
      description: 'Associacao Brasileira das Entidades dos Mercados Financeiro e de Capitais',
    },
    { value: 'B3', name: 'B3', description: 'B3 - Brasil, Bolsa, Balcao' },
    { value: 'BACEN', name: 'BACEN', description: 'Banco Central do Brasil' },
    { value: 'IBGE', name: 'IBGE', description: 'Instituto Brasileiro de Geografia e Estatistica' },
    { value: 'FGV', name: 'FGV', description: 'Fundacao Getulio Vargas' },
    { value: 'MANUAL', name: 'Manual', description: 'Entrada manual' },
  ];

  /**
   * Gets paginated list of indexadores with optional filtering and sorting.
   * Supports both search term (filter) and structured filters (tipo, ativo, etc.)
   */
  getIndexadores(
    limit: number = 100,
    offset: number = 0,
    sortBy?: string,
    sortDirection: 'asc' | 'desc' = 'asc',
    filterParams?: Record<string, string>
  ): Observable<PaginatedResponse<Indexador>> {
    return from(
      this.apiClient.indexadores.get({
        queryParameters: {
          limit,
          offset,
          sortBy,
          sortDirection,
          filter: filterParams?.['filter'],
          tipo: filterParams?.['tipo'] ? parseInt(filterParams['tipo'], 10) : undefined,
          periodicidade: filterParams?.['periodicidade']
            ? parseInt(filterParams['periodicidade'], 10)
            : undefined,
          fonte: filterParams?.['fonte'],
          ativo: filterParams?.['ativo'] ? filterParams['ativo'] === 'true' : undefined,
          importacaoAutomatica: filterParams?.['importacaoAutomatica']
            ? filterParams['importacaoAutomatica'] === 'true'
            : undefined,
        },
      })
    ).pipe(map((response) => response as unknown as PaginatedResponse<Indexador>));
  }

  /**
   * Gets a single indexador by ID
   */
  getIndexadorById(id: number): Observable<Indexador> {
    return from(this.apiClient.indexadores.byId(id).get()).pipe(
      map((response) => response as unknown as Indexador)
    );
  }

  /**
   * Creates a new indexador
   */
  createIndexador(indexador: CreateIndexador): Observable<Indexador> {
    return from(
      this.apiClient.indexadores.post({
        codigo: indexador.codigo,
        nome: indexador.nome,
        tipo: indexador.tipo,
        fonte: indexador.fonte,
        periodicidade: indexador.periodicidade,
        fatorAcumulado: indexador.fatorAcumulado,
        dataBase: indexador.dataBase ? new Date(indexador.dataBase) : null,
        urlFonte: indexador.urlFonte,
        importacaoAutomatica: indexador.importacaoAutomatica,
        ativo: indexador.ativo,
      })
    ).pipe(map((response) => response as unknown as Indexador));
  }

  /**
   * Updates an existing indexador
   * Note: Tipo and Periodicidade cannot be changed after creation
   */
  updateIndexador(id: number, indexador: UpdateIndexador): Observable<Indexador> {
    return from(
      this.apiClient.indexadores.byId(id).put({
        nome: indexador.nome,
        fonte: indexador.fonte,
        fatorAcumulado: indexador.fatorAcumulado,
        dataBase: indexador.dataBase ? new Date(indexador.dataBase) : null,
        urlFonte: indexador.urlFonte,
        importacaoAutomatica: indexador.importacaoAutomatica,
        ativo: indexador.ativo,
      })
    ).pipe(map((response) => response as unknown as Indexador));
  }

  /**
   * Deletes an indexador
   */
  deleteIndexador(id: number): Observable<void> {
    return from(this.apiClient.indexadores.byId(id).delete()).pipe(map(() => void 0));
  }

  /**
   * Gets paginated history for an indexador with optional date range filtering
   */
  getHistorico(
    indexadorId: number,
    limit: number = 100,
    offset: number = 0,
    sortBy: string = 'dataReferencia',
    sortDirection: 'asc' | 'desc' = 'desc',
    dataInicio?: string,
    dataFim?: string
  ): Observable<PaginatedResponse<HistoricoIndexador>> {
    return from(
      this.apiClient.indexadores.byId(indexadorId).historico.get({
        queryParameters: {
          limit,
          offset,
          sortBy,
          sortDirection,
          dataInicio: dataInicio as any,
          dataFim: dataFim as any,
        },
      })
    ).pipe(map((response) => response as unknown as PaginatedResponse<HistoricoIndexador>));
  }

  /**
   * Creates a new history entry
   */
  createHistorico(historico: CreateHistoricoIndexador): Observable<HistoricoIndexador> {
    return from(
      this.apiClient.client.api.historicosIndexadores.post({
        indexadorId: historico.indexadorId,
        dataReferencia: historico.dataReferencia ? new Date(historico.dataReferencia) : null,
        valor: historico.valor,
        fatorDiario: historico.fatorDiario,
        variacaoPercentual: historico.variacaoPercentual,
        fonte: historico.fonte,
        importacaoId: historico.importacaoId as any,
      })
    ).pipe(map((response) => response as unknown as HistoricoIndexador));
  }

  /**
   * Deletes a history entry
   */
  deleteHistorico(id: number): Observable<void> {
    return from(this.apiClient.client.api.historicosIndexadores.byId(id).delete()).pipe(
      map(() => void 0)
    );
  }

  /**
   * Exports history to CSV
   * NOTE: Kept using HttpClient due to blob response type requirement
   */
  exportHistorico(indexadorId: number, dataInicio?: string, dataFim?: string): Observable<Blob> {
    const params: Record<string, string> = {};
    if (dataInicio) params['dataInicio'] = dataInicio;
    if (dataFim) params['dataFim'] = dataFim;

    const queryString = new URLSearchParams(params).toString();
    const url = queryString
      ? `${this.apiUrl}/indexadores/${indexadorId}/historico/exportar?${queryString}`
      : `${this.apiUrl}/indexadores/${indexadorId}/historico/exportar`;

    return this.http.get(url, {
      responseType: 'blob',
    });
  }

  /**
   * Imports history from CSV file
   * NOTE: Kept using HttpClient due to FormData/multipart requirement
   */
  importHistorico(
    indexadorId: number,
    file: File,
    sobrescrever: boolean = false
  ): Observable<ImportHistoricoResult> {
    const formData = new FormData();
    formData.append('file', file);

    const params = new URLSearchParams({ sobrescrever: sobrescrever.toString() }).toString();
    return this.http.post<ImportHistoricoResult>(
      `${this.apiUrl}/indexadores/${indexadorId}/historico/importar?${params}`,
      formData
    );
  }

  /**
   * Triggers automatic import for an indexador
   */
  triggerImport(indexadorId: number): Observable<{ id: number; correlationId: string }> {
    return from(this.apiClient.indexadores.byId(indexadorId).importar.post({})).pipe(
      map((response) => response as unknown as { id: number; correlationId: string })
    );
  }

  /**
   * Helper to get tipo color
   */
  getTipoColor(tipo: TipoIndexador): string {
    return this.tipoIndexadorOptions.find((o) => o.value === tipo)?.color ?? '#6b7280';
  }

  /**
   * Helper to get tipo icon
   */
  getTipoIcon(tipo: TipoIndexador): string {
    return this.tipoIndexadorOptions.find((o) => o.value === tipo)?.icon ?? 'bi-question-circle';
  }
}
