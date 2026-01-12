import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Indexador } from '../../../../models/indexador.model';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { IndexadorService } from '../../../../services/indexador';

@Component({
  selector: 'app-delete-indexador-modal',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './delete-modal.html',
  styleUrl: './delete-modal.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'dialog',
    'aria-modal': 'true',
    '[attr.aria-labelledby]': '"modal-title"',
    '(keydown.escape)': 'onEscape()',
  },
})
export class DeleteIndexadorModal {
  readonly activeModal = inject(NgbActiveModal);
  readonly indexadorService = inject(IndexadorService);
  indexador = signal<Indexador | null>(null);

  getTipoColor(indexador: Indexador): string {
    return this.indexadorService.getTipoColor(indexador.tipo);
  }

  getTipoIcon(indexador: Indexador): string {
    return this.indexadorService.getTipoIcon(indexador.tipo);
  }

  onEscape(): void {
    this.activeModal.dismiss('escape');
  }
}
