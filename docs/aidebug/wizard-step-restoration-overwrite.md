# Wizard Step Restoration Bug: Effects Overwriting Store Data

## Problem

When navigating back from step 3 to step 2 in the fund registration wizard, form fields were not being restored - they appeared empty/null despite being saved to the store.

## Root Cause

Multiple Angular effects in the step component were calling `setValue()` on form controls **without** `{ emitEvent: false }`. This triggered the `form.valueChanges` subscription which saved the current (mostly null) form values to the store, **overwriting** the user's actual data.

### Sequence of Events

1. User fills step 2 fields, data is correctly saved to store via `valueChanges`
2. User navigates to step 3
3. User navigates back to step 2
4. **New** ClassificacaoStep component instance is created
5. Form is initialized with null/default values
6. Restoration effect runs:
   - Sets `isRestoring = true`
   - Patches form with data from store
   - Sets `isRestoring = false`
7. **Other effects run** (tipoFundo restrictions, ANBIMA options):
   - These effects call `setValue()` on form controls
   - `setValue()` triggers `valueChanges`
   - `isRestoring` is already `false` at this point
   - `valueChanges` subscription saves current form state (with nulls) to store
   - **User's data is overwritten**

### Problematic Code

```typescript
// Effect to apply restrictions based on tipo_fundo from Step 1
effect(() => {
  // ...
  if (!currentPublicoAlvo) {
    this.form.get('publicoAlvo')?.setValue(restriction);  // BUG: triggers valueChanges!
  }
  // ...
  if (!currentTributacao) {
    this.form.get('tributacao')?.setValue(suggestedTributacao);  // BUG: triggers valueChanges!
  }
});
```

## Solution

Add `{ emitEvent: false }` to all `setValue()` calls in effects that run during component initialization/restoration:

```typescript
// Fixed: Use emitEvent: false to avoid triggering valueChanges
this.form.get('publicoAlvo')?.setValue(restriction, { emitEvent: false });
this.form.get('tributacao')?.setValue(suggestedTributacao, { emitEvent: false });
this.form.get('classificacaoAnbima')?.setValue(null, { emitEvent: false });
```

## Key Insight

The `isRestoring` flag only protects the restoration effect itself. Other effects that modify the form run **after** `isRestoring` is set back to `false`, so their `setValue()` calls trigger `valueChanges` and overwrite store data.

## When to Use `{ emitEvent: false }`

| Context | Use `emitEvent: false`? | Reason |
|---------|------------------------|--------|
| Effects during initialization | **Yes** | Avoid overwriting store data |
| Effects applying restrictions/defaults | **Yes** | These are programmatic changes, not user actions |
| User-initiated actions (HTTP callbacks) | **No** | User changes should be saved to store |
| Form restoration/patching | **Yes** | Already covered by `isRestoring` flag but safer to add |

## Debugging Tips

1. Add console logs to `valueChanges` subscription to track what triggers saves:
   ```typescript
   console.log('[Step] valueChanges saving:', { data, isRestoring: this.isRestoring });
   ```

2. Add console logs to store's `setStepData` to see all writes:
   ```typescript
   console.log('[Store] setStepData:', { stepKey, data });
   ```

3. Check the restored data in the effect logs:
   ```typescript
   console.log('[Step] Patching form with:', formValue);
   ```

4. If patched data has unexpected nulls, trace backward to find what overwrote the store.

## Related Files

- `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/classificacao-step/classificacao-step.ts`
- `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/wizard-store.ts`

## Tags

`wizard`, `angular-effects`, `form-restoration`, `valueChanges`, `emitEvent`
