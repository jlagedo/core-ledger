/**
 * Wizard Shared Utilities
 *
 * Barrel export for centralized wizard restoration pattern utilities.
 */

// Types
export type {
  WizardStoreInstance,
  AsyncValidatorBypassConfig,
  RestorationEffectConfig,
  RestorationEffectResult,
  FormArrayRestorationConfig,
} from './wizard-restoration.types';

// Core restoration utilities
export {
  createRestorationEffect,
  createReadOnlyRestorationEffect,
} from './wizard-restoration';

// FormArray helpers
export {
  restoreFormArray,
  formArrayHas,
  formArrayCount,
  markFormArrayAsTouched,
  collectFormArrayInvalidFields,
} from './form-array-restoration';

// RxJS operators
export {
  withRestorationGuard,
  withRestorationGuardFn,
} from './restoration-guard';
