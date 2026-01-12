import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Calendario } from '../../../../models/calendario.model';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-delete-calendario-modal',
  imports: [DatePipe],
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
export class DeleteCalendarioModal {
  activeModal = inject(NgbActiveModal);
  calendario = signal<Calendario | null>(null);

  onEscape(): void {
    this.activeModal.dismiss('escape');
  }
}
