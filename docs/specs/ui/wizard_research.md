# Multi-step wizard forms in Angular 21+: Patterns that actually work

**For complex 10+ step wizards with conditional logic, file uploads, and persistence requirements, the winning architecture combines NgRx SignalStore for state management, IndexedDB (via Dexie.js) for persistence including files, and Angular's functional guards for navigation protection.** This approach balances developer experience with production reliability. Angular 21's experimental Signal Forms add powerful reactive validation but aren't yet stable for critical applications—use them for new projects while keeping traditional Reactive Forms as your fallback.

The key insight: **separation of concerns matters more than any single library choice**. Keep wizard navigation logic, form validation, persistence, and recovery as distinct services that communicate through well-defined interfaces.

## State management: Signals have won, but NgRx SignalStore scales better

Angular's native signals work well for simple wizards but fall short for complex 10+ step forms with conditional logic and async operations. **NgRx SignalStore** provides the sweet spot—signal-based reactivity with structured state management and the `rxMethod` helper for debounced saves.

```typescript
import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';

interface WizardState {
  currentStepIndex: number;
  steps: Array<{ id: string; completed: boolean; valid: boolean; data: any }>;
  formData: Record<string, any>;
  saveStatus: 'idle' | 'saving' | 'saved' | 'error';
}

export const WizardStore = signalStore(
  { providedIn: 'root' },
  
  withState<WizardState>({
    currentStepIndex: 0,
    steps: [],
    formData: {},
    saveStatus: 'idle'
  }),
  
  withComputed(({ currentStepIndex, steps }) => ({
    currentStep: computed(() => steps()[currentStepIndex()]),
    progress: computed(() => Math.round(((currentStepIndex() + 1) / steps().length) * 100)),
    visibleSteps: computed(() => steps().filter(s => !s.hidden)),
    canProceed: computed(() => steps()[currentStepIndex()]?.valid ?? false)
  })),
  
  withMethods((store, http = inject(HttpClient)) => ({
    // Debounced auto-save using rxMethod
    autoSave: rxMethod<{ step: number; data: any }>(
      pipe(
        debounceTime(1000),
        tap(() => patchState(store, { saveStatus: 'saving' })),
        switchMap(({ step, data }) =>
          http.post('/api/wizard/draft', { step, data }).pipe(
            tapResponse({
              next: () => patchState(store, { saveStatus: 'saved' }),
              error: () => patchState(store, { saveStatus: 'error' })
            })
          )
        )
      )
    ),
    
    updateStepData(stepIndex: number, data: any): void {
      patchState(store, {
        formData: { ...store.formData(), [`step${stepIndex}`]: data },
        steps: store.steps().map((s, i) => 
          i === stepIndex ? { ...s, data, completed: true } : s
        )
      });
    }
  }))
);
```

**For simpler wizards (3-5 steps)**, a signal-based service suffices:

```typescript
@Injectable({ providedIn: 'root' })
export class SimpleWizardService {
  private readonly _currentStep = signal(0);
  private readonly _formData = signal<Record<string, any>>({});
  
  readonly currentStep = this._currentStep.asReadonly();
  readonly formData = this._formData.asReadonly();
  readonly progress = computed(() => ((this._currentStep() + 1) / this.totalSteps) * 100);
  
  constructor() {
    // Auto-save effect
    effect(() => {
      localStorage.setItem('wizard-draft', JSON.stringify(this._formData()));
    });
  }
}
```

**Angular 21 Signal Forms** (experimental) integrate beautifully with this architecture but carry API stability risk:

```typescript
import { form, FormField, required, email } from '@angular/forms/signals';

userForm = form(this.userModel, (schema) => {
  required(schema.email);
  email(schema.email);
});

// Auto-save integration via effect()
effect(() => {
  if (this.userForm().valid()) {
    this.wizardStore.autoSave({ step: this.currentStep(), data: this.userForm.value() });
  }
});
```

## Persistence: IndexedDB for files, localStorage for everything else

**The critical decision: file uploads require IndexedDB** because localStorage and sessionStorage only store strings with ~5MB limits. IndexedDB natively handles Blob objects and scales to hundreds of megabytes.

**Dexie.js** is the recommended wrapper—TypeScript-native, excellent migration support, and Angular-friendly:

```typescript
import Dexie, { Table } from 'dexie';

interface WizardFile {
  id?: number;
  wizardId: string;
  stepNumber: number;
  fileName: string;
  fileType: string;
  blob: Blob;
  createdAt: Date;
}

interface WizardDraft {
  id: string;
  formData: Record<string, any>;
  version: number;
  updatedAt: Date;
}

class WizardDatabase extends Dexie {
  files!: Table<WizardFile>;
  drafts!: Table<WizardDraft>;

  constructor() {
    super('WizardDB');
    this.version(1).stores({
      files: '++id, wizardId, stepNumber',
      drafts: 'id, updatedAt'
    });
    
    this.version(2).stores({
      files: '++id, wizardId, stepNumber, createdAt',
      drafts: 'id, updatedAt, version'
    }).upgrade(tx => {
      return tx.table('drafts').toCollection().modify(draft => {
        draft.version = draft.version || 1;
      });
    });
  }
}

@Injectable({ providedIn: 'root' })
export class WizardPersistenceService {
  private db = new WizardDatabase();
  
  async saveStepWithFiles(
    wizardId: string, 
    step: number, 
    data: any, 
    files?: File[]
  ): Promise<void> {
    // Save form data
    const existing = await this.db.drafts.get(wizardId);
    await this.db.drafts.put({
      id: wizardId,
      formData: { ...(existing?.formData || {}), [step]: data },
      version: 2,
      updatedAt: new Date()
    });
    
    // Save files separately
    if (files?.length) {
      for (const file of files) {
        await this.db.files.add({
          wizardId,
          stepNumber: step,
          fileName: file.name,
          fileType: file.type,
          blob: file,
          createdAt: new Date()
        });
      }
    }
  }
  
  async loadWizard(wizardId: string): Promise<{
    formData: Record<string, any>;
    files: Map<number, { file: File; url: string }[]>;
  } | null> {
    const draft = await this.db.drafts.get(wizardId);
    if (!draft) return null;
    
    const files = await this.db.files.where('wizardId').equals(wizardId).toArray();
    const filesByStep = new Map<number, { file: File; url: string }[]>();
    
    files.forEach(f => {
      const file = new File([f.blob], f.fileName, { type: f.fileType });
      const entry = { file, url: URL.createObjectURL(f.blob) };
      filesByStep.set(f.stepNumber, [...(filesByStep.get(f.stepNumber) || []), entry]);
    });
    
    return { formData: draft.formData, files: filesByStep };
  }
  
  async cleanupStale(maxAgeDays = 7): Promise<number> {
    const cutoff = new Date(Date.now() - maxAgeDays * 24 * 60 * 60 * 1000);
    const stale = await this.db.drafts.where('updatedAt').below(cutoff).toArray();
    
    for (const draft of stale) {
      await this.db.files.where('wizardId').equals(draft.id).delete();
      await this.db.drafts.delete(draft.id);
    }
    return stale.length;
  }
}
```

**For forms without file uploads**, localStorage with a simple wrapper suffices:

| Storage API | Best Use Case | Capacity | File Support |
|-------------|---------------|----------|--------------|
| **localStorage** | User preferences, small form drafts | ~5-10MB | No (strings only) |
| **sessionStorage** | Sensitive temporary data, single-tab flows | ~5-10MB | No |
| **IndexedDB** | File uploads, large datasets, offline-first | Hundreds of MB | Yes (Blobs native) |

## Navigation guards protect users from accidental data loss

Angular 17+ functional guards are cleaner than class-based guards. Combine `CanDeactivate` for router navigation with `beforeunload` for browser close/refresh:

```typescript
// unsaved-changes.guard.ts
import { CanDeactivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

export interface DirtyComponent {
  isDirty: () => boolean;
}

export const unsavedChangesGuard: CanDeactivateFn<DirtyComponent> = async (component) => {
  if (!component.isDirty()) return true;
  
  const dialog = inject(MatDialog);
  const dialogRef = dialog.open(UnsavedChangesDialogComponent);
  
  return dialogRef.afterClosed().toPromise();
};

// wizard.component.ts
@Component({...})
export class WizardComponent implements DirtyComponent {
  wizardForm: FormGroup;
  private submitted = false;
  
  isDirty(): boolean {
    return this.wizardForm.dirty && !this.submitted;
  }
  
  @HostListener('window:beforeunload', ['$event'])
  handleBeforeUnload(event: BeforeUnloadEvent): void {
    if (this.isDirty()) {
      event.preventDefault();
      event.returnValue = ''; // Triggers browser's native dialog
    }
  }
}

// Route configuration
export const routes: Routes = [
  {
    path: 'wizard',
    component: WizardComponent,
    canDeactivate: [unsavedChangesGuard]
  }
];
```

## Form architecture: Separate FormGroups per step with centralized collection

**The recommended pattern**: each step owns its own `FormGroup` but the parent wizard component collects them for final submission. This enables lazy loading of step components and cleaner partial validation.

```typescript
// wizard-container.component.ts
@Component({
  template: `
    <mat-stepper [linear]="true" [selectedIndex]="currentStep()">
      @for (step of visibleSteps(); track step.id; let i = $index) {
        <mat-step [stepControl]="stepForms.get(step.id)">
          <ng-template matStepLabel>{{ step.label }}</ng-template>
          <ng-container *ngComponentOutlet="step.component; injector: stepInjector(step.id)" />
          <div class="step-actions">
            <button mat-button matStepperPrevious [disabled]="i === 0">Back</button>
            <button mat-raised-button matStepperNext 
                    [disabled]="!stepForms.get(step.id)?.valid">
              {{ i === visibleSteps().length - 1 ? 'Submit' : 'Next' }}
            </button>
          </div>
        </mat-step>
      }
    </mat-stepper>
  `
})
export class WizardContainerComponent {
  stepForms = new Map<string, FormGroup>();
  
  registerStepForm(stepId: string, form: FormGroup): void {
    this.stepForms.set(stepId, form);
    
    // Auto-save on changes
    form.valueChanges.pipe(
      debounceTime(500),
      takeUntilDestroyed()
    ).subscribe(value => {
      this.store.autoSave({ step: stepId, data: value });
    });
  }
  
  getAllFormData(): Record<string, any> {
    const result: Record<string, any> = {};
    this.stepForms.forEach((form, id) => {
      result[id] = form.value;
    });
    return result;
  }
}

// step.component.ts (each step)
@Component({...})
export class PersonalInfoStepComponent implements OnInit {
  private wizard = inject(WizardContainerComponent);
  
  form = new FormGroup({
    firstName: new FormControl('', [Validators.required, Validators.minLength(2)]),
    lastName: new FormControl('', Validators.required),
    email: new FormControl('', {
      validators: [Validators.required, Validators.email],
      asyncValidators: [this.emailValidator.checkUnique()],
      updateOn: 'blur' // Reduce async validation calls
    })
  });
  
  ngOnInit(): void {
    this.wizard.registerStepForm('personal-info', this.form);
  }
}
```

**Async validators** should use `updateOn: 'blur'` to prevent excessive API calls:

```typescript
@Injectable({ providedIn: 'root' })
export class AsyncValidatorService {
  constructor(private http: HttpClient) {}
  
  checkEmailUnique(): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors | null> => {
      if (!control.value) return of(null);
      
      return timer(300).pipe( // Built-in debounce
        switchMap(() => this.http.get<boolean>(`/api/check-email?email=${control.value}`)),
        map(isTaken => isTaken ? { emailTaken: true } : null),
        catchError(() => of(null))
      );
    };
  }
}
```

## Conditional steps require computed visibility with validation awareness

When steps appear/disappear based on previous answers, maintain a master list and compute visibility:

```typescript
interface WizardStep {
  id: string;
  label: string;
  component: Type<any>;
  isVisible: (formData: Record<string, any>) => boolean;
}

@Injectable({ providedIn: 'root' })
export class DynamicStepService {
  private formData = signal<Record<string, any>>({});
  
  private allSteps: WizardStep[] = [
    { id: 'user-type', label: 'Account Type', component: UserTypeStep, 
      isVisible: () => true },
    { id: 'business', label: 'Business Info', component: BusinessStep,
      isVisible: (data) => data['user-type']?.type === 'business' },
    { id: 'payment', label: 'Payment', component: PaymentStep,
      isVisible: (data) => data['user-type']?.tier === 'premium' },
    { id: 'review', label: 'Review', component: ReviewStep,
      isVisible: () => true }
  ];
  
  visibleSteps = computed(() => 
    this.allSteps.filter(step => step.isVisible(this.formData()))
  );
  
  // Critical: update current step index when steps change
  currentStepIndex = computed(() => {
    const visible = this.visibleSteps();
    const currentId = this.currentStepId();
    const index = visible.findIndex(s => s.id === currentId);
    return index >= 0 ? index : 0;
  });
}
```

**Material Stepper integration** requires careful handling of dynamic steps—avoid `*ngIf` on `mat-step` elements as it breaks linear validation. Instead, filter the array before rendering.

## Auto-save: Debounced RxJS pipelines with retry and status indicators

The production-ready auto-save pattern combines **debouncing**, **distinctUntilChanged** to prevent redundant saves, and **exponential backoff retry**:

```typescript
@Component({
  template: `
    <div class="save-status">
      @switch (saveStatus()) {
        @case ('saving') { <mat-spinner diameter="16" /> Saving... }
        @case ('saved') { <mat-icon>check_circle</mat-icon> Saved }
        @case ('error') { 
          <mat-icon color="warn">error</mat-icon> 
          Save failed 
          <button mat-button (click)="retrySave()">Retry</button>
        }
      }
    </div>
  `
})
export class WizardComponent {
  saveStatus = signal<'idle' | 'saving' | 'saved' | 'error'>('idle');
  
  private setupAutoSave(): void {
    this.form.valueChanges.pipe(
      debounceTime(800),
      distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)),
      tap(() => this.saveStatus.set('saving')),
      switchMap(value => this.saveService.save(value).pipe(
        retry({ count: 3, delay: this.exponentialBackoff }),
        catchError(err => {
          this.saveStatus.set('error');
          return EMPTY;
        })
      )),
      tap(() => {
        this.saveStatus.set('saved');
        setTimeout(() => this.saveStatus.set('idle'), 2000);
      }),
      takeUntilDestroyed()
    ).subscribe();
  }
  
  private exponentialBackoff(error: any, retryCount: number): Observable<number> {
    const delay = Math.pow(2, retryCount) * 1000; // 1s, 2s, 4s
    return timer(delay);
  }
}
```

## Recovery: Detect, prompt, and merge intelligently

On wizard initialization, check for recovery data and prompt users:

```typescript
@Component({
  template: `
    @if (showRecoveryBanner()) {
      <div class="recovery-banner" @slideIn>
        <mat-icon>restore</mat-icon>
        <span>Resume your progress from {{ recoveryTimestamp() | date:'short' }}?</span>
        <button mat-button (click)="restoreData()">Restore</button>
        <button mat-icon-button (click)="dismissRecovery()">
          <mat-icon>close</mat-icon>
        </button>
      </div>
    }
  `
})
export class WizardComponent implements OnInit {
  showRecoveryBanner = signal(false);
  recoveryData = signal<WizardDraft | null>(null);
  
  async ngOnInit(): Promise<void> {
    const draft = await this.persistence.loadWizard(this.wizardId);
    
    if (draft && this.isDataValid(draft)) {
      this.recoveryData.set(draft);
      this.showRecoveryBanner.set(true);
    }
  }
  
  private isDataValid(draft: WizardDraft): boolean {
    const maxAge = 7 * 24 * 60 * 60 * 1000; // 7 days
    return Date.now() - draft.updatedAt.getTime() < maxAge;
  }
  
  restoreData(): void {
    const draft = this.recoveryData();
    if (!draft) return;
    
    // Patch each step's form with recovered data
    Object.entries(draft.formData).forEach(([stepId, data]) => {
      const stepForm = this.stepForms.get(stepId);
      stepForm?.patchValue(data, { emitEvent: false });
    });
    
    // Restore files
    draft.files.forEach((files, stepIndex) => {
      this.fileInputs.get(stepIndex)?.setValue(files);
    });
    
    this.showRecoveryBanner.set(false);
  }
}
```

## Libraries worth evaluating

- **NgRx SignalStore** — Best-in-class for wizard state management with signals
- **Dexie.js** — IndexedDB wrapper essential for file upload persistence  
- **Angular Material Stepper** — Production-ready but limited customization; use CDK Stepper for custom designs
- **ng-wizard** — Good for Bootstrap-themed wizards with conditional step support
- **ngx-formly** — JSON-powered dynamic forms, excellent for admin-configurable wizards
- **@ngneat/dirty-check-forms** — Sophisticated dirty detection comparing against store state

## Conclusion

The most maintainable wizard architecture separates state (NgRx SignalStore), persistence (Dexie.js + IndexedDB), navigation (functional guards), and forms (Reactive Forms with step-level FormGroups). Angular 21's Signal Forms show promise for simplifying reactive validation but need another release or two before production adoption. **Start with this foundation**: signal-based state management, IndexedDB for file persistence, debounced auto-save with retry logic, and always implement both CanDeactivate guards and beforeunload handlers. The investment in proper architecture pays dividends when requirements inevitably expand.