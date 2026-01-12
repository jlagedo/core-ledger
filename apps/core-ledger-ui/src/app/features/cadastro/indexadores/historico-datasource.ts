import { IDatasource, IGetRowsParams } from 'ag-grid-community';
import { IndexadorService } from '../../../services/indexador';
import { LoggerService } from '../../../services/logger';

export interface HistoricoPaginationStats {
  totalCount: number;
  loadedCount: number;
  currentRangeStart: number;
  currentRangeEnd: number;
  isLoading: boolean;
}

export interface HistoricoDatasourceConfig {
  /** Indexador ID to fetch history for */
  indexadorId: number;
  /** Get current date range filter */
  getDateRange: () => { dataInicio?: string; dataFim?: string };
  /** Callback for pagination stats updates */
  onPaginationUpdate?: (stats: HistoricoPaginationStats) => void;
}

/**
 * Creates an AG Grid datasource for infinite scrolling of indexador history.
 * Integrates with IndexadorService to fetch paginated historical data.
 */
export function createHistoricoDatasource(
  indexadorService: IndexadorService,
  logger: LoggerService,
  config: HistoricoDatasourceConfig
): IDatasource {
  let totalLoadedRows = 0;

  return {
    getRows(params: IGetRowsParams): void {
      const { startRow, endRow, sortModel, successCallback, failCallback } = params;

      // Calculate limit and offset for API
      const limit = endRow - startRow;
      const offset = startRow;

      // Extract sort parameters from AG Grid sort model
      let sortBy = 'dataReferencia';
      let sortDirection: 'asc' | 'desc' = 'desc';

      if (sortModel.length > 0) {
        sortBy = sortModel[0].colId;
        sortDirection = sortModel[0].sort as 'asc' | 'desc';
      }

      // Get date range filters
      const { dataInicio, dataFim } = config.getDateRange();

      // Notify loading started
      config.onPaginationUpdate?.({
        totalCount: 0,
        loadedCount: totalLoadedRows,
        currentRangeStart: startRow,
        currentRangeEnd: endRow,
        isLoading: true,
      });

      // Fetch data from API
      indexadorService
        .getHistorico(config.indexadorId, limit, offset, sortBy, sortDirection, dataInicio, dataFim)
        .subscribe({
          next: (response) => {
            // Calculate lastRow for infinite scroll termination
            const lastRow = response.totalCount <= endRow ? response.totalCount : -1;

            // Track total loaded rows
            totalLoadedRows = Math.max(totalLoadedRows, endRow);
            if (lastRow !== -1) {
              totalLoadedRows = response.totalCount;
            }

            // Notify pagination update
            config.onPaginationUpdate?.({
              totalCount: response.totalCount,
              loadedCount: Math.min(totalLoadedRows, response.totalCount),
              currentRangeStart: startRow + 1,
              currentRangeEnd: Math.min(endRow, response.totalCount),
              isLoading: false,
            });

            successCallback(response.items, lastRow);
          },
          error: (err) => {
            logger.logHttpError('load historico', err, 'Failed to load historico indexador');
            config.onPaginationUpdate?.({
              totalCount: 0,
              loadedCount: 0,
              currentRangeStart: 0,
              currentRangeEnd: 0,
              isLoading: false,
            });
            failCallback();
          },
        });
    },
  };
}
