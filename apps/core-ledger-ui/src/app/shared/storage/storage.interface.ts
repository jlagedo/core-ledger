/**
 * Storage interface for key-value persistence.
 * Abstracts browser storage APIs to enable testing and flexibility.
 */
export interface StorageService {
  /**
   * Retrieves the value associated with the given key.
   * @param key The storage key
   * @returns The stored value, or null if not found
   */
  getItem(key: string): string | null;

  /**
   * Stores a value associated with the given key.
   * @param key The storage key
   * @param value The value to store (must be a string)
   */
  setItem(key: string, value: string): void;

  /**
   * Removes the value associated with the given key.
   * @param key The storage key
   */
  removeItem(key: string): void;

  /**
   * Clears all stored values.
   */
  clear(): void;
}
