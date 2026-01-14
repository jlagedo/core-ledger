import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { InstituicoesService } from '../../../../../../../services/instituicoes-service';
import { InstituicaoAutocompleteItem, formatCnpj } from '../../../models/vinculos.model';
import { cnpjValidator } from '../../../../../../../shared/validators/cnpj.validator';
import { CnpjMaskDirective } from '../../../../../../../directives/cnpj-mask.directive';

/**
 * Modal for quick institution registration (RF-04).
 * Allows creating a new institution with minimal required fields.
 */
@Component({
  selector: 'app-instituicao-modal',
  imports: [ReactiveFormsModule, CnpjMaskDirective],
  templateUrl: './instituicao-modal.html',
  styleUrl: './instituicao-modal.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'dialog',
    'aria-modal': 'true',
    '[attr.aria-labelledby]': '"modal-title"',
    '(keydown.escape)': 'onEscape()',
  },
})
export class InstituicaoModal {
  readonly activeModal = inject(NgbActiveModal);
  private readonly instituicoesService = inject(InstituicoesService);
  private readonly formBuilder = inject(FormBuilder);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  /** Pre-filled search term passed from parent */
  readonly initialSearchTerm = signal<string>('');

  readonly form = this.formBuilder.group({
    cnpj: ['', [Validators.required, cnpjValidator()]],
    razaoSocial: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
    nomeFantasia: ['', [Validators.maxLength(100)]],
  });

  onEscape(): void {
    if (!this.isSubmitting()) {
      this.activeModal.dismiss('escape');
    }
  }

  cancel(): void {
    this.activeModal.dismiss('cancel');
  }

  goToFullRegistration(): void {
    this.activeModal.dismiss('full-registration');
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) return;

    const formValue = this.form.value;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.instituicoesService
      .create({
        cnpj: formValue.cnpj!,
        razaoSocial: formValue.razaoSocial!,
        nomeFantasia: formValue.nomeFantasia || undefined,
      })
      .subscribe({
        next: (instituicao) => {
          this.isSubmitting.set(false);
          // Return the created institution as autocomplete item
          const item: InstituicaoAutocompleteItem = {
            id: instituicao.id,
            cnpj: instituicao.cnpj,
            cnpjFormatado: formatCnpj(instituicao.cnpj),
            razaoSocial: instituicao.razaoSocial,
            nomeFantasia: instituicao.nomeFantasia || '',
            displayName: instituicao.nomeFantasia
              ? `${instituicao.nomeFantasia} (${instituicao.razaoSocial})`
              : instituicao.razaoSocial,
          };
          this.activeModal.close(item);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          if (err.status === 409) {
            this.errorMessage.set('Ja existe uma instituicao com este CNPJ');
          } else {
            this.errorMessage.set('Erro ao cadastrar instituicao. Tente novamente.');
          }
        },
      });
  }

  isInvalid(field: string): boolean {
    const control = this.form.get(field);
    return control ? control.invalid && control.touched : false;
  }

  getFieldError(field: string, errorKey: string): boolean {
    const control = this.form.get(field);
    return control?.hasError(errorKey) ?? false;
  }
}
