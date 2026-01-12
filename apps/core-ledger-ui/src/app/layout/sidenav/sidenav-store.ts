import {patchState, signalStore, withHooks, withMethods, withState} from '@ngrx/signals';
import {inject} from '@angular/core';
import {LoggerService} from '../../services/logger';
import {SessionStorageService} from '../../shared/storage/session-storage.service';
import {StorageService} from '../../shared/storage/storage.interface';
import {environment} from '../../../environments/environment';

export type SidenavState = {
  menuItemsCollapsedState: Record<string, boolean>;
  isSidenavCollapsed: boolean;
  isDeadlineTickerMinimized: boolean;
};

const STORAGE_KEY = environment.storage.sidenavState;

const initialState: SidenavState = {
  menuItemsCollapsedState: {},
  isSidenavCollapsed: false,
  isDeadlineTickerMinimized: true, // Default to minimized
};

/**
 * Loads sidenav state from storage
 */
function loadStateFromSession(logger: LoggerService, storage: StorageService): SidenavState {
  try {
    const stored = storage.getItem(STORAGE_KEY);
    if (stored) {
      const loadedState: SidenavState = {...initialState, ...JSON.parse(stored)};
      logger.debug('Loaded sidenav state from sessionStorage', loadedState, STORAGE_KEY);
      return loadedState;
    }
  } catch (error) {
    logger.warn('Failed to load sidenav state from sessionStorage', error, STORAGE_KEY);
  }
  logger.debug('Using initial sidenav state (no stored state found)', undefined, STORAGE_KEY);
  return initialState;
}

/**
 * Saves sidenav state to storage
 */
function saveStateToSession(state: SidenavState, logger: LoggerService, storage: StorageService): void {
  try {
    storage.setItem(STORAGE_KEY, JSON.stringify(state));
    logger.debug('Saved sidenav state to sessionStorage', state, STORAGE_KEY);
  } catch (error) {
    // Storage can fail if:
    // - Storage quota exceeded
    // - Privacy mode/incognito blocks storage
    // - Browser doesn't support sessionStorage
    logger.warn('Failed to save sidenav state to sessionStorage', error, STORAGE_KEY);
  }
}

/**
 * Signal store for managing sidenav state with sessionStorage persistence
 */
export const SidenavStore = signalStore(
  {providedIn: 'root'},
  withState(initialState),
  withMethods((store) => {
    const logger = inject(LoggerService);
    const storage = inject(SessionStorageService);

    const saveCurrentState = () => {
      const state: SidenavState = {
        menuItemsCollapsedState: store.menuItemsCollapsedState(),
        isSidenavCollapsed: store.isSidenavCollapsed(),
        isDeadlineTickerMinimized: store.isDeadlineTickerMinimized(),
      };
      saveStateToSession(state, logger, storage);
    };

    return {
      /**
       * Toggle a menu item's collapsed state
       */
      toggleMenuItem(label: string) {
        const currentState = store.menuItemsCollapsedState();
        const isCurrentlyCollapsed = currentState[label] ?? true;

        patchState(store, {
          menuItemsCollapsedState: {
            ...currentState,
            [label]: !isCurrentlyCollapsed,
          },
        });
        saveCurrentState();
      },

      /**
       * Set a menu item's collapsed state explicitly
       */
      setMenuItemCollapsed(label: string, collapsed: boolean) {
        const currentState = store.menuItemsCollapsedState();

        patchState(store, {
          menuItemsCollapsedState: {
            ...currentState,
            [label]: collapsed,
          },
        });
        saveCurrentState();
      },

      /**
       * Check if a menu item is collapsed
       */
      isMenuItemCollapsed(label: string): boolean {
        return store.menuItemsCollapsedState()[label] ?? true;
      },

      /**
       * Toggle the entire sidenav collapsed state
       */
      toggleSidenav() {
        patchState(store, {
          isSidenavCollapsed: !store.isSidenavCollapsed(),
        });
        saveCurrentState();
      },

      /**
       * Set the sidenav collapsed state explicitly
       */
      setSidenavCollapsed(collapsed: boolean) {
        patchState(store, {
          isSidenavCollapsed: collapsed,
        });
        saveCurrentState();
      },

      /**
       * Toggle the deadline ticker minimized state
       */
      toggleDeadlineTickerMinimized() {
        patchState(store, {
          isDeadlineTickerMinimized: !store.isDeadlineTickerMinimized(),
        });
        saveCurrentState();
      },

      /**
       * Set the deadline ticker minimized state explicitly
       */
      setDeadlineTickerMinimized(minimized: boolean) {
        patchState(store, {
          isDeadlineTickerMinimized: minimized,
        });
        saveCurrentState();
      },

      /**
       * Initialize menu items with default collapsed state
       * Called by sidenav component on init with menu items from MenuService
       */
      initializeMenuItems(menuLabels: string[]) {
        const currentState = store.menuItemsCollapsedState();
        const updatedState = {...currentState};
        let hasChanges = false;

        // Only initialize items that don't already have a state
        menuLabels.forEach((label) => {
          if (!(label in updatedState)) {
            updatedState[label] = true; // Default to collapsed
            hasChanges = true;
          }
        });

        if (hasChanges) {
          patchState(store, {menuItemsCollapsedState: updatedState});
          saveCurrentState();
        }
      },
    };
  }),
  withHooks({
    onInit(store) {
      const logger = inject(LoggerService);
      const storage = inject(SessionStorageService);
      const savedState = loadStateFromSession(logger, storage);
      patchState(store, savedState);
    },
  })
);
