import {Injectable, signal, computed, inject, DOCUMENT} from '@angular/core';
import {ENVIRONMENT} from '../config/environment.config';

@Injectable({providedIn: 'root'})
export class ThemeService {
  private readonly environment = inject(ENVIRONMENT);
  private readonly STORAGE_KEY = this.environment.storage.theme;
  private readonly document = inject(DOCUMENT);

  // Sinal privado gravável
  private readonly _currentTheme = signal<'light' | 'dark'>('dark');

  // Sinal público somente leitura
  readonly currentTheme = this._currentTheme.asReadonly();

  // Sinal computado para estado da UI
  readonly isDarkMode = computed(() => this._currentTheme() === 'dark');

  constructor() {
    this.initializeTheme();
  }

  private initializeTheme(): void {
    const storedTheme = this.getStoredTheme();
    const theme = storedTheme || 'dark';
    this._currentTheme.set(theme);
    this.applyTheme(theme);
  }

  private getStoredTheme(): 'light' | 'dark' | null {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      if (stored === 'light' || stored === 'dark') {
        return stored;
      }
      return null;
    } catch (error) {
      console.warn('Falha ao ler tema do localStorage:', error);
      return null;
    }
  }

  setTheme(theme: 'light' | 'dark'): void {
    this._currentTheme.set(theme);
    this.applyTheme(theme);
    this.persistTheme(theme);
  }

  toggleTheme(): void {
    const newTheme = this._currentTheme() === 'dark' ? 'light' : 'dark';
    this.setTheme(newTheme);
  }

  private applyTheme(theme: 'light' | 'dark'): void {
    this.document.documentElement.setAttribute('data-bs-theme', theme);
  }

  private persistTheme(theme: 'light' | 'dark'): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, theme);
    } catch (error) {
      console.warn('Falha ao persistir tema no localStorage:', error);
    }
  }
}
