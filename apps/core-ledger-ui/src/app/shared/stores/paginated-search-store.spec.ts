import {TestBed} from '@angular/core/testing';
import {vi} from 'vitest';
import {createPaginatedSearchStore, PaginatedSearchState} from './paginated-search-store';
import {LoggerService} from '../../services/logger';
import {SessionStorageService} from '../storage/session-storage.service';
import {InMemoryStorageService} from '../../testing/in-memory-storage.service';

describe('PaginatedSearchStore', () => {
  type StoreInstance = InstanceType<ReturnType<typeof createPaginatedSearchStore>>;
  let store: StoreInstance;
  let storageService: InMemoryStorageService;
  let loggerService: any;

  beforeEach(() => {
    // Create fresh storage and logger for each test
    storageService = new InMemoryStorageService();

    loggerService = {
      debug: vi.fn(),
      warn: vi.fn(),
      error: vi.fn(),
      info: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        {provide: LoggerService, useValue: loggerService},
        {provide: SessionStorageService, useValue: storageService},
      ]
    });

    const StoreClass = createPaginatedSearchStore({
      storageKey: 'test-store',
      initialSort: {column: 'name', direction: 'asc'},
      defaultPageSize: 20
    });

    store = TestBed.runInInjectionContext(() => new StoreClass());
  });

  it('should initialize with default state', () => {
    expect(store.searchTerm()).toBe('');
    expect(store.page()).toBe(1);
    expect(store.pageSize()).toBe(20);
    expect(store.sortColumn()).toBe('name');
    expect(store.sortDirection()).toBe('asc');
  });

  it('should set search term and reset page to 1', () => {
    store.setSearchTerm('test');
    expect(store.searchTerm()).toBe('test');
    expect(store.page()).toBe(1);
  });

  it('should set page', () => {
    store.setPage(3);
    expect(store.page()).toBe(3);
  });

  it('should set page size and reset page to 1', () => {
    store.setPage(5);
    store.setPageSize(50);
    expect(store.pageSize()).toBe(50);
    expect(store.page()).toBe(1);
  });

  it('should set sort and reset page to 1', () => {
    store.setPage(5);
    store.setSort('age', 'desc');
    expect(store.sortColumn()).toBe('age');
    expect(store.sortDirection()).toBe('desc');
    expect(store.page()).toBe(1);
  });

  it('should reset sort to initial values', () => {
    store.setSort('age', 'desc');
    store.setPage(5);

    store.resetSort();

    expect(store.sortColumn()).toBe('name');
    expect(store.sortDirection()).toBe('asc');
    expect(store.page()).toBe(1);
  });

  it('should save state to sessionStorage', () => {
    store.setSearchTerm('test');
    store.setPage(2);
    store.setSort('age', 'desc');

    // Verify state was saved to storage
    const saved = storageService.getItem('test-store');
    expect(saved).toBeTruthy();
    const state = JSON.parse(saved!);
    expect(state.sortColumn).toBe('age');
    expect(state.sortDirection).toBe('desc');
  });

  it('should load state from sessionStorage on init', () => {
    const savedState: PaginatedSearchState = {
      searchTerm: 'saved',
      page: 3,
      pageSize: 30,
      sortColumn: 'date',
      sortDirection: 'desc'
    };

    storageService.setItem('test-store-2', JSON.stringify(savedState));

    const StoreClass = createPaginatedSearchStore({
      storageKey: 'test-store-2',
      initialSort: {column: 'name', direction: 'asc'},
      defaultPageSize: 20
    });

    const newStore = TestBed.runInInjectionContext(() => new StoreClass());

    expect(newStore.searchTerm()).toBe('saved');
    expect(newStore.page()).toBe(3);
    expect(newStore.pageSize()).toBe(30);
    expect(newStore.sortColumn()).toBe('date');
    expect(newStore.sortDirection()).toBe('desc');
  });

  it('should use initial state if sessionStorage is empty', () => {
    // Storage is already empty by default, no need to mock
    const StoreClass = createPaginatedSearchStore({
      storageKey: 'empty-store',
      initialSort: {column: 'id', direction: 'desc'},
      defaultPageSize: 10
    });

    const newStore = TestBed.runInInjectionContext(() => new StoreClass());

    expect(newStore.searchTerm()).toBe('');
    expect(newStore.page()).toBe(1);
    expect(newStore.pageSize()).toBe(10);
    expect(newStore.sortColumn()).toBe('id');
    expect(newStore.sortDirection()).toBe('desc');
  });

  it('should handle sessionStorage errors gracefully', () => {
    const errorStorage = new InMemoryStorageService();
    vi.spyOn(errorStorage, 'getItem').mockImplementation(() => {
      throw new Error('Storage error');
    });

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        {provide: LoggerService, useValue: loggerService},
        {provide: SessionStorageService, useValue: errorStorage},
      ]
    });

    const StoreClass = createPaginatedSearchStore({
      storageKey: 'error-store',
      initialSort: {column: 'name', direction: 'asc'}
    });

    const newStore = TestBed.runInInjectionContext(() => new StoreClass());

    // Should still initialize with default values
    expect(newStore.searchTerm()).toBe('');
    expect(newStore.page()).toBe(1);
    expect(newStore.sortColumn()).toBe('name');
  });
});
