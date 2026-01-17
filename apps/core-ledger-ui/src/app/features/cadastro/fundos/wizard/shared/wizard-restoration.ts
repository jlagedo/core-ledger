/**
 * Wizard Restoration Utilities
 *
 * Centralized restoration pattern for wizard step components.
 * Reduces boilerplate (~40-60 lines per step) while maintaining flexibility.
 *
 * @see docs/aidebug/wizard-step-restoration-overwrite.md
 * @see docs/aidebug/wizard-multiclasse-restoration-defaults.md
 * @see docs/aidebug/wizard-async-validator-restoration.md
 */

import { effect, signal, untracked, Signal } from '@angular/core';
import { FormGroup, FormArray } from '@angular/forms';
import { WizardStepId } from '../models/wizard.model';
import {
  RestorationEffectConfig,
  RestorationEffectResult,
  AsyncValidatorBypassConfig,
} from './wizard-restoration.types';

/**
 * Creates an effect that handles wizard step restoration from the store.
 *
 * This function encapsulates the common restoration pattern:
 * 1. Track step ID and dataVersion for deduplication
 * 2. Set isRestoring flag to prevent store updates during restoration
 * 3. Reset form → patch data → update validation
 *
 * **IMPORTANT**: The `restoreData` callback MUST use `{ emitEvent: false }` with
 * `patchValue()` and `setValue()` to prevent triggering valueChanges subscriptions
 * that would overwrite the store data.
 *
 * @typeParam TFormData - The type of data stored/restored for this step
 * @param config - Configuration for the restoration effect
 * @returns Object containing the isRestoring signal
 *
 * @example Simple form step:
 * ```typescript
 * constructor() {
 *   const { isRestoring } = createRestorationEffect<CaracteristicasFormData>({
 *     stepConfig: () => this.stepConfig(),
 *     wizardStore: this.wizardStore,
 *     form: this.form,
 *     resetForm: () => this.form.reset({
 *       condominio: null,
 *       prazo: null,
 *     }),
 *     restoreData: (data) => {
 *       this.form.patchValue(this.prepareDataForForm(data), { emitEvent: false });
 *       this.form.markAsDirty();
 *     },
 *     updateStepValidation: () => this.updateStepValidation(),
 *   });
 *
 *   // Guard valueChanges with isRestoring
 *   this.form.valueChanges.pipe(
 *     takeUntilDestroyed(this.destroyRef),
 *     withRestorationGuard(isRestoring),
 *   ).subscribe((value) => {
 *     this.wizardStore.setStepData(stepConfig().key, this.prepareDataForStore(value));
 *   });
 * }
 * ```
 *
 * @example FormArray step with defaults:
 * ```typescript
 * const { isRestoring } = createRestorationEffect<PrazosFormData>({
 *   stepConfig: () => this.stepConfig(),
 *   wizardStore: this.wizardStore,
 *   form: this.form,
 *   resetForm: () => {
 *     const array = this.form.get('prazos') as FormArray;
 *     array.clear();
 *   },
 *   restoreData: (data) => {
 *     const array = this.form.get('prazos') as FormArray;
 *     data.prazos.forEach(prazo => array.push(this.createPrazoFormGroup(prazo)));
 *   },
 *   createDefaultData: () => {
 *     const array = this.form.get('prazos') as FormArray;
 *     array.push(this.createPrazoFormGroup(createPrazoAplicacao()));
 *     array.push(this.createPrazoFormGroup(createPrazoResgate()));
 *   },
 *   updateStepValidation: () => this.updateStepValidation(),
 * });
 * ```
 *
 * @example Step with async validator bypass (Identificacao):
 * ```typescript
 * const { isRestoring } = createRestorationEffect<IdentificacaoFormData>({
 *   stepConfig: () => this.stepConfig(),
 *   wizardStore: this.wizardStore,
 *   form: this.form,
 *   resetForm: () => this.form.reset({}),
 *   restoreData: (data) => {
 *     this.form.patchValue(this.prepareDataForForm(data), { emitEvent: false });
 *     this.form.markAsDirty();
 *   },
 *   updateStepValidation: () => this.updateStepValidation(),
 *   asyncValidatorBypass: {
 *     controls: [this.form.get('cnpj')!],
 *     shouldBypass: () => {
 *       const completedSteps = this.wizardStore.completedSteps();
 *       const stepData = this.wizardStore.stepData()[this.stepConfig().key];
 *       return completedSteps.has(this.stepConfig().id) && !!stepData;
 *     },
 *   },
 * });
 * ```
 */
export function createRestorationEffect<TFormData>(
  config: RestorationEffectConfig<TFormData>
): RestorationEffectResult {
  // Internal state for deduplication
  let lastLoadedStepId: WizardStepId | null = null;
  let lastDataVersion = -1;

  // Signal for restoration state (exposed to consumers)
  const isRestoring = signal(false);

  effect(() => {
    const stepConfig = config.stepConfig();
    const stepId = stepConfig.id;
    const dataVersion = config.wizardStore.dataVersion();

    // Skip if same step AND same dataVersion (no changes)
    const sameStep = lastLoadedStepId === stepId;
    const sameVersion = lastDataVersion === dataVersion;
    if (sameStep && sameVersion) {
      return;
    }
    lastLoadedStepId = stepId;
    lastDataVersion = dataVersion;

    // Set restoration flag to prevent store updates
    isRestoring.set(true);

    // Reset form to default state
    config.resetForm();

    // Get step data from store (untracked to avoid reactive dependency)
    const stepData = untracked(
      () => config.wizardStore.stepData()[stepConfig.key] as TFormData | undefined
    );

    // Handle async restoration path
    if (config.asyncRestoration && stepData) {
      untracked(() => {
        config.asyncRestoration!(stepData).finally(() => {
          isRestoring.set(false);
          config.updateStepValidation();
        });
      });
      return;
    }

    // Sync restoration path
    if (stepData) {
      // Handle async validator bypass if configured
      if (config.asyncValidatorBypass) {
        handleAsyncValidatorBypass(
          config.asyncValidatorBypass,
          config.form,
          config.wizardStore,
          stepId,
          stepData,
          config.restoreData,
          config.markAllAsTouched
        );
      } else {
        // Standard restoration: patch form with saved data
        config.restoreData(stepData);
      }
    } else if (config.createDefaultData) {
      // No saved data - create defaults if configured
      config.createDefaultData();
    }

    // Clear restoration flag
    isRestoring.set(false);

    // Mark all fields as touched to show validation state
    if (config.markAllAsTouched) {
      config.markAllAsTouched();
    } else {
      markAllControlsAsTouched(config.form);
    }

    // Update form validity
    config.form.updateValueAndValidity();

    // Update step validation in store
    untracked(() => config.updateStepValidation());
  });

  return { isRestoring };
}

/**
 * Handles async validator bypass during restoration.
 *
 * This prevents async validators from re-triggering when restoring
 * previously validated data (e.g., CNPJ uniqueness check).
 *
 * Sequence:
 * 1. Store original async validators
 * 2. Clear async validators
 * 3. Call updateValueAndValidity to apply the removal
 * 4. Patch form data
 * 5. Restore validators WITHOUT calling updateValueAndValidity
 */
function handleAsyncValidatorBypass<TFormData>(
  bypassConfig: AsyncValidatorBypassConfig,
  form: FormGroup | FormArray,
  wizardStore: RestorationEffectConfig<TFormData>['wizardStore'],
  stepId: WizardStepId,
  stepData: TFormData,
  restoreData: (data: TFormData) => void,
  markAllAsTouched?: () => void
): void {
  if (bypassConfig.shouldBypass()) {
    // Store original validators
    const originalValidators = bypassConfig.controls.map((control) => control.asyncValidator);

    // Clear async validators
    bypassConfig.controls.forEach((control) => {
      control.clearAsyncValidators();
      control.updateValueAndValidity({ emitEvent: false });
    });

    // Update form validity
    form.updateValueAndValidity({ emitEvent: false });

    // Restore data
    restoreData(stepData);

    // Mark as touched
    if (markAllAsTouched) {
      markAllAsTouched();
    } else {
      markAllControlsAsTouched(form);
    }

    // Restore async validators WITHOUT triggering validation
    bypassConfig.controls.forEach((control, index) => {
      const originalValidator = originalValidators[index];
      if (originalValidator) {
        control.setAsyncValidators(originalValidator);
        // IMPORTANT: Don't call updateValueAndValidity() here!
      }
    });

    // Directly set validation state
    wizardStore.setStepValidation(stepId, {
      isValid: form.valid,
      isDirty: true,
      errors: [],
      invalidFields: [],
    });

    if (form.valid) {
      wizardStore.markStepComplete(stepId);
    }
  } else {
    // Normal restoration - run all validators
    restoreData(stepData);

    if (markAllAsTouched) {
      markAllAsTouched();
    } else {
      markAllControlsAsTouched(form);
    }

    form.updateValueAndValidity();
  }
}

/**
 * Marks all controls in a form as touched.
 * Works for both FormGroup and FormArray.
 */
function markAllControlsAsTouched(form: FormGroup | FormArray): void {
  if (form instanceof FormGroup) {
    Object.keys(form.controls).forEach((key) => {
      const control = form.get(key);
      if (control) {
        control.markAsTouched();
        if (control instanceof FormGroup || control instanceof FormArray) {
          markAllControlsAsTouched(control);
        }
      }
    });
  } else if (form instanceof FormArray) {
    form.controls.forEach((control) => {
      if (control instanceof FormGroup) {
        Object.keys(control.controls).forEach((key) => {
          control.get(key)?.markAsTouched();
        });
      } else {
        control.markAsTouched();
      }
    });
  }
}

/**
 * Creates a simple restoration effect for read-only steps (like Revisao).
 *
 * This is a lightweight version that only handles deduplication and
 * validation updates, without form restoration logic.
 *
 * @param config - Configuration for the effect
 *
 * @example
 * ```typescript
 * createReadOnlyRestorationEffect({
 *   stepConfig: () => this.stepConfig(),
 *   wizardStore: this.wizardStore,
 *   updateStepValidation: () => this.updateStepValidation(),
 * });
 * ```
 */
export function createReadOnlyRestorationEffect(config: {
  stepConfig: () => { id: WizardStepId };
  wizardStore: { dataVersion: Signal<number> };
  updateStepValidation: () => void;
}): void {
  let lastLoadedStepId: WizardStepId | null = null;
  let lastDataVersion = -1;

  effect(() => {
    const stepConfig = config.stepConfig();
    const stepId = stepConfig.id;
    const dataVersion = config.wizardStore.dataVersion();

    const sameStep = lastLoadedStepId === stepId;
    const sameVersion = lastDataVersion === dataVersion;
    if (sameStep && sameVersion) {
      return;
    }
    lastLoadedStepId = stepId;
    lastDataVersion = dataVersion;

    untracked(() => config.updateStepValidation());
  });
}
