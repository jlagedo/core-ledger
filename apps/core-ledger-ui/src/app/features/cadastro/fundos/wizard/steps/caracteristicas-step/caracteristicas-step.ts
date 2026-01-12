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
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NgbInputDatepicker } from '@ng-bootstrap/ng-bootstrap';
import { WizardStepConfig, WizardStepId } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';
import { IdentificacaoFormData } from '../../models/identificacao.model';
import {
  ClassificacaoFormData,
  ClassificacaoCvm,
  CLASSIFICACAO_CVM_OPTIONS,
} from '../../models/classificacao.model';
import {
  CaracteristicasFormData,
  Condominio,
  CONDOMINIO_OPTIONS,
  Prazo,
  PRAZO_OPTIONS,
  TIPO_FUNDO_CONDOMINIO_PADRAO,
  TIPO_FUNDO_PERMITE_ALAVANCAGEM,
  TIPO_FUNDO_PERMITE_EXTERIOR,
} from '../../models/caracteristicas.model';

/**
 * Validador customizado: data deve ser futura
 */
function dataFuturaValidator(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;

  const dataEncerramento = new Date(control.value);
  const hoje = new Date();
  hoje.setHours(0, 0, 0, 0);

  return dataEncerramento > hoje ? null : { dataPassada: true };
}

/**
 * Componente para Etapa 3 do wizard: Caracteristicas do Fundo
 * Captura condominio, prazo, alavancagem, cripto, exterior
 */
@Component({
  selector: 'app-caracteristicas-step',
  imports: [ReactiveFormsModule, NgbInputDatepicker],
  templateUrl: './caracteristicas-step.html',
  styleUrl: './caracteristicas-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CaracteristicasStep {
  stepConfig = input.required<WizardStepConfig>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly wizardStore = inject(WizardStore);
  private readonly destroyRef = inject(DestroyRef);

  // Enum options for template (static)
  readonly condominioOptions = CONDOMINIO_OPTIONS;
  readonly prazoOptions = PRAZO_OPTIONS;

  // Signals for reactive UI
  readonly showCondominioWarning = signal(false);
  readonly condominioWarningMessage = signal<string | null>(null);
  readonly showCriptoInfo = signal(false);
  readonly showCriptoRestriction = signal(false);
  readonly criptoRestrictionMessage = signal<string | null>(null);
  readonly showCambialInfo = signal(false);
  readonly cambialMinPercentual = signal<number>(0);
  readonly exteriorDisabled = signal(false);
  readonly alavancagemDisabled = signal(false);

  // Track step ID to avoid re-loading
  private lastLoadedStepId: WizardStepId | null = null;

  // Computed signals for conditional fields
  readonly showDataEncerramento = computed(() => {
    return this.form.get('prazo')?.value === Prazo.DETERMINADO;
  });

  readonly showLimiteAlavancagem = computed(() => {
    return this.form.get('permiteAlavancagem')?.value === true;
  });

  readonly reservadoDisabled = computed(() => {
    return this.form.get('exclusivo')?.value === true;
  });

  form = this.formBuilder.group({
    condominio: [null as Condominio | null, [Validators.required]],
    prazo: [null as Prazo | null, [Validators.required]],
    dataEncerramento: [null as string | null],
    exclusivo: [false, [Validators.required]],
    reservado: [false, [Validators.required]],
    permiteAlavancagem: [false, [Validators.required]],
    limiteAlavancagem: [null as number | null],
    aceitaCripto: [false, [Validators.required]],
    percentualExterior: [null as number | null, [Validators.min(0), Validators.max(100)]],
  });

  constructor() {
    // Setup form subscriptions (outside of effect)
    this.form.statusChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.updateStepValidation());

    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      const stepConfig = untracked(() => this.stepConfig());
      const dataForStore = this.prepareDataForStore(value);
      this.wizardStore.setStepData(stepConfig.key, dataForStore);
    });

    // Effect: Apply rules based on tipo_fundo and classificacao from previous steps
    effect(
      () => {
        const stepConfig = this.stepConfig();
        const identificacaoData = untracked(
          () => this.wizardStore.stepData()['identificacao'] as IdentificacaoFormData | undefined
        );
        const classificacaoData = untracked(
          () => this.wizardStore.stepData()['classificacao'] as ClassificacaoFormData | undefined
        );

        if (!identificacaoData?.tipoFundo) return;

        const tipoFundo = identificacaoData.tipoFundo;

        // Apply condominio padrao (only if not already set)
        const condomionioPadrao = TIPO_FUNDO_CONDOMINIO_PADRAO[tipoFundo];
        const currentCondominio = this.form.get('condominio')?.value;
        if (!currentCondominio) {
          this.form.get('condominio')?.setValue(condomionioPadrao);
        }

        // Check if condominio diverges from default
        if (currentCondominio && currentCondominio !== condomionioPadrao) {
          this.showCondominioWarning.set(true);
          this.condominioWarningMessage.set(
            `Atencao: O tipo ${tipoFundo} normalmente usa condominio ${condomionioPadrao.toLowerCase()}`
          );
        } else {
          this.showCondominioWarning.set(false);
          this.condominioWarningMessage.set(null);
        }

        // Disable alavancagem if not allowed for tipo_fundo
        const permiteAlavancagem = TIPO_FUNDO_PERMITE_ALAVANCAGEM[tipoFundo];
        if (!permiteAlavancagem) {
          this.alavancagemDisabled.set(true);
          this.form.get('permiteAlavancagem')?.setValue(false);
        } else {
          this.alavancagemDisabled.set(false);
        }

        // Disable exterior if not allowed for tipo_fundo
        const permiteExterior = TIPO_FUNDO_PERMITE_EXTERIOR[tipoFundo];
        if (!permiteExterior) {
          this.exteriorDisabled.set(true);
          this.form.get('percentualExterior')?.setValue(null);
        } else {
          this.exteriorDisabled.set(false);
        }

        // RF-03: Validate crypto against CVM classification
        // Crypto is restricted for pure fixed income and real estate funds
        const CRYPTO_RESTRICTED_CLASSES = [
          ClassificacaoCvm.RENDA_FIXA,
          ClassificacaoCvm.FII,
          ClassificacaoCvm.FIAGRO,
          ClassificacaoCvm.FI_INFRA,
        ];

        const aceitaCripto = this.form.get('aceitaCripto')?.value;
        if (
          classificacaoData?.classificacaoCvm &&
          CRYPTO_RESTRICTED_CLASSES.includes(classificacaoData.classificacaoCvm) &&
          aceitaCripto
        ) {
          this.showCriptoRestriction.set(true);
          const classificacaoLabel = CLASSIFICACAO_CVM_OPTIONS.find(
            (opt) => opt.value === classificacaoData.classificacaoCvm
          )?.label;
          this.criptoRestrictionMessage.set(
            `A classificacao ${classificacaoLabel} nao permite investimento em criptoativos`
          );
        } else {
          this.showCriptoRestriction.set(false);
          this.criptoRestrictionMessage.set(null);
        }

        // RF-04: Enforce 80% minimum for Cambial funds
        const percentualExteriorControl = this.form.get('percentualExterior');
        if (classificacaoData?.classificacaoCvm === ClassificacaoCvm.CAMBIAL) {
          this.showCambialInfo.set(true);
          this.cambialMinPercentual.set(80);
          // Enforce 80% minimum for Cambial
          percentualExteriorControl?.setValidators([
            Validators.min(80),
            Validators.max(100),
          ]);
        } else {
          this.showCambialInfo.set(false);
          this.cambialMinPercentual.set(0);
          // Reset to standard validators
          percentualExteriorControl?.setValidators([Validators.min(0), Validators.max(100)]);
        }
        percentualExteriorControl?.updateValueAndValidity({ emitEvent: false });
      },
      { allowSignalWrites: true }
    );

    // Effect: Handle conditional fields (prazo)
    effect(
      () => {
        const prazo = this.form.get('prazo')?.value;
        const dataEncerramentoControl = this.form.get('dataEncerramento');

        if (prazo === Prazo.DETERMINADO) {
          // Make dataEncerramento required
          dataEncerramentoControl?.setValidators([Validators.required, dataFuturaValidator]);
        } else {
          // Clear validators and value
          dataEncerramentoControl?.clearValidators();
          dataEncerramentoControl?.setValue(null);
        }

        dataEncerramentoControl?.updateValueAndValidity({ emitEvent: false });
      },
      { allowSignalWrites: true }
    );

    // Effect: Handle conditional fields (alavancagem)
    effect(
      () => {
        const permiteAlavancagem = this.form.get('permiteAlavancagem')?.value;
        const limiteAlavancagemControl = this.form.get('limiteAlavancagem');

        if (permiteAlavancagem) {
          // Make limiteAlavancagem required with range
          limiteAlavancagemControl?.setValidators([
            Validators.required,
            Validators.min(1.01),
            Validators.max(10.0),
          ]);
        } else {
          // Clear validators and value
          limiteAlavancagemControl?.clearValidators();
          limiteAlavancagemControl?.setValue(null);
        }

        limiteAlavancagemControl?.updateValueAndValidity({ emitEvent: false });
      },
      { allowSignalWrites: true }
    );

    // Effect: Handle exclusivo => reservado
    effect(
      () => {
        const exclusivo = this.form.get('exclusivo')?.value;
        const reservadoControl = this.form.get('reservado');

        if (exclusivo) {
          reservadoControl?.setValue(true, { emitEvent: false });
        }
      },
      { allowSignalWrites: true }
    );

    // Effect: Show cripto info
    effect(
      () => {
        const aceitaCripto = this.form.get('aceitaCripto')?.value;
        this.showCriptoInfo.set(aceitaCripto === true);
      },
      { allowSignalWrites: true }
    );

    // Effect: Load data when step changes
    effect(() => {
      const stepConfig = this.stepConfig();
      const stepId = stepConfig.id;

      if (this.lastLoadedStepId === stepId) {
        return;
      }
      this.lastLoadedStepId = stepId;

      this.form.reset(
        {
          condominio: null,
          prazo: null,
          dataEncerramento: null,
          exclusivo: false,
          reservado: false,
          permiteAlavancagem: false,
          limiteAlavancagem: null,
          aceitaCripto: false,
          percentualExterior: null,
        },
        { emitEvent: false }
      );

      const stepData = untracked(
        () =>
          this.wizardStore.stepData()[stepConfig.key] as
            | Partial<CaracteristicasFormData>
            | undefined
      );

      if (stepData) {
        const formValue = this.prepareDataForForm(stepData);
        this.form.patchValue(formValue, { emitEvent: false });

        // Mark all fields as touched to show validation state
        Object.keys(this.form.controls).forEach((key) => {
          this.form.get(key)?.markAsTouched();
        });

        // Mark form as dirty since we have data
        this.form.markAsDirty();
      }

      untracked(() => this.updateStepValidation());
    });
  }

  private prepareDataForStore(formValue: any): CaracteristicasFormData {
    return {
      condominio: formValue.condominio ?? null,
      prazo: formValue.prazo ?? null,
      dataEncerramento: formValue.dataEncerramento ?? null,
      exclusivo: formValue.exclusivo ?? false,
      reservado: formValue.reservado ?? false,
      permiteAlavancagem: formValue.permiteAlavancagem ?? false,
      limiteAlavancagem: formValue.limiteAlavancagem ?? null,
      aceitaCripto: formValue.aceitaCripto ?? false,
      percentualExterior: formValue.percentualExterior ?? null,
    };
  }

  private prepareDataForForm(data: Partial<CaracteristicasFormData>): any {
    return {
      condominio: data.condominio ?? null,
      prazo: data.prazo ?? null,
      dataEncerramento: data.dataEncerramento ?? null,
      exclusivo: data.exclusivo ?? false,
      reservado: data.reservado ?? false,
      permiteAlavancagem: data.permiteAlavancagem ?? false,
      limiteAlavancagem: data.limiteAlavancagem ?? null,
      aceitaCripto: data.aceitaCripto ?? false,
      percentualExterior: data.percentualExterior ?? null,
    };
  }

  private updateStepValidation(): void {
    const stepId = this.stepConfig().id;

    this.wizardStore.setStepValidation(stepId, {
      isValid: this.form.valid,
      isDirty: this.form.dirty,
      errors: [],
    });

    if (this.form.valid && this.form.dirty) {
      this.wizardStore.markStepComplete(stepId);
    } else if (this.form.invalid && this.form.dirty) {
      this.wizardStore.markStepIncomplete(stepId);
    }
  }

  // Helper methods for template
  isInvalid(name: string): boolean {
    const control = this.form.get(name);
    return control ? control.touched && control.invalid : false;
  }

  isValid(name: string): boolean {
    const control = this.form.get(name);
    return control ? control.touched && control.valid : false;
  }
}
