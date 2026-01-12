import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { IndexadorService } from '../../../../services/indexador';
import { HistoricoIndexador } from '../../../../models/indexador.model';

@Component({
  selector: 'app-delete-value-modal',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './delete-value-modal.html',
  styleUrl: './delete-value-modal.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'dialog',
    'aria-modal': 'true',
    '[attr.aria-labelledby]': '"modal-title"',
    '(keydown.escape)': 'onEscape()',
  },
})
export class DeleteValueModal {
  readonly activeModal = inject(NgbActiveModal);
  private readonly indexadorService = inject(IndexadorService);

  readonly historico = signal<HistoricoIndexador | null>(null);
  readonly isDeleting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  onEscape(): void {
    if (!this.isDeleting()) {
      this.activeModal.dismiss('escape');
    }
  }

  cancel(): void {
    this.activeModal.dismiss('cancel');
  }

  confirm(): void {
    const hist = this.historico();
    if (!hist || this.isDeleting()) return;

    this.isDeleting.set(true);
    this.errorMessage.set(null);

    this.indexadorService.deleteHistorico(hist.id).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.activeModal.close('deleted');
      },
      error: () => {
        this.isDeleting.set(false);
        this.errorMessage.set('Erro ao excluir valor. Tente novamente.');
      },
    });
  }
}
