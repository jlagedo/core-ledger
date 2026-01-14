# Wizard Step Navigation - Form Storage Issue Diagnostic Report

## Executive Summary

The wizard form has **multiple dependencies** preventing step navigation, and there's a **potential data persistence issue** with how form data is reloaded when navigating back to a step. The data persistence issue is **hidden** by the validation requirements that prevent navigation.

---

## Issue #1: Form Validation Blocking Navigation (PRIMARY BLOCKER)

### Symptom
The "Next" button (Próximo) remains **disabled** even after filling required fields like CNPJ, Razão Social, and Nome Fantasia.

### Root Cause
The form requires **ALL fields** to be valid before navigation is allowed, not just a subset. Required fields include:

1. **CNPJ** - Requires:
   - Valid CNPJ format (mask: XX.XXX.XXX/XXXX-XX)
   - Passes CNPJ check digit validation
   - Async validator to check CNPJ uniqueness (causes `ng-pending` state)

2. **Razão Social** - Requires:
   - Min length: 10 characters
   - Max length: 200 characters

3. **Nome Fantasia** - Requires:
   - Min length: 3 characters
   - Max length: 100 characters

4. **Nome Curto** - Optional field

5. **Tipo Fundo** - Requires:
   - Must select a value (not the placeholder "Selecione o tipo")
   - Option values: `FI`, `FIC`, `FIDC`, `FIDC_NP`, `FIP`, `FII`, `FIAGRO`, `FI_INFRA`, `ETF`, `FMP_FGTS`

6. **Data Constituição** - Requires:
   - Valid date in DD/MM/YYYY format
   - Cannot be in the future
   - Must be <= today

7. **Data Início Atividade** - Requires:
   - Valid date in DD/MM/YYYY format
   - Must be >= Data Constituição

### Form State Observation
```
Form classes: step-form ng-touched ng-dirty ng-pending
```

The `ng-pending` state indicates async validators are running (CNPJ uniqueness check).

### Evidence
From Playwright diagnostic output:
```
CNPJ input classes: form-control step-field__input font-monospace ng-touched ng-dirty is-valid ng-valid
Next button disabled: true
Validation errors: []
```

The CNPJ is valid and there are no reported errors, but the button is still disabled. This suggests either:
- The form has unmet required fields
- An async validator is blocking with `ng-pending` state
- The form's overall validation status doesn't match the individual field status

---

## Issue #2: Data Persistence Problem (SECONDARY - HIDDEN)

### Symptom
**Potential issue**: When navigating back to a previous step, form data may not be restored from `store.stepData()`.

### Root Cause (Code Analysis)

In `identificacao-step.ts` (lines 134-197), the component uses an effect to load data:

```typescript
effect(() => {
  const stepConfig = this.stepConfig();
  const stepId = stepConfig.id;
  const dataVersion = this.wizardStore.dataVersion();

  // Skip if same step AND same dataVersion (no changes)
  const sameStep = this.lastLoadedStepId === stepId;
  const sameVersion = this.lastDataVersion === dataVersion;
  if (sameStep && sameVersion) {
    return;
  }
```

**The Problem**: The effect only re-runs when `dataVersion` changes. However, in `wizard-store.ts`, `dataVersion` is **only incremented** in `loadDraft()` (line 501):

```typescript
async loadDraft(draftId: string): Promise<boolean> {
  // ...
  const newDataVersion = store.dataVersion() + 1;
  // ...
  dataVersion: newDataVersion,
}
```

**During normal step navigation** using `goNext()` or `goPrevious()`, `dataVersion` is **NOT changed**. Therefore:

1. User fills Step 1 → data stored in `store.stepData()['identificacao']`
2. User clicks "Next" → navigates to Step 2 (currentStep changes, but dataVersion stays same)
3. User clicks "Previous" → navigates back to Step 1
4. IdentificacaoStep's effect doesn't trigger because:
   - `sameStep === true` (still on step 1)
   - `sameVersion === true` (dataVersion unchanged)
   - Effect returns early without reloading form data

**Result**: Form component doesn't reload its values from `store.stepData()`, leaving the form empty!

### Evidence
Playwright tests timed out waiting for successful navigation, suggesting the form validation issue prevents reaching the navigation step to test data persistence. However, the code analysis clearly shows the bug exists in the effect logic.

---

## Proposed Solutions

### Solution 1: Fix the Data Loading Effect (RECOMMENDED)
**Modify**: `identificacao-step.ts` lines 134-197

Instead of skipping when on the same step with the same dataVersion, also check if we're returning to a previously completed step:

```typescript
effect(() => {
  const stepConfig = this.stepConfig();
  const stepId = stepConfig.id;
  const dataVersion = this.wizardStore.dataVersion();

  // Always reload if:
  // 1. Step changed (even if dataVersion same)
  // 2. DataVersion changed (draft loaded)
  const stepChanged = this.lastLoadedStepId !== stepId;
  const versionChanged = this.lastDataVersion !== dataVersion;

  if (!stepChanged && !versionChanged) {
    return;
  }

  // ... rest of load logic
});
```

Or trigger data load whenever `currentStep` changes:

```typescript
effect(() => {
  const currentStep = this.wizardStore.currentStep(); // Add dependency
  // Reload form when step changes
  const stepConfig = this.stepConfig();
  // ... load data
});
```

### Solution 2: Increment dataVersion During Navigation
**Modify**: `wizard-store.ts` methods `goNext()` and `goPrevious()`

Increment dataVersion whenever navigating to a step that already has data:

```typescript
goNext(): void {
  if (store.canGoNext() && !store.isLastStep()) {
    const nextStep = findNextVisibleStep(store.currentStep());
    if (nextStep !== null) {
      patchState(store, { currentStep: nextStep });
      // Increment dataVersion so components reload data
      patchState(store, { dataVersion: store.dataVersion() + 1 });
    }
  }
},
```

### Solution 3: Fix Form Validation Blocking Navigation
**Modify**: Form validation logic or button enable condition

The form requires all 7 fields to be valid. Consider:
- Making more fields optional
- Splitting step validation into "required" vs "optional" fields
- Allowing navigation with incomplete forms and validate on submit

---

## Test Recommendations

To properly test data persistence once form validation is fixed:

```typescript
test('should preserve form data when navigating between steps', async ({ page }) => {
  // Fill all required fields with valid data
  // Navigate to step 2
  // Navigate back to step 1
  // Assert all form values match what was entered
});
```

---

## Additional Observations

### Form Async Validation
- The CNPJ field has an **async validator** that checks for uniqueness
- The form enters `ng-pending` state while this validates
- This delays button enabling and could contribute to timeout issues

### Store Architecture
- Store is correctly managing form data in `stepData`
- Persistence via IndexedDB is implemented properly
- The issue is in the **effect trigger logic**, not storage

### Navigation Architecture
- `goToStep()`, `goNext()`, and `goPrevious()` all work at store level
- Step components don't know about navigation, only react to signal changes
- Effect-based data loading is the right pattern, but implementation has a bug

---

## Files Affected

1. `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/identificacao-step/identificacao-step.ts` (lines 134-197)
2. `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/wizard-store.ts` (lines 336-356)
3. All other step components have the same potential issue

---

## Severity Assessment

- **Blocking**: Form validation prevents testing navigation
- **Hidden**: Data persistence bug exists but is masked by validation issue
- **Impact**: Users cannot progress beyond step 1 if form validation fails
- **User Experience**: Confusing - Next button disabled with no clear error messages

---

## Next Steps

1. **Immediate**: Fix data loading effect to always reload when returning to a step
2. **Short-term**: Review form validation requirements and async validator behavior
3. **Long-term**: Consider implementing better error messaging for disabled buttons
4. **Testing**: Add E2E tests that verify data persistence through multi-step navigation
