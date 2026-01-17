# Wizard Step Restoration Bug: Subscription Adds Defaults Before Data Restored

## Problem

When navigating back to the Classes step (Step 7) in the fund registration wizard, previously saved classes were not being restored correctly. The form showed duplicate classes or wrong default values instead of the user's saved data.

## Root Cause

The `setValue()` call for the `multiclasse` toggle during restoration was **missing `{ emitEvent: false }`**. This triggered the `handleMulticlasseChange` subscription which adds default classes when the array is empty - but the array was empty because it had just been cleared, and the saved classes hadn't been pushed yet.

### Sequence of Events

1. User fills Classes step with 2 custom classes, data is correctly saved to store
2. User navigates to step 8 or later
3. User navigates back to step 7 (Classes)
4. **New** ClassesStep component instance is created
5. Restoration effect runs:
   - Sets `isRestoring = true`
   - **Clears the FormArray**: `classesArray.clear()`
   - Calls `setValue(stepData.multiclasse)` **without `emitEvent: false`**
   - This triggers `handleMulticlasseChange(true)` subscription
   - Subscription sees `classesArray.length === 0` and **adds a default SENIOR class**
   - THEN the saved classes are pushed via `forEach`
   - Result: 3 classes instead of 2 (1 unwanted default + 2 saved)

### Problematic Code Pattern

```typescript
// In restoration effect
classesArray.clear();  // Array is now empty

// BUG: This triggers handleMulticlasseChange subscription!
this.form.get('multiclasse')?.setValue(stepData.multiclasse);
this.isMulticlasse.set(stepData.multiclasse);

// Saved classes are pushed AFTER the subscription already added a default
stepData.classes.forEach((classe) => {
  classesArray.push(this.createClasseFormGroup(classe, isFidc));
});
```

```typescript
// The subscription that causes the bug
this.form.get('multiclasse')?.valueChanges
  .pipe(takeUntilDestroyed(this.destroyRef))
  .subscribe((value) => {
    this.isMulticlasse.set(value ?? false);
    this.handleMulticlasseChange(value ?? false);  // Adds default class if array empty!
  });
```

```typescript
// handleMulticlasseChange adds defaults when array is empty
private handleMulticlasseChange(multiclasse: boolean): void {
  const classesArray = this.form.get('classes') as FormArray;

  if (multiclasse && classesArray.length === 0) {
    // This runs BEFORE the saved classes are restored!
    classesArray.push(this.createClasseFormGroup(this.getDefaultClasse(), false));
  }
}
```

## Solution

Add `{ emitEvent: false }` to all `setValue()` calls in the restoration effect to prevent subscriptions from firing during data restoration:

```typescript
// Fixed: Use emitEvent: false to prevent handleMulticlasseChange from triggering
this.form.get('multiclasse')?.setValue(stepData.multiclasse, { emitEvent: false });
this.isMulticlasse.set(stepData.multiclasse);

// Now classes can be restored without interference
stepData.classes.forEach((classe) => {
  classesArray.push(this.createClasseFormGroup(classe, isFidc));
});
```

## Key Insight

The `isRestoring` flag protects `valueChanges` subscription from saving to store, but it does NOT protect other subscriptions (like `handleMulticlasseChange`) from running side effects. Any subscription that performs actions based on form control values will still fire during restoration if `emitEvent: false` is not used.

## Detection Pattern

Look for this bug pattern in step components:

1. **Form has a toggle/select that controls dynamic content** (multiclasse, tipoFundo, etc.)
2. **A subscription on that control adds/removes/modifies other form controls**
3. **Restoration effect sets the toggle value without `emitEvent: false`**
4. **The subscription fires before the dependent data is restored**

### Grep Patterns to Find This Bug

```bash
# Find setValue calls without emitEvent: false in effect blocks
rg "setValue\([^)]+\)" --type ts -A 2 | grep -v "emitEvent.*false"

# Find valueChanges subscriptions that modify the form
rg "valueChanges.*subscribe.*\{" --type ts -A 10 | grep -E "(push|clear|removeAt|setControl)"

# Find restoration effects that set values
rg "isRestoring\s*=\s*true" --type ts -A 20 | grep "setValue"
```

### Code Pattern to Audit

```typescript
// SUSPICIOUS: setValue in restoration without emitEvent: false
if (stepData) {
  this.form.get('controlName')?.setValue(stepData.value);  // Missing { emitEvent: false }?
}

// CHECK: Does this control have a subscription that modifies form structure?
this.form.get('controlName')?.valueChanges.subscribe((value) => {
  if (value && someArray.length === 0) {
    someArray.push(defaultItem);  // This will fire before restoration completes!
  }
});
```

## When to Use `{ emitEvent: false }`

| Context | Use `emitEvent: false`? | Reason |
|---------|------------------------|--------|
| Restoration/initialization setValue | **Yes** | Prevent subscriptions from adding defaults before data loaded |
| setValue after clear/reset | **Yes** | Array/structure is temporarily empty during restoration |
| Programmatic changes in effects | **Yes** | These are not user actions |
| User-initiated actions | **No** | User changes should trigger dependent logic |

## Affected Components

Any step component that has:
- A FormArray with dynamic items
- A toggle/select that controls whether items are shown or defaults are added
- A subscription that auto-populates defaults when array is empty

### Known Affected Steps

- `classes-step.ts` - multiclasse toggle adds default SENIOR class (FIXED)
- `taxas-step.ts` - similar pattern with administracao fee (verify)
- `vinculos-step.ts` - similar pattern with vinculo types (verify)

## Debugging Tips

1. Add console logs to subscriptions that modify form structure:
   ```typescript
   .subscribe((value) => {
     console.log('[Step] handleToggleChange:', { value, arrayLength: array.length, isRestoring: this.isRestoring });
     // ...
   });
   ```

2. Log the array state before and after restoration:
   ```typescript
   console.log('[Step] Before restore:', classesArray.length);
   // ... restoration code ...
   console.log('[Step] After restore:', classesArray.length);
   ```

3. If array has more items than expected after restoration, a subscription is adding defaults.

## Related Files

- `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/classes-step/classes-step.ts`
- `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/wizard-store.ts`

## Related Documentation

- `wizard-step-restoration-overwrite.md` - Similar issue with effects overwriting store data

## Tags

`wizard`, `angular-forms`, `form-restoration`, `valueChanges`, `emitEvent`, `FormArray`, `subscription-side-effects`
