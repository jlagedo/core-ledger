import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  Injectable,
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

import { Calendario, TipoDia, Praca } from '../../../models/calendario.model';
import { CalendarioService } from '../../../services/calendario';
import {
  CalendarioStore,
  CALENDARIO_PRESETS,
  CalendarioPresetId,
} from './calendario-store';
import { LoggerService } from '../../../services/logger';
import { PageHeader } from '../../../layout/page-header/page-header';
import { ThemeService } from '../../../services/theme-service';
import { ToastService } from '../../../services/toast-service';
import { createCalendarioDatasource, PaginationStats } from './calendario-datasource';
import { agGridDarkTheme, agGridLightTheme } from '../../../shared/themes/ag-grid-theme';
import {
  NgbDropdownModule,
  NgbModal,
  NgbDatepickerModule,
  NgbDateStruct,
  NgbCalendar,
  NgbDatepickerI18n,
} from '@ng-bootstrap/ng-bootstrap';
import { DeleteCalendarioModal } from './delete-modal/delete-modal';

/**
 * Portuguese (Brazil) i18n for NgbDatepicker
 */
@Injectable()
class NgbDatepickerI18nPortuguese extends NgbDatepickerI18n {
  private readonly weekdaysNarrow = ['D', 'S', 'T', 'Q', 'Q', 'S', 'S'];
  private readonly weekdaysShort = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];
  private readonly weekdaysLong = [
    'Domingo',
    'Segunda',
    'Terça',
    'Quarta',
    'Quinta',
    'Sexta',
    'Sábado',
  ];
  private readonly months = [
    'Janeiro',
    'Fevereiro',
    'Março',
    'Abril',
    'Maio',
    'Junho',
    'Julho',
    'Agosto',
    'Setembro',
    'Outubro',
    'Novembro',
    'Dezembro',
  ];
  private readonly monthsShort = [
    'Jan',
    'Fev',
    'Mar',
    'Abr',
    'Mai',
    'Jun',
    'Jul',
    'Ago',
    'Set',
    'Out',
    'Nov',
    'Dez',
  ];

  override getWeekdayLabel(
    weekday: number,
    width?: Exclude<Intl.DateTimeFormatOptions['weekday'], undefined>
  ): string {
    // weekday: 1=Monday, 7=Sunday (ISO 8601)
    // Convert to 0-indexed array where 0=Monday, 6=Sunday
    const index = weekday === 7 ? 0 : weekday;
    switch (width) {
      case 'narrow':
        return this.weekdaysNarrow[index];
      case 'long':
        return this.weekdaysLong[index];
      case 'short':
      default:
        return this.weekdaysShort[index];
    }
  }

  override getMonthShortName(month: number): string {
    return this.monthsShort[month - 1];
  }

  override getMonthFullName(month: number): string {
    return this.months[month - 1];
  }

  override getDayAriaLabel(date: NgbDateStruct): string {
    return `${date.day} de ${this.months[date.month - 1]} de ${date.year}`;
  }
}

@Component({
  selector: 'app-calendario-list',
  imports: [
    RouterLink,
    PageHeader,
    AgGridAngular,
    FormsModule,
    DecimalPipe,
    NgbDropdownModule,
    NgbDatepickerModule,
  ],
  providers: [
    CalendarioStore,
    DatePipe,
    { provide: NgbDatepickerI18n, useClass: NgbDatepickerI18nPortuguese },
  ],
  templateUrl: './calendario-list.html',
  styleUrl: './calendario-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CalendarioList {
  private readonly store = inject(CalendarioStore);
  private readonly calendarioService = inject(CalendarioService);
  private readonly logger = inject(LoggerService);
  private readonly datePipe = inject(DatePipe);
  private readonly router = inject(Router);
  private readonly themeService = inject(ThemeService);
  private readonly toastService = inject(ToastService);
  private readonly modal = inject(NgbModal);
  private readonly calendar = inject(NgbCalendar);

  private gridApi: GridApi<Calendario> | null = null;

  // Search term bound to input
  searchTerm = signal('');

  // Date range picker state using NgbDateStruct
  fromDate = signal<NgbDateStruct | null>(null);
  toDate = signal<NgbDateStruct | null>(null);
  hoveredDate = signal<NgbDateStruct | null>(null);

  // Expose enum options for template
  readonly tipoDiaOptions = this.calendarioService.tipoDiaOptions;
  readonly pracaOptions = this.calendarioService.pracaOptions;
  readonly diaUtilOptions = [
    { value: true, name: 'Sim' },
    { value: false, name: 'Não' },
  ];

  // Store state exposed to template
  readonly filters = this.store.filters;
  readonly hasActiveFilters = this.store.hasActiveFilters;
  readonly activeFilterCount = this.store.activeFilterCount;
  readonly activePreset = this.store.activePreset;

  // Presets configuration
  readonly presets = CALENDARIO_PRESETS;

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
  readonly selectedPracaLabel = computed(() => {
    const praca = this.filters().praca;
    if (praca === null) return null;
    return this.pracaOptions.find((o) => o.value === praca)?.name || null;
  });

  readonly selectedTipoDiaLabel = computed(() => {
    const tipoDia = this.filters().tipoDia;
    if (tipoDia === null) return null;
    return this.tipoDiaOptions.find((o) => o.value === tipoDia)?.name || null;
  });

  readonly selectedDiaUtilLabel = computed(() => {
    const diaUtil = this.filters().diaUtil;
    if (diaUtil === null) return null;
    return diaUtil ? 'Sim' : 'Não';
  });

  readonly selectedDateRangeLabel = computed(() => {
    const f = this.filters();
    if (!f.dataInicio && !f.dataFim) return null;
    if (f.dataInicio && f.dataFim) {
      return `${this.formatDateShort(f.dataInicio)} - ${this.formatDateShort(f.dataFim)}`;
    }
    if (f.dataInicio) return `A partir de ${this.formatDateShort(f.dataInicio)}`;
    return `Até ${this.formatDateShort(f.dataFim!)}`;
  });

  // Row ID for selection persistence
  readonly getRowId: GetRowIdFunc<Calendario> = (params: GetRowIdParams<Calendario>) =>
    String(params.data.id);

  // Column definitions - filters removed
  readonly columnDefs: ColDef<Calendario>[] = [
    {
      field: 'data',
      headerName: 'Data',
      width: 110,
      minWidth: 100,
      maxWidth: 130,
      sortable: true,
      cellClass: 'text-center',
      headerClass: 'text-center',
      valueFormatter: (params) => this.datePipe.transform(params.value, 'dd/MM/yyyy') || '',
    },
    {
      field: 'praca',
      headerName: 'Praça',
      width: 130,
      minWidth: 110,
      sortable: true,
      valueFormatter: (params) => {
        const option = this.pracaOptions.find((opt) => opt.value === params.value);
        return option?.name || '';
      },
    },
    {
      field: 'tipoDia',
      headerName: 'Tipo de Dia',
      width: 150,
      minWidth: 130,
      sortable: true,
      valueFormatter: (params) => {
        const option = this.tipoDiaOptions.find((opt) => opt.value === params.value);
        return option?.name || '';
      },
    },
    {
      field: 'diaUtil',
      headerName: 'Dia Útil',
      width: 110,
      minWidth: 110,
      maxWidth: 130,
      sortable: true,
      cellClass: 'text-center',
      headerClass: 'text-center',
      cellRenderer: (params: { value: boolean }) => {
        if (params.value === true) {
          return '<span class="status-badge status-badge--active">Sim</span>';
        } else {
          return '<span class="status-badge status-badge--inactive">Não</span>';
        }
      },
    },
    {
      field: 'descricao',
      headerName: 'Descrição',
      minWidth: 180,
      flex: 1,
      sortable: false,
      valueFormatter: (params) => params.value || '—',
    },
    {
      headerName: '',
      colId: 'actions',
      width: 80,
      minWidth: 80,
      maxWidth: 80,
      sortable: false,
      suppressMovable: true,
      lockPosition: 'right',
      cellClass: 'text-center',
      headerClass: 'text-center',
      cellRenderer: (params: { data: Calendario }) => {
        if (!params.data) return '';
        return `
          <div class="row-actions">
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
  onGridReady(params: GridReadyEvent<Calendario>): void {
    this.gridApi = params.api;

    // Initialize search term from store
    this.searchTerm.set(this.store.searchTerm());

    // Initialize date picker from store
    const f = this.store.filters();
    this.fromDate.set(this.isoStringToNgbDate(f.dataInicio));
    this.toDate.set(this.isoStringToNgbDate(f.dataFim));

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
  onSortChanged(event: SortChangedEvent<Calendario>): void {
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
  onPracaChange(praca: Praca | null): void {
    this.store.setFilter('praca', praca);
    this.refreshGrid();
  }

  onTipoDiaChange(tipoDia: TipoDia | null): void {
    this.store.setFilter('tipoDia', tipoDia);
    this.refreshGrid();
  }

  onDiaUtilChange(diaUtil: boolean | null): void {
    this.store.setFilter('diaUtil', diaUtil);
    this.refreshGrid();
  }

  // Date picker range selection handler
  onDateSelection(date: NgbDateStruct): void {
    const from = this.fromDate();
    const to = this.toDate();

    if (!from && !to) {
      // First click: set fromDate
      this.fromDate.set(date);
    } else if (from && !to && this.isAfterOrEqual(date, from)) {
      // Second click: set toDate if it's after fromDate
      this.toDate.set(date);
    } else {
      // Reset and start new selection
      this.toDate.set(null);
      this.fromDate.set(date);
    }
  }

  onDateRangeApply(): void {
    this.store.setFilters({
      dataInicio: this.ngbDateToIsoString(this.fromDate()),
      dataFim: this.ngbDateToIsoString(this.toDate()),
    });
    this.refreshGrid();
  }

  onClearDateRange(): void {
    this.fromDate.set(null);
    this.toDate.set(null);
    this.hoveredDate.set(null);
    this.store.setFilters({
      dataInicio: null,
      dataFim: null,
    });
    this.refreshGrid();
  }

  // Date range highlighting helpers for template
  isHovered(date: NgbDateStruct): boolean {
    const from = this.fromDate();
    const hovered = this.hoveredDate();
    return (
      from !== null &&
      this.toDate() === null &&
      hovered !== null &&
      this.isAfter(date, from) &&
      this.isBeforeOrEqual(date, hovered)
    );
  }

  isInside(date: NgbDateStruct): boolean {
    const from = this.fromDate();
    const to = this.toDate();
    return from !== null && to !== null && this.isAfter(date, from) && this.isBefore(date, to);
  }

  isFrom(date: NgbDateStruct): boolean {
    return this.equals(date, this.fromDate());
  }

  isTo(date: NgbDateStruct): boolean {
    return this.equals(date, this.toDate());
  }

  clearFilter(key: 'praca' | 'tipoDia' | 'diaUtil' | 'dateRange'): void {
    if (key === 'dateRange') {
      this.onClearDateRange();
    } else {
      this.store.clearFilter(key);
      this.refreshGrid();
    }
  }

  clearAllFilters(): void {
    this.searchTerm.set('');
    this.fromDate.set(null);
    this.toDate.set(null);
    this.hoveredDate.set(null);
    this.store.clearAllFilters();
    this.refreshGrid();
  }

  // Preset handlers
  onPresetClick(presetId: CalendarioPresetId): void {
    this.store.togglePreset(presetId);
    // Sync UI state with store
    this.searchTerm.set('');
    this.fromDate.set(this.isoStringToNgbDate(this.store.filters().dataInicio));
    this.toDate.set(this.isoStringToNgbDate(this.store.filters().dataFim));
    this.refreshGrid();
  }

  isPresetActive(presetId: CalendarioPresetId): boolean {
    return this.activePreset() === presetId;
  }

  // Refresh grid data
  onRefresh(): void {
    this.refreshGrid();
  }

  // Handle action button clicks in row
  onCellClicked(event: CellClickedEvent<Calendario>): void {
    if (event.column.getColId() !== 'actions') return;

    const target = event.event?.target as HTMLElement;
    const actionBtn = target.closest('[data-action]') as HTMLElement;
    if (!actionBtn || !event.data) return;

    const action = actionBtn.dataset['action'];
    switch (action) {
      case 'edit':
        this.onEditCalendario(event.data);
        break;
      case 'delete':
        this.onDeleteCalendario(event.data);
        break;
    }
  }

  // Row-level actions
  onEditCalendario(calendario: Calendario): void {
    this.router.navigate(['/cadastro/calendario', calendario.id, 'edit']);
  }

  onDeleteCalendario(calendario: Calendario): void {
    const modalRef = this.modal.open(DeleteCalendarioModal);
    modalRef.componentInstance.calendario.set(calendario);

    modalRef.result.then(
      (result) => {
        if (result === 'confirm') {
          this.calendarioService.deleteCalendario(calendario.id).subscribe({
            next: () => {
              this.toastService.success('Calendário removido com sucesso');
              this.onRefresh();
            },
            error: (err) => {
              const errorMessage =
                err?.error?.message ||
                err?.message ||
                'Falha ao remover calendário. Por favor, tente novamente.';
              this.logger.logHttpError('delete calendario', err, errorMessage, false);
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

  // Helper to format date for display
  private formatDateShort(dateStr: string): string {
    return this.datePipe.transform(dateStr, 'dd/MM/yy') || dateStr;
  }

  // NgbDateStruct to ISO string conversion
  private ngbDateToIsoString(date: NgbDateStruct | null): string | null {
    if (!date) return null;
    const month = date.month.toString().padStart(2, '0');
    const day = date.day.toString().padStart(2, '0');
    return `${date.year}-${month}-${day}`;
  }

  // ISO string to NgbDateStruct conversion
  private isoStringToNgbDate(dateStr: string | null): NgbDateStruct | null {
    if (!dateStr) return null;
    const parts = dateStr.split('-');
    if (parts.length !== 3) return null;
    return {
      year: parseInt(parts[0], 10),
      month: parseInt(parts[1], 10),
      day: parseInt(parts[2], 10),
    };
  }

  // Date comparison helpers
  private equals(d1: NgbDateStruct | null, d2: NgbDateStruct | null): boolean {
    if (!d1 || !d2) return false;
    return d1.year === d2.year && d1.month === d2.month && d1.day === d2.day;
  }

  private isBefore(d1: NgbDateStruct, d2: NgbDateStruct): boolean {
    if (d1.year !== d2.year) return d1.year < d2.year;
    if (d1.month !== d2.month) return d1.month < d2.month;
    return d1.day < d2.day;
  }

  private isAfter(d1: NgbDateStruct, d2: NgbDateStruct): boolean {
    if (d1.year !== d2.year) return d1.year > d2.year;
    if (d1.month !== d2.month) return d1.month > d2.month;
    return d1.day > d2.day;
  }

  private isBeforeOrEqual(d1: NgbDateStruct, d2: NgbDateStruct): boolean {
    return this.isBefore(d1, d2) || this.equals(d1, d2);
  }

  private isAfterOrEqual(d1: NgbDateStruct, d2: NgbDateStruct): boolean {
    return this.isAfter(d1, d2) || this.equals(d1, d2);
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

    return createCalendarioDatasource(this.calendarioService, this.logger, {
      getFilterParams: () => this.store.filterParams(),
      onPaginationUpdate: (stats) => this.paginationStats.set(stats),
    });
  }
}
