# Wizard Validation State Refactoring Plan

## Executive Summary

Refactor the cadastro fundo wizard steps to use a **loading flag pattern** instead of manually calling validator methods after form restoration. This approach leverages Angular's natural event flow while preventing store update loops.

---

## Problem Statement

### Current Issue
When navigating between wizard steps, form data is restored using `patchValue(..., { emitEvent: false })`. This prevents:
1. `valueChanges` subscriptions from firing (used for conditional validators)
2. FormArray-level validators from being re-evaluated
3. Cross-field validation from running

### Current Solution (To Be Replaced)
Each step has helper methods that manually re-apply conditional validators after restoration:
- `taxas-step.ts`: `applyTipoTaxaValidators()`
- `prazos-step.ts`: `applyResgateProgramadoValidators()`
- `classes-step.ts`: `applyTipoClasseDefaults()`
- `vinculos-step.ts`: `revalidateDates()`
- `caracteristicas-step.ts`: `reapplyConditionalValidators()`

### Target Solution
Use a **loading flag** to skip store updates during restoration while allowing events to emit naturally:

```typescript
private isRestoring = false;

// Guard store updates
this.form.valueChanges
  .pipe(filter(() => !this.isRestoring))
  .subscribe(value => this.wizardStore.setStepData(...));

// During restoration
this.isRestoring = true;
this.form.patchValue(data); // Events fire, validators run
this.isRestoring = false;
```

---

## Files to Modify

### Primary Files (Wizard Steps)

| File | Path | Type |
|------|------|------|
| taxas-step.ts | `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/taxas-step/taxas-step.ts` | FormArray |
| prazos-step.ts | `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/prazos-step/prazos-step.ts` | FormArray |
| classes-step.ts | `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/classes-step/classes-step.ts` | FormArray |
| vinculos-step.ts | `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/vinculos-step/vinculos-step.ts` | FormArray |
| caracteristicas-step.ts | `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/caracteristicas-step/caracteristicas-step.ts` | FormGroup |

### Secondary Files (Review for Consistency)

| File | Path |
|------|------|
| identificacao-step.ts | `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/identificacao-step/identificacao-step.ts` |
| classificacao-step.ts | `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/classificacao-step/classificacao-step.ts` |
| parametros-cota-step.ts | `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/parametros-cota-step/parametros-cota-step.ts` |
| parametros-fidc-step.ts | `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/parametros-fidc-step/parametros-fidc-step.ts` |

---

## Refactoring Pattern

### Before (Current Implementation)

```typescript
export class ExampleStep {
  private lastLoadedStepId: WizardStepId | null = null;

  constructor() {
    // Store update subscription - NO FILTER
    this.form.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        const stepConfig = untracked(() => this.stepConfig());
        this.wizardStore.setStepData(stepConfig.key, this.prepareDataForStore(value));
      });

    // Conditional validator subscription
    this.form.get('fieldA')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        this.applyConditionalValidators(value);
      });

    // Data loading effect
    effect(() => {
      const stepConfig = this.stepConfig();
      if (this.lastLoadedStepId === stepConfig.id) return;
      this.lastLoadedStepId = stepConfig.id;

      // Reset and restore with emitEvent: false
      this.form.reset({}, { emitEvent: false });

      const stepData = untracked(() => this.wizardStore.stepData()[stepConfig.key]);
      if (stepData) {
        this.form.patchValue(stepData, { emitEvent: false });

        // MANUALLY re-apply validators (to be removed)
        this.applyConditionalValidators(stepData.fieldA);
      }

      this.form.updateValueAndValidity({ emitEvent: false });
      this.markAllAsTouched();
      untracked(() => this.updateStepValidation());
    });
  }

  // Helper method to be REMOVED
  private applyConditionalValidators(value: any): void {
    // ... validator logic
  }
}
```

### After (Target Implementation)

```typescript
export class ExampleStep {
  private lastLoadedStepId: WizardStepId | null = null;

  // NEW: Loading flag to prevent store updates during restoration
  private isRestoring = false;

  constructor() {
    // Store update subscription - WITH FILTER
    this.form.valueChanges
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        filter(() => !this.isRestoring) // Skip during restoration
      )
      .subscribe((value) => {
        const stepConfig = untracked(() => this.stepConfig());
        this.wizardStore.setStepData(stepConfig.key, this.prepareDataForStore(value));
      });

    // Conditional validator subscription - UNCHANGED
    // (will fire naturally when patchValue is called without emitEvent: false)
    this.form.get('fieldA')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        // Apply conditional validators - this will fire during restoration
        const fieldBControl = this.form.get('fieldB');
        if (value === 'X') {
          fieldBControl?.setValidators([Validators.required]);
        } else {
          fieldBControl?.clearValidators();
        }
        fieldBControl?.updateValueAndValidity({ emitEvent: false });
      });

    // Data loading effect
    effect(() => {
      const stepConfig = this.stepConfig();
      if (this.lastLoadedStepId === stepConfig.id) return;
      this.lastLoadedStepId = stepConfig.id;

      // Set restoration flag
      this.isRestoring = true;

      // Reset form (events can emit, store updates filtered out)
      this.form.reset({});

      const stepData = untracked(() => this.wizardStore.stepData()[stepConfig.key]);
      if (stepData) {
        // Patch WITHOUT emitEvent: false - let events fire naturally
        this.form.patchValue(this.prepareDataForForm(stepData));
      }

      // Clear restoration flag
      this.isRestoring = false;

      // Final validation update
      this.form.updateValueAndValidity();
      this.markAllAsTouched();
      untracked(() => this.updateStepValidation());
    });
  }

  // REMOVED: applyConditionalValidators() method no longer needed
}
```

---

## Step-by-Step Refactoring Instructions

### Step 1: taxas-step.ts

**Location**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/taxas-step/taxas-step.ts`

#### 1.1 Add loading flag property

```typescript
// After line ~93 (after lastLoadedStepId declaration)
private isRestoring = false;
```

#### 1.2 Add filter to valueChanges subscription

Find this code block (around line 117-121):
```typescript
this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
  const stepConfig = untracked(() => this.stepConfig());
  const dataForStore = this.prepareDataForStore(value);
  this.wizardStore.setStepData(stepConfig.key, dataForStore);
});
```

Replace with:
```typescript
this.form.valueChanges
  .pipe(
    takeUntilDestroyed(this.destroyRef),
    filter(() => !this.isRestoring)
  )
  .subscribe((value) => {
    const stepConfig = untracked(() => this.stepConfig());
    const dataForStore = this.prepareDataForStore(value);
    this.wizardStore.setStepData(stepConfig.key, dataForStore);
  });
```

#### 1.3 Add filter import

Add `filter` to the rxjs imports:
```typescript
import { filter } from 'rxjs/operators';
// or add to existing import line
```

#### 1.4 Modify the data loading effect

Find the effect (around line 152-197) and modify:

```typescript
effect(() => {
  const stepConfig = this.stepConfig();
  const stepId = stepConfig.id;

  if (this.lastLoadedStepId === stepId) {
    return;
  }
  this.lastLoadedStepId = stepId;

  // Set restoration flag
  this.isRestoring = true;

  // Reset form array - events can emit now
  const taxasArray = this.form.get('taxas') as FormArray;
  taxasArray.clear();

  const stepData = untracked(
    () => this.wizardStore.stepData()[stepConfig.key] as TaxasFormData | undefined
  );

  if (stepData?.taxas && stepData.taxas.length > 0) {
    // Restore saved data - events fire, setupConditionalValidators runs automatically
    stepData.taxas.forEach((taxa) => {
      taxasArray.push(this.createTaxaFormGroup(taxa));
    });
  } else {
    // RF-01: Add default administracao tax
    taxasArray.push(this.createTaxaFormGroup(this.getDefaultAdministracaoTaxa()));
  }

  // Clear restoration flag
  this.isRestoring = false;

  // Final validation update
  taxasArray.updateValueAndValidity();
  this.form.updateValueAndValidity();

  // Mark all fields as touched
  this.markAllAsTouched();

  untracked(() => this.updateStepValidation());
});
```

#### 1.5 Revert setupConditionalValidators to original

The `setupConditionalValidators` method should contain the actual validator logic (not call a helper):

```typescript
private setupConditionalValidators(group: FormGroup): void {
  group
    .get('tipoTaxa')
    ?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe((tipoTaxa: TipoTaxa | null) => {
      const percentualControl = group.get('percentual');
      const benchmarkIdControl = group.get('benchmarkId');
      const percentualBenchmarkControl = group.get('percentualBenchmark');

      if (tipoTaxa) {
        const maxPercentual = LIMITES_PERCENTUAL[tipoTaxa];
        percentualControl?.setValidators([
          Validators.required,
          Validators.min(0.000001),
          Validators.max(maxPercentual),
        ]);

        if (tipoTaxa === TipoTaxa.PERFORMANCE) {
          benchmarkIdControl?.setValidators([Validators.required]);
          percentualBenchmarkControl?.setValidators([
            Validators.min(0),
            Validators.max(200),
          ]);
          if (!group.get('baseCalculo')?.value) {
            group.get('baseCalculo')?.setValue(BaseCalculo.RENDIMENTO_ACIMA_BENCHMARK, { emitEvent: false });
          }
        } else {
          benchmarkIdControl?.clearValidators();
          benchmarkIdControl?.setValue(null, { emitEvent: false });
          percentualBenchmarkControl?.clearValidators();
          group.get('possuiHurdle')?.setValue(false, { emitEvent: false });
          group.get('possuiHighWaterMark')?.setValue(false, { emitEvent: false });
        }
      }

      percentualControl?.updateValueAndValidity({ emitEvent: false });
      benchmarkIdControl?.updateValueAndValidity({ emitEvent: false });
      percentualBenchmarkControl?.updateValueAndValidity({ emitEvent: false });
    });
}
```

#### 1.6 Remove helper method

Delete the `applyTipoTaxaValidators()` method entirely.

---

### Step 2: prazos-step.ts

**Location**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/prazos-step/prazos-step.ts`

#### 2.1 Add loading flag property

```typescript
// After lastLoadedStepId declaration
private isRestoring = false;
```

#### 2.2 Add filter to valueChanges subscription

```typescript
this.form.valueChanges
  .pipe(
    takeUntilDestroyed(this.destroyRef),
    filter(() => !this.isRestoring)
  )
  .subscribe((value) => {
    const stepConfig = untracked(() => this.stepConfig());
    const dataForStore = this.prepareDataForStore(value as { prazos: Partial<FundoPrazo>[] });
    this.wizardStore.setStepData(stepConfig.key, dataForStore);
  });
```

#### 2.3 Add filter import

```typescript
import { filter } from 'rxjs/operators';
```

#### 2.4 Modify the data loading effect

```typescript
effect(() => {
  const stepConfig = this.stepConfig();
  const stepId = stepConfig.id;

  if (this.lastLoadedStepId === stepId) {
    return;
  }
  this.lastLoadedStepId = stepId;

  const identificacaoData = untracked(
    () => this.wizardStore.stepData()['identificacao'] as IdentificacaoFormData | undefined
  );
  const tipoFundo = identificacaoData?.tipoFundo ?? null;
  this.tipoFundoSignal.set(tipoFundo);

  // Set restoration flag
  this.isRestoring = true;

  const prazosArray = this.form.get('prazos') as FormArray;
  prazosArray.clear();

  const stepData = untracked(
    () => this.wizardStore.stepData()[stepConfig.key] as PrazosFormData | undefined
  );

  if (stepData?.prazos && stepData.prazos.length > 0) {
    stepData.prazos.forEach((prazo) => {
      prazosArray.push(this.createPrazoFormGroup(prazo));
    });
  } else {
    prazosArray.push(this.createPrazoFormGroup(createPrazoAplicacao(tipoFundo)));
    prazosArray.push(this.createPrazoFormGroup(createPrazoResgate(tipoFundo)));
  }

  // Clear restoration flag
  this.isRestoring = false;

  prazosArray.updateValueAndValidity();
  this.form.updateValueAndValidity();
  this.markAllAsTouched();
  untracked(() => this.updateStepValidation());
});
```

#### 2.5 Revert setupConditionalValidators to original

```typescript
private setupConditionalValidators(group: FormGroup): void {
  group
    .get('permiteResgateProgramado')
    ?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe((permiteProgramado: boolean) => {
      const prazoMaximoControl = group.get('prazoMaximoProgramacao');
      if (permiteProgramado) {
        prazoMaximoControl?.setValidators([
          Validators.required,
          Validators.min(1),
          Validators.max(MAX_PRAZO_DIAS),
        ]);
      } else {
        prazoMaximoControl?.clearValidators();
        prazoMaximoControl?.setValidators([Validators.min(1), Validators.max(MAX_PRAZO_DIAS)]);
        prazoMaximoControl?.setValue(null, { emitEvent: false });
      }
      prazoMaximoControl?.updateValueAndValidity({ emitEvent: false });
    });
}
```

#### 2.6 Remove helper method

Delete `applyResgateProgramadoValidators()` method.

---

### Step 3: classes-step.ts

**Location**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/classes-step/classes-step.ts`

#### 3.1 Add loading flag property

```typescript
private isRestoring = false;
```

#### 3.2 Add filter to valueChanges subscription

```typescript
this.form.valueChanges
  .pipe(
    takeUntilDestroyed(this.destroyRef),
    filter(() => !this.isRestoring)
  )
  .subscribe((value) => {
    const stepConfig = untracked(() => this.stepConfig());
    const dataForStore = this.prepareDataForStore(value);
    this.wizardStore.setStepData(stepConfig.key, dataForStore);
  });
```

#### 3.3 Add filter import

```typescript
import { filter } from 'rxjs/operators';
```

#### 3.4 Modify the data loading effect

```typescript
effect(() => {
  const stepConfig = this.stepConfig();
  const stepId = stepConfig.id;
  const isFidc = this.isFidc();

  if (this.lastLoadedStepId === stepId) {
    return;
  }
  this.lastLoadedStepId = stepId;

  // Set restoration flag
  this.isRestoring = true;

  const classesArray = this.form.get('classes') as FormArray;
  classesArray.clear();

  const stepData = untracked(() => this.wizardStore.stepData()[stepConfig.key] as ClassesFormData | undefined);

  if (stepData) {
    this.form.get('multiclasse')?.setValue(stepData.multiclasse);
    this.isMulticlasse.set(stepData.multiclasse);

    if (stepData.classes && stepData.classes.length > 0) {
      stepData.classes.forEach((classe) => {
        classesArray.push(this.createClasseFormGroup(classe, isFidc));
      });
    }
  } else if (isFidc) {
    this.form.get('multiclasse')?.setValue(true);
    this.isMulticlasse.set(true);
    classesArray.push(this.createClasseFormGroup(this.getDefaultSeniorClasse(), true));
  }

  // Clear restoration flag
  this.isRestoring = false;

  classesArray.updateValueAndValidity();
  this.form.updateValueAndValidity();
  this.markAllAsTouched();
  untracked(() => this.updateStepValidation());
});
```

#### 3.5 Revert setupTipoClasseChangeHandler to original

```typescript
private setupTipoClasseChangeHandler(group: FormGroup): void {
  group
    .get('tipoClasseFidc')
    ?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe((tipoClasse: TipoClasseFidc | null) => {
      if (tipoClasse) {
        const defaultResp = getDefaultResponsabilidadeLimitada(tipoClasse);
        group.get('responsabilidadeLimitada')?.setValue(defaultResp, { emitEvent: false });

        if (!group.get('ordemSubordinacao')?.value) {
          const defaultOrdem = getDefaultOrdemSubordinacao(tipoClasse);
          group.get('ordemSubordinacao')?.setValue(defaultOrdem, { emitEvent: false });
        }
      }
    });
}
```

#### 3.6 Remove helper method

Delete `applyTipoClasseDefaults()` method.

---

### Step 4: vinculos-step.ts

**Location**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/vinculos-step/vinculos-step.ts`

#### 4.1 Add loading flag property

```typescript
private isRestoring = false;
```

#### 4.2 Add filter to valueChanges subscription

```typescript
this.form.valueChanges
  .pipe(
    takeUntilDestroyed(this.destroyRef),
    filter(() => !this.isRestoring)
  )
  .subscribe((value) => {
    const stepConfig = untracked(() => this.stepConfig());
    const dataForStore = this.prepareDataForStore(value);
    this.wizardStore.setStepData(stepConfig.key, dataForStore);
  });
```

#### 4.3 Add filter import

```typescript
import { filter } from 'rxjs/operators';
```

#### 4.4 Modify the data loading effect

```typescript
effect(() => {
  const stepConfig = this.stepConfig();
  const stepId = stepConfig.id;

  if (this.lastLoadedStepId === stepId) {
    return;
  }
  this.lastLoadedStepId = stepId;

  // Set restoration flag
  this.isRestoring = true;

  const vinculosArray = this.form.get('vinculos') as FormArray;
  vinculosArray.clear();

  const stepData = untracked(
    () => this.wizardStore.stepData()[stepConfig.key] as VinculosFormData | undefined
  );

  if (stepData && stepData.vinculos && stepData.vinculos.length > 0) {
    stepData.vinculos.forEach((vinculo) => {
      vinculosArray.push(this.createVinculoFormGroup(vinculo));
    });
  } else {
    VINCULOS_OBRIGATORIOS.forEach((tipo) => {
      vinculosArray.push(
        this.createVinculoFormGroup({
          ...VINCULO_DEFAULT,
          tipoVinculo: tipo,
          dataInicio: this.getTodayISODate(),
        } as VinculoFormData)
      );
    });
  }

  // Clear restoration flag
  this.isRestoring = false;

  vinculosArray.updateValueAndValidity();
  this.form.updateValueAndValidity();
  this.markAllAsTouched();
  untracked(() => this.updateStepValidation());
});
```

#### 4.5 Revert setupDateValidation to original

```typescript
private setupDateValidation(group: FormGroup): void {
  group.get('dataFim')?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
    const dataInicio = group.get('dataInicio')?.value;
    const dataFim = group.get('dataFim')?.value;
    const dataFimControl = group.get('dataFim');

    if (dataInicio && dataFim && dataFim < dataInicio) {
      dataFimControl?.setErrors({ dataFimAnterior: true });
    } else if (dataFimControl?.hasError('dataFimAnterior')) {
      const errors = { ...dataFimControl.errors };
      delete errors['dataFimAnterior'];
      dataFimControl.setErrors(Object.keys(errors).length > 0 ? errors : null);
    }
  });

  group.get('dataInicio')?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
    const dataInicio = group.get('dataInicio')?.value;
    const today = this.getTodayISODate();
    const dataInicioControl = group.get('dataInicio');

    if (dataInicio && dataInicio > today) {
      dataInicioControl?.setErrors({ dataInicioFutura: true });
    } else if (dataInicioControl?.hasError('dataInicioFutura')) {
      const errors = { ...dataInicioControl.errors };
      delete errors['dataInicioFutura'];
      dataInicioControl.setErrors(Object.keys(errors).length > 0 ? errors : null);
    }
  });
}
```

#### 4.6 Remove helper methods

Delete these methods:
- `validateDataFim()`
- `validateDataInicio()`
- `revalidateDates()`

---

### Step 5: caracteristicas-step.ts

**Location**: `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/caracteristicas-step/caracteristicas-step.ts`

#### 5.1 Add loading flag property

```typescript
private isRestoring = false;
```

#### 5.2 Add filter to valueChanges subscription

```typescript
this.form.valueChanges
  .pipe(
    takeUntilDestroyed(this.destroyRef),
    filter(() => !this.isRestoring)
  )
  .subscribe((value) => {
    const stepConfig = untracked(() => this.stepConfig());
    const dataForStore = this.prepareDataForStore(value);
    this.wizardStore.setStepData(stepConfig.key, dataForStore);
  });
```

#### 5.3 Add filter import

```typescript
import { filter } from 'rxjs/operators';
```

#### 5.4 Convert effects to valueChanges subscriptions

The caracteristicas-step uses `effect()` for conditional validators which read form values imperatively. Convert these to `valueChanges` subscriptions:

**Replace the prazo effect with:**
```typescript
this.form.get('prazo')?.valueChanges
  .pipe(takeUntilDestroyed(this.destroyRef))
  .subscribe((prazo: Prazo | null) => {
    const dataEncerramentoControl = this.form.get('dataEncerramento');

    if (prazo === Prazo.DETERMINADO) {
      dataEncerramentoControl?.setValidators([Validators.required, dataFuturaValidator]);
    } else {
      dataEncerramentoControl?.clearValidators();
      dataEncerramentoControl?.setValue(null, { emitEvent: false });
    }

    dataEncerramentoControl?.updateValueAndValidity({ emitEvent: false });
  });
```

**Replace the alavancagem effect with:**
```typescript
this.form.get('permiteAlavancagem')?.valueChanges
  .pipe(takeUntilDestroyed(this.destroyRef))
  .subscribe((permiteAlavancagem: boolean) => {
    const limiteAlavancagemControl = this.form.get('limiteAlavancagem');

    if (permiteAlavancagem) {
      limiteAlavancagemControl?.setValidators([
        Validators.required,
        Validators.min(1.01),
        Validators.max(10.0),
      ]);
    } else {
      limiteAlavancagemControl?.clearValidators();
      limiteAlavancagemControl?.setValue(null, { emitEvent: false });
    }

    limiteAlavancagemControl?.updateValueAndValidity({ emitEvent: false });
  });
```

**Replace the exclusivo effect with:**
```typescript
this.form.get('exclusivo')?.valueChanges
  .pipe(takeUntilDestroyed(this.destroyRef))
  .subscribe((exclusivo: boolean) => {
    const reservadoControl = this.form.get('reservado');
    if (exclusivo) {
      reservadoControl?.setValue(true, { emitEvent: false });
    }
  });
```

**Replace the cripto info effect with:**
```typescript
this.form.get('aceitaCripto')?.valueChanges
  .pipe(takeUntilDestroyed(this.destroyRef))
  .subscribe((aceitaCripto: boolean) => {
    this.showCriptoInfo.set(aceitaCripto === true);
  });
```

#### 5.5 Modify the data loading effect

```typescript
effect(() => {
  const stepConfig = this.stepConfig();
  const stepId = stepConfig.id;

  if (this.lastLoadedStepId === stepId) {
    return;
  }
  this.lastLoadedStepId = stepId;

  // Set restoration flag
  this.isRestoring = true;

  this.form.reset({
    condominio: null,
    prazo: null,
    dataEncerramento: null,
    exclusivo: false,
    reservado: false,
    permiteAlavancagem: false,
    limiteAlavancagem: null,
    aceitaCripto: false,
    percentualExterior: null,
  });

  const stepData = untracked(
    () => this.wizardStore.stepData()[stepConfig.key] as Partial<CaracteristicasFormData> | undefined
  );

  if (stepData) {
    const formValue = this.prepareDataForForm(stepData);
    this.form.patchValue(formValue); // Events fire, validators run
    this.form.markAsDirty();
  }

  // Clear restoration flag
  this.isRestoring = false;

  // Mark all fields as touched
  Object.keys(this.form.controls).forEach((key) => {
    this.form.get(key)?.markAsTouched();
  });

  this.form.updateValueAndValidity();
  untracked(() => this.updateStepValidation());
});
```

#### 5.6 Remove helper methods

Delete these methods:
- `applyPrazoValidators()`
- `applyAlavancagemValidators()`
- `applyExclusivoReservadoLogic()`
- `reapplyConditionalValidators()`

---

## Testing Checklist

### Manual Testing Steps

For each wizard step, perform these tests:

#### Test 1: Initial Load
1. Navigate to the wizard
2. Fill out the step completely with valid data
3. Verify all fields show valid state (green checkmarks)
4. Verify step is marked as complete

#### Test 2: Navigation Forward and Back
1. Complete step N with valid data
2. Navigate to step N+1
3. Navigate back to step N
4. **Verify**: All fields retain their values
5. **Verify**: All fields retain their valid/invalid state
6. **Verify**: Conditional fields are shown/hidden correctly
7. **Verify**: Step completion status is preserved

#### Test 3: Conditional Validators
1. Set a field that triggers conditional validation (e.g., `tipoTaxa = PERFORMANCE`)
2. Fill the conditional required field (e.g., `benchmarkId`)
3. Navigate away and back
4. **Verify**: Conditional field is still shown
5. **Verify**: Conditional field retains its required validator
6. **Verify**: Form validity is correct

#### Test 4: FormArray Steps
1. Add multiple items to the FormArray
2. Set different values for each item (including conditional fields)
3. Navigate away and back
4. **Verify**: All items are restored
5. **Verify**: Each item's conditional validators are applied correctly
6. **Verify**: Array-level validators work (e.g., unique codes, sequence order)

#### Test 5: Cross-Step Dependencies
1. Set a value in step 1 that affects step 3 (e.g., `tipoFundo`)
2. Complete step 3 with values that depend on step 1
3. Navigate back to step 1 and change the dependent value
4. Navigate forward to step 3
5. **Verify**: Step 3 reflects the updated dependency correctly

### Specific Tests per Step

#### taxas-step
- [ ] Add ADMINISTRACAO taxa (default)
- [ ] Add PERFORMANCE taxa with benchmark
- [ ] Verify benchmark is required when tipoTaxa = PERFORMANCE
- [ ] Navigate away and back - benchmark validator still applied
- [ ] Change tipoTaxa from PERFORMANCE to GESTAO - benchmark fields clear

#### prazos-step
- [ ] Enable permiteResgateProgramado
- [ ] Fill prazoMaximoProgramacao
- [ ] Navigate away and back
- [ ] Verify prazoMaximoProgramacao is still required
- [ ] Disable permiteResgateProgramado - field clears

#### classes-step
- [ ] For FIDC fund: verify tipoClasseFidc is required
- [ ] Add multiple classes with different ordemSubordinacao
- [ ] Verify ordemGap validator works
- [ ] Navigate away and back - validators still work

#### vinculos-step
- [ ] Fill dataInicio for each vinculo
- [ ] Set dataFim before dataInicio (should show error)
- [ ] Navigate away and back
- [ ] Verify date validation errors persist correctly

#### caracteristicas-step
- [ ] Set prazo = DETERMINADO
- [ ] Fill dataEncerramento
- [ ] Navigate away and back
- [ ] Verify dataEncerramento is still shown and required
- [ ] Set prazo = INDETERMINADO - field should clear and hide

---

## Build Verification

After completing all refactoring:

```bash
# Build the UI
nx build core-ledger-ui

# Run tests
nx test core-ledger-ui

# Serve and test manually
nx serve core-ledger-ui
```

---

## Rollback Plan

If issues arise, the previous implementation (manual validator calls) is preserved in git. To rollback:

```bash
git checkout HEAD~1 -- apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/steps/
```

---

## Summary of Changes per File

| File | Add Flag | Add Filter | Remove Methods | Convert Effects |
|------|----------|------------|----------------|-----------------|
| taxas-step.ts | `isRestoring` | Yes | `applyTipoTaxaValidators` | No |
| prazos-step.ts | `isRestoring` | Yes | `applyResgateProgramadoValidators` | No |
| classes-step.ts | `isRestoring` | Yes | `applyTipoClasseDefaults` | No |
| vinculos-step.ts | `isRestoring` | Yes | `validateDataFim`, `validateDataInicio`, `revalidateDates` | No |
| caracteristicas-step.ts | `isRestoring` | Yes | `applyPrazoValidators`, `applyAlavancagemValidators`, `applyExclusivoReservadoLogic`, `reapplyConditionalValidators` | Yes (4 effects → valueChanges) |
