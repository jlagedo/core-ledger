import {StorageService} from '../shared/storage/storage.interface';

/**
 * In-memory implementation of StorageService for testing.
 * Provides a clean, deterministic storage implementation that doesn't rely on
 * browser APIs or mocking.
 */
export class InMemoryStorageService implements StorageService {
  private storage = new Map<string, string>();

  getItem(key: string): string | null {
    return this.storage.get(key) ?? null;
  }

  setItem(key: string, value: string): void {
    this.storage.set(key, value);
  }

  removeItem(key: string): void {
    this.storage.delete(key);
  }

  clear(): void {
    this.storage.clear();
  }

  /**
   * Test helper: Get all stored keys
   */
  keys(): string[] {
    return Array.from(this.storage.keys());
  }

  /**
   * Test helper: Get the number of stored items
   */
  size(): number {
    return this.storage.size;
  }
}
