/**
 * Restoration Guard RxJS Operator
 *
 * Guards valueChanges subscriptions from firing during restoration.
 *
 * @see docs/aidebug/wizard-step-restoration-overwrite.md
 */

import { Signal } from '@angular/core';
import { MonoTypeOperatorFunction, filter } from 'rxjs';

/**
 * RxJS operator that filters out emissions while isRestoring is true.
 *
 * Use this operator in valueChanges subscriptions to prevent store updates
 * during form restoration.
 *
 * @param isRestoring - Signal indicating whether restoration is in progress
 * @returns RxJS operator that filters emissions
 *
 * @example
 * ```typescript
 * const { isRestoring } = createRestorationEffect<MyFormData>({...});
 *
 * this.form.valueChanges.pipe(
 *   takeUntilDestroyed(this.destroyRef),
 *   withRestorationGuard(isRestoring),
 * ).subscribe((value) => {
 *   // This won't fire during restoration
 *   this.wizardStore.setStepData(this.stepConfig().key, value);
 * });
 * ```
 */
export function withRestorationGuard<T>(isRestoring: Signal<boolean>): MonoTypeOperatorFunction<T> {
  return filter(() => !isRestoring());
}

/**
 * Alternative form using a getter function instead of a signal.
 *
 * Useful when working with class properties that aren't signals.
 *
 * @param isRestoringFn - Function that returns the restoration state
 * @returns RxJS operator that filters emissions
 *
 * @example
 * ```typescript
 * // With a private property
 * private isRestoring = false;
 *
 * this.form.valueChanges.pipe(
 *   takeUntilDestroyed(this.destroyRef),
 *   withRestorationGuardFn(() => this.isRestoring),
 * ).subscribe((value) => {
 *   this.wizardStore.setStepData(this.stepConfig().key, value);
 * });
 * ```
 */
export function withRestorationGuardFn<T>(isRestoringFn: () => boolean): MonoTypeOperatorFunction<T> {
  return filter(() => !isRestoringFn());
}
