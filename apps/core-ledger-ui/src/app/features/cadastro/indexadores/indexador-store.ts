import { patchState, signalStore, withComputed, withHooks, withMethods, withState } from '@ngrx/signals';
import { computed, inject } from '@angular/core';
import { LoggerService } from '../../../services/logger';
import { SessionStorageService } from '../../../shared/storage/session-storage.service';
import { LocalStorageService } from '../../../shared/storage/local-storage.service';
import { StorageService } from '../../../shared/storage/storage.interface';
import { TipoIndexador, Periodicidade } from '../../../models/indexador.model';

/**
 * Filter model for indexador quick filters
 */
export interface IndexadorFilters {
  tipo: TipoIndexador | null;
  periodicidade: Periodicidade | null;
  fonte: string | null;
  ativo: boolean | null;
  importacaoAutomatica: boolean | null;
}

/**
 * Preset identifiers for quick filter configurations
 */
export type IndexadorPresetId =
  | 'taxas-juros'
  | 'inflacao'
  | 'indices-bolsa'
  | 'cambio'
  | 'renda-fixa'
  | 'crypto'
  | 'diarios-ativos'
  | 'importacao-auto'
  | 'todos-ativos'
  | 'inativos'
  | null;

/**
 * Preset configuration with display info and filter values
 */
export interface IndexadorPreset {
  id: IndexadorPresetId;
  label: string;
  shortLabel: string;
  icon: string;
  description: string;
  getFilters: () => Partial<IndexadorFilters>;
}

/**
 * Available preset configurations
 */
export const INDEXADOR_PRESETS: IndexadorPreset[] = [
  {
    id: 'taxas-juros',
    label: 'Taxas de Juros',
    shortLabel: 'Juros',
    icon: 'bi-percent',
    description: 'CDI, SELIC e outras taxas de juros',
    getFilters: () => ({
      tipo: TipoIndexador.Juros,
      ativo: true,
    }),
  },
  {
    id: 'inflacao',
    label: 'Inflacao',
    shortLabel: 'Inflacao',
    icon: 'bi-graph-up-arrow',
    description: 'IPCA, IGP-M, INPC e outros indices de inflacao',
    getFilters: () => ({
      tipo: TipoIndexador.Inflacao,
      ativo: true,
    }),
  },
  {
    id: 'indices-bolsa',
    label: 'Indices de Bolsa',
    shortLabel: 'Bolsa',
    icon: 'bi-bar-chart-line',
    description: 'IBOVESPA, IBRX100 e outros indices de bolsa',
    getFilters: () => ({
      tipo: TipoIndexador.IndiceBolsa,
      ativo: true,
    }),
  },
  {
    id: 'cambio',
    label: 'Cambio',
    shortLabel: 'Cambio',
    icon: 'bi-currency-exchange',
    description: 'PTAX e outras taxas de cambio',
    getFilters: () => ({
      tipo: TipoIndexador.Cambio,
      ativo: true,
    }),
  },
  {
    id: 'renda-fixa',
    label: 'Indices Renda Fixa',
    shortLabel: 'Renda Fixa',
    icon: 'bi-graph-down',
    description: 'IMA-B, IMA-S e outros indices de renda fixa',
    getFilters: () => ({
      tipo: TipoIndexador.IndiceRendaFixa,
      ativo: true,
    }),
  },
  {
    id: 'crypto',
    label: 'Criptomoedas',
    shortLabel: 'Crypto',
    icon: 'bi-currency-bitcoin',
    description: 'BTC, ETH e outras criptomoedas',
    getFilters: () => ({
      tipo: TipoIndexador.Crypto,
      ativo: true,
    }),
  },
  {
    id: 'diarios-ativos',
    label: 'Diarios Ativos',
    shortLabel: 'Diarios',
    icon: 'bi-calendar-day',
    description: 'Indexadores com periodicidade diaria',
    getFilters: () => ({
      periodicidade: Periodicidade.Diaria,
      ativo: true,
    }),
  },
  {
    id: 'importacao-auto',
    label: 'Importacao Automatica',
    shortLabel: 'Auto',
    icon: 'bi-cloud-download',
    description: 'Indexadores com importacao automatica habilitada',
    getFilters: () => ({
      importacaoAutomatica: true,
      ativo: true,
    }),
  },
  {
    id: 'todos-ativos',
    label: 'Todos Ativos',
    shortLabel: 'Ativos',
    icon: 'bi-check-circle',
    description: 'Todos os indexadores ativos',
    getFilters: () => ({
      ativo: true,
    }),
  },
  {
    id: 'inativos',
    label: 'Inativos',
    shortLabel: 'Inativos',
    icon: 'bi-x-circle',
    description: 'Indexadores inativos',
    getFilters: () => ({
      ativo: false,
    }),
  },
];

export interface IndexadorSearchState {
  searchTerm: string;
  filters: IndexadorFilters;
  activePreset: IndexadorPresetId;
  sortColumn: string;
  sortDirection: 'asc' | 'desc';
}

const STORAGE_KEY = 'indexador-search-state';
const PRESET_STORAGE_KEY = 'indexador-preset';
const DEFAULT_PRESET: IndexadorPresetId = 'todos-ativos';

const initialFilters: IndexadorFilters = {
  tipo: null,
  periodicidade: null,
  fonte: null,
  ativo: null,
  importacaoAutomatica: null,
};

const initialState: IndexadorSearchState = {
  searchTerm: '',
  filters: initialFilters,
  activePreset: null,
  sortColumn: 'codigo',
  sortDirection: 'asc',
};

/**
 * Load preset selection from localStorage (persists across sessions)
 */
function loadPresetFromLocalStorage(
  logger: LoggerService,
  localStorage: StorageService
): IndexadorPresetId {
  try {
    const stored = localStorage.getItem(PRESET_STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored) as IndexadorPresetId;
      // Validate that preset still exists
      if (parsed === null || INDEXADOR_PRESETS.some((p) => p.id === parsed)) {
        logger.debug('Loaded indexador preset from localStorage', parsed, PRESET_STORAGE_KEY);
        return parsed;
      }
    }
  } catch (error) {
    logger.warn('Failed to load indexador preset from localStorage', error, PRESET_STORAGE_KEY);
  }
  // Return default preset for first-time users
  return DEFAULT_PRESET;
}

/**
 * Save preset selection to localStorage
 */
function savePresetToLocalStorage(
  presetId: IndexadorPresetId,
  logger: LoggerService,
  localStorage: StorageService
): void {
  try {
    localStorage.setItem(PRESET_STORAGE_KEY, JSON.stringify(presetId));
    logger.debug('Saved indexador preset to localStorage', presetId, PRESET_STORAGE_KEY);
  } catch (error) {
    logger.warn('Failed to save indexador preset to localStorage', error, PRESET_STORAGE_KEY);
  }
}

/**
 * Load filter state from sessionStorage (per-session state)
 */
function loadStateFromSession(
  logger: LoggerService,
  storage: StorageService
): Omit<IndexadorSearchState, 'activePreset'> {
  try {
    const stored = storage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored);
      const loadedState = {
        searchTerm: parsed.searchTerm ?? '',
        filters: { ...initialFilters, ...parsed.filters },
        sortColumn: parsed.sortColumn ?? 'codigo',
        sortDirection: parsed.sortDirection ?? 'asc',
      };
      logger.debug('Loaded indexador state from sessionStorage', loadedState, STORAGE_KEY);
      return loadedState;
    }
  } catch (error) {
    logger.warn('Failed to load indexador state from sessionStorage', error, STORAGE_KEY);
  }
  return {
    searchTerm: '',
    filters: initialFilters,
    sortColumn: 'codigo',
    sortDirection: 'asc',
  };
}

/**
 * Save filter state to sessionStorage (excludes preset which goes to localStorage)
 */
function saveStateToSession(
  state: IndexadorSearchState,
  logger: LoggerService,
  storage: StorageService
): void {
  try {
    const toSave = {
      searchTerm: state.searchTerm,
      filters: state.filters,
      sortColumn: state.sortColumn,
      sortDirection: state.sortDirection,
    };
    storage.setItem(STORAGE_KEY, JSON.stringify(toSave));
    logger.debug('Saved indexador state to sessionStorage', toSave, STORAGE_KEY);
  } catch (error) {
    logger.warn('Failed to save indexador state to sessionStorage', error, STORAGE_KEY);
  }
}

/**
 * Get filters for a preset by ID
 */
function getPresetFilters(presetId: IndexadorPresetId): IndexadorFilters {
  if (!presetId) return initialFilters;
  const preset = INDEXADOR_PRESETS.find((p) => p.id === presetId);
  if (!preset) return initialFilters;
  return { ...initialFilters, ...preset.getFilters() };
}

export const IndexadorStore = signalStore(
  withState(initialState),
  withComputed((store) => ({
    /**
     * Check if any filter is active
     */
    hasActiveFilters: computed(() => {
      const f = store.filters();
      return (
        f.tipo !== null ||
        f.periodicidade !== null ||
        f.fonte !== null ||
        f.ativo !== null ||
        f.importacaoAutomatica !== null ||
        store.searchTerm().trim() !== ''
      );
    }),

    /**
     * Count of active filters (excluding search term)
     */
    activeFilterCount: computed(() => {
      const f = store.filters();
      let count = 0;
      if (f.tipo !== null) count++;
      if (f.periodicidade !== null) count++;
      if (f.fonte !== null) count++;
      if (f.ativo !== null) count++;
      if (f.importacaoAutomatica !== null) count++;
      return count;
    }),

    /**
     * Build filter params for API call
     */
    filterParams: computed(() => {
      const params: Record<string, string> = {};
      const f = store.filters();
      const search = store.searchTerm().trim();

      if (search) {
        params['filter'] = search;
      }
      if (f.tipo !== null) {
        params['tipo'] = f.tipo.toString();
      }
      if (f.periodicidade !== null) {
        params['periodicidade'] = f.periodicidade.toString();
      }
      if (f.fonte !== null) {
        params['fonte'] = f.fonte;
      }
      if (f.ativo !== null) {
        params['ativo'] = f.ativo.toString();
      }
      if (f.importacaoAutomatica !== null) {
        params['importacaoAutomatica'] = f.importacaoAutomatica.toString();
      }

      return params;
    }),
  })),
  withMethods((store) => {
    const logger = inject(LoggerService);
    const sessionStorage = inject(SessionStorageService);
    const localStorage = inject(LocalStorageService);

    const saveCurrentState = () => {
      const state: IndexadorSearchState = {
        searchTerm: store.searchTerm(),
        filters: store.filters(),
        activePreset: store.activePreset(),
        sortColumn: store.sortColumn(),
        sortDirection: store.sortDirection(),
      };
      saveStateToSession(state, logger, sessionStorage);
    };

    const savePreset = (presetId: IndexadorPresetId) => {
      savePresetToLocalStorage(presetId, logger, localStorage);
    };

    return {
      setSearchTerm(searchTerm: string) {
        // Clear preset when user manually searches
        patchState(store, { searchTerm, activePreset: null });
        saveCurrentState();
        savePreset(null);
      },

      setSort(sortColumn: string, sortDirection: 'asc' | 'desc') {
        patchState(store, { sortColumn, sortDirection });
        saveCurrentState();
      },

      setFilter<K extends keyof IndexadorFilters>(key: K, value: IndexadorFilters[K]) {
        // Clear preset when user manually changes filters
        patchState(store, {
          filters: { ...store.filters(), [key]: value },
          activePreset: null,
        });
        saveCurrentState();
        savePreset(null);
      },

      setFilters(filters: Partial<IndexadorFilters>) {
        // Clear preset when user manually changes filters
        patchState(store, {
          filters: { ...store.filters(), ...filters },
          activePreset: null,
        });
        saveCurrentState();
        savePreset(null);
      },

      clearFilter<K extends keyof IndexadorFilters>(key: K) {
        patchState(store, {
          filters: { ...store.filters(), [key]: null },
          activePreset: null,
        });
        saveCurrentState();
        savePreset(null);
      },

      clearAllFilters() {
        patchState(store, {
          searchTerm: '',
          filters: initialFilters,
          activePreset: null,
        });
        saveCurrentState();
        savePreset(null);
      },

      resetSort() {
        patchState(store, {
          sortColumn: 'codigo',
          sortDirection: 'asc',
        });
        saveCurrentState();
      },

      /**
       * Apply a preset - sets filters and saves to localStorage
       */
      applyPreset(presetId: IndexadorPresetId) {
        if (!presetId) {
          // Clear preset
          patchState(store, {
            searchTerm: '',
            filters: initialFilters,
            activePreset: null,
          });
        } else {
          const filters = getPresetFilters(presetId);
          patchState(store, {
            searchTerm: '',
            filters,
            activePreset: presetId,
          });
        }
        saveCurrentState();
        savePreset(presetId);
      },

      /**
       * Toggle a preset - if already active, clear it; otherwise apply it
       */
      togglePreset(presetId: IndexadorPresetId) {
        if (store.activePreset() === presetId) {
          // Deactivate preset
          patchState(store, {
            searchTerm: '',
            filters: initialFilters,
            activePreset: null,
          });
          savePreset(null);
        } else {
          // Activate preset
          const filters = getPresetFilters(presetId);
          patchState(store, {
            searchTerm: '',
            filters,
            activePreset: presetId,
          });
          savePreset(presetId);
        }
        saveCurrentState();
      },

      /**
       * Initialize store - called from onInit hook
       */
      _initializeFromStorage() {
        // Load preset from localStorage (persists across sessions)
        const savedPreset = loadPresetFromLocalStorage(logger, localStorage);

        // Load session state (per-session filters/sort)
        const sessionState = loadStateFromSession(logger, sessionStorage);

        // If there's a saved preset, apply its filters
        // Otherwise use session state filters
        if (savedPreset) {
          const presetFilters = getPresetFilters(savedPreset);
          patchState(store, {
            ...sessionState,
            filters: presetFilters,
            activePreset: savedPreset,
          });
        } else {
          patchState(store, {
            ...sessionState,
            activePreset: null,
          });
        }
      },
    };
  }),
  withHooks({
    onInit(store) {
      store._initializeFromStorage();
    },
  })
);
