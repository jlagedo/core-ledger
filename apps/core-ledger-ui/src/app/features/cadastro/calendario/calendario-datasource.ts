import { IDatasource, IGetRowsParams } from 'ag-grid-community';
import { CalendarioService } from '../../../services/calendario';
import { LoggerService } from '../../../services/logger';

export interface PaginationStats {
  totalCount: number;
  loadedCount: number;
  currentRangeStart: number;
  currentRangeEnd: number;
  isLoading: boolean;
}

export interface CalendarioDatasourceConfig {
  /** Get current filter params from store */
  getFilterParams: () => Record<string, string>;
  /** Callback for pagination stats updates */
  onPaginationUpdate?: (stats: PaginationStats) => void;
}

/**
 * Creates an AG Grid datasource for infinite scrolling of calendarios.
 * Integrates with CalendarioService to fetch paginated data.
 */
export function createCalendarioDatasource(
  calendarioService: CalendarioService,
  logger: LoggerService,
  config: CalendarioDatasourceConfig
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
      let sortDirection: 'asc' | 'desc' = 'desc';

      if (sortModel.length > 0) {
        sortBy = sortModel[0].colId;
        sortDirection = sortModel[0].sort as 'asc' | 'desc';
      }

      // Get filter params from store (already built)
      const filterParams = config.getFilterParams();

      // Notify loading started
      config.onPaginationUpdate?.({
        totalCount: 0,
        loadedCount: totalLoadedRows,
        currentRangeStart: startRow,
        currentRangeEnd: endRow,
        isLoading: true,
      });

      // Fetch data from API
      calendarioService.getCalendarios(limit, offset, sortBy, sortDirection, filterParams).subscribe({
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
          logger.logHttpError('load calendarios', err, 'Failed to load calendarios');
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
