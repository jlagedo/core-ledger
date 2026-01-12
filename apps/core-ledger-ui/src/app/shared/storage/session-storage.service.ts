import {Injectable} from '@angular/core';
import {StorageService} from './storage.interface';

/**
 * Production implementation of StorageService using browser's sessionStorage.
 * This service wraps the native sessionStorage API to enable dependency injection
 * and easier testing.
 */
@Injectable({providedIn: 'root'})
export class SessionStorageService implements StorageService {
  getItem(key: string): string | null {
    return sessionStorage.getItem(key);
  }

  setItem(key: string, value: string): void {
    sessionStorage.setItem(key, value);
  }

  removeItem(key: string): void {
    sessionStorage.removeItem(key);
  }

  clear(): void {
    sessionStorage.clear();
  }
}
