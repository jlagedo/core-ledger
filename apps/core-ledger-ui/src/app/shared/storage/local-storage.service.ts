import { Injectable } from '@angular/core';
import { StorageService } from './storage.interface';

/**
 * Production implementation of StorageService using browser's localStorage.
 * This service wraps the native localStorage API to enable dependency injection
 * and easier testing. Unlike sessionStorage, localStorage persists across browser sessions.
 */
@Injectable({ providedIn: 'root' })
export class LocalStorageService implements StorageService {
  getItem(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }

  setItem(key: string, value: string): void {
    try {
      localStorage.setItem(key, value);
    } catch {
      // localStorage may be unavailable or full
    }
  }

  removeItem(key: string): void {
    try {
      localStorage.removeItem(key);
    } catch {
      // Silently ignore errors
    }
  }

  clear(): void {
    try {
      localStorage.clear();
    } catch {
      // Silently ignore errors
    }
  }
}
