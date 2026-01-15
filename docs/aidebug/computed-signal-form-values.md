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

## Angular Documentation Reference

From Angular's official documentation (`angular.dev`):
- Use `toSignal()` from `@angular/core/rxjs-interop` to convert Observables to Signals
- `computed()` only tracks signal dependencies, not regular property accesses
- Angular also has experimental "Signal Forms" (`@angular/forms/signals`) for native integration

## Files Affected

- `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/caracteristicas-step/caracteristicas-step.ts`

## Pattern to Watch For

Any time you see `computed()` reading from:
- `this.form.get('fieldName')?.value`
- `this.formControl.value`
- Any non-signal reactive form property

This is likely a bug. Convert to `toSignal()` pattern instead.

## Date

2025-01-15
