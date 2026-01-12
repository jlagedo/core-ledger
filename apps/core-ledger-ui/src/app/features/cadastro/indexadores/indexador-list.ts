import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { AgGridAngular } from 'ag-grid-angular';
import {
  ColDef,
  GridApi,
  GridReadyEvent,
  SortChangedEvent,
  IDatasource,
  CellClickedEvent,
  GetRowIdFunc,
  GetRowIdParams,
} from 'ag-grid-community';

import { Indexador, TipoIndexador, Periodicidade } from '../../../models/indexador.model';
import { IndexadorService } from '../../../services/indexador';
import {
  IndexadorStore,
  INDEXADOR_PRESETS,
  IndexadorPresetId,
} from './indexador-store';
import { LoggerService } from '../../../services/logger';
import { PageHeader } from '../../../layout/page-header/page-header';
import { ThemeService } from '../../../services/theme-service';
import { ToastService } from '../../../services/toast-service';
import { createIndexadorDatasource, PaginationStats } from './indexador-datasource';
import { agGridDarkTheme, agGridLightTheme } from '../../../shared/themes/ag-grid-theme';
import { NgbDropdownModule, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { DeleteIndexadorModal } from './delete-modal/delete-modal';

@Component({
  selector: 'app-indexador-list',
  imports: [
    RouterLink,
    PageHeader,
    AgGridAngular,
    FormsModule,
    DecimalPipe,
    NgbDropdownModule,
  ],
  providers: [IndexadorStore, DatePipe],
  templateUrl: './indexador-list.html',
  styleUrl: './indexador-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IndexadorList {
  private readonly store = inject(IndexadorStore);
  private readonly indexadorService = inject(IndexadorService);
  private readonly logger = inject(LoggerService);
  private readonly datePipe = inject(DatePipe);
  private readonly router = inject(Router);
  private readonly themeService = inject(ThemeService);
  private readonly toastService = inject(ToastService);
  private readonly modal = inject(NgbModal);

  private gridApi: GridApi<Indexador> | null = null;

  // Search term bound to input
  searchTerm = signal('');

  // Expose enum options for template
  readonly tipoOptions = this.indexadorService.tipoIndexadorOptions;
  readonly periodicidadeOptions = this.indexadorService.periodicidadeOptions;
  readonly fonteOptions = this.indexadorService.fonteOptions;
  readonly ativoOptions = [
    { value: true, name: 'Ativo' },
    { value: false, name: 'Inativo' },
  ];
  readonly importacaoAutoOptions = [
    { value: true, name: 'Sim' },
    { value: false, name: 'Nao' },
  ];

  // Store state exposed to template
  readonly filters = this.store.filters;
  readonly hasActiveFilters = this.store.hasActiveFilters;
  readonly activeFilterCount = this.store.activeFilterCount;
  readonly activePreset = this.store.activePreset;

  // Presets configuration
  readonly presets = INDEXADOR_PRESETS;

  // Pagination statistics for status bar
  readonly paginationStats = signal<PaginationStats>({
    totalCount: 0,
    loadedCount: 0,
    currentRangeStart: 0,
    currentRangeEnd: 0,
    isLoading: false,
  });

  // Computed helpers for template
  readonly totalCount = computed(() => this.paginationStats().totalCount);
  readonly isLoading = computed(() => this.paginationStats().isLoading);

  // AG Grid theme - reactive to app theme changes
  readonly gridTheme = computed(() =>
    this.themeService.isDarkMode() ? agGridDarkTheme : agGridLightTheme
  );

  // Get selected filter labels for display
  readonly selectedTipoLabel = computed(() => {
    const tipo = this.filters().tipo;
    if (tipo === null) return null;
    return this.tipoOptions.find((o) => o.value === tipo)?.name || null;
  });

  readonly selectedPeriodicidadeLabel = computed(() => {
    const periodicidade = this.filters().periodicidade;
    if (periodicidade === null) return null;
    return this.periodicidadeOptions.find((o) => o.value === periodicidade)?.name || null;
  });

  readonly selectedFonteLabel = computed(() => {
    const fonte = this.filters().fonte;
    if (fonte === null) return null;
    return this.fonteOptions.find((o) => o.value === fonte)?.name || fonte;
  });

  readonly selectedAtivoLabel = computed(() => {
    const ativo = this.filters().ativo;
    if (ativo === null) return null;
    return ativo ? 'Ativo' : 'Inativo';
  });

  readonly selectedImportacaoAutoLabel = computed(() => {
    const importacaoAuto = this.filters().importacaoAutomatica;
    if (importacaoAuto === null) return null;
    return importacaoAuto ? 'Sim' : 'Nao';
  });

  // Row ID for selection persistence
  readonly getRowId: GetRowIdFunc<Indexador> = (params: GetRowIdParams<Indexador>) =>
    String(params.data.id);

  // Column definitions
  readonly columnDefs: ColDef<Indexador>[] = [
    {
      field: 'codigo',
      headerName: 'Codigo',
      width: 100,
      minWidth: 80,
      maxWidth: 120,
      sortable: true,
      cellClass: 'fw-semibold',
    },
    {
      field: 'nome',
      headerName: 'Nome',
      minWidth: 200,
      flex: 1,
      sortable: true,
    },
    {
      field: 'tipo',
      headerName: 'Tipo',
      width: 140,
      minWidth: 120,
      sortable: true,
      cellRenderer: (params: { data: Indexador }) => {
        if (!params.data) return '';
        const tipo = params.data.tipo;
        const option = this.tipoOptions.find((o) => o.value === tipo);
        if (!option) return params.data.tipoDescricao || '';
        return `
          <span class="tipo-badge" style="--tipo-color: ${option.color}">
            <i class="bi ${option.icon}"></i>
            <span>${option.name}</span>
          </span>
        `;
      },
    },
    {
      field: 'periodicidade',
      headerName: 'Periodicidade',
      width: 120,
      minWidth: 100,
      sortable: true,
      valueFormatter: (params) => {
        const option = this.periodicidadeOptions.find((o) => o.value === params.value);
        return option?.name || params.data?.periodicidadeDescricao || '';
      },
    },
    {
      field: 'fonte',
      headerName: 'Fonte',
      width: 100,
      minWidth: 80,
      sortable: true,
      valueFormatter: (params) => params.value || '—',
    },
    {
      field: 'fatorAcumulado',
      headerName: 'Fator Acum.',
      width: 130,
      minWidth: 110,
      sortable: true,
      cellClass: 'text-end',
      headerClass: 'text-end',
      valueFormatter: (params) => {
        if (params.value === null || params.value === undefined) return '—';
        return params.value.toFixed(9);
      },
    },
    {
      field: 'dataBase',
      headerName: 'Data Base',
      width: 110,
      minWidth: 100,
      sortable: true,
      cellClass: 'text-center',
      headerClass: 'text-center',
      valueFormatter: (params) => {
        if (!params.value) return '—';
        return this.datePipe.transform(params.value, 'dd/MM/yyyy') || '';
      },
    },
    {
      field: 'ativo',
      headerName: 'Status',
      width: 90,
      minWidth: 80,
      sortable: true,
      cellClass: 'text-center',
      headerClass: 'text-center',
      cellRenderer: (params: { value: boolean }) => {
        if (params.value === true) {
          return '<span class="status-badge status-badge--active">Ativo</span>';
        } else {
          return '<span class="status-badge status-badge--inactive">Inativo</span>';
        }
      },
    },
    {
      field: 'ultimoValor',
      headerName: 'Último Valor',
      width: 130,
      minWidth: 110,
      sortable: false,
      cellClass: 'text-end',
      headerClass: 'text-end',
      valueFormatter: (params) => {
        if (params.value === null || params.value === undefined) return '—';
        return params.value.toFixed(6);
      },
    },
    {
      field: 'ultimaData',
      headerName: 'Última Data',
      width: 110,
      minWidth: 100,
      sortable: false,
      cellClass: 'text-center',
      headerClass: 'text-center',
      valueFormatter: (params) => {
        if (!params.value) return '—';
        return this.datePipe.transform(params.value, 'dd/MM/yyyy') || '';
      },
    },
    {
      field: 'importacaoAutomatica',
      headerName: 'Auto',
      width: 70,
      minWidth: 60,
      sortable: true,
      cellClass: 'text-center',
      headerClass: 'text-center',
      cellRenderer: (params: { value: boolean }) => {
        if (params.value === true) {
          return '<i class="bi bi-cloud-download text-success" title="Importacao automatica habilitada"></i>';
        }
        return '<i class="bi bi-dash text-muted" title="Importacao manual"></i>';
      },
    },
    {
      headerName: '',
      colId: 'actions',
      width: 100,
      minWidth: 100,
      maxWidth: 100,
      sortable: false,
      suppressMovable: true,
      lockPosition: 'right',
      cellClass: 'text-center',
      headerClass: 'text-center',
      cellRenderer: (params: { data: Indexador }) => {
        if (!params.data) return '';
        return `
          <div class="row-actions">
            <button class="row-action-btn row-action-btn--view" data-action="view" title="Ver detalhes">
              <i class="bi bi-eye"></i>
            </button>
            <button class="row-action-btn row-action-btn--edit" data-action="edit" title="Editar">
              <i class="bi bi-pencil"></i>
            </button>
            <button class="row-action-btn row-action-btn--cancel" data-action="delete" title="Excluir">
              <i class="bi bi-trash"></i>
            </button>
          </div>
        `;
      },
    },
  ];

  // Default column definitions
  readonly defaultColDef: ColDef = {
    resizable: true,
    minWidth: 50,
  };

  // Grid ready handler
  onGridReady(params: GridReadyEvent<Indexador>): void {
    this.gridApi = params.api;

    // Initialize search term from store
    this.searchTerm.set(this.store.searchTerm());

    // Apply stored sort state if available
    const sortColumn = this.store.sortColumn();
    const sortDirection = this.store.sortDirection();

    if (sortColumn) {
      params.api.applyColumnState({
        state: [{ colId: sortColumn, sort: sortDirection }],
      });
    }

    // Set the datasource
    params.api.setGridOption('datasource', this.createDatasource());
  }

  // Sort changed handler - sync with store
  onSortChanged(event: SortChangedEvent<Indexador>): void {
    const sortModel = event.api.getColumnState().filter((col) => col.sort);

    if (sortModel.length > 0) {
      const { colId, sort } = sortModel[0];
      if (colId && sort) {
        this.store.setSort(colId, sort as 'asc' | 'desc');
      }
    }
  }

  // Search handler
  onSearch(): void {
    this.store.setSearchTerm(this.searchTerm().trim());
    this.refreshGrid();
  }

  // Clear search
  onClearSearch(): void {
    this.searchTerm.set('');
    this.store.setSearchTerm('');
    this.refreshGrid();
  }

  // Filter handlers
  onTipoChange(tipo: TipoIndexador | null): void {
    this.store.setFilter('tipo', tipo);
    this.refreshGrid();
  }

  onPeriodicidadeChange(periodicidade: Periodicidade | null): void {
    this.store.setFilter('periodicidade', periodicidade);
    this.refreshGrid();
  }

  onFonteChange(fonte: string | null): void {
    this.store.setFilter('fonte', fonte);
    this.refreshGrid();
  }

  onAtivoChange(ativo: boolean | null): void {
    this.store.setFilter('ativo', ativo);
    this.refreshGrid();
  }

  onImportacaoAutoChange(importacaoAuto: boolean | null): void {
    this.store.setFilter('importacaoAutomatica', importacaoAuto);
    this.refreshGrid();
  }

  clearFilter(key: 'tipo' | 'periodicidade' | 'fonte' | 'ativo' | 'importacaoAutomatica'): void {
    this.store.clearFilter(key);
    this.refreshGrid();
  }

  clearAllFilters(): void {
    this.searchTerm.set('');
    this.store.clearAllFilters();
    this.refreshGrid();
  }

  // Preset handlers
  onPresetClick(presetId: IndexadorPresetId): void {
    this.store.togglePreset(presetId);
    // Sync UI state with store
    this.searchTerm.set('');
    this.refreshGrid();
  }

  isPresetActive(presetId: IndexadorPresetId): boolean {
    return this.activePreset() === presetId;
  }

  // Refresh grid data
  onRefresh(): void {
    this.refreshGrid();
  }

  // Handle action button clicks in row
  onCellClicked(event: CellClickedEvent<Indexador>): void {
    if (event.column.getColId() !== 'actions') return;

    const target = event.event?.target as HTMLElement;
    const actionBtn = target.closest('[data-action]') as HTMLElement;
    if (!actionBtn || !event.data) return;

    const action = actionBtn.dataset['action'];
    switch (action) {
      case 'view':
        this.onViewIndexador(event.data);
        break;
      case 'edit':
        this.onEditIndexador(event.data);
        break;
      case 'delete':
        this.onDeleteIndexador(event.data);
        break;
    }
  }

  // Row-level actions
  onViewIndexador(indexador: Indexador): void {
    this.router.navigate(['/cadastro/indexadores', indexador.id]);
  }

  onEditIndexador(indexador: Indexador): void {
    this.router.navigate(['/cadastro/indexadores', indexador.id, 'edit']);
  }

  onDeleteIndexador(indexador: Indexador): void {
    const modalRef = this.modal.open(DeleteIndexadorModal);
    modalRef.componentInstance.indexador.set(indexador);

    modalRef.result.then(
      (result) => {
        if (result === 'confirm') {
          this.indexadorService.deleteIndexador(indexador.id).subscribe({
            next: () => {
              this.toastService.success('Indexador removido com sucesso');
              this.onRefresh();
            },
            error: (err) => {
              const errorMessage =
                err?.error?.message ||
                err?.message ||
                'Falha ao remover indexador. Por favor, tente novamente.';
              this.logger.logHttpError('delete indexador', err, errorMessage, false);
              this.toastService.error(errorMessage);
            },
          });
        } else if (result === 'inactivate') {
          // Inactivate the indexador instead of deleting
          this.indexadorService
            .updateIndexador(indexador.id, {
              nome: indexador.nome,
              fonte: indexador.fonte,
              fatorAcumulado: indexador.fatorAcumulado,
              dataBase: indexador.dataBase,
              urlFonte: indexador.urlFonte,
              importacaoAutomatica: indexador.importacaoAutomatica,
              ativo: false,
            })
            .subscribe({
              next: () => {
                this.toastService.success('Indexador inativado com sucesso');
                this.onRefresh();
              },
              error: (err) => {
                const errorMessage =
                  err?.error?.message ||
                  err?.message ||
                  'Falha ao inativar indexador. Por favor, tente novamente.';
                this.logger.logHttpError('inactivate indexador', err, errorMessage, false);
                this.toastService.error(errorMessage);
              },
            });
        }
      },
      () => {
        // Modal dismissed
      }
    );
  }

  // Helper to get tipo color
  getTipoColor(tipo: TipoIndexador): string {
    return this.indexadorService.getTipoColor(tipo);
  }

  // Refresh grid with new datasource
  private refreshGrid(): void {
    if (this.gridApi) {
      this.gridApi.setGridOption('datasource', this.createDatasource());
    }
  }

  // Create datasource for infinite scrolling
  private createDatasource(): IDatasource {
    // Reset pagination stats when creating new datasource
    this.paginationStats.set({
      totalCount: 0,
      loadedCount: 0,
      currentRangeStart: 0,
      currentRangeEnd: 0,
      isLoading: true,
    });

    return createIndexadorDatasource(this.indexadorService, this.logger, {
      getFilterParams: () => this.store.filterParams(),
      onPaginationUpdate: (stats) => this.paginationStats.set(stats),
    });
  }
}
