import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiClientService } from '../api/api-client.service';
import {
  CreateIndexadorDto,
  UpdateIndexadorDto,
  CreateHistoricoIndexadorDto,
} from '@core-ledger/api-client';
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
    limit = 100,
    offset = 0,
    sortBy?: string,
    sortDirection: 'asc' | 'desc' = 'asc',
    filterParams?: Record<string, string>
  ): Observable<PaginatedResponse<Indexador>> {
    return this.apiClient.indexadores
      .getAllIndexadores(
        limit,
        offset,
        sortBy,
        sortDirection,
        filterParams?.['filter'],
        filterParams?.['tipo'] ? parseInt(filterParams['tipo'], 10) : undefined,
        filterParams?.['periodicidade'] ? parseInt(filterParams['periodicidade'], 10) : undefined,
        filterParams?.['fonte'],
        filterParams?.['ativo'] ? filterParams['ativo'] === 'true' : undefined,
        filterParams?.['importacaoAutomatica']
          ? filterParams['importacaoAutomatica'] === 'true'
          : undefined
      )
      .pipe(map((response) => response as unknown as PaginatedResponse<Indexador>));
  }

  /**
   * Gets a single indexador by ID
   */
  getIndexadorById(id: number): Observable<Indexador> {
    return this.apiClient.indexadores
      .getIndexadorById(id)
      .pipe(map((response) => response as unknown as Indexador));
  }

  /**
   * Creates a new indexador
   */
  createIndexador(indexador: CreateIndexador): Observable<Indexador> {
    // Map local model to NSwag DTO with explicit type handling
    const dto: CreateIndexadorDto = {
      codigo: indexador.codigo,
      nome: indexador.nome,
      tipo: indexador.tipo as any,
      fonte: indexador.fonte ?? undefined,
      periodicidade: indexador.periodicidade as any,
      fatorAcumulado: indexador.fatorAcumulado ?? undefined,
      dataBase: indexador.dataBase ? new Date(indexador.dataBase) : undefined,
      urlFonte: indexador.urlFonte ?? undefined,
      importacaoAutomatica: indexador.importacaoAutomatica,
      ativo: indexador.ativo,
    };
    return this.apiClient.indexadores
      .createIndexador(dto)
      .pipe(map((response) => response as unknown as Indexador));
  }

  /**
   * Updates an existing indexador
   * Note: Tipo and Periodicidade cannot be changed after creation
   */
  updateIndexador(id: number, indexador: UpdateIndexador): Observable<Indexador> {
    // Map local model to NSwag DTO with explicit type handling
    const dto: UpdateIndexadorDto = {
      nome: indexador.nome,
      fonte: indexador.fonte ?? undefined,
      fatorAcumulado: indexador.fatorAcumulado ?? undefined,
      dataBase: indexador.dataBase ? new Date(indexador.dataBase) : undefined,
      urlFonte: indexador.urlFonte ?? undefined,
      importacaoAutomatica: indexador.importacaoAutomatica,
      ativo: indexador.ativo,
    };
    return this.apiClient.indexadores
      .updateIndexador(id, dto)
      .pipe(map((response) => response as unknown as Indexador));
  }

  /**
   * Deletes an indexador
   */
  deleteIndexador(id: number): Observable<void> {
    return this.apiClient.indexadores.deleteIndexador(id).pipe(map(() => void 0));
  }

  /**
   * Gets paginated history for an indexador with optional date range filtering
   */
  getHistorico(
    indexadorId: number,
    limit = 100,
    offset = 0,
    sortBy = 'dataReferencia',
    sortDirection: 'asc' | 'desc' = 'desc',
    dataInicio?: string,
    dataFim?: string
  ): Observable<PaginatedResponse<HistoricoIndexador>> {
    return this.apiClient.indexadores
      .getIndexadorHistorico(
        indexadorId,
        limit,
        offset,
        sortBy,
        sortDirection,
        undefined, // filter
        dataInicio ? new Date(dataInicio) : undefined,
        dataFim ? new Date(dataFim) : undefined
      )
      .pipe(map((response) => response as unknown as PaginatedResponse<HistoricoIndexador>));
  }

  /**
   * Creates a new history entry
   */
  createHistorico(historico: CreateHistoricoIndexador): Observable<HistoricoIndexador> {
    // Map local model to NSwag DTO with explicit type handling
    const dto: CreateHistoricoIndexadorDto = {
      indexadorId: historico.indexadorId,
      dataReferencia: historico.dataReferencia
        ? new Date(historico.dataReferencia)
        : new Date(),
      valor: historico.valor ?? undefined,
      fatorDiario: historico.fatorDiario ?? undefined,
      variacaoPercentual: historico.variacaoPercentual ?? undefined,
      fonte: historico.fonte ?? undefined,
      importacaoId: (historico.importacaoId as any) ?? undefined,
    };
    return this.apiClient.historicosIndexadores
      .createHistoricoIndexador(dto)
      .pipe(map((response) => response as unknown as HistoricoIndexador));
  }

  /**
   * Deletes a history entry
   */
  deleteHistorico(id: number): Observable<void> {
    return this.apiClient.historicosIndexadores.deleteHistoricoIndexador(id).pipe(map(() => void 0));
  }

  /**
   * Exports history to CSV
   */
  exportHistorico(indexadorId: number, dataInicio?: string, dataFim?: string): Observable<Blob> {
    return this.apiClient.indexadores
      .exportIndexadorHistorico(
        indexadorId,
        dataInicio ? new Date(dataInicio) : undefined,
        dataFim ? new Date(dataFim) : undefined
      )
      .pipe(map((response) => response.data));
  }

  /**
   * Imports history from CSV file
   */
  importHistorico(
    indexadorId: number,
    file: File,
    sobrescrever = false
  ): Observable<ImportHistoricoResult> {
    return this.apiClient.indexadores
      .importIndexadorHistorico(indexadorId, sobrescrever, { data: file, fileName: file.name })
      .pipe(map((response) => response as unknown as ImportHistoricoResult));
  }

  /**
   * Triggers automatic import for an indexador
   */
  triggerImport(indexadorId: number): Observable<{ id: number; correlationId: string }> {
    return this.apiClient.indexadores
      .importarIndexador(indexadorId)
      .pipe(map((response) => response as unknown as { id: number; correlationId: string }));
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
