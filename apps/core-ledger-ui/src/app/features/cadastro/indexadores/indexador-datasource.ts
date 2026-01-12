import { IDatasource, IGetRowsParams } from 'ag-grid-community';
import { IndexadorService } from '../../../services/indexador';
import { LoggerService } from '../../../services/logger';

export interface PaginationStats {
  totalCount: number;
  loadedCount: number;
  currentRangeStart: number;
  currentRangeEnd: number;
  isLoading: boolean;
}

export interface IndexadorDatasourceConfig {
  /** Get current filter params from store */
  getFilterParams: () => Record<string, string>;
  /** Callback for pagination stats updates */
  onPaginationUpdate?: (stats: PaginationStats) => void;
}

/**
 * Creates an AG Grid datasource for infinite scrolling of indexadores.
 * Integrates with IndexadorService to fetch paginated data.
 */
export function createIndexadorDatasource(
  indexadorService: IndexadorService,
  logger: LoggerService,
  config: IndexadorDatasourceConfig
): IDatasource {
  let totalLoadedRows = 0;

  return {
    getRows(params: IGetRowsParams): void {
      const { startRow, endRow, sortModel, successCallback, failCallback } = params;

      // Calculate limit and offset for API
      const limit = endRow - startRow;
      const offset = startRow;

      // Extract sort parameters from AG Grid sort model
      let sortBy: string | undefined;
      let sortDirection: 'asc' | 'desc' = 'asc';

      if (sortModel.length > 0) {
        sortBy = sortModel[0].colId;
        sortDirection = sortModel[0].sort as 'asc' | 'desc';
      }

      // Get filter params from store (already built with all structured filters)
      const filterParams = config.getFilterParams();

      // Notify loading started
      config.onPaginationUpdate?.({
        totalCount: 0,
        loadedCount: totalLoadedRows,
        currentRangeStart: startRow,
        currentRangeEnd: endRow,
        isLoading: true,
      });

      // Fetch data from API with all filter params
      indexadorService.getIndexadores(limit, offset, sortBy, sortDirection, filterParams).subscribe({
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
          logger.logHttpError('load indexadores', err, 'Failed to load indexadores');
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
