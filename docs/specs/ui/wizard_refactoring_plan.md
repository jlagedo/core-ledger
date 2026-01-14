# Wizard Refactoring Plan: IndexedDB Persistence & Architecture Overhaul

> **Created**: 2026-01-14
> **Based on**: `docs/specs/ui/wizard_research.md`
> **Target**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/`

---

## Table of Contents

1. [Problem Summary](#problem-summary)
2. [Solution Overview](#solution-overview)
3. [Architecture Diagram](#architecture-diagram)
4. [Files to Create](#files-to-create)
5. [Files to Modify](#files-to-modify)
6. [Implementation Phases](#implementation-phases)
7. [Code Examples](#code-examples)
8. [Verification Checklist](#verification-checklist)
9. [Dependencies](#dependencies)
10. [Risk Mitigation](#risk-mitigation)

---

## Problem Summary

The cadastro fundo wizard currently has three critical issues:

| Issue | Description | Impact |
|-------|-------------|--------|
| **Data loss on navigation** | No persistence mechanism; all data lost on page refresh or browser close | Users lose work, frustration |
| **Validation issues** | Validators not firing correctly after form data restoration when navigating between steps | Users blocked from proceeding, invalid data submitted |
| **File upload problems** | Documents not persisting; File objects lost on step changes or refresh | Uploaded documents disappear |

### Root Causes

1. **No persistence layer** - State only exists in memory via `WizardStore`
2. **`patchValue` with `emitEvent: false`** - Prevents `valueChanges` from firing, conditional validators don't apply
3. **File objects are not serializable** - Cannot be stored in localStorage; need IndexedDB for Blob storage

---

## Solution Overview

Implement patterns from the research document:

| Feature | Technology | Purpose |
|---------|------------|---------|
| **Form Persistence** | IndexedDB via Dexie.js | Store form data with schema versioning |
| **File Persistence** | IndexedDB Blob storage | Native Blob support, survives refresh |
| **Auto-save** | `rxMethod` from `@ngrx/signals/rxjs-interop` | Debounced saves with retry logic |
| **Recovery System** | Recovery banner component | Prompt user to restore draft on init |
| **Navigation Guards** | Functional `CanDeactivate` + `beforeunload` | Prevent accidental data loss |
| **Validation Fix** | `isRestoring` flag pattern | Prevent loops, force validator update |

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        WizardContainer                               │
│  - Implements DirtyComponent interface                              │
│  - @HostListener('window:beforeunload')                             │
│  - Recovery logic in ngOnInit()                                      │
└────────────────┬──────────────────────────────────────────────────┬─┘
                 │                                                  │
    ┌────────────▼────────────┐                    ┌────────────────▼───────────────┐
    │   RecoveryBanner        │                    │   SaveStatusIndicator          │
    │   - Restore/Dismiss     │                    │   - idle/saving/saved/error    │
    └─────────────────────────┘                    └────────────────────────────────┘
                 │
         ┌───────▼───────────────────────────────────────┐
         │              WizardStore (Enhanced)            │
         │  + draftId: string | null                     │
         │  + saveStatus: SaveStatus                      │
         │  + lastSavedAt: Date | null                   │
         │  + autoSave: rxMethod<void>                   │
         │  + loadDraft(id): void                        │
         │  + initializeDraft(): void                    │
         └───────────────────────┬───────────────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │  WizardPersistenceService│
                    │  - saveDraft()          │
                    │  - loadDraft()          │
                    │  - saveFile()           │
                    │  - loadFiles()          │
                    │  - cleanupStaleDrafts() │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │     WizardDatabase      │
                    │     (Dexie.js)          │
                    │  Tables:                │
                    │  - drafts               │
                    │  - files                │
                    └─────────────────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │       IndexedDB         │
                    │  (Browser Storage)      │
                    └─────────────────────────┘
```

---

## Files to Create

### 1. Persistence Models

**Path**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/models/persistence.model.ts`

```typescript
/**
 * Represents a saved wizard draft in IndexedDB
 */
export interface WizardDraft {
  /** UUID identifying this draft */
  id: string;
  /** All step data keyed by step key (e.g., 'identificacao', 'classificacao') */
  formData: Record<string, unknown>;
  /** Current step ID (1-11) */
  currentStep: number;
  /** Array of completed step IDs */
  completedSteps: number[];
  /** Schema version for migrations */
  version: number;
  /** When draft was first created */
  createdAt: Date;
  /** When draft was last updated */
  updatedAt: Date;
}

/**
 * Represents a file stored in IndexedDB
 */
export interface WizardFile {
  /** Auto-incremented ID */
  id?: number;
  /** Reference to parent draft */
  draftId: string;
  /** Step key (e.g., 'documentos') */
  stepKey: string;
  /** Client-side temp ID matching DocumentoFundo.tempId */
  tempId: string;
  /** Original filename */
  fileName: string;
  /** MIME type */
  fileType: string;
  /** File content as Blob */
  blob: Blob;
  /** Additional metadata (tipoDocumento, versao, dataVigencia, etc.) */
  metadata: Record<string, unknown>;
  /** When file was added */
  createdAt: Date;
}

/**
 * Auto-save status indicator states
 */
export type SaveStatus = 'idle' | 'saving' | 'saved' | 'error';
```

---

### 2. Dexie Database Class

**Path**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/services/wizard-database.ts`

```typescript
import Dexie, { Table } from 'dexie';
import { WizardDraft, WizardFile } from '../models/persistence.model';

/**
 * IndexedDB database for wizard draft persistence.
 * Uses Dexie.js for TypeScript-native IndexedDB access.
 */
export class WizardDatabase extends Dexie {
  drafts!: Table<WizardDraft>;
  files!: Table<WizardFile>;

  constructor() {
    super('CoreLedgerWizardDB');

    // Version 1: Initial schema
    this.version(1).stores({
      drafts: 'id, updatedAt',
      files: '++id, draftId, stepKey, tempId, createdAt',
    });

    // Version 2: Add version field to drafts for future migrations
    this.version(2)
      .stores({
        drafts: 'id, updatedAt, version',
        files: '++id, draftId, stepKey, tempId, createdAt',
      })
      .upgrade((tx) => {
        return tx
          .table('drafts')
          .toCollection()
          .modify((draft) => {
            draft.version = draft.version || 1;
          });
      });
  }
}
```

---

### 3. Persistence Service

**Path**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/services/wizard-persistence.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { WizardDatabase } from './wizard-database';
import { WizardDraft, WizardFile } from '../models/persistence.model';

/**
 * Service for persisting wizard state to IndexedDB.
 * Handles both form data and file uploads.
 */
@Injectable({ providedIn: 'root' })
export class WizardPersistenceService {
  private readonly db = new WizardDatabase();

  /**
   * Save or update a draft
   */
  async saveDraft(
    draftId: string,
    formData: Record<string, unknown>,
    currentStep: number,
    completedSteps: number[]
  ): Promise<void> {
    const existing = await this.db.drafts.get(draftId);
    const now = new Date();

    await this.db.drafts.put({
      id: draftId,
      formData,
      currentStep,
      completedSteps,
      version: 2,
      createdAt: existing?.createdAt || now,
      updatedAt: now,
    });
  }

  /**
   * Save a file associated with a draft
   */
  async saveFile(
    draftId: string,
    stepKey: string,
    file: File,
    tempId: string,
    metadata: Record<string, unknown>
  ): Promise<number> {
    return this.db.files.add({
      draftId,
      stepKey,
      tempId,
      fileName: file.name,
      fileType: file.type,
      blob: file,
      metadata,
      createdAt: new Date(),
    });
  }

  /**
   * Load a draft by ID
   */
  async loadDraft(draftId: string): Promise<WizardDraft | null> {
    const draft = await this.db.drafts.get(draftId);
    return draft || null;
  }

  /**
   * Load files for a draft, optionally filtered by step
   */
  async loadFiles(draftId: string, stepKey?: string): Promise<WizardFile[]> {
    let query = this.db.files.where('draftId').equals(draftId);
    if (stepKey) {
      const files = await query.toArray();
      return files.filter((f) => f.stepKey === stepKey);
    }
    return query.toArray();
  }

  /**
   * Check if a draft exists
   */
  async hasDraft(draftId: string): Promise<boolean> {
    const count = await this.db.drafts.where('id').equals(draftId).count();
    return count > 0;
  }

  /**
   * Delete a draft and its associated files
   */
  async deleteDraft(draftId: string): Promise<void> {
    await this.db.files.where('draftId').equals(draftId).delete();
    await this.db.drafts.delete(draftId);
  }

  /**
   * Delete a specific file
   */
  async deleteFile(fileId: number): Promise<void> {
    await this.db.files.delete(fileId);
  }

  /**
   * Delete file by tempId (for matching DocumentoFundo)
   */
  async deleteFileByTempId(draftId: string, tempId: string): Promise<void> {
    await this.db.files
      .where(['draftId', 'tempId'])
      .equals([draftId, tempId])
      .delete();
  }

  /**
   * Cleanup drafts older than maxAgeDays
   * @returns Number of drafts deleted
   */
  async cleanupStaleDrafts(maxAgeDays = 7): Promise<number> {
    const cutoff = new Date(Date.now() - maxAgeDays * 24 * 60 * 60 * 1000);
    const staleDrafts = await this.db.drafts.where('updatedAt').below(cutoff).toArray();

    for (const draft of staleDrafts) {
      await this.deleteDraft(draft.id);
    }

    return staleDrafts.length;
  }

  /**
   * Get all draft IDs (for recovery detection)
   */
  async getAllDraftIds(): Promise<string[]> {
    const drafts = await this.db.drafts.toArray();
    return drafts.map((d) => d.id);
  }

  /**
   * Get the most recent draft
   */
  async getMostRecentDraft(): Promise<WizardDraft | null> {
    const drafts = await this.db.drafts.orderBy('updatedAt').reverse().limit(1).toArray();
    return drafts[0] || null;
  }
}
```

---

### 4. Navigation Guard

**Path**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/guards/unsaved-changes.guard.ts`

```typescript
import { CanDeactivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

/**
 * Interface for components that track dirty state
 */
export interface DirtyComponent {
  isDirty(): boolean;
}

/**
 * Functional guard to prevent navigation with unsaved changes.
 * Shows confirmation dialog when component has dirty state.
 */
export const unsavedChangesGuard: CanDeactivateFn<DirtyComponent> = (component) => {
  if (!component.isDirty()) {
    return true;
  }

  // Use native confirm for simplicity; can be replaced with NgbModal
  return confirm(
    'Existem alterações não salvas. Seu progresso foi salvo automaticamente.\n\n' +
      'Deseja realmente sair?'
  );
};
```

---

### 5. Recovery Banner Component

**Path**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/components/recovery-banner/recovery-banner.ts`

```typescript
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-recovery-banner',
  imports: [DatePipe],
  template: `
    @if (showBanner()) {
      <div class="alert alert-info d-flex align-items-center gap-3 mb-3 shadow-sm">
        <i class="bi bi-clock-history fs-4"></i>
        <div class="flex-grow-1">
          <strong>Rascunho encontrado</strong>
          <p class="mb-0 small text-body-secondary">
            Deseja continuar de onde parou? Salvo em {{ lastSavedAt() | date : 'dd/MM/yyyy HH:mm' }}
          </p>
        </div>
        <div class="d-flex gap-2">
          <button class="btn btn-primary btn-sm" (click)="restore.emit()">
            <i class="bi bi-arrow-counterclockwise me-1"></i>
            Restaurar
          </button>
          <button class="btn btn-outline-secondary btn-sm" (click)="dismiss.emit()">
            Descartar
          </button>
        </div>
      </div>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecoveryBanner {
  showBanner = input.required<boolean>();
  lastSavedAt = input<Date | null>(null);

  restore = output<void>();
  dismiss = output<void>();
}
```

**Path**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/components/recovery-banner/index.ts`

```typescript
export { RecoveryBanner } from './recovery-banner';
```

---

### 6. Save Status Indicator Component

**Path**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/components/save-status-indicator/save-status-indicator.ts`

```typescript
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { SaveStatus } from '../../models/persistence.model';

@Component({
  selector: 'app-save-status-indicator',
  template: `
    <div class="save-status d-flex align-items-center gap-2 text-body-secondary small">
      @switch (status()) {
        @case ('saving') {
          <div class="spinner-border spinner-border-sm" role="status">
            <span class="visually-hidden">Salvando...</span>
          </div>
          <span>Salvando...</span>
        }
        @case ('saved') {
          <i class="bi bi-check-circle text-success"></i>
          <span>Salvo</span>
        }
        @case ('error') {
          <i class="bi bi-exclamation-circle text-danger"></i>
          <span>Erro ao salvar</span>
          <button class="btn btn-link btn-sm p-0 text-decoration-underline" (click)="retry.emit()">
            Tentar novamente
          </button>
        }
      }
    </div>
  `,
  styles: `
    .save-status {
      min-height: 1.5rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SaveStatusIndicator {
  status = input.required<SaveStatus>();
  retry = output<void>();
}
```

**Path**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/components/save-status-indicator/index.ts`

```typescript
export { SaveStatusIndicator } from './save-status-indicator';
```

---

## Files to Modify

### 1. `wizard-store.ts` - Major Changes

**Location**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/wizard-store.ts`

**Changes:**

1. Add new state fields:
```typescript
interface WizardState {
  // ... existing fields
  draftId: string | null;
  saveStatus: SaveStatus;
  lastSavedAt: Date | null;
}
```

2. Add `rxMethod` import and auto-save implementation
3. Add `initializeDraft()`, `loadDraft()`, `setDraftId()` methods
4. Add computed signal for `needsRecovery`

---

### 2. `wizard-container.ts` - Major Changes

**Location**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/wizard-container.ts`

**Changes:**

1. Implement `DirtyComponent` interface
2. Add `@HostListener('window:beforeunload')` handler
3. Add recovery logic in `ngOnInit()`
4. Add signals: `showRecoveryBanner`, `recoveryDraft`
5. Add methods: `restoreDraft()`, `dismissRecovery()`
6. Inject `WizardPersistenceService`

---

### 3. `wizard-container.html` - Medium Changes

**Location**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/wizard-container.html`

**Add at top of file:**
```html
<app-recovery-banner
  [showBanner]="showRecoveryBanner()"
  [lastSavedAt]="recoveryDraft()?.updatedAt"
  (restore)="restoreDraft()"
  (dismiss)="dismissRecovery()" />
```

**Add save status indicator** (near navigation or stepper)

---

### 4. `wizard.routes.ts` - Minor Changes

**Location**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/wizard.routes.ts`

**Add canDeactivate guard:**
```typescript
import { unsavedChangesGuard } from './guards/unsaved-changes.guard';

export const WIZARD_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./wizard-container').then((m) => m.WizardContainer),
    data: { breadcrumb: 'Cadastro' },
    canDeactivate: [unsavedChangesGuard],
  },
];
```

---

### 5. `documentos-step.ts` - Medium Changes

**Location**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/documentos-step/documentos-step.ts`

**Add:**
1. Inject `WizardPersistenceService`
2. Method `saveFileToIndexedDB()` - called when adding document
3. Method `loadFilesFromIndexedDB()` - called in effect on step load
4. Method `deleteFileFromIndexedDB()` - called when removing document
5. Track object URLs for cleanup in `ngOnDestroy`

---

### 6. All Step Components - Minor Changes

**Pattern to standardize across all steps:**

```typescript
// Flag to prevent store updates during restoration
private isRestoring = false;

constructor() {
  // Filter store updates during restoration
  this.form.valueChanges
    .pipe(
      takeUntilDestroyed(this.destroyRef),
      filter(() => !this.isRestoring)
    )
    .subscribe((value) => {
      const stepConfig = untracked(() => this.stepConfig());
      this.wizardStore.setStepData(stepConfig.key, this.prepareDataForStore(value));
    });

  // Effect for loading data from store
  effect(() => {
    const stepConfig = this.stepConfig();
    if (this.lastLoadedStepId === stepConfig.id) return;
    this.lastLoadedStepId = stepConfig.id;

    this.isRestoring = true;
    try {
      const savedData = this.wizardStore.stepData()[stepConfig.key];
      if (savedData) {
        this.form.patchValue(savedData, { emitEvent: false });
      }
    } finally {
      this.isRestoring = false;
    }

    // Force validation update after restoration
    this.form.updateValueAndValidity();
    this.applyConditionalValidators();
    untracked(() => this.updateStepValidation());
  });
}
```

**Steps to update:**
- `caracteristicas-step.ts`
- `classes-step.ts`
- `classificacao-step.ts`
- `parametros-cota-step.ts`
- `parametros-fidc-step.ts`
- `prazos-step.ts`
- `taxas-step.ts`
- `vinculos-step.ts`

---

## Implementation Phases

### Phase 1: Core Infrastructure (Checkpoint: P1)

**Goal**: Create the persistence layer foundation

| # | Task | File | Est. |
|---|------|------|------|
| 1.1 | Install Dexie.js | `package.json` | 5m |
| 1.2 | Create persistence models | `models/persistence.model.ts` | 15m |
| 1.3 | Create database class | `services/wizard-database.ts` | 20m |
| 1.4 | Create persistence service | `services/wizard-persistence.service.ts` | 45m |
| 1.5 | Write unit tests | `services/wizard-persistence.service.spec.ts` | 30m |

**Checkpoint P1 Verification:**
- [ ] `npm install dexie` completes without errors
- [ ] `WizardDatabase` class instantiates without errors
- [ ] Can create, read, update, delete drafts via service
- [ ] Can store and retrieve File blobs
- [ ] Unit tests pass for persistence service

---

### Phase 2: Store Integration (Checkpoint: P2)

**Goal**: Add persistence state and auto-save to WizardStore

| # | Task | File | Est. |
|---|------|------|------|
| 2.1 | Add persistence state fields | `wizard-store.ts` | 15m |
| 2.2 | Import rxMethod from @ngrx/signals/rxjs-interop | `wizard-store.ts` | 5m |
| 2.3 | Implement autoSave rxMethod | `wizard-store.ts` | 45m |
| 2.4 | Add initializeDraft(), loadDraft() methods | `wizard-store.ts` | 20m |
| 2.5 | Add computed signals for recovery | `wizard-store.ts` | 10m |

**Checkpoint P2 Verification:**
- [ ] Store has `draftId`, `saveStatus`, `lastSavedAt` signals
- [ ] `autoSave` triggers on `setStepData()` calls
- [ ] Auto-save debounces (800ms)
- [ ] Auto-save retries on failure (3 attempts)
- [ ] `saveStatus` signal updates correctly through lifecycle

---

### Phase 3: Navigation Guards (Checkpoint: P3)

**Goal**: Protect users from accidental data loss

| # | Task | File | Est. |
|---|------|------|------|
| 3.1 | Create unsaved changes guard | `guards/unsaved-changes.guard.ts` | 20m |
| 3.2 | Update routes with guard | `wizard.routes.ts` | 5m |
| 3.3 | Implement DirtyComponent in container | `wizard-container.ts` | 10m |
| 3.4 | Add @HostListener beforeunload | `wizard-container.ts` | 10m |
| 3.5 | Write guard tests | `guards/unsaved-changes.guard.spec.ts` | 20m |

**Checkpoint P3 Verification:**
- [ ] Router navigation with dirty form shows confirmation
- [ ] Clean form allows navigation without prompt
- [ ] Browser close/refresh shows native dialog when dirty
- [ ] Browser close/refresh passes through when clean
- [ ] Guard tests pass

---

### Phase 4: Recovery System (Checkpoint: P4)

**Goal**: Allow users to restore saved drafts

| # | Task | File | Est. |
|---|------|------|------|
| 4.1 | Create RecoveryBanner component | `components/recovery-banner/` | 30m |
| 4.2 | Create SaveStatusIndicator component | `components/save-status-indicator/` | 20m |
| 4.3 | Add recovery logic to container ngOnInit | `wizard-container.ts` | 30m |
| 4.4 | Add restoreDraft(), dismissRecovery() methods | `wizard-container.ts` | 20m |
| 4.5 | Update container template | `wizard-container.html` | 15m |
| 4.6 | Add startup cleanup of stale drafts | `wizard-container.ts` | 10m |

**Checkpoint P4 Verification:**
- [ ] Recovery banner shows when draft exists
- [ ] "Restaurar" button loads form data into steps
- [ ] "Descartar" button deletes draft and starts fresh
- [ ] Save status indicator shows saving/saved/error states
- [ ] Stale drafts (7+ days) are cleaned up on init

---

### Phase 5: File Persistence (Checkpoint: P5)

**Goal**: Persist uploaded documents in IndexedDB

| # | Task | File | Est. |
|---|------|------|------|
| 5.1 | Inject persistence service | `documentos-step.ts` | 5m |
| 5.2 | Implement saveFileToIndexedDB() | `documentos-step.ts` | 30m |
| 5.3 | Implement loadFilesFromIndexedDB() | `documentos-step.ts` | 30m |
| 5.4 | Implement deleteFileFromIndexedDB() | `documentos-step.ts` | 15m |
| 5.5 | Add object URL tracking and cleanup | `documentos-step.ts` | 15m |
| 5.6 | Integrate with step effect | `documentos-step.ts` | 20m |

**Checkpoint P5 Verification:**
- [ ] Uploading a file saves it to IndexedDB
- [ ] Navigating away and back restores files with previews
- [ ] Refreshing browser restores uploaded files
- [ ] Deleting a file removes it from IndexedDB
- [ ] Object URLs are revoked on component destroy

---

### Phase 6: Validation Cleanup (Checkpoint: P6)

**Goal**: Fix validation restoration across all steps

| # | Task | File | Est. |
|---|------|------|------|
| 6.1 | Audit all steps for isRestoring pattern | All step components | 30m |
| 6.2 | Standardize restoration logic | All step components | 60m |
| 6.3 | Ensure validators fire after patchValue | All step components | 30m |
| 6.4 | Test all steps with data restoration | Manual testing | 45m |
| 6.5 | Remove debug panel (or gate to dev only) | `wizard-navigation.html` | 5m |

**Steps to audit:**
- [ ] `identificacao-step.ts`
- [ ] `classificacao-step.ts`
- [ ] `caracteristicas-step.ts`
- [ ] `parametros-cota-step.ts`
- [ ] `taxas-step.ts`
- [ ] `prazos-step.ts`
- [ ] `classes-step.ts`
- [ ] `parametros-fidc-step.ts`
- [ ] `vinculos-step.ts`
- [ ] `documentos-step.ts`

**Checkpoint P6 Verification:**
- [ ] All steps use consistent isRestoring pattern
- [ ] Navigating forward/back preserves validation state
- [ ] Conditional validators apply after restoration
- [ ] No validation loops or infinite updates
- [ ] Debug panel hidden in production

---

## Code Examples

### Auto-Save with rxMethod (wizard-store.ts)

```typescript
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, from, timer } from 'rxjs';
import { debounceTime, switchMap, tap, retry } from 'rxjs/operators';
import { tapResponse } from '@ngrx/operators';

// Inside withMethods:
autoSave: rxMethod<void>(
  pipe(
    debounceTime(800),
    tap(() => patchState(store, { saveStatus: 'saving' })),
    switchMap(() =>
      from(
        persistenceService.saveDraft(
          store.draftId()!,
          store.stepData(),
          store.currentStep(),
          Array.from(store.completedSteps())
        )
      ).pipe(
        retry({
          count: 3,
          delay: (error, retryCount) => timer(Math.pow(2, retryCount) * 1000),
        })
      )
    ),
    tapResponse({
      next: () =>
        patchState(store, {
          saveStatus: 'saved',
          lastSavedAt: new Date(),
        }),
      error: () => patchState(store, { saveStatus: 'error' }),
    })
  )
)
```

### beforeunload Handler (wizard-container.ts)

```typescript
@HostListener('window:beforeunload', ['$event'])
handleBeforeUnload(event: BeforeUnloadEvent): void {
  if (this.isDirty() && !this.isSubmitting()) {
    event.preventDefault();
    event.returnValue = ''; // Triggers browser's native dialog
  }
}

isDirty(): boolean {
  return this.store.isDirty() && !this.store.isSubmitting();
}
```

### Recovery Flow (wizard-container.ts)

```typescript
async ngOnInit(): Promise<void> {
  // Cleanup stale drafts first
  await this.persistenceService.cleanupStaleDrafts(7);

  // Check for existing draft (from route param or most recent)
  const draftId = this.route.snapshot.queryParamMap.get('draftId');
  const draft = draftId
    ? await this.persistenceService.loadDraft(draftId)
    : await this.persistenceService.getMostRecentDraft();

  if (draft && this.isDraftValid(draft)) {
    this.recoveryDraft.set(draft);
    this.showRecoveryBanner.set(true);
  } else {
    this.store.initializeDraft();
  }
}

private isDraftValid(draft: WizardDraft): boolean {
  const maxAge = 7 * 24 * 60 * 60 * 1000; // 7 days
  return Date.now() - draft.updatedAt.getTime() < maxAge;
}

restoreDraft(): void {
  const draft = this.recoveryDraft();
  if (!draft) return;

  this.store.loadDraft(draft.id);
  this.showRecoveryBanner.set(false);
}

dismissRecovery(): void {
  const draft = this.recoveryDraft();
  if (draft) {
    this.persistenceService.deleteDraft(draft.id);
  }
  this.recoveryDraft.set(null);
  this.showRecoveryBanner.set(false);
  this.store.initializeDraft();
}
```

### File Restoration (documentos-step.ts)

```typescript
private objectUrls: string[] = [];

private async loadFilesFromIndexedDB(): Promise<void> {
  const draftId = this.wizardStore.draftId();
  if (!draftId) return;

  const files = await this.persistenceService.loadFiles(draftId, 'documentos');

  const documentos = files.map((f) => {
    const file = new File([f.blob], f.fileName, { type: f.fileType });
    const url = URL.createObjectURL(f.blob);
    this.objectUrls.push(url); // Track for cleanup

    return {
      tempId: f.tempId,
      tipoDocumento: f.metadata['tipoDocumento'] as TipoDocumento,
      versao: f.metadata['versao'] as number,
      dataVigencia: f.metadata['dataVigencia'] as string,
      dataFimVigencia: f.metadata['dataFimVigencia'] as string | null,
      observacoes: f.metadata['observacoes'] as string | null,
      arquivoNome: f.fileName,
      arquivoTamanho: f.blob.size,
      arquivoTipo: f.fileType,
      arquivoConteudo: file,
      arquivoUrl: url,
      uploadStatus: 'uploaded' as const,
      createdAt: f.createdAt.toISOString(),
    };
  });

  this.documentos.set(documentos);
}

ngOnDestroy(): void {
  // Revoke all object URLs to prevent memory leaks
  this.objectUrls.forEach((url) => URL.revokeObjectURL(url));
}
```

---

## Verification Checklist

### Manual Testing Scenarios

| # | Scenario | Expected Result | Pass |
|---|----------|-----------------|------|
| 1 | Fill step 1-5, refresh browser | Recovery banner appears | [ ] |
| 2 | Click "Restaurar" | Form data restored to all steps | [ ] |
| 3 | Click "Descartar" | Fresh wizard starts, draft deleted | [ ] |
| 4 | Upload 3 documents, refresh | Documents restored with previews | [ ] |
| 5 | Navigate steps 1→5→3→7 | All step data preserved | [ ] |
| 6 | Fill data, close browser tab | Native "Leave site?" dialog shown | [ ] |
| 7 | Use router link to leave wizard (dirty) | Confirmation dialog shown | [ ] |
| 8 | Use router link to leave wizard (clean) | No dialog, navigation proceeds | [ ] |
| 9 | Complete wizard, submit | Draft deleted from IndexedDB | [ ] |
| 10 | Wait 8 days, open wizard | Stale draft not offered | [ ] |

### Browser DevTools Verification

1. Open **Application** tab → **IndexedDB** → `CoreLedgerWizardDB`
2. Verify `drafts` table has entries with correct structure
3. Verify `files` table stores Blob data
4. Check storage quota usage
5. Verify cleanup removes old entries

### Automated Tests

| Test File | Coverage |
|-----------|----------|
| `wizard-persistence.service.spec.ts` | CRUD ops, file blobs, cleanup |
| `unsaved-changes.guard.spec.ts` | Dirty/clean scenarios |
| `wizard-store.spec.ts` | Auto-save, state transitions |
| `recovery-banner.spec.ts` | Input/output bindings |
| `save-status-indicator.spec.ts` | Status display states |

---

## Dependencies

### New Package

```bash
npm install dexie
```

**Dexie.js**: TypeScript-native IndexedDB wrapper
- Version: Latest stable (^4.x)
- Size: ~20KB gzipped
- Browser support: All modern browsers

### Existing Dependencies Used

- `@ngrx/signals` - Already installed
- `@ngrx/signals/rxjs-interop` - `rxMethod` for auto-save
- `@ngrx/operators` - `tapResponse` for error handling

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| **IndexedDB quota exceeded** | Implement cleanup of old drafts; add error handling with user notification |
| **Object URL memory leaks** | Track URLs in array; revoke all in ngOnDestroy |
| **Schema migration failures** | Dexie handles migrations; add try-catch with fallback to fresh start |
| **Browser without IndexedDB** | Add feature detection; fall back to localStorage for form data (no files) |
| **Concurrent tab editing** | Use draft ID from route param; warn if editing same draft in multiple tabs |
| **Large file uploads** | Set max file size limit; chunk large files if needed |
| **Auto-save conflicts** | Debounce prevents rapid saves; last-write-wins is acceptable for drafts |

---

## File Structure After Refactoring

```
wizard/
├── wizard-container.ts          # MODIFIED: Recovery, guards, beforeunload
├── wizard-container.html        # MODIFIED: Recovery banner, save status
├── wizard-container.scss
├── wizard-store.ts              # MODIFIED: Persistence state, rxMethod
├── wizard.routes.ts             # MODIFIED: canDeactivate guard
├── models/
│   ├── wizard.model.ts
│   ├── persistence.model.ts     # NEW
│   ├── identificacao.model.ts
│   └── ... (other models)
├── services/
│   ├── wizard-database.ts       # NEW
│   └── wizard-persistence.service.ts  # NEW
├── guards/
│   └── unsaved-changes.guard.ts # NEW
├── components/
│   ├── wizard-stepper/
│   ├── wizard-navigation/
│   ├── recovery-banner/         # NEW
│   │   ├── recovery-banner.ts
│   │   └── index.ts
│   └── save-status-indicator/   # NEW
│       ├── save-status-indicator.ts
│       └── index.ts
└── steps/
    ├── identificacao-step/
    ├── classificacao-step/
    ├── caracteristicas-step/
    ├── parametros-cota-step/
    ├── taxas-step/
    ├── prazos-step/
    ├── classes-step/
    ├── parametros-fidc-step/
    ├── vinculos-step/
    ├── documentos-step/         # MODIFIED: File persistence
    └── placeholder-step/
```

---

## Summary

This refactoring addresses all three identified issues through:

1. **IndexedDB persistence** with Dexie.js for both form data and file uploads
2. **Auto-save** with debounced rxMethod and exponential backoff retry
3. **Recovery system** with user-friendly banner and restore/dismiss options
4. **Navigation guards** protecting against accidental data loss
5. **Validation fixes** through standardized isRestoring pattern

The implementation follows Angular 21 best practices, uses signals throughout, and integrates cleanly with the existing NgRx SignalStore architecture.
