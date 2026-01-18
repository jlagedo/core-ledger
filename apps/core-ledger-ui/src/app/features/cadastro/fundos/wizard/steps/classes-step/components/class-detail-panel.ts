import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import {
  TipoClasseFidc,
  PublicoAlvo,
  TIPO_CLASSE_FIDC_OPTIONS,
  PUBLICO_ALVO_OPTIONS,
} from '../../../models/classes.model';

/**
 * Detail Panel for Classes Step
 * Form for editing the selected class with progressive disclosure sections
 */
@Component({
  selector: 'app-class-detail-panel',
  imports: [ReactiveFormsModule],
  templateUrl: './class-detail-panel.html',
  styleUrl: './class-detail-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClassDetailPanel {
  // Inputs
  readonly classeFormGroup = input.required<FormGroup>();
  readonly classeIndex = input.required<number>();
  readonly isFidc = input.required<boolean>();
  readonly isSubclass = input<boolean>(false);

  // Outputs
  readonly addSubclass = output<void>();

  // Enum options for template
  readonly tipoClasseFidcOptions = TIPO_CLASSE_FIDC_OPTIONS;
  readonly publicoAlvoOptions = PUBLICO_ALVO_OPTIONS;

  /**
   * Get the class code for display
   */
  get classCode(): string {
    return this.classeFormGroup().get('codigoClasse')?.value || 'Nova Classe';
  }

  /**
   * Check if field is invalid (touched and invalid)
   */
  isFieldInvalid(fieldName: string): boolean {
    const control = this.classeFormGroup().get(fieldName);
    return control ? control.touched && control.invalid : false;
  }

  /**
   * Check if field is valid (touched and valid)
   */
  isFieldValid(fieldName: string): boolean {
    const control = this.classeFormGroup().get(fieldName);
    return control ? control.touched && control.valid : false;
  }

  /**
   * Check for specific field error
   */
  hasFieldError(fieldName: string, errorKey: string): boolean {
    const control = this.classeFormGroup().get(fieldName);
    return control?.hasError(errorKey) ?? false;
  }

  /**
   * Get badge class based on tipo classe
   */
  getTipoBadgeClass(): string {
    const tipoClasse = this.classeFormGroup().get('tipoClasseFidc')?.value as TipoClasseFidc | null;
    if (!tipoClasse) return 'badge--default';

    switch (tipoClasse) {
      case TipoClasseFidc.SENIOR:
        return 'badge--senior';
      case TipoClasseFidc.MEZANINO:
        return 'badge--mezanino';
      case TipoClasseFidc.SUBORDINADA:
      case TipoClasseFidc.SUBORDINADA_JUNIOR:
        return 'badge--subordinada';
      default:
        return 'badge--default';
    }
  }

  /**
   * Handle add subclass action
   */
  onAddSubclass(): void {
    this.addSubclass.emit();
  }
}
