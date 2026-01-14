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
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter, startWith } from 'rxjs/operators';
import { WizardStepConfig, WizardStepId, InvalidFieldInfo } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';
import { IdentificacaoFormData, TipoFundo } from '../../models/identificacao.model';
import {
  AGENCIA_RATING_OPTIONS,
  createDefaultFidcParametros,
  DEFAULT_LIMITE_CEDENTE,
  DEFAULT_LIMITE_SACADO,
  MAX_PERCENTUAL,
  MAX_PRAZO_DIAS,
  ParametrosFidcFormData,
  REGISTRADORA_OPTIONS,
  RegistradoraRecebiveis,
  TIPO_FIDC_OPTIONS,
  TIPO_RECEBIVEIS_OPTIONS,
  TipoFidc,
  TipoRecebiveis,
} from '../../models/parametros-fidc.model';

/**
 * Validador: Se rating preenchido, agência é obrigatória (RF-06)
 */
function ratingAgenciaValidator(group: AbstractControl): ValidationErrors | null {
  const ratingMinimo = group.get('ratingMinimo')?.value;
  const agenciaRating = group.get('agenciaRating')?.value;

  if (ratingMinimo && !agenciaRating) {
    return { agenciaObrigatoria: true };
  }
  return null;
}

/**
 * Componente para Etapa 7.1 - Parâmetros FIDC
 * Etapa condicional exibida apenas para fundos FIDC/FIDC_NP
 */
@Component({
  selector: 'app-parametros-fidc-step',
  imports: [ReactiveFormsModule],
  templateUrl: './parametros-fidc-step.html',
  styleUrl: './parametros-fidc-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParametrosFidcStep {
  stepConfig = input.required<WizardStepConfig>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly wizardStore = inject(WizardStore);
  private readonly destroyRef = inject(DestroyRef);

  // Enum options for template
  readonly tipoFidcOptions = TIPO_FIDC_OPTIONS;
  readonly tipoRecebiveisOptions = TIPO_RECEBIVEIS_OPTIONS;
  readonly registradoraOptions = REGISTRADORA_OPTIONS;
  readonly agenciaRatingOptions = AGENCIA_RATING_OPTIONS;

  // Constants
  readonly maxPrazoDias = MAX_PRAZO_DIAS;
  readonly maxPercentual = MAX_PERCENTUAL;
  readonly defaultLimiteCedente = DEFAULT_LIMITE_CEDENTE;
  readonly defaultLimiteSacado = DEFAULT_LIMITE_SACADO;

  // Track step ID and dataVersion to avoid re-loading unless store data changes
  private lastLoadedStepId: WizardStepId | null = null;
  private lastDataVersion = -1;

  // Loading flag to prevent store updates during restoration
  private isRestoring = false;

  // Track current tipo fundo for defaults
  private readonly tipoFundoSignal = signal<TipoFundo | null>(null);

  // Main form
  form = this.formBuilder.group(
    {
      tipoFidc: [null as TipoFidc | null, [Validators.required]],
      tipoRecebiveis: this.formBuilder.array(
        this.tipoRecebiveisOptions.map(() => this.formBuilder.control(false as boolean))
      ),
      prazoMedioCarteira: [null as number | null, [Validators.min(1), Validators.max(MAX_PRAZO_DIAS)]],
      indiceSubordinacaoAlvo: [null as number | null, [Validators.min(0), Validators.max(MAX_PERCENTUAL)]],
      provisaoDevedoresDuvidosos: [null as number | null, [Validators.min(0), Validators.max(MAX_PERCENTUAL)]],
      limiteConcentracaoCedente: [DEFAULT_LIMITE_CEDENTE, [Validators.min(0), Validators.max(MAX_PERCENTUAL)]],
      limiteConcentracaoSacado: [DEFAULT_LIMITE_SACADO, [Validators.min(0), Validators.max(MAX_PERCENTUAL)]],
      possuiCoobrigacao: [false],
      percentualCoobrigacao: [null as number | null, [Validators.min(0), Validators.max(MAX_PERCENTUAL)]],
      permiteCessaoParcial: [true],
      ratingMinimo: [null as string | null, [Validators.maxLength(10)]],
      agenciaRating: [null as string | null],
      registradoraRecebiveis: [null as RegistradoraRecebiveis | null],
      contaRegistradora: [null as string | null, [Validators.maxLength(50)]],
      integracaoRegistradora: [false],
    },
    { validators: ratingAgenciaValidator }
  );

  // Computed signals for conditional rendering
  readonly possuiCoobrigacao = computed(() => this.form.get('possuiCoobrigacao')?.value === true);
  readonly hasRegistradora = computed(() => this.form.get('registradoraRecebiveis')?.value !== null);
  readonly hasRating = computed(() => {
    const rating = this.form.get('ratingMinimo')?.value;
    return rating !== null && rating !== '';
  });

  // Count selected recebiveis (RF-03)
  readonly recebiveisSelecionados = signal<number>(0);

  constructor() {
    // Setup form subscriptions
    this.form.statusChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.updateStepValidation());

    this.form.valueChanges
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        filter(() => !this.isRestoring)
      )
      .subscribe((value) => {
        const stepConfig = untracked(() => this.stepConfig());
        const dataForStore = this.prepareDataForStore(value);
        this.wizardStore.setStepData(stepConfig.key, dataForStore);

        // Update recebiveis count
        const recebiveis = this.getSelectedRecebiveis();
        this.recebiveisSelecionados.set(recebiveis.length);
      });

    // RF-04: Setup conditional validator for coobrigação percentual
    const coobrigacaoControl = this.form.get('possuiCoobrigacao');
    coobrigacaoControl?.valueChanges
      .pipe(
        startWith(coobrigacaoControl.value),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((possuiCoobrigacao: boolean | null) => {
        const percentualControl = this.form.get('percentualCoobrigacao');
        if (possuiCoobrigacao) {
          percentualControl?.setValidators([Validators.required, Validators.min(0), Validators.max(MAX_PERCENTUAL)]);
        } else {
          percentualControl?.clearValidators();
          percentualControl?.setValue(null, { emitEvent: false });
        }
        percentualControl?.updateValueAndValidity({ emitEvent: false });
      });

    // RF-07: Setup conditional visibility for registradora fields
    const registradoraControl = this.form.get('registradoraRecebiveis');
    registradoraControl?.valueChanges
      .pipe(
        startWith(registradoraControl.value),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((registradora: RegistradoraRecebiveis | null) => {
        const contaControl = this.form.get('contaRegistradora');
        const integracaoControl = this.form.get('integracaoRegistradora');
        if (!registradora) {
          contaControl?.setValue(null, { emitEvent: false });
          integracaoControl?.setValue(false, { emitEvent: false });
        }
      });

    // Effect: Load data when step changes or dataVersion changes (draft restoration)
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
      this.lastLoadedStepId = stepId;
      this.lastDataVersion = dataVersion;

      // Set restoration flag to prevent store updates
      this.isRestoring = true;

      // Get identificacao data for tipo fundo defaults (RF-02)
      const identificacaoData = untracked(
        () => this.wizardStore.stepData()['identificacao'] as IdentificacaoFormData | undefined
      );
      const tipoFundo = identificacaoData?.tipoFundo ?? null;
      this.tipoFundoSignal.set(tipoFundo);

      const stepData = untracked(
        () => this.wizardStore.stepData()[stepConfig.key] as ParametrosFidcFormData | undefined
      );

      if (stepData && stepData.tipoFidc !== null) {
        // Restore saved data - events fire naturally
        this.form.patchValue({
          tipoFidc: stepData.tipoFidc,
          prazoMedioCarteira: stepData.prazoMedioCarteira,
          indiceSubordinacaoAlvo: stepData.indiceSubordinacaoAlvo,
          provisaoDevedoresDuvidosos: stepData.provisaoDevedoresDuvidosos,
          limiteConcentracaoCedente: stepData.limiteConcentracaoCedente,
          limiteConcentracaoSacado: stepData.limiteConcentracaoSacado,
          possuiCoobrigacao: stepData.possuiCoobrigacao,
          percentualCoobrigacao: stepData.percentualCoobrigacao,
          permiteCessaoParcial: stepData.permiteCessaoParcial,
          ratingMinimo: stepData.ratingMinimo,
          agenciaRating: stepData.agenciaRating,
          registradoraRecebiveis: stepData.registradoraRecebiveis,
          contaRegistradora: stepData.contaRegistradora,
          integracaoRegistradora: stepData.integracaoRegistradora,
        });

        // Restore recebiveis selection
        this.setRecebiveisFromArray(stepData.tipoRecebiveis);
        this.form.markAsDirty();
      } else {
        // RF-02: Set default tipo based on tipo_fundo
        const isFidcNp = tipoFundo === TipoFundo.FIDC_NP;
        const defaults = createDefaultFidcParametros(isFidcNp);
        this.form.patchValue({
          tipoFidc: defaults.tipoFidc,
          limiteConcentracaoCedente: defaults.limiteConcentracaoCedente,
          limiteConcentracaoSacado: defaults.limiteConcentracaoSacado,
          possuiCoobrigacao: defaults.possuiCoobrigacao,
          permiteCessaoParcial: defaults.permiteCessaoParcial,
          integracaoRegistradora: defaults.integracaoRegistradora,
        });
      }

      // Clear restoration flag
      this.isRestoring = false;

      // Mark all fields as touched
      this.markAllAsTouched();

      this.form.updateValueAndValidity();
      untracked(() => this.updateStepValidation());
    });
  }

  /**
   * Get selected recebiveis as array of enum values
   */
  getSelectedRecebiveis(): TipoRecebiveis[] {
    const formArray = this.form.get('tipoRecebiveis') as FormGroup;
    const selected: TipoRecebiveis[] = [];

    if (formArray && formArray.value) {
      const values = formArray.value as boolean[];
      values.forEach((checked, index) => {
        if (checked && this.tipoRecebiveisOptions[index]) {
          selected.push(this.tipoRecebiveisOptions[index].value);
        }
      });
    }

    return selected;
  }

  /**
   * Set recebiveis checkboxes from array of values
   */
  private setRecebiveisFromArray(recebiveis: TipoRecebiveis[]): void {
    const formArray = this.form.get('tipoRecebiveis');
    if (!formArray) return;

    const values = this.tipoRecebiveisOptions.map((option) => recebiveis.includes(option.value));
    formArray.setValue(values, { emitEvent: false });
    this.recebiveisSelecionados.set(recebiveis.length);
  }

  /**
   * Check if a specific recebivel is selected
   */
  isRecebiveisSelected(index: number): boolean {
    const formArray = this.form.get('tipoRecebiveis');
    if (!formArray || !formArray.value) return false;
    return (formArray.value as boolean[])[index] ?? false;
  }

  /**
   * Toggle a recebivel selection
   */
  toggleRecebiveis(index: number): void {
    const formArray = this.form.get('tipoRecebiveis');
    if (!formArray || !formArray.value) return;

    const values = [...(formArray.value as boolean[])];
    values[index] = !values[index];
    formArray.setValue(values);
  }

  private prepareDataForStore(formValue: any): ParametrosFidcFormData {
    return {
      tipoFidc: formValue.tipoFidc ?? null,
      tipoRecebiveis: this.getSelectedRecebiveis(),
      prazoMedioCarteira: formValue.prazoMedioCarteira ?? null,
      indiceSubordinacaoAlvo: formValue.indiceSubordinacaoAlvo ?? null,
      provisaoDevedoresDuvidosos: formValue.provisaoDevedoresDuvidosos ?? null,
      limiteConcentracaoCedente: formValue.limiteConcentracaoCedente ?? null,
      limiteConcentracaoSacado: formValue.limiteConcentracaoSacado ?? null,
      possuiCoobrigacao: formValue.possuiCoobrigacao ?? false,
      percentualCoobrigacao: formValue.percentualCoobrigacao ?? null,
      permiteCessaoParcial: formValue.permiteCessaoParcial ?? true,
      ratingMinimo: formValue.ratingMinimo ?? null,
      agenciaRating: formValue.agenciaRating ?? null,
      registradoraRecebiveis: formValue.registradoraRecebiveis ?? null,
      contaRegistradora: formValue.contaRegistradora ?? null,
      integracaoRegistradora: formValue.integracaoRegistradora ?? false,
    };
  }

  private markAllAsTouched(): void {
    Object.keys(this.form.controls).forEach((key) => {
      const control = this.form.get(key);
      control?.markAsTouched();
    });
  }

  private updateStepValidation(): void {
    const stepId = this.stepConfig().id;
    const recebiveis = this.getSelectedRecebiveis();

    // RF-03: At least one recebivel must be selected
    const hasRecebiveis = recebiveis.length > 0;
    const isValid = this.form.valid && hasRecebiveis;

    const errors: string[] = [];
    if (!hasRecebiveis) {
      errors.push('Selecione pelo menos um tipo de recebivel');
    }
    if (this.form.hasError('agenciaObrigatoria')) {
      errors.push('Agencia de rating e obrigatoria quando rating minimo e informado');
    }

    const invalidFields = this.collectInvalidFields();

    this.wizardStore.setStepValidation(stepId, {
      isValid,
      isDirty: this.form.dirty,
      errors,
      invalidFields,
    });

    if (isValid && this.form.dirty) {
      this.wizardStore.markStepComplete(stepId);
    } else {
      this.wizardStore.markStepIncomplete(stepId);
    }
  }

  private collectInvalidFields(): InvalidFieldInfo[] {
    const invalidFields: InvalidFieldInfo[] = [];
    Object.keys(this.form.controls).forEach((key) => {
      const control = this.form.get(key);
      if (control && control.invalid && key !== 'tipoRecebiveis') {
        const fieldErrors: string[] = [];
        if (control.errors) {
          Object.keys(control.errors).forEach((errorKey) => {
            fieldErrors.push(errorKey);
          });
        }
        invalidFields.push({ field: key, errors: fieldErrors });
      }
    });
    return invalidFields;
  }

  // Helper methods for template
  isFieldInvalid(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    return control ? control.touched && control.invalid : false;
  }

  isFieldValid(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    return control ? control.touched && control.valid : false;
  }

  getFieldError(fieldName: string, errorKey: string): boolean {
    const control = this.form.get(fieldName);
    return control?.hasError(errorKey) ?? false;
  }

  /**
   * Get label for tipo FIDC
   */
  getTipoFidcLabel(tipoFidc: TipoFidc | null): string {
    if (!tipoFidc) return '';
    const option = TIPO_FIDC_OPTIONS.find((opt) => opt.value === tipoFidc);
    return option?.label ?? tipoFidc;
  }
}
