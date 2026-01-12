import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject } from '@angular/core';
import { Toast, ToastService, ToastType } from '../../services/toast-service';

@Component({
  selector: 'app-toasts-container',
  imports: [],
  templateUrl: './toasts-container.html',
  styleUrl: './toasts-container.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastsContainer {
  readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly activeTimers = new Map<Toast, ReturnType<typeof setTimeout>>();

  constructor() {
    // Watch for new toasts and set up auto-dismiss timers
    effect(() => {
      const toasts = this.toastService.toasts();
      for (const toast of toasts) {
        if (!this.activeTimers.has(toast)) {
          const delay = toast.delay || 5000;
          const timer = setTimeout(() => {
            this.toastService.remove(toast);
            this.activeTimers.delete(toast);
          }, delay);
          this.activeTimers.set(toast, timer);
        }
      }

      // Clean up timers for removed toasts
      for (const [toast, timer] of this.activeTimers) {
        if (!toasts.includes(toast)) {
          clearTimeout(timer);
          this.activeTimers.delete(toast);
        }
      }
    });

    // Clean up all timers on destroy
    this.destroyRef.onDestroy(() => {
      for (const timer of this.activeTimers.values()) {
        clearTimeout(timer);
      }
      this.activeTimers.clear();
    });
  }

  getToastIcon(type: ToastType): string {
    const iconMap: Record<ToastType, string> = {
      success: 'bi-check-circle-fill',
      error: 'bi-exclamation-octagon-fill',
      warning: 'bi-exclamation-triangle-fill',
      info: 'bi-info-circle-fill',
    };
    return iconMap[type];
  }

  getTypeLabel(type: ToastType): string {
    const labelMap: Record<ToastType, string> = {
      success: 'SYS.OK',
      error: 'SYS.ERR',
      warning: 'SYS.WARN',
      info: 'SYS.INFO',
    };
    return labelMap[type];
  }

  formatTimestamp(): string {
    const now = new Date();
    const hours = now.getHours().toString().padStart(2, '0');
    const minutes = now.getMinutes().toString().padStart(2, '0');
    const seconds = now.getSeconds().toString().padStart(2, '0');
    return `${hours}:${minutes}:${seconds}`;
  }
}
