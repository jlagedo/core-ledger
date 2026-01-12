import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgbActiveModal, NgbDatepickerModule, NgbDateStruct } from '@ng-bootstrap/ng-bootstrap';
import { IndexadorService } from '../../../../services/indexador';
import { Indexador, CreateHistoricoIndexador } from '../../../../models/indexador.model';

@Component({
  selector: 'app-add-value-modal',
  imports: [ReactiveFormsModule, NgbDatepickerModule],
  templateUrl: './add-value-modal.html',
  styleUrl: './add-value-modal.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'dialog',
    'aria-modal': 'true',
    '[attr.aria-labelledby]': '"modal-title"',
    '(keydown.escape)': 'onEscape()',
  },
})
export class AddValueModal {
  readonly activeModal = inject(NgbActiveModal);
  private readonly indexadorService = inject(IndexadorService);
  private readonly formBuilder = inject(FormBuilder);

  readonly indexador = signal<Indexador | null>(null);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly fonteOptions = this.indexadorService.fonteOptions;

  readonly form = this.formBuilder.group({
    dataReferencia: [null as NgbDateStruct | null, [Validators.required]],
    valor: [null as number | null, [Validators.required]],
    fatorDiario: [null as number | null],
    variacaoPercentual: [null as number | null],
    fonte: [null as string | null],
  });

  onEscape(): void {
    if (!this.isSubmitting()) {
      this.activeModal.dismiss('escape');
    }
  }

  cancel(): void {
    this.activeModal.dismiss('cancel');
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) return;

    const idx = this.indexador();
    if (!idx) return;

    const formValue = this.form.value;
    const dataReferencia = formValue.dataReferencia;
    if (!dataReferencia) return;

    // Format date for API (ISO format)
    const year = dataReferencia.year;
    const month = String(dataReferencia.month).padStart(2, '0');
    const day = String(dataReferencia.day).padStart(2, '0');
    const dateString = `${year}-${month}-${day}T00:00:00`;

    const payload: CreateHistoricoIndexador = {
      indexadorId: idx.id,
      dataReferencia: dateString,
      valor: formValue.valor!,
      fatorDiario: formValue.fatorDiario ?? null,
      variacaoPercentual: formValue.variacaoPercentual ?? null,
      fonte: formValue.fonte ?? null,
      importacaoId: null,
    };

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.indexadorService.createHistorico(payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.activeModal.close('created');
      },
      error: (err) => {
        this.isSubmitting.set(false);
        if (err.status === 409) {
          this.errorMessage.set('Ja existe um valor para esta data de referencia');
        } else {
          this.errorMessage.set('Erro ao adicionar valor. Tente novamente.');
        }
      },
    });
  }

  isInvalid(field: string): boolean {
    const control = this.form.get(field);
    return control ? control.invalid && control.touched : false;
  }
}
