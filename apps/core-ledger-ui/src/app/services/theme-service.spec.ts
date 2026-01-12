import { TestBed } from '@angular/core/testing';
import { DOCUMENT } from '@angular/common';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { ThemeService } from './theme-service';
import { setupLocalStorageMock } from '../testing/test-helpers';

describe('ThemeService', () => {
  let service: ThemeService;
  let document: Document;
  let storageMock: ReturnType<typeof setupLocalStorageMock>;

  beforeEach(() => {
    storageMock = setupLocalStorageMock();

    TestBed.configureTestingModule({});
    document = TestBed.inject(DOCUMENT);
    service = TestBed.inject(ThemeService);
  });

  afterEach(() => {
    storageMock.clear();
  });

  describe('initialization', () => {
    it('should initialize with dark theme by default', () => {
      expect(service.currentTheme()).toBe('dark');
      expect(service.isDarkMode()).toBe(true);
    });

    it('should initialize with dark theme and set data-bs-theme attribute', () => {
      expect(document.documentElement.getAttribute('data-bs-theme')).toBe('dark');
    });

    it('should load theme from localStorage if present', () => {
      // Reset and initialize with light theme in storage
      TestBed.resetTestingModule();
      storageMock.store.set('core-ledger-theme', 'light');

      TestBed.configureTestingModule({});
      const newService = TestBed.inject(ThemeService);
      expect(newService.currentTheme()).toBe('light');
    });

    it('should fallback to dark theme if localStorage has invalid value', () => {
      TestBed.resetTestingModule();
      storageMock.store.set('core-ledger-theme', 'invalid');

      TestBed.configureTestingModule({});
      const newService = TestBed.inject(ThemeService);
      expect(newService.currentTheme()).toBe('dark');
    });
  });

  describe('setTheme', () => {
    it('should update theme to light', () => {
      service.setTheme('light');
      expect(service.currentTheme()).toBe('light');
      expect(service.isDarkMode()).toBe(false);
    });

    it('should update data-bs-theme attribute on document', () => {
      service.setTheme('light');
      expect(document.documentElement.getAttribute('data-bs-theme')).toBe('light');
    });

    it('should persist theme to localStorage', () => {
      service.setTheme('light');
      expect(storageMock.store.get('core-ledger-theme')).toBe('light');
    });
  });

  describe('toggleTheme', () => {
    it('should toggle from dark to light', () => {
      expect(service.currentTheme()).toBe('dark');
      service.toggleTheme();
      expect(service.currentTheme()).toBe('light');
    });

    it('should toggle from light to dark', () => {
      service.setTheme('light');
      service.toggleTheme();
      expect(service.currentTheme()).toBe('dark');
    });

    it('should update DOM attribute when toggling', () => {
      service.toggleTheme();
      expect(document.documentElement.getAttribute('data-bs-theme')).toBe('light');
      service.toggleTheme();
      expect(document.documentElement.getAttribute('data-bs-theme')).toBe('dark');
    });

    it('should persist toggled theme to localStorage', () => {
      service.toggleTheme();
      expect(storageMock.store.get('core-ledger-theme')).toBe('light');
    });
  });

  describe('computed signals', () => {
    it('should update isDarkMode computed signal when theme changes', () => {
      expect(service.isDarkMode()).toBe(true);
      service.setTheme('light');
      expect(service.isDarkMode()).toBe(false);
      service.setTheme('dark');
      expect(service.isDarkMode()).toBe(true);
    });
  });
});
