/**
 * Wizard de Cadastro de Fundo - Recovery Banner Component
 *
 * Displays a banner when a saved draft is detected, allowing the user
 * to restore or discard the draft.
 */

import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-recovery-banner',
  imports: [DatePipe],
  template: `
    @if (showBanner()) {
      <div
        class="alert alert-info d-flex align-items-center gap-3 mb-3 shadow-sm rounded-3"
        role="alert"
      >
        <i class="bi bi-clock-history fs-4" aria-hidden="true"></i>
        <div class="flex-grow-1">
          <strong>Rascunho encontrado</strong>
          <p class="mb-0 small text-body-secondary">
            Deseja continuar de onde parou?
            @if (lastSavedAt()) {
              Salvo em {{ lastSavedAt() | date: 'dd/MM/yyyy HH:mm' }}
            }
          </p>
        </div>
        <div class="d-flex gap-2">
          <button
            type="button"
            class="btn btn-primary btn-sm"
            (click)="restore.emit()"
          >
            <i class="bi bi-arrow-counterclockwise me-1" aria-hidden="true"></i>
            Restaurar
          </button>
          <button
            type="button"
            class="btn btn-outline-secondary btn-sm"
            (click)="dismiss.emit()"
          >
            Descartar
          </button>
        </div>
      </div>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecoveryBanner {
  /** Whether to show the recovery banner */
  showBanner = input.required<boolean>();

  /** Timestamp of when the draft was last saved */
  lastSavedAt = input<Date | null>(null);

  /** Emitted when user clicks "Restaurar" */
  restore = output<void>();

  /** Emitted when user clicks "Descartar" */
  dismiss = output<void>();
}
