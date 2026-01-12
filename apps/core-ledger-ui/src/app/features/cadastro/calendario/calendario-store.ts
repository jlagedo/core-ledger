import { patchState, signalStore, withComputed, withHooks, withMethods, withState } from '@ngrx/signals';
import { computed, inject } from '@angular/core';
import { LoggerService } from '../../../services/logger';
import { SessionStorageService } from '../../../shared/storage/session-storage.service';
import { LocalStorageService } from '../../../shared/storage/local-storage.service';
import { StorageService } from '../../../shared/storage/storage.interface';
import { TipoDia, Praca } from '../../../models/calendario.model';

/**
 * Filter model for calendario quick filters
 */
export interface CalendarioFilters {
  praca: Praca | null;
  tipoDia: TipoDia | null;
  diaUtil: boolean | null;
  dataInicio: string | null; // ISO date string
  dataFim: string | null; // ISO date string
}

/**
 * Preset identifiers for quick filter configurations
 */
export type CalendarioPresetId =
  | 'feriados-nacionais'
  | 'feriados-eua'
  | 'feriados-europa'
  | 'feriados-nacionais-30d'
  | 'feriados-bancarios-30d'
  | null;

/**
 * Preset configuration with display info and filter values
 */
export interface CalendarioPreset {
  id: CalendarioPresetId;
  label: string;
  shortLabel: string;
  icon: string;
  description: string;
  getFilters: () => Partial<CalendarioFilters>;
}

/**
 * Helper to get current year date range
 */
function getCurrentYearRange(): { dataInicio: string; dataFim: string } {
  const year = new Date().getFullYear();
  return {
    dataInicio: `${year}-01-01`,
    dataFim: `${year}-12-31`,
  };
}

/**
 * Helper to get current month date range
 */
function getCurrentMonthRange(): { dataInicio: string; dataFim: string } {
  const now = new Date();
  const year = now.getFullYear();
  const month = (now.getMonth() + 1).toString().padStart(2, '0');
  const lastDay = new Date(year, now.getMonth() + 1, 0).getDate();
  return {
    dataInicio: `${year}-${month}-01`,
    dataFim: `${year}-${month}-${lastDay}`,
  };
}

/**
 * Helper to get next N days date range
 */
function getNextDaysRange(days: number): { dataInicio: string; dataFim: string } {
  const now = new Date();
  const future = new Date(now);
  future.setDate(future.getDate() + days);

  const formatDate = (d: Date) => {
    const year = d.getFullYear();
    const month = (d.getMonth() + 1).toString().padStart(2, '0');
    const day = d.getDate().toString().padStart(2, '0');
    return `${year}-${month}-${day}`;
  };

  return {
    dataInicio: formatDate(now),
    dataFim: formatDate(future),
  };
}

/**
 * Available preset configurations
 * Presets filter only holidays (feriados), excluding weekends
 */
export const CALENDARIO_PRESETS: CalendarioPreset[] = [
  {
    id: 'feriados-nacionais',
    label: 'Feriados Nacionais',
    shortLabel: 'Nacionais',
    icon: 'bi-flag',
    description: 'Feriados nacionais do ano corrente',
    getFilters: () => ({
      praca: Praca.Nacional,
      tipoDia: TipoDia.FeriadoNacional,
      diaUtil: null,
      ...getCurrentYearRange(),
    }),
  },
  {
    id: 'feriados-eua',
    label: 'Feriados EUA',
    shortLabel: 'EUA',
    icon: 'bi-globe-americas',
    description: 'Feriados do mercado americano no ano corrente',
    getFilters: () => ({
      praca: Praca.ExteriorEua,
      tipoDia: TipoDia.FeriadoNacional,
      diaUtil: null,
      ...getCurrentYearRange(),
    }),
  },
  {
    id: 'feriados-europa',
    label: 'Feriados Europa',
    shortLabel: 'Europa',
    icon: 'bi-globe-europe-africa',
    description: 'Feriados do mercado europeu no ano corrente',
    getFilters: () => ({
      praca: Praca.ExteriorEur,
      tipoDia: TipoDia.FeriadoNacional,
      diaUtil: null,
      ...getCurrentYearRange(),
    }),
  },
  {
    id: 'feriados-nacionais-30d',
    label: 'Feriados Nacionais (30 dias)',
    shortLabel: 'Nacionais 30d',
    icon: 'bi-calendar-event',
    description: 'Feriados nacionais nos próximos 30 dias',
    getFilters: () => ({
      praca: null,
      tipoDia: TipoDia.FeriadoNacional,
      diaUtil: null,
      ...getNextDaysRange(30),
    }),
  },
  {
    id: 'feriados-bancarios-30d',
    label: 'Feriados Bancários (30 dias)',
    shortLabel: 'Bancários 30d',
    icon: 'bi-bank',
    description: 'Feriados bancários nos próximos 30 dias',
    getFilters: () => ({
      praca: null,
      tipoDia: TipoDia.FeriadoBancario,
      diaUtil: null,
      ...getNextDaysRange(30),
    }),
  },
];

export interface CalendarioSearchState {
  searchTerm: string;
  filters: CalendarioFilters;
  activePreset: CalendarioPresetId;
  sortColumn: string;
  sortDirection: 'asc' | 'desc';
}

const STORAGE_KEY = 'calendario-search-state';
const PRESET_STORAGE_KEY = 'calendario-preset';
const DEFAULT_PRESET: CalendarioPresetId = 'feriados-nacionais';

const initialFilters: CalendarioFilters = {
  praca: null,
  tipoDia: null,
  diaUtil: null,
  dataInicio: null,
  dataFim: null,
};

const initialState: CalendarioSearchState = {
  searchTerm: '',
  filters: initialFilters,
  activePreset: null,
  sortColumn: 'data',
  sortDirection: 'desc',
};

/**
 * Load preset selection from localStorage (persists across sessions)
 */
function loadPresetFromLocalStorage(
  logger: LoggerService,
  localStorage: StorageService
): CalendarioPresetId {
  try {
    const stored = localStorage.getItem(PRESET_STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored) as CalendarioPresetId;
      // Validate that preset still exists
      if (parsed === null || CALENDARIO_PRESETS.some((p) => p.id === parsed)) {
        logger.debug('Loaded calendario preset from localStorage', parsed, PRESET_STORAGE_KEY);
        return parsed;
      }
    }
  } catch (error) {
    logger.warn('Failed to load calendario preset from localStorage', error, PRESET_STORAGE_KEY);
  }
  // Return default preset for first-time users
  return DEFAULT_PRESET;
}

/**
 * Save preset selection to localStorage
 */
function savePresetToLocalStorage(
  presetId: CalendarioPresetId,
  logger: LoggerService,
  localStorage: StorageService
): void {
  try {
    localStorage.setItem(PRESET_STORAGE_KEY, JSON.stringify(presetId));
    logger.debug('Saved calendario preset to localStorage', presetId, PRESET_STORAGE_KEY);
  } catch (error) {
    logger.warn('Failed to save calendario preset to localStorage', error, PRESET_STORAGE_KEY);
  }
}

/**
 * Load filter state from sessionStorage (per-session state)
 */
function loadStateFromSession(
  logger: LoggerService,
  storage: StorageService
): Omit<CalendarioSearchState, 'activePreset'> {
  try {
    const stored = storage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored);
      const loadedState = {
        searchTerm: parsed.searchTerm ?? '',
        filters: { ...initialFilters, ...parsed.filters },
        sortColumn: parsed.sortColumn ?? 'data',
        sortDirection: parsed.sortDirection ?? 'desc',
      };
      logger.debug('Loaded calendario state from sessionStorage', loadedState, STORAGE_KEY);
      return loadedState;
    }
  } catch (error) {
    logger.warn('Failed to load calendario state from sessionStorage', error, STORAGE_KEY);
  }
  return {
    searchTerm: '',
    filters: initialFilters,
    sortColumn: 'data',
    sortDirection: 'desc',
  };
}

/**
 * Save filter state to sessionStorage (excludes preset which goes to localStorage)
 */
function saveStateToSession(
  state: CalendarioSearchState,
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
    logger.debug('Saved calendario state to sessionStorage', toSave, STORAGE_KEY);
  } catch (error) {
    logger.warn('Failed to save calendario state to sessionStorage', error, STORAGE_KEY);
  }
}

/**
 * Get filters for a preset by ID
 */
function getPresetFilters(presetId: CalendarioPresetId): CalendarioFilters {
  if (!presetId) return initialFilters;
  const preset = CALENDARIO_PRESETS.find((p) => p.id === presetId);
  if (!preset) return initialFilters;
  return { ...initialFilters, ...preset.getFilters() };
}

export const CalendarioStore = signalStore(
  withState(initialState),
  withComputed((store) => ({
    /**
     * Check if any filter is active
     */
    hasActiveFilters: computed(() => {
      const f = store.filters();
      return (
        f.praca !== null ||
        f.tipoDia !== null ||
        f.diaUtil !== null ||
        f.dataInicio !== null ||
        f.dataFim !== null ||
        store.searchTerm().trim() !== ''
      );
    }),

    /**
     * Count of active filters (excluding search term)
     */
    activeFilterCount: computed(() => {
      const f = store.filters();
      let count = 0;
      if (f.praca !== null) count++;
      if (f.tipoDia !== null) count++;
      if (f.diaUtil !== null) count++;
      if (f.dataInicio !== null || f.dataFim !== null) count++;
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
        params['search'] = search;
      }
      if (f.praca !== null) {
        params['praca'] = f.praca.toString();
      }
      if (f.tipoDia !== null) {
        params['tipoDia'] = f.tipoDia.toString();
      }
      if (f.diaUtil !== null) {
        params['diaUtil'] = f.diaUtil.toString();
      }
      if (f.dataInicio) {
        params['dataInicio'] = f.dataInicio;
      }
      if (f.dataFim) {
        params['dataFim'] = f.dataFim;
      }

      return params;
    }),
  })),
  withMethods((store) => {
    const logger = inject(LoggerService);
    const sessionStorage = inject(SessionStorageService);
    const localStorage = inject(LocalStorageService);

    const saveCurrentState = () => {
      const state: CalendarioSearchState = {
        searchTerm: store.searchTerm(),
        filters: store.filters(),
        activePreset: store.activePreset(),
        sortColumn: store.sortColumn(),
        sortDirection: store.sortDirection(),
      };
      saveStateToSession(state, logger, sessionStorage);
    };

    const savePreset = (presetId: CalendarioPresetId) => {
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

      setFilter<K extends keyof CalendarioFilters>(key: K, value: CalendarioFilters[K]) {
        // Clear preset when user manually changes filters
        patchState(store, {
          filters: { ...store.filters(), [key]: value },
          activePreset: null,
        });
        saveCurrentState();
        savePreset(null);
      },

      setFilters(filters: Partial<CalendarioFilters>) {
        // Clear preset when user manually changes filters
        patchState(store, {
          filters: { ...store.filters(), ...filters },
          activePreset: null,
        });
        saveCurrentState();
        savePreset(null);
      },

      clearFilter<K extends keyof CalendarioFilters>(key: K) {
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
          sortColumn: 'data',
          sortDirection: 'desc',
        });
        saveCurrentState();
      },

      /**
       * Apply a preset - sets filters and saves to localStorage
       */
      applyPreset(presetId: CalendarioPresetId) {
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
      togglePreset(presetId: CalendarioPresetId) {
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
