/**
 * Wizard de Cadastro de Fundo - Save Status Indicator Component
 *
 * Displays the current auto-save status (idle, saving, saved, error)
 * with appropriate visual feedback.
 */

import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { SaveStatus } from '../../models/persistence.model';

@Component({
  selector: 'app-save-status-indicator',
  template: `
    <div class="save-status d-flex align-items-center gap-2 text-body-secondary small">
      @switch (status()) {
        @case ('saving') {
          <div class="spinner-border spinner-border-sm" role="status">
            <span class="visually-hidden">Salvando...</span>
          </div>
          <span>Salvando...</span>
        }
        @case ('saved') {
          <i class="bi bi-check-circle text-success" aria-hidden="true"></i>
          <span>Salvo</span>
        }
        @case ('error') {
          <i class="bi bi-exclamation-circle text-danger" aria-hidden="true"></i>
          <span>Erro ao salvar</span>
          <button
            type="button"
            class="btn btn-link btn-sm p-0 text-decoration-underline"
            (click)="retry.emit()"
          >
            Tentar novamente
          </button>
        }
      }
    </div>
  `,
  styles: `
    .save-status {
      min-height: 1.5rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SaveStatusIndicator {
  /** Current save status */
  status = input.required<SaveStatus>();

  /** Emitted when user clicks "Tentar novamente" on error */
  retry = output<void>();
}
