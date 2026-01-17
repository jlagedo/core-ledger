import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  DestroyRef,
  untracked,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { WizardStepConfig } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';
import { createRestorationEffect, withRestorationGuard } from '../../shared';

/**
 * Componente placeholder para testar o wizard
 * Este componente será substituído pelos componentes reais em slices futuros
 */
@Component({
  selector: 'app-placeholder-step',
  imports: [ReactiveFormsModule],
  templateUrl: './placeholder-step.html',
  styleUrl: './placeholder-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaceholderStep {
  // Input do passo atual
  stepConfig = input.required<WizardStepConfig>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly wizardStore = inject(WizardStore);
  private readonly destroyRef = inject(DestroyRef);

  // Formulário simples para teste
  form = this.formBuilder.group({
    campo1: ['', Validators.required],
    campo2: [''],
  });

  constructor() {
    // Create restoration effect (centralizes deduplication, isRestoring flag, and form restore)
    const { isRestoring } = createRestorationEffect<Record<string, string>>({
      stepConfig: () => this.stepConfig(),
      wizardStore: this.wizardStore,
      form: this.form,
      resetForm: () => this.form.reset({ campo1: '', campo2: '' }),
      restoreData: (data) => {
        this.form.patchValue(data, { emitEvent: false });
        this.form.markAsDirty();
      },
      updateStepValidation: () => this.updateStepValidation(),
    });

    // Setup form subscriptions
    this.form.statusChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.updateStepValidation();
    });

    this.form.valueChanges
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        withRestorationGuard(isRestoring)
      )
      .subscribe((value) => {
        const stepConfig = untracked(() => this.stepConfig());
        this.wizardStore.setStepData(stepConfig.key, value);
      });
  }

  private updateStepValidation(): void {
    const stepId = this.stepConfig().id;

    this.wizardStore.setStepValidation(stepId, {
      isValid: this.form.valid,
      isDirty: this.form.dirty,
      errors: this.form.invalid ? ['Preencha todos os campos obrigatórios'] : [],
    });

    // Marcar como completo se válido e sujo
    if (this.form.valid && this.form.dirty) {
      this.wizardStore.markStepComplete(stepId);
    } else if (this.form.invalid && this.form.dirty) {
      this.wizardStore.markStepIncomplete(stepId);
    }
  }
}
