# Bug: Computed Signals Don't Track Reactive Form Values

## Problem

Conditional UI elements (fields appearing/disappearing based on checkbox state) were not working. The `limiteAlavancagem` field should appear when `permiteAlavancagem` checkbox is checked, but nothing happened.

## Root Cause

The implementation used `computed()` signals that read directly from form control values:

```typescript
// BROKEN - computed() only tracks signal dependencies, not form control values
readonly showLimiteAlavancagem = computed(() => {
  return this.form.get('permiteAlavancagem')?.value === true;
});
```

**Why this fails:**
- `computed()` in Angular only re-evaluates when its **signal dependencies** change
- `this.form.get('permiteAlavancagem')?.value` is a regular property read, NOT a signal
- The computed signal evaluates once at creation and never updates when the form changes

## Solution

Use `toSignal()` from `@angular/core/rxjs-interop` to convert form control `valueChanges` observables into signals:

```typescript
import { toSignal } from '@angular/core/rxjs-interop';
import { startWith } from 'rxjs/operators';

// Convert form control valueChanges to a signal
private readonly permiteAlavancagemValue = toSignal(
  this.form.get('permiteAlavancagem')!.valueChanges.pipe(
    startWith(this.form.get('permiteAlavancagem')!.value)
  ),
  { initialValue: false }
);

// Now computed() works correctly because it tracks a signal dependency
readonly showLimiteAlavancagem = computed(() => this.permiteAlavancagemValue() === true);
```

**Key points:**
1. `toSignal()` converts an Observable into a Signal
2. `startWith()` ensures the signal has the current value immediately (not just on first change)
3. `initialValue` provides a fallback before the observable emits
4. The form must be defined **before** the `toSignal()` calls (class field initialization order matters)

## FormArray Pattern

For components with `FormArray`, convert the entire form's `valueChanges` and map to the array:

```typescript
// For FormArray-based computed signals
private readonly itemsFormValue = toSignal(
  this.form.valueChanges.pipe(
    startWith(this.form.value),
    map((value) => (value.items ?? []) as Partial<ItemType>[])
  ),
  { initialValue: [] as Partial<ItemType>[] }
);

// Now computed signals can react to array changes
readonly itemsCount = computed(() => this.itemsFormValue().length);
readonly hasSpecialItem = computed(() =>
  this.itemsFormValue().some((item) => item.type === ItemType.SPECIAL)
);
```

**Why the whole form?** Individual FormArray controls don't have stable `valueChanges` observables since controls are added/removed dynamically.

## Form Validation Errors Pattern

For computed signals that check validation errors (e.g., `hasError()`), track `statusChanges`:

```typescript
// Convert statusChanges to signal for validation tracking
private readonly formStatus = toSignal(
  this.form.statusChanges.pipe(startWith(this.form.status)),
  { initialValue: this.form.status }
);

// Computed re-runs when validation state changes
readonly hasCustomError = computed(() => {
  this.formStatus(); // Triggers re-evaluation on status change
  return this.form.get('items')?.hasError('customValidator') ?? false;
});
```

**Why needed?** Validation errors change on `statusChanges`, not `valueChanges`. Without this, error-checking computed signals won't update.

## Angular Documentation Reference

From Angular's official documentation (`angular.dev`):
- Use `toSignal()` from `@angular/core/rxjs-interop` to convert Observables to Signals
- `computed()` only tracks signal dependencies, not regular property accesses
- Angular also has experimental "Signal Forms" (`@angular/forms/signals`) for native integration

## Files Affected

**Originally fixed:**
- `caracteristicas-step.ts` - Single control pattern

**Later discovered and fixed (2025-01-16):**
- `classificacao-step.ts` - Single control + effect dependency
- `parametros-cota-step.ts` - Single control converted from effect to computed
- `taxas-step.ts` - FormArray pattern
- `vinculos-step.ts` - FormArray pattern
- `classes-step.ts` - FormArray + statusChanges pattern
- `parametros-fidc-step.ts` - Multiple single controls

**Not affected (already correct):**
- `identificacao-step.ts`
- `prazos-step.ts`
- `documentos-step.ts`

## Pattern to Watch For

Any time you see `computed()` reading from:
- `this.form.get('fieldName')?.value` - Use single control toSignal pattern
- `this.formControl.value` - Use single control toSignal pattern
- `this.formArray.length` - Use FormArray pattern
- `this.formArray.controls.map(...)` - Use FormArray pattern
- `this.form.get('field')?.hasError(...)` - Use statusChanges pattern
- Any non-signal reactive form property

This is likely a bug. Convert to the appropriate `toSignal()` pattern.

## Common Mistake: Effects Reading Form Values

Effects have the same problem - they won't re-run when form values change:

```typescript
// BROKEN - effect won't re-run when form changes
effect(() => {
  const value = this.form.get('field')?.value;
  this.doSomething(value);
});

// FIXED - use toSignal dependency
private readonly fieldValue = toSignal(...);
effect(() => {
  const value = this.fieldValue();
  this.doSomething(value);
});
```

## Date

2025-01-15 (original), 2025-01-16 (expanded with FormArray and statusChanges patterns)
