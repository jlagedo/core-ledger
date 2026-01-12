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
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpClient } from '@angular/common/http';
import { API_URL } from '../../../../../../config/api.config';
import { WizardStepConfig, WizardStepId } from '../../models/wizard.model';
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
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(API_URL);

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

  // Track step ID to avoid re-loading
  private lastLoadedStepId: WizardStepId | null = null;

  // Computed signal for checking if CVM classification has ANBIMA options
  readonly cvmHasAnbimaOptions = computed(() => {
    const cvmValue = this.form.get('classificacaoCvm')?.value;
    return cvmValue ? CVM_HAS_ANBIMA_OPTIONS.has(cvmValue) : false;
  });

  form = this.formBuilder.group({
    classificacaoCvm: [null as ClassificacaoCvm | null, [Validators.required]],
    classificacaoAnbima: [null as string | null],
    codigoAnbima: ['', [Validators.maxLength(20)]],
    publicoAlvo: [null as PublicoAlvo | null, [Validators.required]],
    tributacao: [null as Tributacao | null, [Validators.required]],
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

    // Effect to load ANBIMA options when CVM classification changes
    effect(
      () => {
        const cvmValue = this.form.get('classificacaoCvm')?.value;

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
          this.form.get('classificacaoAnbima')?.setValue(null);
          return;
        }

        // Fetch ANBIMA options
        untracked(() => this.loadAnbimaOptions(cvmValue));
      },
      { allowSignalWrites: true }
    );

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
          if (!currentPublicoAlvo) {
            this.form.get('publicoAlvo')?.setValue(restriction);
          }
        } else {
          this.restrictedPublicoAlvo.set(null);
        }

        // Apply tributacao suggestion (only if not already set)
        const currentTributacao = this.form.get('tributacao')?.value;
        if (!currentTributacao) {
          const suggestedTributacao = TIPO_FUNDO_TRIBUTACAO_SUGGESTION[tipoFundo];
          if (suggestedTributacao) {
            this.form.get('tributacao')?.setValue(suggestedTributacao);
          }
        }
      },
      { allowSignalWrites: true }
    );

    // Effect to load data when step changes
    effect(() => {
      const stepConfig = this.stepConfig();
      const stepId = stepConfig.id;

      if (this.lastLoadedStepId === stepId) {
        return;
      }
      this.lastLoadedStepId = stepId;

      this.form.reset({}, { emitEvent: false });

      const stepData = untracked(
        () =>
          this.wizardStore.stepData()[stepConfig.key] as
            | Partial<ClassificacaoFormData>
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

  private loadAnbimaOptions(cvmValue: ClassificacaoCvm): void {
    this.loadingAnbima.set(true);
    this.anbimaDisabled.set(true);

    this.http
      .get<ClassificacaoAnbimaResponse>(
        `${this.apiUrl}/v1/parametros/classificacoes-anbima`,
        { params: { classificacaoCvm: cvmValue } }
      )
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
              this.form.get('classificacaoAnbima')?.setValue(null);
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
