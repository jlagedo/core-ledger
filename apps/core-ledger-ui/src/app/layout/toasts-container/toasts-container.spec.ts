import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';

import { ToastsContainer } from './toasts-container';
import { ToastService } from '../../services/toast-service';

describe('ToastsContainer', () => {
  let component: ToastsContainer;
  let fixture: ComponentFixture<ToastsContainer>;
  let toastService: ToastService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToastsContainer],
    }).compileComponents();

    fixture = TestBed.createComponent(ToastsContainer);
    component = fixture.componentInstance;
    toastService = TestBed.inject(ToastService);
    toastService.clear();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('getToastIcon', () => {
    it('should return correct icon for success', () => {
      expect(component.getToastIcon('success')).toBe('bi-check-circle-fill');
    });

    it('should return correct icon for error', () => {
      expect(component.getToastIcon('error')).toBe('bi-exclamation-octagon-fill');
    });

    it('should return correct icon for warning', () => {
      expect(component.getToastIcon('warning')).toBe('bi-exclamation-triangle-fill');
    });

    it('should return correct icon for info', () => {
      expect(component.getToastIcon('info')).toBe('bi-info-circle-fill');
    });
  });

  describe('getTypeLabel', () => {
    it('should return SYS.OK for success', () => {
      expect(component.getTypeLabel('success')).toBe('SYS.OK');
    });

    it('should return SYS.ERR for error', () => {
      expect(component.getTypeLabel('error')).toBe('SYS.ERR');
    });

    it('should return SYS.WARN for warning', () => {
      expect(component.getTypeLabel('warning')).toBe('SYS.WARN');
    });

    it('should return SYS.INFO for info', () => {
      expect(component.getTypeLabel('info')).toBe('SYS.INFO');
    });
  });

  describe('formatTimestamp', () => {
    it('should return a valid timestamp format HH:MM:SS', () => {
      const timestamp = component.formatTimestamp();
      expect(timestamp).toMatch(/^\d{2}:\d{2}:\d{2}$/);
    });
  });

  describe('auto-dismiss', () => {
    beforeEach(() => {
      vi.useFakeTimers();
    });

    afterEach(() => {
      vi.useRealTimers();
    });

    it('should auto-dismiss toast after specified delay', async () => {
      toastService.success('Test message', 500);
      fixture.detectChanges();

      expect(toastService.toasts().length).toBe(1);

      vi.advanceTimersByTime(500);
      fixture.detectChanges();

      expect(toastService.toasts().length).toBe(0);
    });

    it('should not dismiss toast before delay expires', async () => {
      toastService.warning('Test message', 1000);
      fixture.detectChanges();

      expect(toastService.toasts().length).toBe(1);

      vi.advanceTimersByTime(500);
      fixture.detectChanges();

      // Should still be there
      expect(toastService.toasts().length).toBe(1);

      vi.advanceTimersByTime(500);
      fixture.detectChanges();

      // Now it should be gone
      expect(toastService.toasts().length).toBe(0);
    });
  });
});
