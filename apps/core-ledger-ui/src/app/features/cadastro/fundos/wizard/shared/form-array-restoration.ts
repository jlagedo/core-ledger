/**
 * FormArray Restoration Helpers
 *
 * Utilities for restoring FormArray data in wizard steps.
 *
 * @see docs/aidebug/wizard-multiclasse-restoration-defaults.md
 */

import { FormArray, FormGroup } from '@angular/forms';
import { FormArrayRestorationConfig } from './wizard-restoration.types';

/**
 * Restores items to a FormArray, clearing existing items first.
 *
 * This utility ensures proper restoration order:
 * 1. Clear existing items
 * 2. Push restored items one by one
 *
 * @typeParam TItem - The type of items in the FormArray
 * @param config - Configuration for the restoration
 *
 * @example
 * ```typescript
 * restoreFormArray({
 *   array: this.form.get('prazos') as FormArray,
 *   createFormGroup: (prazo) => this.createPrazoFormGroup(prazo),
 *   items: stepData.prazos,
 * });
 * ```
 */
export function restoreFormArray<TItem>(config: FormArrayRestorationConfig<TItem>): void {
  const { array, createFormGroup, items } = config;

  // Clear existing items
  array.clear();

  // Push restored items
  items.forEach((item) => {
    array.push(createFormGroup(item));
  });
}

/**
 * Checks if a FormArray has items of a specific type.
 *
 * @typeParam TItem - The type of items in the FormArray
 * @param array - The FormArray to check
 * @param predicate - Function to test each item
 * @returns True if any item matches the predicate
 *
 * @example
 * ```typescript
 * const hasAplicacao = formArrayHas<FundoPrazo>(
 *   prazosArray,
 *   (prazo) => prazo.tipoOperacao === TipoOperacao.APLICACAO
 * );
 * ```
 */
export function formArrayHas<TItem>(
  array: FormArray,
  predicate: (item: Partial<TItem>) => boolean
): boolean {
  return array.controls.some((control) => {
    if (control instanceof FormGroup) {
      return predicate(control.value as Partial<TItem>);
    }
    return false;
  });
}

/**
 * Counts items in a FormArray matching a predicate.
 *
 * @typeParam TItem - The type of items in the FormArray
 * @param array - The FormArray to count
 * @param predicate - Function to test each item
 * @returns Number of items matching the predicate
 *
 * @example
 * ```typescript
 * const seniorCount = formArrayCount<FundoClasse>(
 *   classesArray,
 *   (classe) => classe.tipoClasseFidc === TipoClasseFidc.SENIOR
 * );
 * ```
 */
export function formArrayCount<TItem>(
  array: FormArray,
  predicate: (item: Partial<TItem>) => boolean
): number {
  return array.controls.filter((control) => {
    if (control instanceof FormGroup) {
      return predicate(control.value as Partial<TItem>);
    }
    return false;
  }).length;
}

/**
 * Marks all controls in a FormArray as touched.
 *
 * @param array - The FormArray to mark
 *
 * @example
 * ```typescript
 * markFormArrayAsTouched(this.form.get('prazos') as FormArray);
 * ```
 */
export function markFormArrayAsTouched(array: FormArray): void {
  array.controls.forEach((control) => {
    if (control instanceof FormGroup) {
      Object.keys(control.controls).forEach((key) => {
        control.get(key)?.markAsTouched();
      });
    } else {
      control.markAsTouched();
    }
  });
}

/**
 * Collects invalid fields from a FormArray for validation reporting.
 *
 * @param array - The FormArray to check
 * @param arrayName - The name of the array (used in field path)
 * @param excludeFields - Optional array of field names to exclude from validation
 * @returns Array of invalid field info objects
 *
 * @example
 * ```typescript
 * const invalidFields = collectFormArrayInvalidFields(
 *   this.form.get('prazos') as FormArray,
 *   'prazos'
 * );
 * // Returns: [{ field: 'prazos[0].tipoOperacao', errors: ['required'] }]
 *
 * // With excluded fields:
 * const invalidFields = collectFormArrayInvalidFields(
 *   this.form.get('vinculos') as FormArray,
 *   'vinculos',
 *   ['searchTerm']  // Exclude searchTerm from validation
 * );
 * ```
 */
export function collectFormArrayInvalidFields(
  array: FormArray,
  arrayName: string,
  excludeFields: string[] = []
): Array<{ field: string; errors: string[] }> {
  const invalidFields: Array<{ field: string; errors: string[] }> = [];

  array.controls.forEach((group, index) => {
    if (group instanceof FormGroup) {
      Object.keys(group.controls).forEach((key) => {
        // Skip excluded fields
        if (excludeFields.includes(key)) {
          return;
        }

        const control = group.get(key);
        if (control && control.invalid) {
          const fieldErrors: string[] = [];
          if (control.errors) {
            Object.keys(control.errors).forEach((errorKey) => {
              fieldErrors.push(errorKey);
            });
          }
          invalidFields.push({
            field: `${arrayName}[${index}].${key}`,
            errors: fieldErrors,
          });
        }
      });
    }
  });

  return invalidFields;
}
