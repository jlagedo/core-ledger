# Wizard Async Validator Restoration Issue

**Date:** 2025-01-15
**Component:** `identificacao-step.ts` (Fund Registration Wizard Step 1)
**Issue:** CNPJ field showing infinite loading spinner when navigating back in wizard

## Problem Description

When a user completed step 1 of the fund registration wizard, navigated to step 2, and then navigated back to step 1, the CNPJ field would:
1. Show an infinite loading spinner (validation pending)
2. Never complete validation
3. Prevent the user from proceeding (Next button disabled)

### Debug Output Observed
```
Step Validation:
isValid: false
isDirty: true
Button disabled because: Step validation isValid = false
```

## Root Cause Analysis

### Issue 1: Draft Restoration Doesn't Persist `stepValidation`

The wizard store's `loadDraft()` method restores:
- `stepData` (form values)
- `completedSteps` (Set of completed step IDs)
- `currentStep`

But it does NOT restore `stepValidation` state. This meant:
```typescript
// This was always false when restoring from draft
const storedValidation = this.wizardStore.stepValidation()[stepId];
const wasAlreadyValidated = storedValidation?.isValid && storedValidation?.isDirty;
// Result: wasAlreadyValidated = false (because stepValidation was not persisted)
```

**Fix:** Check `completedSteps` instead of `stepValidation`:
```typescript
const completedSteps = this.wizardStore.completedSteps();
const wasAlreadyCompleted = completedSteps.has(stepId);
const hasStepData = !!stepData;

if (wasAlreadyCompleted && hasStepData) {
  // Bypass async validation
}
```

### Issue 2: `updateOn: 'blur'` Interferes with Programmatic Validation

Initially, we added `updateOn: 'blur'` to prevent excessive validation:
```typescript
cnpj: ['', {
  validators: [Validators.required, cnpjValidator()],
  asyncValidators: [this.cnpjUniqueValidator.validate()],
  updateOn: 'blur', // THIS CAUSED PROBLEMS
}]
```

**Problem:** When `updateOn: 'blur'` is set, calling `updateValueAndValidity()` programmatically doesn't properly update the control status. The control remains in `PENDING` state even after clearing async validators.

**Debug output with `updateOn: 'blur'`:**
```javascript
{
  cnpjStatus: "PENDING",  // Still PENDING even after clearing validators!
  cnpjErrors: null,
  formValid: false
}
```

**Fix:** Remove `updateOn: 'blur'`. The async validator already has built-in debouncing with `timer(500)`.

### Issue 3: Incorrect Order of Operations for Validator Removal

Angular documentation states:
> "When you add or remove a validator at run time, you must call `updateValueAndValidity()` for the new validation to take effect."

**Wrong approach:**
```typescript
cnpjControl.clearAsyncValidators();
this.form.updateValueAndValidity(); // Only updates form, not the control's validator state
```

**Correct approach:**
```typescript
cnpjControl.clearAsyncValidators();
cnpjControl.updateValueAndValidity({ emitEvent: false }); // Apply removal to control
this.form.updateValueAndValidity({ emitEvent: false });   // Then update form
```

## Final Solution

### Location
`apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/identificacao-step/identificacao-step.ts`

### Code Pattern for Bypassing Async Validators on Restoration

```typescript
// In the effect that handles step restoration:

// Check if step was already completed (from draft or previous navigation)
const completedSteps = untracked(() => this.wizardStore.completedSteps());
const wasAlreadyCompleted = completedSteps.has(stepId);
const hasStepData = !!stepData;

if (stepData) {
  // Patch form values without triggering subscriptions
  this.form.patchValue(formValue, { emitEvent: false });
  this.form.markAsDirty();

  // Mark all fields as touched for validation display
  Object.keys(this.form.controls).forEach((key) => {
    this.form.get(key)?.markAsTouched();
  });

  if (wasAlreadyCompleted && hasStepData) {
    // BYPASS ASYNC VALIDATION for already-completed steps
    const controlWithAsyncValidator = this.form.get('fieldName')!;
    const originalAsyncValidators = controlWithAsyncValidator.asyncValidator;

    // Step 1: Clear async validators
    controlWithAsyncValidator.clearAsyncValidators();

    // Step 2: Apply removal by calling updateValueAndValidity on the CONTROL
    controlWithAsyncValidator.updateValueAndValidity({ emitEvent: false });

    // Step 3: Update entire form (only sync validators run now)
    this.form.updateValueAndValidity({ emitEvent: false });

    // Step 4: Restore async validators for future edits (without triggering them)
    if (originalAsyncValidators) {
      controlWithAsyncValidator.setAsyncValidators(originalAsyncValidators);
      // IMPORTANT: Do NOT call updateValueAndValidity() here!
    }

    // Step 5: Manually update store validation state
    this.wizardStore.setStepValidation(stepId, {
      isValid: this.form.valid,
      isDirty: true,
      errors: [],
      invalidFields: [],
    });

    if (this.form.valid) {
      this.wizardStore.markStepComplete(stepId);
    }
  } else {
    // Fresh validation - run all validators normally
    this.form.updateValueAndValidity();
  }
}
```

### Key Points

1. **Do NOT use `updateOn: 'blur'`** for controls that need programmatic validation
2. **Check `completedSteps`** not `stepValidation` for draft restoration scenarios
3. **Call `updateValueAndValidity()` on the control** after clearing validators, not just the form
4. **Do NOT call `updateValueAndValidity()` after restoring** async validators - this would trigger them
5. **Use `{ emitEvent: false }`** to prevent triggering valueChanges/statusChanges subscriptions during restoration

## Async Validator Best Practices

### Built-in Debouncing (Recommended)
Instead of `updateOn: 'blur'`, use `timer()` in the async validator itself:

```typescript
// In cnpj-unique.validator.ts
validate(): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    const value = control.value;

    if (!value || value.length < 14) {
      return of(null);
    }

    // Built-in 500ms debounce - prevents excessive API calls
    return timer(500).pipe(
      switchMap(() => this.apiService.checkUnique(value)),
      map(response => response.exists ? { duplicate: true } : null),
      catchError(() => of(null))
    );
  };
}
```

### Debugging Form Validation Issues

Add this debug code to understand form state:
```typescript
console.log('Form state:', {
  formValid: this.form.valid,
  formStatus: this.form.status,
  formErrors: this.form.errors,
  controlStatuses: Object.keys(this.form.controls).map(key => ({
    key,
    status: this.form.get(key)?.status,
    errors: this.form.get(key)?.errors,
  })),
});
```

## Related Files

- `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/identificacao-step/identificacao-step.ts` - Main fix location
- `apps/core-ledger-ui/src/app/shared/validators/cnpj-unique.validator.ts` - Async validator with debouncing
- `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/wizard-store.ts` - Store (loadDraft doesn't persist stepValidation)
- `docs/specs/ui/wizard_research.md` - General wizard patterns research

## Angular Documentation References

- [AbstractControl.clearAsyncValidators()](https://angular.dev/api/forms/AbstractControl#clearAsyncValidators)
- [AbstractControl.updateValueAndValidity()](https://angular.dev/api/forms/AbstractControl#updateValueAndValidity)
- [FormControlStatus](https://angular.dev/api/forms/FormControlStatus) - VALID | INVALID | PENDING | DISABLED

## Checklist for Applying to Other Fields

When adding async validators to wizard steps:

- [ ] Use `timer()` debouncing in the validator (not `updateOn: 'blur'`)
- [ ] In step restoration effect, check `completedSteps.has(stepId)`
- [ ] Clear async validators before `updateValueAndValidity()`
- [ ] Call `control.updateValueAndValidity()` after clearing (not just form)
- [ ] Restore async validators WITHOUT calling `updateValueAndValidity()`
- [ ] Use `{ emitEvent: false }` to prevent subscription triggers
- [ ] Manually update store validation state after restoration
