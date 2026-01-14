import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  signal,
  untracked,
} from '@angular/core';
import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter, startWith } from 'rxjs/operators';
import { CurrencyMaskDirective } from '../../../../../../directives/currency-mask.directive';
import { WizardStepConfig, WizardStepId, InvalidFieldInfo } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';
import { IdentificacaoFormData, TipoFundo } from '../../models/identificacao.model';
import {
  createPrazoAplicacao,
  createPrazoResgate,
  FundoPrazo,
  MAX_PRAZO_DIAS,
  MAX_PRAZOS,
  MAX_VALOR_MINIMO,
  PrazosFormData,
  TIPO_CALENDARIO_OPTIONS,
  TIPO_OPERACAO_OPTIONS,
  TipoCalendario,
  TipoOperacao,
} from '../../models/prazos.model';

/**
 * Validador customizado: prazo liquidacao >= prazo cotizacao (RF-03)
 */
function prazoLiquidacaoValidator(group: AbstractControl): ValidationErrors | null {
  const cotizacao = group.get('prazoCotizacao')?.value;
  const liquidacao = group.get('prazoLiquidacao')?.value;

  if (cotizacao === null || liquidacao === null) return null;

  return liquidacao >= cotizacao ? null : { liquidacaoMenorQueCotizacao: true };
}

/**
 * Componente para Etapa 6 do wizard: Prazos Operacionais
 * Gerencia prazos de aplicacao e resgate do fundo
 */
@Component({
  selector: 'app-prazos-step',
  imports: [ReactiveFormsModule, CurrencyMaskDirective],
  templateUrl: './prazos-step.html',
  styleUrl: './prazos-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PrazosStep {
  stepConfig = input.required<WizardStepConfig>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly wizardStore = inject(WizardStore);
  private readonly destroyRef = inject(DestroyRef);

  // Enum options for template
  readonly tipoOperacaoOptions = TIPO_OPERACAO_OPTIONS;
  readonly tipoCalendarioOptions = TIPO_CALENDARIO_OPTIONS;
  readonly maxPrazos = MAX_PRAZOS;
  readonly maxPrazoDias = MAX_PRAZO_DIAS;

  // Track step ID to avoid re-loading
  private lastLoadedStepId: WizardStepId | null = null;

  // Flag to prevent store updates during data restoration
  private isRestoring = false;

  // Main form with FormArray
  form = this.formBuilder.group({
    prazos: this.formBuilder.array<FormGroup>([]),
  });

  // Computed signals
  readonly prazosArray = computed(() => this.form.get('prazos') as FormArray);
  readonly prazosCount = computed(() => this.prazosArray().length);
  readonly canAddPrazo = computed(() => this.prazosCount() < MAX_PRAZOS);

  readonly hasAplicacao = computed(() => {
    const prazos = this.prazosArray().controls;
    return prazos.some(
      (control) => control.get('tipoOperacao')?.value === TipoOperacao.APLICACAO
    );
  });

  readonly hasResgate = computed(() => {
    const prazos = this.prazosArray().controls;
    return prazos.some(
      (control) => control.get('tipoOperacao')?.value === TipoOperacao.RESGATE
    );
  });

  // Track current tipo fundo for defaults
  private readonly tipoFundoSignal = signal<TipoFundo | null>(null);

  constructor() {
    // Setup form subscriptions
    this.form.statusChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.updateStepValidation());

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

    // Effect: Load data when step changes
    effect(() => {
      const stepConfig = this.stepConfig();
      const stepId = stepConfig.id;

      if (this.lastLoadedStepId === stepId) {
        return;
      }
      this.lastLoadedStepId = stepId;

      // Get identificacao data for tipo fundo defaults (RF-02)
      const identificacaoData = untracked(
        () => this.wizardStore.stepData()['identificacao'] as IdentificacaoFormData | undefined
      );
      const tipoFundo = identificacaoData?.tipoFundo ?? null;
      this.tipoFundoSignal.set(tipoFundo);

      // Set restoration flag to prevent store updates
      this.isRestoring = true;

      // Reset form array - events can emit now (store updates are filtered)
      const prazosArray = this.form.get('prazos') as FormArray;
      prazosArray.clear();

      const stepData = untracked(
        () => this.wizardStore.stepData()[stepConfig.key] as PrazosFormData | undefined
      );

      if (stepData?.prazos && stepData.prazos.length > 0) {
        // Restore saved data - setupConditionalValidators runs automatically via startWith
        stepData.prazos.forEach((prazo) => {
          prazosArray.push(this.createPrazoFormGroup(prazo));
        });
      } else {
        // RF-01: Add default aplicacao and resgate prazos
        prazosArray.push(this.createPrazoFormGroup(createPrazoAplicacao(tipoFundo)));
        prazosArray.push(this.createPrazoFormGroup(createPrazoResgate(tipoFundo)));
      }

      // Clear restoration flag
      this.isRestoring = false;

      // Final validation update
      prazosArray.updateValueAndValidity();
      this.form.updateValueAndValidity();

      // Mark all fields as touched
      this.markAllAsTouched();

      untracked(() => this.updateStepValidation());
    });
  }

  /**
   * Create a FormGroup for a single prazo
   */
  private createPrazoFormGroup(prazo?: Partial<FundoPrazo>): FormGroup {
    const group = this.formBuilder.group(
      {
        tipoOperacao: [prazo?.tipoOperacao ?? null, [Validators.required]],
        prazoCotizacao: [
          prazo?.prazoCotizacao ?? 0,
          [Validators.required, Validators.min(0), Validators.max(MAX_PRAZO_DIAS)],
        ],
        prazoLiquidacao: [
          prazo?.prazoLiquidacao ?? 0,
          [Validators.required, Validators.min(0), Validators.max(MAX_PRAZO_DIAS)],
        ],
        tipoCalendario: [prazo?.tipoCalendario ?? TipoCalendario.NACIONAL, [Validators.required]],
        valorMinimoInicial: [prazo?.valorMinimoInicial ?? null, [Validators.min(0), Validators.max(MAX_VALOR_MINIMO)]],
        valorMinimoAdicional: [prazo?.valorMinimoAdicional ?? null, [Validators.min(0), Validators.max(MAX_VALOR_MINIMO)]],
        valorMinimoResgate: [prazo?.valorMinimoResgate ?? null, [Validators.min(0), Validators.max(MAX_VALOR_MINIMO)]],
        valorMinimoPermanencia: [prazo?.valorMinimoPermanencia ?? null, [Validators.min(0), Validators.max(MAX_VALOR_MINIMO)]],
        prazoCarenciaDias: [prazo?.prazoCarenciaDias ?? null, [Validators.min(0), Validators.max(MAX_PRAZO_DIAS)]],
        permiteResgateTotal: [prazo?.permiteResgateTotal ?? true],
        permiteResgateProgramado: [prazo?.permiteResgateProgramado ?? false],
        prazoMaximoProgramacao: [prazo?.prazoMaximoProgramacao ?? null, [Validators.min(1), Validators.max(MAX_PRAZO_DIAS)]],
        classeId: [prazo?.classeId ?? null],
        ativo: [prazo?.ativo ?? true],
      },
      { validators: prazoLiquidacaoValidator }
    );

    // Setup conditional validators
    this.setupConditionalValidators(group);

    return group;
  }

  /**
   * Setup conditional validators when fields change.
   * Uses startWith to apply validators immediately for initial value.
   */
  private setupConditionalValidators(group: FormGroup): void {
    // RF-07: prazoMaximoProgramacao required when permiteResgateProgramado is true
    const permiteControl = group.get('permiteResgateProgramado');
    permiteControl?.valueChanges
      .pipe(
        startWith(permiteControl.value),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((permiteProgramado: boolean) => {
        this.applyResgateProgramadoValidators(group, permiteProgramado);
      });
  }

  /**
   * Apply validators based on permiteResgateProgramado value.
   * Called from valueChanges subscription (including startWith for initial value).
   */
  private applyResgateProgramadoValidators(group: FormGroup, permiteProgramado: boolean): void {
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
  }

  /**
   * Add a new prazo to the list
   */
  addPrazo(): void {
    if (!this.canAddPrazo()) return;

    const prazosArray = this.form.get('prazos') as FormArray;
    const newPrazo: Partial<FundoPrazo> = {
      tipoOperacao: null,
      prazoCotizacao: 0,
      prazoLiquidacao: 0,
      tipoCalendario: TipoCalendario.NACIONAL,
      permiteResgateTotal: true,
      permiteResgateProgramado: false,
      ativo: true,
    };

    prazosArray.push(this.createPrazoFormGroup(newPrazo));
  }

  /**
   * Remove a prazo from the list
   * Cannot remove the last aplicacao or resgate prazo (RF-01)
   */
  removePrazo(index: number): void {
    const prazosArray = this.form.get('prazos') as FormArray;
    const prazo = prazosArray.at(index);
    const tipoOperacao = prazo.get('tipoOperacao')?.value;

    // Count prazos by type
    const countByType = (tipo: TipoOperacao) =>
      prazosArray.controls.filter((c) => c.get('tipoOperacao')?.value === tipo).length;

    // Cannot remove last aplicacao or resgate
    if (tipoOperacao === TipoOperacao.APLICACAO && countByType(TipoOperacao.APLICACAO) <= 1) {
      return;
    }
    if (tipoOperacao === TipoOperacao.RESGATE && countByType(TipoOperacao.RESGATE) <= 1) {
      return;
    }

    prazosArray.removeAt(index);
  }

  /**
   * Check if prazo can be removed
   */
  canRemovePrazo(index: number): boolean {
    const prazosArray = this.form.get('prazos') as FormArray;
    const prazo = prazosArray.at(index);
    const tipoOperacao = prazo.get('tipoOperacao')?.value;

    // Count prazos by type
    const countByType = (tipo: TipoOperacao) =>
      prazosArray.controls.filter((c) => c.get('tipoOperacao')?.value === tipo).length;

    if (tipoOperacao === TipoOperacao.APLICACAO) {
      return countByType(TipoOperacao.APLICACAO) > 1;
    }
    if (tipoOperacao === TipoOperacao.RESGATE) {
      return countByType(TipoOperacao.RESGATE) > 1;
    }
    return true;
  }

  /**
   * Check if prazo is tipo RESGATE (for conditional fields like carencia)
   */
  isResgatePrazo(index: number): boolean {
    const prazosArray = this.form.get('prazos') as FormArray;
    const prazo = prazosArray.at(index);
    const tipoOperacao = prazo.get('tipoOperacao')?.value;
    return tipoOperacao === TipoOperacao.RESGATE || tipoOperacao === TipoOperacao.RESGATE_CRISE;
  }

  /**
   * Check if resgate programado is enabled for prazo
   */
  isResgateProgramadoEnabled(index: number): boolean {
    const prazosArray = this.form.get('prazos') as FormArray;
    const prazo = prazosArray.at(index);
    return prazo.get('permiteResgateProgramado')?.value === true;
  }

  /**
   * Check if permite resgate total is false (RF-07)
   */
  isResgateTotalDisabled(index: number): boolean {
    const prazosArray = this.form.get('prazos') as FormArray;
    const prazo = prazosArray.at(index);
    return prazo.get('permiteResgateTotal')?.value === false;
  }

  private prepareDataForStore(formValue: { prazos: Partial<FundoPrazo>[] }): PrazosFormData {
    return {
      prazos: (formValue.prazos || []).map((prazo) => ({
        tipoOperacao: prazo.tipoOperacao ?? null,
        prazoCotizacao: prazo.prazoCotizacao ?? 0,
        prazoLiquidacao: prazo.prazoLiquidacao ?? 0,
        tipoCalendario: prazo.tipoCalendario ?? TipoCalendario.NACIONAL,
        valorMinimoInicial: prazo.valorMinimoInicial ?? null,
        valorMinimoAdicional: prazo.valorMinimoAdicional ?? null,
        valorMinimoResgate: prazo.valorMinimoResgate ?? null,
        valorMinimoPermanencia: prazo.valorMinimoPermanencia ?? null,
        prazoCarenciaDias: prazo.prazoCarenciaDias ?? null,
        permiteResgateTotal: prazo.permiteResgateTotal ?? true,
        permiteResgateProgramado: prazo.permiteResgateProgramado ?? false,
        prazoMaximoProgramacao: prazo.prazoMaximoProgramacao ?? null,
        classeId: prazo.classeId ?? null,
        ativo: prazo.ativo ?? true,
      })),
    };
  }

  private markAllAsTouched(): void {
    const prazosArray = this.form.get('prazos') as FormArray;
    prazosArray.controls.forEach((group) => {
      Object.keys((group as FormGroup).controls).forEach((key) => {
        group.get(key)?.markAsTouched();
      });
    });
  }

  private updateStepValidation(): void {
    const stepId = this.stepConfig().id;
    const prazosArray = this.form.get('prazos') as FormArray;

    // Validate: must have aplicacao and resgate (RF-01)
    const hasAplicacao = this.hasAplicacao();
    const hasResgate = this.hasResgate();
    const hasBothRequired = hasAplicacao && hasResgate;

    const isValid = this.form.valid && hasBothRequired;
    const invalidFields = this.collectInvalidFields();

    const errors: string[] = [];
    if (!hasAplicacao) errors.push('Prazo de aplicacao e obrigatorio');
    if (!hasResgate) errors.push('Prazo de resgate e obrigatorio');

    this.wizardStore.setStepValidation(stepId, {
      isValid,
      isDirty: this.form.dirty || prazosArray.length > 0,
      errors,
      invalidFields,
    });

    if (isValid && (this.form.dirty || prazosArray.length > 0)) {
      this.wizardStore.markStepComplete(stepId);
    } else {
      this.wizardStore.markStepIncomplete(stepId);
    }
  }

  private collectInvalidFields(): InvalidFieldInfo[] {
    const invalidFields: InvalidFieldInfo[] = [];
    const prazosArray = this.form.get('prazos') as FormArray;
    prazosArray.controls.forEach((group, index) => {
      Object.keys((group as FormGroup).controls).forEach((key) => {
        const control = group.get(key);
        if (control && control.invalid) {
          const fieldErrors: string[] = [];
          if (control.errors) {
            Object.keys(control.errors).forEach((errorKey) => {
              fieldErrors.push(errorKey);
            });
          }
          invalidFields.push({ field: `prazos[${index}].${key}`, errors: fieldErrors });
        }
      });
    });
    return invalidFields;
  }

  // Helper methods for template
  isFieldInvalid(index: number, fieldName: string): boolean {
    const prazosArray = this.form.get('prazos') as FormArray;
    const control = prazosArray.at(index)?.get(fieldName);
    return control ? control.touched && control.invalid : false;
  }

  isFieldValid(index: number, fieldName: string): boolean {
    const prazosArray = this.form.get('prazos') as FormArray;
    const control = prazosArray.at(index)?.get(fieldName);
    return control ? control.touched && control.valid : false;
  }

  getFieldError(index: number, fieldName: string, errorKey: string): boolean {
    const prazosArray = this.form.get('prazos') as FormArray;
    const control = prazosArray.at(index)?.get(fieldName);
    return control?.hasError(errorKey) ?? false;
  }

  getPrazoGroup(index: number): FormGroup {
    const prazosArray = this.form.get('prazos') as FormArray;
    return prazosArray.at(index) as FormGroup;
  }

  /**
   * Check if prazo group has liquidacao validation error (RF-03)
   */
  hasLiquidacaoError(index: number): boolean {
    const prazosArray = this.form.get('prazos') as FormArray;
    const group = prazosArray.at(index);
    return group?.hasError('liquidacaoMenorQueCotizacao') ?? false;
  }

  /**
   * Get label for tipo operacao
   */
  getTipoOperacaoLabel(tipoOperacao: TipoOperacao | null): string {
    if (!tipoOperacao) return 'Novo Prazo';
    const option = TIPO_OPERACAO_OPTIONS.find((opt) => opt.value === tipoOperacao);
    return option?.label ?? tipoOperacao;
  }
}
