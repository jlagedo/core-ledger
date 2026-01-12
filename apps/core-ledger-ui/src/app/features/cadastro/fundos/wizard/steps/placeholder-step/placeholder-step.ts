import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  input,
  DestroyRef,
  untracked,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { WizardStepConfig, WizardStepId } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';

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

  // Track which step we last loaded data for to avoid re-loading
  private lastLoadedStepId: WizardStepId | null = null;

  // Formulário simples para teste
  form = this.formBuilder.group({
    campo1: ['', Validators.required],
    campo2: [''],
  });

  constructor() {
    // Setup form subscriptions (ONCE, outside of effect)
    this.form.statusChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.updateStepValidation();
    });

    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      // Use untracked to read stepConfig without creating dependencies
      const stepConfig = untracked(() => this.stepConfig());
      this.wizardStore.setStepData(stepConfig.key, value);
    });

    // Effect apenas para carregar dados quando o passo MUDAR
    // Use untracked para ler stepData sem rastrear mudanças
    effect(() => {
      const stepConfig = this.stepConfig(); // Tracked - effect re-runs when step changes
      const stepId = stepConfig.id;

      // Only load data if step actually changed
      if (this.lastLoadedStepId === stepId) {
        return;
      }
      this.lastLoadedStepId = stepId;

      // Reset form for new step
      this.form.reset({ campo1: '', campo2: '' }, { emitEvent: false });

      // Use untracked to read stepData without tracking it
      const stepData = untracked(() => this.wizardStore.stepData()[stepConfig.key]);

      if (stepData) {
        this.form.patchValue(stepData as Record<string, string>, { emitEvent: false });
      }

      // Initial validation for this step (use untracked to avoid cycles)
      untracked(() => this.updateStepValidation());
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
