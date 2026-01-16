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
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { filter, startWith } from 'rxjs/operators';
import { ParametrosService } from '../../../../../../services/parametros';
import { WizardStepConfig, WizardStepId, InvalidFieldInfo } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';
import { IdentificacaoFormData } from '../../models/identificacao.model';
import {
  ClassificacaoAnbimaOption,
  ClassificacaoAnbimaResponse,
  ClassificacaoFormData,
  ClassificacaoCvm,
  CLASSIFICACAO_CVM_OPTIONS,
  CVM_HAS_ANBIMA_OPTIONS,
  PublicoAlvo,
  PUBLICO_ALVO_OPTIONS,
  TIPO_FUNDO_PUBLICO_ALVO_RESTRICTION,
  TIPO_FUNDO_TRIBUTACAO_SUGGESTION,
  Tributacao,
  TRIBUTACAO_OPTIONS,
} from '../../models/classificacao.model';

/**
 * Componente para Etapa 2 do wizard: Classificacao do Fundo
 * Captura classificacao CVM, ANBIMA, publico-alvo e regime de tributacao
 */
@Component({
  selector: 'app-classificacao-step',
  imports: [ReactiveFormsModule],
  templateUrl: './classificacao-step.html',
  styleUrl: './classificacao-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClassificacaoStep {
  stepConfig = input.required<WizardStepConfig>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly wizardStore = inject(WizardStore);
  private readonly destroyRef = inject(DestroyRef);
  private readonly parametrosService = inject(ParametrosService);

  // Enum options for template (static)
  readonly classificacaoCvmOptions = CLASSIFICACAO_CVM_OPTIONS;
  readonly publicoAlvoOptions = PUBLICO_ALVO_OPTIONS;
  readonly tributacaoOptions = TRIBUTACAO_OPTIONS;

  // Signals for reactive UI
  readonly classificacoesAnbimaOptions = signal<ClassificacaoAnbimaOption[]>([]);
  readonly loadingAnbima = signal(false);
  readonly anbimaDisabled = signal(true);
  readonly anbimaMessage = signal<string | null>(null);
  readonly restrictedPublicoAlvo = signal<PublicoAlvo | null>(null);

  // Track step ID and dataVersion to avoid re-loading unless store data changes
  private lastLoadedStepId: WizardStepId | null = null;
  private lastDataVersion = -1;

  // Loading flag to prevent store updates during restoration
  private isRestoring = false;

  // Form must be defined before toSignal() calls
  form = this.formBuilder.group({
    classificacaoCvm: [null as ClassificacaoCvm | null, [Validators.required]],
    classificacaoAnbima: [null as string | null],
    codigoAnbima: ['', [Validators.maxLength(20)]],
    publicoAlvo: [null as PublicoAlvo | null, [Validators.required]],
    tributacao: [null as Tributacao | null, [Validators.required]],
  });

  // Convert form control valueChanges to signal for reactive computed dependencies
  // Fixes bug: computed() doesn't track Reactive Forms values directly
  // See: docs/aidebug/computed-signal-form-values.md
  private readonly classificacaoCvmValue = toSignal(
    this.form.get('classificacaoCvm')!.valueChanges.pipe(
      startWith(this.form.get('classificacaoCvm')!.value)
    ),
    { initialValue: null as ClassificacaoCvm | null }
  );

  // Computed signal for checking if CVM classification has ANBIMA options
  readonly cvmHasAnbimaOptions = computed(() => {
    const cvmValue = this.classificacaoCvmValue();
    return cvmValue ? CVM_HAS_ANBIMA_OPTIONS.has(cvmValue) : false;
  });

  constructor() {
    // Setup form subscriptions (outside of effect)
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
        const dataForStore = this.prepareDataForStore(value);
        this.wizardStore.setStepData(stepConfig.key, dataForStore);
      });

    // Effect to load ANBIMA options when CVM classification changes
    // Uses classificacaoCvmValue signal for proper reactivity
    effect(
      () => {
        const cvmValue = this.classificacaoCvmValue();

        if (!cvmValue) {
          this.classificacoesAnbimaOptions.set([]);
          this.anbimaDisabled.set(true);
          this.anbimaMessage.set(null);
          return;
        }

        if (!CVM_HAS_ANBIMA_OPTIONS.has(cvmValue)) {
          this.classificacoesAnbimaOptions.set([]);
          this.anbimaDisabled.set(true);
          this.anbimaMessage.set('Esta classificacao CVM nao possui subclassificacoes ANBIMA');
          this.form.get('classificacaoAnbima')?.setValue(null, { emitEvent: false });
          return;
        }

        // Fetch ANBIMA options
        untracked(() => this.loadAnbimaOptions(cvmValue));
      });

    // Effect to apply restrictions based on tipo_fundo from Step 1
    effect(
      () => {
        const stepConfig = this.stepConfig();
        const identificacaoData = untracked(
          () => this.wizardStore.stepData()['identificacao'] as IdentificacaoFormData | undefined
        );

        if (!identificacaoData?.tipoFundo) return;

        const tipoFundo = identificacaoData.tipoFundo;

        // Apply publico_alvo restriction
        const restriction = TIPO_FUNDO_PUBLICO_ALVO_RESTRICTION[tipoFundo];
        if (restriction) {
          this.restrictedPublicoAlvo.set(restriction);
          const currentPublicoAlvo = this.form.get('publicoAlvo')?.value;

          // Set minimum requirement if not already set
          // Use emitEvent: false to avoid triggering valueChanges and overwriting store data
          if (!currentPublicoAlvo) {
            this.form.get('publicoAlvo')?.setValue(restriction, { emitEvent: false });
          }
        } else {
          this.restrictedPublicoAlvo.set(null);
        }

        // Apply tributacao suggestion (only if not already set)
        // Use emitEvent: false to avoid triggering valueChanges and overwriting store data
        const currentTributacao = this.form.get('tributacao')?.value;
        if (!currentTributacao) {
          const suggestedTributacao = TIPO_FUNDO_TRIBUTACAO_SUGGESTION[tipoFundo];
          if (suggestedTributacao) {
            this.form.get('tributacao')?.setValue(suggestedTributacao, { emitEvent: false });
          }
        }
      });

    // Effect to load data when step changes OR when store data is restored (dataVersion changes)
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

      this.form.reset({});

      const stepData = untracked(
        () =>
          this.wizardStore.stepData()[stepConfig.key] as
            | Partial<ClassificacaoFormData>
            | undefined
      );

      if (stepData) {
        const formValue = this.prepareDataForForm(stepData);
        // Patch WITHOUT emitEvent: false - let events fire naturally
        this.form.patchValue(formValue);
        this.form.markAsDirty();
      }

      // Clear restoration flag
      this.isRestoring = false;

      // Mark all fields as touched to show validation state
      Object.keys(this.form.controls).forEach((key) => {
        this.form.get(key)?.markAsTouched();
      });

      this.form.updateValueAndValidity();
      untracked(() => this.updateStepValidation());
    });
  }

  private loadAnbimaOptions(cvmValue: ClassificacaoCvm): void {
    this.loadingAnbima.set(true);
    this.anbimaDisabled.set(true);

    this.parametrosService
      .getClassificacoesAnbima(cvmValue)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.classificacoesAnbimaOptions.set(response.items);
          this.anbimaDisabled.set(false);
          this.loadingAnbima.set(false);
          this.anbimaMessage.set(null);

          // Clear ANBIMA selection if current value is not in the new options
          const currentAnbima = this.form.get('classificacaoAnbima')?.value;
          if (currentAnbima) {
            const isValid = response.items.some((item) => item.codigo === currentAnbima);
            if (!isValid) {
              this.form.get('classificacaoAnbima')?.setValue(null, { emitEvent: false });
            }
          }
        },
        error: () => {
          this.loadingAnbima.set(false);
          this.anbimaMessage.set('Erro ao carregar classificacoes ANBIMA');
        },
      });
  }

  private prepareDataForStore(formValue: any): ClassificacaoFormData {
    return {
      classificacaoCvm: formValue.classificacaoCvm ?? null,
      classificacaoAnbima: formValue.classificacaoAnbima ?? null,
      codigoAnbima: formValue.codigoAnbima ?? null,
      publicoAlvo: formValue.publicoAlvo ?? null,
      tributacao: formValue.tributacao ?? null,
    };
  }

  private prepareDataForForm(data: Partial<ClassificacaoFormData>): any {
    return {
      classificacaoCvm: data.classificacaoCvm ?? null,
      classificacaoAnbima: data.classificacaoAnbima ?? null,
      codigoAnbima: data.codigoAnbima ?? '',
      publicoAlvo: data.publicoAlvo ?? null,
      tributacao: data.tributacao ?? null,
    };
  }

  private updateStepValidation(): void {
    const stepId = this.stepConfig().id;
    const invalidFields = this.collectInvalidFields();

    this.wizardStore.setStepValidation(stepId, {
      isValid: this.form.valid,
      isDirty: this.form.dirty,
      errors: [],
      invalidFields,
    });

    if (this.form.valid && this.form.dirty) {
      this.wizardStore.markStepComplete(stepId);
    } else if (this.form.invalid && this.form.dirty) {
      this.wizardStore.markStepIncomplete(stepId);
    }
  }

  private collectInvalidFields(): InvalidFieldInfo[] {
    const invalidFields: InvalidFieldInfo[] = [];
    Object.keys(this.form.controls).forEach((key) => {
      const control = this.form.get(key);
      if (control && control.invalid) {
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
  isInvalid(name: string): boolean {
    const control = this.form.get(name);
    return control ? control.touched && control.invalid : false;
  }

  isValid(name: string): boolean {
    const control = this.form.get(name);
    return control ? control.touched && control.valid : false;
  }

  isPublicoAlvoDisabled(publicoAlvo: PublicoAlvo): boolean {
    const restriction = this.restrictedPublicoAlvo();
    if (!restriction) return false;

    // PROFISSIONAL restriction: only PROFISSIONAL allowed
    if (restriction === PublicoAlvo.PROFISSIONAL) {
      return publicoAlvo !== PublicoAlvo.PROFISSIONAL;
    }

    // QUALIFICADO restriction: GERAL not allowed
    if (restriction === PublicoAlvo.QUALIFICADO) {
      return publicoAlvo === PublicoAlvo.GERAL;
    }

    return false;
  }

  getPublicoAlvoRestrictionMessage(): string {
    const restriction = this.restrictedPublicoAlvo();
    if (restriction === PublicoAlvo.PROFISSIONAL) {
      return 'FIDC-NP requer investidores profissionais';
    }
    if (restriction === PublicoAlvo.QUALIFICADO) {
      return 'Este tipo de fundo requer investidores qualificados ou profissionais';
    }
    return '';
  }
}
