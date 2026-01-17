import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  signal,
  untracked,
} from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { startWith } from 'rxjs/operators';
import { WizardStepConfig, InvalidFieldInfo } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';
import { createRestorationEffect, withRestorationGuard } from '../../shared';
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

  // Convert form control valueChanges to signal for reactive computed dependencies
  // Fixes bug: computed() doesn't track Reactive Forms values directly
  // See: docs/aidebug/computed-signal-form-values.md
  private readonly possuiCoobrigacaoValue = toSignal(
    this.form.get('possuiCoobrigacao')!.valueChanges.pipe(
      startWith(this.form.get('possuiCoobrigacao')!.value)
    ),
    { initialValue: false }
  );

  private readonly registradoraRecebiveisValue = toSignal(
    this.form.get('registradoraRecebiveis')!.valueChanges.pipe(
      startWith(this.form.get('registradoraRecebiveis')!.value)
    ),
    { initialValue: null as RegistradoraRecebiveis | null }
  );

  private readonly ratingMinimoValue = toSignal(
    this.form.get('ratingMinimo')!.valueChanges.pipe(
      startWith(this.form.get('ratingMinimo')!.value)
    ),
    { initialValue: null as string | null }
  );

  // Computed signals for conditional rendering using signals for proper reactivity
  readonly possuiCoobrigacao = computed(() => this.possuiCoobrigacaoValue() === true);
  readonly hasRegistradora = computed(() => this.registradoraRecebiveisValue() !== null);
  readonly hasRating = computed(() => {
    const rating = this.ratingMinimoValue();
    return rating !== null && rating !== '';
  });

  // Count selected recebiveis (RF-03)
  readonly recebiveisSelecionados = signal<number>(0);

  constructor() {
    // Create restoration effect (centralizes deduplication, isRestoring flag, and form restore)
    const { isRestoring } = createRestorationEffect<ParametrosFidcFormData>({
      stepConfig: () => this.stepConfig(),
      wizardStore: this.wizardStore,
      form: this.form,
      resetForm: () => {
        // Get identificacao data for tipo fundo defaults (RF-02)
        const identificacaoData = untracked(
          () => this.wizardStore.stepData()['identificacao'] as IdentificacaoFormData | undefined
        );
        const tipoFundo = identificacaoData?.tipoFundo ?? null;
        this.tipoFundoSignal.set(tipoFundo);

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
      },
      restoreData: (data) => {
        // Restore saved data with emitEvent: false to prevent cascading updates
        this.form.patchValue({
          tipoFidc: data.tipoFidc,
          prazoMedioCarteira: data.prazoMedioCarteira,
          indiceSubordinacaoAlvo: data.indiceSubordinacaoAlvo,
          provisaoDevedoresDuvidosos: data.provisaoDevedoresDuvidosos,
          limiteConcentracaoCedente: data.limiteConcentracaoCedente,
          limiteConcentracaoSacado: data.limiteConcentracaoSacado,
          possuiCoobrigacao: data.possuiCoobrigacao,
          percentualCoobrigacao: data.percentualCoobrigacao,
          permiteCessaoParcial: data.permiteCessaoParcial,
          ratingMinimo: data.ratingMinimo,
          agenciaRating: data.agenciaRating,
          registradoraRecebiveis: data.registradoraRecebiveis,
          contaRegistradora: data.contaRegistradora,
          integracaoRegistradora: data.integracaoRegistradora,
        }, { emitEvent: false });

        // Restore recebiveis selection
        this.setRecebiveisFromArray(data.tipoRecebiveis);
        this.form.markAsDirty();
      },
      updateStepValidation: () => this.updateStepValidation(),
    });

    // Setup form subscriptions
    this.form.statusChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.updateStepValidation());

    this.form.valueChanges
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        withRestorationGuard(isRestoring)
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
