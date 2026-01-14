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
import { filter } from 'rxjs/operators';
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

  // Track step ID and dataVersion to avoid re-loading unless store data changes
  private lastLoadedStepId: WizardStepId | null = null;
  private lastDataVersion = -1;

  // Flag to prevent store updates during data restoration
  private isRestoring = false;

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

    this.form.valueChanges
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        filter(() => !this.isRestoring)
      )
      .subscribe((value) => {
        // Use untracked to read stepConfig without creating dependencies
        const stepConfig = untracked(() => this.stepConfig());
        this.wizardStore.setStepData(stepConfig.key, value);
      });

    // Effect: Load data when step changes or dataVersion changes (draft restoration)
    // Use untracked para ler stepData sem rastrear mudanças
    effect(() => {
      const stepConfig = this.stepConfig(); // Tracked - effect re-runs when step changes
      const stepId = stepConfig.id;
      const dataVersion = this.wizardStore.dataVersion();

      // Skip if same step AND same dataVersion (no changes)
      const sameStep = this.lastLoadedStepId === stepId;
      const sameVersion = this.lastDataVersion === dataVersion;
      if (sameStep && sameVersion) {
        return;
      }
      this.lastLoadedStepId = stepId;
      this.lastDataVersion = dataVersion;

      // Set restoration flag to prevent store updates
      this.isRestoring = true;

      // Reset form for new step
      this.form.reset({ campo1: '', campo2: '' }, { emitEvent: false });

      // Use untracked to read stepData without tracking it
      const stepData = untracked(() => this.wizardStore.stepData()[stepConfig.key]);

      if (stepData) {
        this.form.patchValue(stepData as Record<string, string>, { emitEvent: false });
        this.form.markAsDirty();
      }

      // Clear restoration flag
      this.isRestoring = false;

      // Mark all fields as touched to show validation state
      Object.keys(this.form.controls).forEach((key) => {
        this.form.get(key)?.markAsTouched();
      });

      // Force validation update after restoration
      this.form.updateValueAndValidity();
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
