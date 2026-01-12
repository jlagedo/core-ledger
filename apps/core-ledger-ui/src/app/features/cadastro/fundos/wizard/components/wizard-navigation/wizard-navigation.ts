import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-wizard-navigation',
  templateUrl: './wizard-navigation.html',
  styleUrl: './wizard-navigation.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WizardNavigation {
  // Inputs
  canGoNext = input.required<boolean>();
  canGoPrevious = input.required<boolean>();
  canSubmit = input.required<boolean>();
  isSubmitting = input.required<boolean>();
  isFirstStep = input.required<boolean>();
  isLastStep = input.required<boolean>();

  // Outputs
  previous = output<void>();
  next = output<void>();
  submit = output<void>();
  cancel = output<void>();

  /**
   * Handler do botão Anterior
   */
  onPrevious(): void {
    if (!this.isSubmitting()) {
      this.previous.emit();
    }
  }

  /**
   * Handler do botão Próximo
   */
  onNext(): void {
    if (!this.isSubmitting()) {
      this.next.emit();
    }
  }

  /**
   * Handler do botão Salvar/Submeter
   */
  onSubmit(): void {
    if (!this.isSubmitting() && this.canSubmit()) {
      this.submit.emit();
    }
  }

  /**
   * Handler do botão Cancelar
   */
  onCancel(): void {
    if (!this.isSubmitting()) {
      this.cancel.emit();
    }
  }
}
