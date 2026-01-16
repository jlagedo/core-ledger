# DocumentosStep Infinite Loop - Browser Freeze on Navigation

## Problem Summary

**Symptom:** Browser freezes when navigating from step 09 (Vínculos) to step 10 (Documentos) in the fund registration wizard.

**Root Cause:** An Angular `effect()` that watched the `documentos()` signal and called `setStepData()` created an infinite reactive loop, blocking the main thread.

**Date:** 2025-01-16

## Technical Details

### The Problematic Code

In `documentos-step.ts`, there were two effects in the constructor:

```typescript
constructor() {
  // Effect 1: Load data when step changes or dataVersion changes
  effect(() => {
    const stepConfig = this.stepConfig();
    const dataVersion = this.wizardStore.dataVersion();

    // ... skip logic ...

    this.isRestoring = true;
    const stepData = untracked(() => this.wizardStore.stepData()[stepConfig.key]);

    if (stepData && Array.isArray(stepData)) {
      this.documentos.set(stepData);
      // async file loading...
    } else {
      this.documentos.set([]);  // <-- Triggers Effect 2
      this.isRestoring = false; // <-- Runs BEFORE Effect 2 executes
    }
  });

  // Effect 2: Sync documentos changes to store (THE PROBLEM)
  effect(() => {
    const docs = this.documentos();  // <-- Reads signal, creates dependency

    if (this.isRestoring) {
      return;  // Early return if restoring
    }

    this.wizardStore.setStepData(stepConfig.key, docs);  // <-- Updates store
  });
}
```

### Why It Caused an Infinite Loop

The execution sequence when navigating to step 10:

1. **Effect 1 runs** (component initialization)
2. `this.documentos.set([])` is called
3. `this.isRestoring = false` executes synchronously
4. **Effect 1 completes**
5. **Effect 2 is scheduled** (because `documentos` changed)
6. Effect 2 reads `this.documentos()` → `[]`
7. `isRestoring` is `false`, so it doesn't early-return
8. Effect 2 calls `setStepData('documentos', [])`
9. `setStepData` updates the store via `patchState()`
10. **Angular's reactive system detects a change** and re-schedules Effect 2
11. **Steps 6-10 repeat infinitely**

### The Timing Issue

The critical bug was the timing between `isRestoring` flag and effect execution:

```typescript
// In the else branch of Effect 1:
this.documentos.set([]);    // Queues Effect 2 to run
this.isRestoring = false;   // Executes IMMEDIATELY (before Effect 2)
```

When Effect 2 finally ran, `isRestoring` was already `false`, so the guard didn't protect against the loop.

### Console Output Pattern (Diagnostic)

The infinite loop was visible in the console:

```
[DocumentosStep] documentos effect() COMPLETE in - "0.00" - "ms"
[DocumentosStep] documentos effect() START - docs count: - 0 - "isRestoring:" - false
[DocumentosStep] documentos effect() - calling setStepData
[DocumentosStep] updateStepValidation() START
[WizardStore] setStepValidation() - stepId: - 10 - "isValid:" - true
[WizardStore] setStepValidation() completed in - "0.00" - "ms"
[DocumentosStep] updateStepValidation() COMPLETE in - "0.00" - "ms"
[DocumentosStep] documentos effect() COMPLETE in - "0.00" - "ms"
[DocumentosStep] documentos effect() START - docs count: - 0 - "isRestoring:" - false
... (repeats infinitely)
```

Key indicators:
- `isRestoring: false` when it should have been `true`
- Effect completing and immediately starting again
- No time gap between iterations (0.00 ms)

## Solution

### Approach: Replace Effect with Explicit Calls

Instead of using an effect to sync `documentos` to the store, call the sync method explicitly when documents are added or removed.

### The Fix

1. **Removed the problematic effect** that watched `documentos()`:

```typescript
// REMOVED - This caused the infinite loop:
// effect(() => {
//   const docs = this.documentos();
//   if (this.isRestoring) return;
//   this.wizardStore.setStepData(stepConfig.key, docs);
// });
```

2. **Added explicit `saveToStore()` method**:

```typescript
/**
 * Save current documentos to the wizard store.
 * Called explicitly after adding/removing documents (not via effect).
 */
private saveToStore(): void {
  if (this.isRestoring) {
    return;
  }
  const docs = this.documentos();
  const stepConfig = this.stepConfig();
  this.wizardStore.setStepData(stepConfig.key, docs);
  this.updateStepValidation();
}
```

3. **Called `saveToStore()` from mutation methods**:

```typescript
private addDocumento(file: File): void {
  // ... create documento ...
  this.documentos.update((docs) => [...docs, documento]);
  this.saveToStore();  // Explicit call
  this.saveFileToIndexedDB(documento, file);
}

removeDocumento(tempId: string): void {
  this.documentos.update((docs) => docs.filter((d) => d.tempId !== tempId));
  this.saveToStore();  // Explicit call
  this.deleteFileFromIndexedDB(tempId);
}
```

## Key Lessons

### 1. Avoid Effects That Read and Write to Related State

Effects that read a signal and then trigger updates to related state (even indirectly through a store) can create infinite loops.

**Anti-pattern:**
```typescript
effect(() => {
  const value = this.mySignal();      // Read
  this.store.updateValue(value);      // Write (may trigger reactive updates)
});
```

**Better pattern:**
```typescript
// Call store updates explicitly from the source of the change
updateValue(newValue: T): void {
  this.mySignal.set(newValue);
  this.store.updateValue(newValue);  // Explicit, predictable
}
```

### 2. `isRestoring` Flag Timing is Tricky

When using a flag to prevent store updates during restoration:
- The flag must be set **before** any signal updates
- The flag must remain set **until all effects have run**
- Async operations complicate this further

### 3. Effects Run Asynchronously (Scheduled)

When you call `signal.set()`, dependent effects are **scheduled**, not run immediately. This means code after `set()` executes before the effect runs:

```typescript
this.mySignal.set(value);  // Schedules effect
this.flag = false;         // Runs BEFORE effect
// Effect runs HERE with flag = false
```

### 4. Use `untracked()` for Reads That Shouldn't Create Dependencies

If you need to read a signal inside an effect without creating a dependency, wrap it in `untracked()`:

```typescript
effect(() => {
  const tracked = this.trackedSignal();      // Creates dependency
  const notTracked = untracked(() => this.otherSignal());  // No dependency
});
```

## Files Changed

- `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/documentos-step/documentos-step.ts`

## Related Issues

- Similar pattern exists in other step components but they don't have the same issue because:
  - They use `{ emitEvent: false }` on form updates
  - Their effects have proper skip conditions
  - They don't have async operations that complicate the `isRestoring` timing

## Debugging Approach

1. **Added timing logs** to all critical paths:
   - `WizardStore.goNext()` - Step transition
   - `VinculosStep` constructor, effect, ngOnDestroy
   - `DocumentosStep` constructor, effects, `loadFilesFromIndexedDB()`
   - `WizardPersistenceService.loadFiles()`

2. **Identified the loop** by observing:
   - Effect completing and immediately starting again
   - `isRestoring: false` when it should be `true`
   - 0.00ms between iterations (no actual work being done)

3. **Fixed by removing the effect** and using explicit method calls instead.
