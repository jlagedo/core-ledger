import { ChangeDetectionStrategy, Component, inject, effect, signal, computed } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { map } from 'rxjs/operators';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbNavModule, NgbModal, NgbDatepickerModule, NgbDateStruct } from '@ng-bootstrap/ng-bootstrap';
import { AgGridAngular } from 'ag-grid-angular';
import { ColDef, GridApi, GridReadyEvent, IDatasource, GridOptions } from 'ag-grid-community';
import { IndexadorService } from '../../../../services/indexador';
import { LoggerService } from '../../../../services/logger';
import { ToastService } from '../../../../services/toast-service';
import { Indexador, HistoricoIndexador } from '../../../../models/indexador.model';
import { PageHeader } from '../../../../layout/page-header/page-header';
import { createHistoricoDatasource, HistoricoPaginationStats } from '../historico-datasource';
import { AddValueModal } from '../history/add-value-modal';
import { DeleteValueModal } from '../history/delete-value-modal';
import { ImportModal } from '../history/import-modal';
import { ChartTab } from './chart-tab';

@Component({
  selector: 'app-indexador-detail',
  imports: [
    RouterLink,
    PageHeader,
    DatePipe,
    DecimalPipe,
    FormsModule,
    NgbNavModule,
    NgbDatepickerModule,
    AgGridAngular,
    ChartTab,
  ],
  providers: [DatePipe],
  templateUrl: './indexador-detail.html',
  styleUrl: './indexador-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IndexadorDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  // Signal-based route parameter (replaces ngOnInit snapshot)
  private readonly paramId = toSignal(this.route.paramMap.pipe(map((params) => params.get('id'))));

  private readonly indexadorId = computed(() => {
    const id = this.paramId();
    return id ? +id : null;
  });
  private readonly indexadorService = inject(IndexadorService);
  private readonly loggerService = inject(LoggerService);
  private readonly toastService = inject(ToastService);
  private readonly modalService = inject(NgbModal);
  private readonly datePipe = inject(DatePipe);

  readonly indexador = signal<Indexador | null>(null);
  readonly isLoading = signal(true);
  readonly activeTab = signal<'historico' | 'grafico' | 'configuracao'>('historico');

  // History grid state
  private gridApi: GridApi | null = null;
  private datasource: IDatasource | null = null;
  readonly paginationStats = signal<HistoricoPaginationStats>({
    totalCount: 0,
    loadedCount: 0,
    currentRangeStart: 0,
    currentRangeEnd: 0,
    isLoading: false,
  });

  // Date range filters
  readonly dataInicioFilter = signal<NgbDateStruct | null>(null);
  readonly dataFimFilter = signal<NgbDateStruct | null>(null);

  // Computed for stats display
  readonly statsDisplay = computed(() => {
    const stats = this.paginationStats();
    if (stats.isLoading) {
      return 'Carregando...';
    }
    if (stats.totalCount === 0) {
      return 'Nenhum registro encontrado';
    }
    return `Exibindo ${stats.currentRangeStart} - ${stats.currentRangeEnd} de ${stats.totalCount}`;
  });

  // AG Grid column definitions for history
  readonly columnDefs: ColDef[] = [
    {
      field: 'dataReferencia',
      headerName: 'Data Ref.',
      sort: 'desc',
      sortable: true,
      width: 120,
      valueFormatter: (params) => {
        if (!params.value) return '—';
        return this.datePipe.transform(params.value, 'dd/MM/yyyy') || params.value;
      },
      cellClass: 'font-monospace',
    },
    {
      field: 'valor',
      headerName: 'Valor',
      sortable: true,
      width: 140,
      valueFormatter: (params) => {
        if (params.value === null || params.value === undefined) return '—';
        return new Intl.NumberFormat('pt-BR', {
          minimumFractionDigits: 2,
          maximumFractionDigits: 8,
        }).format(params.value);
      },
      cellClass: 'font-monospace text-end',
    },
    {
      field: 'fatorDiario',
      headerName: 'Fator Diario',
      sortable: true,
      width: 160,
      valueFormatter: (params) => {
        if (params.value === null || params.value === undefined) return '—';
        return new Intl.NumberFormat('pt-BR', {
          minimumFractionDigits: 9,
          maximumFractionDigits: 12,
        }).format(params.value);
      },
      cellClass: 'font-monospace text-end',
    },
    {
      field: 'variacaoPercentual',
      headerName: 'Var. %',
      sortable: true,
      width: 100,
      valueFormatter: (params) => {
        if (params.value === null || params.value === undefined) return '—';
        return new Intl.NumberFormat('pt-BR', {
          minimumFractionDigits: 2,
          maximumFractionDigits: 4,
          signDisplay: 'exceptZero',
        }).format(params.value) + '%';
      },
      cellClass: (params) => {
        const classes = ['font-monospace', 'text-end'];
        if (params.value > 0) classes.push('text-success');
        else if (params.value < 0) classes.push('text-danger');
        return classes;
      },
    },
    {
      field: 'fonte',
      headerName: 'Fonte',
      sortable: true,
      width: 100,
      valueFormatter: (params) => params.value || '—',
    },
    {
      field: 'createdAt',
      headerName: 'Criado em',
      sortable: true,
      width: 140,
      valueFormatter: (params) => {
        if (!params.value) return '—';
        return this.datePipe.transform(params.value, 'dd/MM/yyyy HH:mm') || params.value;
      },
      cellClass: 'text-muted small',
    },
    {
      headerName: '',
      field: 'actions',
      sortable: false,
      width: 60,
      cellRenderer: () => {
        return `<button class="btn btn-sm btn-outline-danger" title="Excluir">
          <i class="bi bi-trash"></i>
        </button>`;
      },
      onCellClicked: (event) => {
        if (event.data) {
          this.openDeleteValueModal(event.data);
        }
      },
    },
  ];

  readonly gridOptions: GridOptions = {
    rowModelType: 'infinite',
    cacheBlockSize: 50,
    paginationPageSize: 50,
    cacheOverflowSize: 2,
    maxConcurrentDatasourceRequests: 1,
    infiniteInitialRowCount: 1,
    maxBlocksInCache: 10,
    rowHeight: 40,
    headerHeight: 44,
    rowSelection: { mode: 'singleRow', enableClickSelection: false },
    animateRows: false,
    defaultColDef: {
      resizable: true,
      suppressMovable: true,
    },
  };

  constructor() {
    // React to route parameter changes
    effect(() => {
      const id = this.indexadorId();
      if (id) {
        this.loadIndexador(id);
      } else {
        this.router.navigate(['/cadastro/indexadores']);
      }
    });
  }

  private loadIndexador(id: number): void {
    this.isLoading.set(true);
    this.indexadorService.getIndexadorById(id).subscribe({
      next: (indexador) => {
        this.indexador.set(indexador);
        this.isLoading.set(false);
        // Initialize grid datasource after indexador is loaded
        if (this.gridApi) {
          this.initDatasource();
        }
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.error('Falha ao carregar indexador');
        this.router.navigate(['/cadastro/indexadores']);
      },
    });
  }

  onGridReady(event: GridReadyEvent): void {
    this.gridApi = event.api;
    // Initialize datasource if indexador is already loaded
    const idx = this.indexador();
    if (idx) {
      this.initDatasource();
    }
  }

  private initDatasource(): void {
    const idx = this.indexador();
    if (!idx || !this.gridApi) return;

    this.datasource = createHistoricoDatasource(this.indexadorService, this.loggerService, {
      indexadorId: idx.id,
      getDateRange: () => ({
        dataInicio: this.formatDateForApi(this.dataInicioFilter()),
        dataFim: this.formatDateForApi(this.dataFimFilter()),
      }),
      onPaginationUpdate: (stats) => this.paginationStats.set(stats),
    });

    this.gridApi.setGridOption('datasource', this.datasource);
  }

  private formatDateForApi(date: NgbDateStruct | null): string | undefined {
    if (!date) return undefined;
    const year = date.year;
    const month = String(date.month).padStart(2, '0');
    const day = String(date.day).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  onDateFilterChange(): void {
    // Refresh grid with new date filters
    this.gridApi?.purgeInfiniteCache();
  }

  clearDateFilters(): void {
    this.dataInicioFilter.set(null);
    this.dataFimFilter.set(null);
    this.gridApi?.purgeInfiniteCache();
  }

  refreshHistory(): void {
    this.gridApi?.purgeInfiniteCache();
  }

  // Modal methods
  openAddValueModal(): void {
    const idx = this.indexador();
    if (!idx) return;

    const modalRef = this.modalService.open(AddValueModal, {
      centered: true,
      backdrop: 'static',
    });

    modalRef.componentInstance.indexador.set(idx);

    modalRef.result.then(
      (result) => {
        if (result === 'created') {
          this.toastService.success('Valor adicionado com sucesso');
          this.refreshHistory();
        }
      },
      () => {
        // Modal dismissed
      }
    );
  }

  openDeleteValueModal(historico: HistoricoIndexador): void {
    const modalRef = this.modalService.open(DeleteValueModal, {
      centered: true,
      backdrop: 'static',
    });

    modalRef.componentInstance.historico.set(historico);

    modalRef.result.then(
      (result) => {
        if (result === 'deleted') {
          this.toastService.success('Valor excluido com sucesso');
          this.refreshHistory();
        }
      },
      () => {
        // Modal dismissed
      }
    );
  }

  exportCsv(): void {
    const idx = this.indexador();
    if (!idx) return;

    const dataInicio = this.formatDateForApi(this.dataInicioFilter());
    const dataFim = this.formatDateForApi(this.dataFimFilter());

    this.indexadorService.exportHistorico(idx.id, dataInicio, dataFim).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `historico_${idx.codigo}_${new Date().toISOString().split('T')[0]}.csv`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.toastService.success('Arquivo CSV exportado');
      },
      error: () => {
        this.toastService.error('Falha ao exportar CSV');
      },
    });
  }

  openImportModal(): void {
    const idx = this.indexador();
    if (!idx) return;

    const modalRef = this.modalService.open(ImportModal, {
      centered: true,
      backdrop: 'static',
      size: 'lg',
    });

    modalRef.componentInstance.indexador.set(idx);

    modalRef.result.then(
      (result) => {
        if (result === 'imported') {
          this.toastService.success('Valores importados com sucesso');
          this.refreshHistory();
        }
      },
      () => {
        // Modal dismissed
      }
    );
  }

  // Helper methods for template
  getTipoColor(indexador: Indexador): string {
    return this.indexadorService.getTipoColor(indexador.tipo);
  }

  getTipoIcon(indexador: Indexador): string {
    return this.indexadorService.getTipoIcon(indexador.tipo);
  }
}
