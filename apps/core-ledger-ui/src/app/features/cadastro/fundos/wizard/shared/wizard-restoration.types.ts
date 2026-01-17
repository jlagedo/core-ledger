/**
 * Wizard Restoration Types
 *
 * Type definitions for centralized wizard step restoration utilities.
 */

import { Signal } from '@angular/core';
import { AbstractControl, FormGroup, FormArray } from '@angular/forms';
import { WizardStepConfig, WizardStepId } from '../models/wizard.model';

/**
 * Type representing the WizardStore instance.
 * Uses a minimal interface to avoid circular dependencies.
 */
export interface WizardStoreInstance {
  /** Signal returning step data keyed by step key */
  stepData: Signal<Record<string, unknown>>;
  /** Signal returning the current data version (increments on draft restoration) */
  dataVersion: Signal<number>;
  /** Signal returning the set of completed step IDs */
  completedSteps: Signal<Set<WizardStepId>>;
  /** Method to update step data in the store */
  setStepData<T>(stepKey: string, data: T): void;
  /** Method to update step validation state */
  setStepValidation(stepId: WizardStepId, validation: {
    isValid: boolean;
    isDirty: boolean;
    errors: string[];
    invalidFields?: Array<{ field: string; errors: string[] }>;
  }): void;
  /** Method to mark a step as complete */
  markStepComplete(stepId: WizardStepId): void;
  /** Method to mark a step as incomplete */
  markStepIncomplete(stepId: WizardStepId): void;
}

/**
 * Configuration for async validator bypass during restoration.
 *
 * Used to prevent async validators (e.g., CNPJ uniqueness check) from
 * re-triggering when restoring previously validated data.
 *
 * @example
 * ```typescript
 * asyncValidatorBypass: {
 *   controls: [form.get('cnpj')!],
 *   shouldBypass: () => {
 *     const completedSteps = wizardStore.completedSteps();
 *     const stepData = wizardStore.stepData()[stepConfig().key];
 *     return completedSteps.has(stepConfig().id) && !!stepData;
 *   },
 * }
 * ```
 */
export interface AsyncValidatorBypassConfig {
  /** Controls that have async validators to bypass */
  controls: AbstractControl[];
  /** Function that returns true when async validation should be bypassed */
  shouldBypass: () => boolean;
}

/**
 * Configuration for createRestorationEffect().
 *
 * @typeParam TFormData - The type of data stored/restored for this step
 */
export interface RestorationEffectConfig<TFormData> {
  /**
   * Signal returning the current step configuration.
   * Typically: `() => this.stepConfig()`
   */
  stepConfig: () => WizardStepConfig;

  /**
   * The wizard store instance.
   * Typically: `this.wizardStore` (injected via `inject(WizardStore)`)
   */
  wizardStore: WizardStoreInstance;

  /**
   * The form to restore. Can be a FormGroup or FormArray.
   */
  form: FormGroup | FormArray;

  /**
   * Function to reset the form to its default state.
   *
   * @example
   * ```typescript
   * resetForm: () => this.form.reset({
   *   campo1: null,
   *   campo2: false,
   * })
   * ```
   *
   * For FormArray steps, this should clear the array:
   * ```typescript
   * resetForm: () => (this.form.get('items') as FormArray).clear()
   * ```
   */
  resetForm: () => void;

  /**
   * Function to restore form data from the store.
   *
   * **CRITICAL**: You MUST use `{ emitEvent: false }` with patchValue/setValue
   * to prevent valueChanges from triggering store updates during restoration.
   *
   * @example
   * ```typescript
   * restoreData: (data) => {
   *   this.form.patchValue(this.prepareDataForForm(data), { emitEvent: false });
   *   this.form.markAsDirty();
   * }
   * ```
   *
   * For FormArray steps:
   * ```typescript
   * restoreData: (data) => {
   *   // Set toggles BEFORE pushing items to avoid triggering default creation
   *   this.form.get('multiclasse')?.setValue(data.multiclasse, { emitEvent: false });
   *   // Then push restored items
   *   data.items.forEach(item => itemsArray.push(this.createFormGroup(item)));
   * }
   * ```
   */
  restoreData: (data: TFormData) => void;

  /**
   * Function to update step validation in the store.
   * Typically: `() => this.updateStepValidation()`
   */
  updateStepValidation: () => void;

  /**
   * Optional function to create default data when no saved data exists.
   * Used primarily by FormArray steps that need default items.
   *
   * @example
   * ```typescript
   * createDefaultData: () => {
   *   const array = this.form.get('prazos') as FormArray;
   *   array.push(this.createPrazoFormGroup(createPrazoAplicacao()));
   *   array.push(this.createPrazoFormGroup(createPrazoResgate()));
   * }
   * ```
   */
  createDefaultData?: () => void;

  /**
   * Optional function to mark all form controls as touched.
   * If not provided, uses default behavior for FormGroup/FormArray.
   */
  markAllAsTouched?: () => void;

  /**
   * Optional async validator bypass configuration.
   * Use this for steps with async validators (e.g., Identificacao with CNPJ check).
   */
  asyncValidatorBypass?: AsyncValidatorBypassConfig;

  /**
   * Optional async restoration handler.
   * Use this for steps that need to perform async operations during restoration
   * (e.g., Documentos loading files from IndexedDB).
   *
   * When provided, the restoration will await this function before clearing
   * the isRestoring flag and calling updateStepValidation.
   *
   * @example
   * ```typescript
   * asyncRestoration: async (data) => {
   *   this.documentos.set(data);
   *   await this.loadFilesFromIndexedDB();
   * }
   * ```
   */
  asyncRestoration?: (data: TFormData) => Promise<void>;
}

/**
 * Result returned by createRestorationEffect().
 */
export interface RestorationEffectResult {
  /**
   * Signal indicating whether the form is currently being restored.
   * Use this to guard valueChanges subscriptions from triggering store updates.
   *
   * @example
   * ```typescript
   * this.form.valueChanges.pipe(
   *   takeUntilDestroyed(this.destroyRef),
   *   withRestorationGuard(isRestoring),
   * ).subscribe((value) => {
   *   this.wizardStore.setStepData(stepConfig().key, value);
   * });
   * ```
   */
  isRestoring: Signal<boolean>;
}

/**
 * Configuration for FormArray restoration.
 *
 * @typeParam TItem - The type of items in the FormArray
 */
export interface FormArrayRestorationConfig<TItem> {
  /** The FormArray to restore */
  array: FormArray;
  /** Function to create a FormGroup for an item */
  createFormGroup: (item?: Partial<TItem>) => FormGroup;
  /** Items to restore */
  items: TItem[];
}
