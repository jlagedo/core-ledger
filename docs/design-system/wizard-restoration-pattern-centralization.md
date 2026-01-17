# Wizard Restoration Pattern Centralization

## Implementation Status: COMPLETE

All 12 wizard step components have been successfully migrated to use the centralized restoration utilities.

| Step | Type | Status | Migration Approach |
|------|------|--------|-------------------|
| caracteristicas-step | Simple form | ✅ Complete | `createRestorationEffect()` |
| classificacao-step | Simple form | ✅ Complete | `createRestorationEffect()` |
| prazos-step | FormArray | ✅ Complete | `createRestorationEffect()` with `createDefaultData` |
| classes-step | FormArray | ✅ Complete | `createRestorationEffect()` with `createDefaultData` |
| identificacao-step | Async validator | ✅ Complete | `createRestorationEffect()` with `asyncValidatorBypass` |
| parametros-cota-step | Preview signals | ✅ Complete | `createRestorationEffect()` with signal updates |
| parametros-fidc-step | FIDC defaults | ✅ Complete | `createRestorationEffect()` |
| taxas-step | FormArray | ✅ Complete | `createRestorationEffect()` with `createDefaultData` |
| vinculos-step | Required items | ✅ Complete | `createRestorationEffect()` with `createDefaultData` |
| documentos-step | Async IndexedDB | ✅ Complete | `createRestorationEffect()` with `asyncRestoration` |
| revisao-step | Read-only | ✅ Complete | `createReadOnlyRestorationEffect()` |
| placeholder-step | Simple form | ✅ Complete | `createRestorationEffect()` |

**Build Status:** ✅ Successful (`nx build core-ledger-ui`)

---

## Overview

Centralize the restoration pattern used across all 12 fund wizard step components into reusable utility functions, reducing boilerplate (~40-60 lines per step) while maintaining flexibility for step-specific needs.

## Current State

Each step component duplicates:
1. `private isRestoring = false` flag
2. `private lastLoadedStepId` and `lastDataVersion` for deduplication
3. An `effect()` with identical structure:
   - Check same step/version → skip
   - Set isRestoring → reset form → patch data → updateValueAndValidity → clear isRestoring → updateStepValidation

**Example** (caracteristicas-step.ts lines 86-298): 200+ lines of restoration logic.

## Proposed Solution: Utility Functions + Composition Helpers

Create `createRestorationEffect()` that encapsulates the common pattern while allowing step-specific customization.

### Why Utility Functions (not Base Class)

- **Flexibility**: Steps keep their forms and handle step-specific logic
- **Composition**: Aligns with Angular's functional patterns (`inject()`, `effect()`)
- **Type Safety**: Generics for form data types
- **Testable**: Functions are easy to unit test in isolation

## Implementation Plan

### Phase 1: Create Core Utilities

**New Files:**

1. `wizard/shared/wizard-restoration.types.ts` - Type definitions
2. `wizard/shared/wizard-restoration.ts` - `createRestorationEffect()` function
3. `wizard/shared/form-array-restoration.ts` - `restoreFormArray()` helper
4. `wizard/shared/restoration-guard.ts` - `withRestorationGuard()` RxJS operator
5. `wizard/shared/index.ts` - Barrel export

**Core Function Signature:**

```typescript
function createRestorationEffect<TFormData>(config: {
  stepConfig: () => WizardStepConfig;
  wizardStore: typeof WizardStore;
  form: FormGroup | FormArray;
  resetForm: () => void;
  restoreData: (data: TFormData) => void;
  updateStepValidation: () => void;
  createDefaultData?: () => void;           // For FormArray steps
  asyncRestoration?: (data: TFormData) => Promise<void>;  // For Documentos
  asyncValidatorBypass?: { ... };           // For Identificacao
}): { isRestoring: Signal<boolean> }
```

### Phase 2: Migrate Steps (Order by Complexity)

| Order | Step | Type | Special Handling |
|-------|------|------|------------------|
| 1 | caracteristicas | Simple form | None |
| 2 | classificacao | Simple form | None |
| 3 | parametros-cota | Simple form | Preview signals |
| 4 | parametros-fidc | Simple form | FIDC defaults |
| 5 | prazos | FormArray | Default items |
| 6 | taxas | FormArray | Default items |
| 7 | classes | FormArray | FIDC SENIOR class |
| 8 | vinculos | FormArray | Required items |
| 9 | identificacao | Simple form | Async validator bypass |
| 10 | documentos | Async | IndexedDB loading |
| 11 | revisao | Read-only | Only deduplication |
| 12 | placeholder | Simple form | None |

### Phase 3: Testing

- Unit tests for utility functions
- Integration tests for each migrated step (fresh load, draft restoration, navigation)

## Files Created

```
apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/shared/
├── index.ts                      # Barrel export
├── wizard-restoration.types.ts   # Type definitions
├── wizard-restoration.ts         # createRestorationEffect()
├── form-array-restoration.ts     # restoreFormArray()
└── restoration-guard.ts          # withRestorationGuard()
```

## Files Modified

All step components in:
```
apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/
├── caracteristicas-step/caracteristicas-step.ts
├── classificacao-step/classificacao-step.ts
├── classes-step/classes-step.ts
├── documentos-step/documentos-step.ts
├── identificacao-step/identificacao-step.ts
├── parametros-cota-step/parametros-cota-step.ts
├── parametros-fidc-step/parametros-fidc-step.ts
├── placeholder-step/placeholder-step.ts
├── prazos-step/prazos-step.ts
├── revisao-step/revisao-step.ts
├── taxas-step/taxas-step.ts
└── vinculos-step/vinculos-step.ts
```

## Example Migration

**Before** (caracteristicas-step.ts - ~50 lines):
```typescript
private lastLoadedStepId: WizardStepId | null = null;
private lastDataVersion = -1;
private isRestoring = false;

effect(() => {
  const stepConfig = this.stepConfig();
  const dataVersion = this.wizardStore.dataVersion();

  const sameStep = this.lastLoadedStepId === stepConfig.id;
  const sameVersion = this.lastDataVersion === dataVersion;
  if (sameStep && sameVersion) return;

  this.lastLoadedStepId = stepConfig.id;
  this.lastDataVersion = dataVersion;
  this.isRestoring = true;

  this.form.reset({ ... });
  const stepData = untracked(() => this.wizardStore.stepData()[stepConfig.key]);
  if (stepData) {
    this.form.patchValue(this.prepareDataForForm(stepData));
    this.form.markAsDirty();
  }
  this.isRestoring = false;

  Object.keys(this.form.controls).forEach(key => this.form.get(key)?.markAsTouched());
  this.form.updateValueAndValidity();
  untracked(() => this.updateStepValidation());
});
```

**After** (~15 lines):
```typescript
import { createRestorationEffect, withRestorationGuard } from '../shared';

constructor() {
  const { isRestoring } = createRestorationEffect<CaracteristicasFormData>({
    stepConfig: () => this.stepConfig(),
    wizardStore: this.wizardStore,
    form: this.form,
    resetForm: () => this.form.reset({ ... }),
    restoreData: (data) => {
      this.form.patchValue(this.prepareDataForForm(data), { emitEvent: false });
      this.form.markAsDirty();
      this.markAllAsTouched();
    },
    updateStepValidation: () => this.updateStepValidation(),
  });

  // Use guard in valueChanges
  this.form.valueChanges.pipe(
    takeUntilDestroyed(this.destroyRef),
    withRestorationGuard(isRestoring),
  ).subscribe(...);
}
```

## Verification

1. **Build**: `nx build core-ledger-ui` - no errors
2. **Tests**: `nx test core-ledger-ui` - all pass
3. **Manual Testing**:
   - Fresh wizard start → all steps load correctly
   - Draft restoration → all step data restored
   - Navigation back/forward → forms maintain state
   - FormArray steps → default items created when empty
   - Documentos step → files load from IndexedDB
   - Identificacao step → CNPJ validator not re-triggered on restoration

## Angular Best Practices Validation

The plan was validated against Angular MCP and Context7 documentation (Angular 21).

### Confirmed Patterns

| Pattern | Angular Docs Validation |
|---------|------------------------|
| **`effect()` for form restoration** | ✅ Appropriate - "Keeping data in sync with different kind of storages" is a valid use case. Form restoration is syncing signal state to imperative Reactive Forms API. |
| **`untracked()` for store reads** | ✅ Correct - "Prevents signal reads from creating dependencies" - avoids infinite loops when reading store data inside effect. |
| **`toSignal()` for form values** | ✅ Required - "Creates a signal which tracks the value of an Observable" - computed signals can't track form control values directly. |
| **`{ emitEvent: false }` on programmatic changes** | ✅ Documented - FormControl docs: "emits an event... unless you pass `{emitEvent: false}`" |

### Angular Effect Guidelines (from angular.dev)

> "Effects should be the last API you reach for. Always prefer `computed()` for derived values... Effects are best for **syncing signal state to imperative, non-signal APIs**."

**Our use case is valid** because:
1. Reactive Forms are imperative, non-signal APIs
2. The effect runs on step/version change (input signals), not on form changes
3. Store data restoration cannot be modeled with `computed()` (forms are mutable)

### When NOT to Use Effects (from docs)

> "Avoid using effects for **propagation of state changes**. This can result in infinite circular updates."

**Our plan avoids this** because:
1. Effect reads signals (`stepConfig`, `dataVersion`) → writes to form (imperative API)
2. Form changes are handled by `valueChanges` subscription → writes to store
3. No effect syncs form changes back to store (documented bug pattern)

### `toSignal()` Pattern Validation

From Angular docs on RxJS interop:
```typescript
// ✅ Correct - toSignal for form valueChanges
private readonly fieldValue = toSignal(
  this.form.get('field')!.valueChanges.pipe(startWith(this.form.get('field')!.value)),
  { initialValue: null }
);

// ✅ Then computed() works correctly
readonly showField = computed(() => this.fieldValue() === 'someValue');
```

This matches our documented pattern in `computed-signal-form-values.md`.

## Known Bug Prevention (from docs/aidebug/)

The utility **MUST** incorporate safeguards for these documented bugs:

### 1. Store Data Overwrite (wizard-step-restoration-overwrite.md)

**Bug**: Effects calling `setValue()` without `{ emitEvent: false }` trigger `valueChanges` which overwrites store data after `isRestoring` is cleared.

**Safeguard in Utility**:
- `restoreData()` callback documentation MUST emphasize `{ emitEvent: false }`
- Example code in JSDoc must show the correct pattern
- Consider adding a lint rule or runtime warning

```typescript
// REQUIRED in restoreData callback:
this.form.patchValue(data, { emitEvent: false });  // ✓
this.form.patchValue(data);                         // ✗ BUG!
```

### 2. Subscription Side Effects (wizard-multiclasse-restoration-defaults.md)

**Bug**: `setValue()` triggers subscriptions that add defaults before saved data is restored (e.g., multiclasse toggle adding default SENIOR class before classes array is populated).

**Safeguard in Utility**:
- Document that ALL form mutations in `restoreData()` must use `{ emitEvent: false }`
- For FormArray steps, order of operations is critical:
  1. Clear array
  2. Set toggles WITH `{ emitEvent: false }`
  3. Push saved items
  4. THEN update UI signals

```typescript
// CORRECT order for FormArray restoration:
restoreData: (data) => {
  classesArray.clear();
  this.form.get('multiclasse')?.setValue(data.multiclasse, { emitEvent: false });  // ✓
  data.classes.forEach(c => classesArray.push(this.createFormGroup(c)));
}
```

### 3. Async Validator Infinite Pending (wizard-async-validator-restoration.md)

**Bug**: CNPJ async validator shows infinite loading spinner on restoration because `updateValueAndValidity()` triggers async validation.

**Safeguard in Utility**:
- Support `asyncValidatorBypass` config option
- Implement the exact sequence:
  1. Clear async validators
  2. Call `control.updateValueAndValidity({ emitEvent: false })`
  3. Patch form data
  4. Restore validators WITHOUT calling `updateValueAndValidity()`

```typescript
asyncValidatorBypass: {
  controls: [this.form.get('cnpj')!],
  shouldBypass: () => completedSteps.has(stepId) && !!stepData,
}
```

### 4. Documentos Infinite Loop (documentos-step-infinite-loop.md)

**Bug**: Effect watching `documentos()` signal and calling `setStepData()` creates infinite loop because `isRestoring` flag timing doesn't work with scheduled effects.

**Safeguard in Utility**:
- The utility MUST NOT create an effect that syncs to store
- Store sync remains in the step's `valueChanges` subscription (guarded by `isRestoring`)
- For signal-based steps (Documentos), use explicit `saveToStore()` calls, not reactive effects

**For Documentos specifically**: Keep the existing explicit `saveToStore()` pattern, only centralize the deduplication and `isRestoring` management.

### 5. Computed Signal Form Values (computed-signal-form-values.md)

**Bug**: `computed()` doesn't track Reactive Forms values; steps must use `toSignal()` pattern.

**Safeguard in Utility**:
- Utility does not interfere with existing `toSignal()` patterns
- Document that `toSignal()` signals must be defined BEFORE forms (class field initialization order)
- Utility's `isRestoring` signal works correctly with computed signals

## Implementation Constraints (Derived from Bugs)

| Constraint | Enforced By |
|-----------|-------------|
| `patchValue()` uses `{ emitEvent: false }` | JSDoc + code review |
| `setValue()` in restoration uses `{ emitEvent: false }` | JSDoc + code review |
| FormArray: clear → set toggles → push items | Documentation + example |
| Async validators: clear → updateValueAndValidity → patch → restore | `asyncValidatorBypass` config |
| No effect syncing to store | Architecture (valueChanges subscription pattern) |
| `isRestoring` signal (not plain boolean) | Return type of `createRestorationEffect()` |

## Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Breaking existing functionality | Migrate one step at a time, test each |
| Learning curve for new pattern | Comprehensive JSDoc with examples |
| Edge cases not covered | Escape hatch: steps can use manual approach |
| Recreating known bugs | Section above documents explicit safeguards |
| `emitEvent: false` forgotten | Code review checklist + PR template |
