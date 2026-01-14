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
import { NgbTypeaheadModule } from '@ng-bootstrap/ng-bootstrap';
import { merge, Observable, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators';
import { WizardStepConfig, WizardStepId } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';
import { IdentificacaoFormData } from '../../models/identificacao.model';
import {
  ParametrosCotaFormData,
  TipoCota,
  FusoHorario,
  FusoHorarioOption,
  TIPO_COTA_OPTIONS,
  FUSO_HORARIO_OPTIONS,
  CASAS_DECIMAIS_COTA_OPTIONS,
  CASAS_DECIMAIS_QUANTIDADE_OPTIONS,
  CASAS_DECIMAIS_PL_OPTIONS,
  formatarPreviewCota,
  formatarPreviewQuantidade,
  PARAMETROS_COTA_DEFAULTS,
} from '../../models/parametros-cota.model';

/**
 * Componente para Etapa 4 do wizard: Parametros de Cota
 * Configuracao de precisao, valor inicial e metodologia de calculo da cota
 */
@Component({
  selector: 'app-parametros-cota-step',
  imports: [ReactiveFormsModule, NgbTypeaheadModule],
  templateUrl: './parametros-cota-step.html',
  styleUrl: './parametros-cota-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParametrosCotaStep {
  stepConfig = input.required<WizardStepConfig>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly wizardStore = inject(WizardStore);
  private readonly destroyRef = inject(DestroyRef);

  // Enum options for template (static)
  readonly tipoCotaOptions = TIPO_COTA_OPTIONS;
  readonly fusoHorarioOptions = FUSO_HORARIO_OPTIONS;
  readonly casasDecimaisCotaOptions = CASAS_DECIMAIS_COTA_OPTIONS;
  readonly casasDecimaisQuantidadeOptions = CASAS_DECIMAIS_QUANTIDADE_OPTIONS;
  readonly casasDecimaisPlOptions = CASAS_DECIMAIS_PL_OPTIONS;

  // Track step ID to avoid re-loading
  private lastLoadedStepId: WizardStepId | null = null;

  // Signals for computed previews
  private readonly casasDecimaisCotaValue = signal<number>(PARAMETROS_COTA_DEFAULTS.casasDecimaisCota);
  private readonly casasDecimaisQuantidadeValue = signal<number>(PARAMETROS_COTA_DEFAULTS.casasDecimaisQuantidade);
  private readonly casasDecimaisPlValue = signal<number>(PARAMETROS_COTA_DEFAULTS.casasDecimaisPl);

  // Computed preview signals (RF-01)
  readonly previewCota = computed(() => formatarPreviewCota(this.casasDecimaisCotaValue()));
  readonly previewQuantidade = computed(() => formatarPreviewQuantidade(this.casasDecimaisQuantidadeValue()));
  readonly previewPl = computed(() => {
    const casas = this.casasDecimaisPlValue();
    const decimais = '00'.substring(0, casas);
    return `R$ 1.234.567,${decimais}`;
  });

  // Signals for UI state
  readonly showCotaEstimadaInfo = signal(false);

  // Fuso horario typeahead (RF-04: Select com busca)
  readonly fusoHorarioFocus$ = new Subject<string>();

  // Search function that shows all options on focus and filters on typing
  searchFusoHorario = (text$: Observable<string>): Observable<FusoHorarioOption[]> => {
    const focusStream$ = this.fusoHorarioFocus$.pipe(
      map(() => FUSO_HORARIO_OPTIONS)
    );

    const typeStream$ = text$.pipe(
      debounceTime(100),
      distinctUntilChanged(),
      map((term) =>
        term.length === 0
          ? FUSO_HORARIO_OPTIONS
          : FUSO_HORARIO_OPTIONS.filter(
              (option) => option.label.toLowerCase().includes(term.toLowerCase())
            )
      )
    );

    return merge(focusStream$, typeStream$);
  };

  // Format option for display in input field
  formatFusoHorario = (option: FusoHorarioOption): string => option.label;

  // Handle focus to show dropdown
  onFusoHorarioFocus(): void {
    this.fusoHorarioFocus$.next('');
  }

  // Helper to find FusoHorarioOption by value
  private readonly defaultFusoHorarioOption = FUSO_HORARIO_OPTIONS.find(
    (o) => o.value === PARAMETROS_COTA_DEFAULTS.fusoHorario
  )!;

  form = this.formBuilder.group({
    casasDecimaisCota: [PARAMETROS_COTA_DEFAULTS.casasDecimaisCota, [Validators.required, Validators.min(4), Validators.max(10)]],
    casasDecimaisQuantidade: [PARAMETROS_COTA_DEFAULTS.casasDecimaisQuantidade, [Validators.required, Validators.min(4), Validators.max(8)]],
    casasDecimaisPl: [PARAMETROS_COTA_DEFAULTS.casasDecimaisPl, [Validators.required, Validators.min(2), Validators.max(4)]],
    tipoCota: [PARAMETROS_COTA_DEFAULTS.tipoCota as TipoCota | null, [Validators.required]],
    horarioCorte: [PARAMETROS_COTA_DEFAULTS.horarioCorte, [Validators.required, Validators.pattern(/^([01]\d|2[0-3]):([0-5]\d)$/)]],
    cotaInicial: [PARAMETROS_COTA_DEFAULTS.cotaInicial as number | null, [Validators.required, Validators.min(0.01), Validators.max(1000000)]],
    dataCotaInicial: [null as string | null, [Validators.required]],
    fusoHorario: [this.defaultFusoHorarioOption as FusoHorarioOption | null, [Validators.required]],
    permiteCotaEstimada: [PARAMETROS_COTA_DEFAULTS.permiteCotaEstimada, [Validators.required]],
  });

  constructor() {
    // Setup form subscriptions
    this.form.statusChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.updateStepValidation());

    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      const stepConfig = untracked(() => this.stepConfig());
      const dataForStore = this.prepareDataForStore(value);
      this.wizardStore.setStepData(stepConfig.key, dataForStore);

      // Update preview signals
      if (value.casasDecimaisCota != null) {
        this.casasDecimaisCotaValue.set(value.casasDecimaisCota);
      }
      if (value.casasDecimaisQuantidade != null) {
        this.casasDecimaisQuantidadeValue.set(value.casasDecimaisQuantidade);
      }
      if (value.casasDecimaisPl != null) {
        this.casasDecimaisPlValue.set(value.casasDecimaisPl);
      }
    });

    // Effect: Handle permiteCotaEstimada info display (RF-05)
    effect(
      () => {
        const permiteCotaEstimada = this.form.get('permiteCotaEstimada')?.value;
        this.showCotaEstimadaInfo.set(permiteCotaEstimada === true);
      },
      { allowSignalWrites: true }
    );

    // Effect: Load data when step changes and set dataCotaInicial from identificacao
    effect(() => {
      const stepConfig = this.stepConfig();
      const stepId = stepConfig.id;

      if (this.lastLoadedStepId === stepId) {
        return;
      }
      this.lastLoadedStepId = stepId;

      // Get identificacao data for default dataCotaInicial
      const identificacaoData = untracked(
        () => this.wizardStore.stepData()['identificacao'] as IdentificacaoFormData | undefined
      );

      // Reset form with defaults
      this.form.reset(
        {
          casasDecimaisCota: PARAMETROS_COTA_DEFAULTS.casasDecimaisCota,
          casasDecimaisQuantidade: PARAMETROS_COTA_DEFAULTS.casasDecimaisQuantidade,
          casasDecimaisPl: PARAMETROS_COTA_DEFAULTS.casasDecimaisPl,
          tipoCota: PARAMETROS_COTA_DEFAULTS.tipoCota,
          horarioCorte: PARAMETROS_COTA_DEFAULTS.horarioCorte,
          cotaInicial: PARAMETROS_COTA_DEFAULTS.cotaInicial,
          dataCotaInicial: identificacaoData?.dataInicioAtividade ?? null,
          fusoHorario: this.defaultFusoHorarioOption,
          permiteCotaEstimada: PARAMETROS_COTA_DEFAULTS.permiteCotaEstimada,
        },
        { emitEvent: false }
      );

      // Load existing step data if available
      const stepData = untracked(
        () =>
          this.wizardStore.stepData()[stepConfig.key] as
            | Partial<ParametrosCotaFormData>
            | undefined
      );

      if (stepData) {
        const formValue = this.prepareDataForForm(stepData);
        this.form.patchValue(formValue, { emitEvent: false });

        // Update preview signals
        if (stepData.casasDecimaisCota != null) {
          this.casasDecimaisCotaValue.set(stepData.casasDecimaisCota);
        }
        if (stepData.casasDecimaisQuantidade != null) {
          this.casasDecimaisQuantidadeValue.set(stepData.casasDecimaisQuantidade);
        }
        if (stepData.casasDecimaisPl != null) {
          this.casasDecimaisPlValue.set(stepData.casasDecimaisPl);
        }

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

  private prepareDataForStore(formValue: Partial<typeof this.form.value>): ParametrosCotaFormData {
    // Extract FusoHorario value from FusoHorarioOption for storage
    const fusoHorarioValue = formValue.fusoHorario?.value ?? PARAMETROS_COTA_DEFAULTS.fusoHorario;

    return {
      casasDecimaisCota: formValue.casasDecimaisCota ?? PARAMETROS_COTA_DEFAULTS.casasDecimaisCota,
      casasDecimaisQuantidade: formValue.casasDecimaisQuantidade ?? PARAMETROS_COTA_DEFAULTS.casasDecimaisQuantidade,
      casasDecimaisPl: formValue.casasDecimaisPl ?? PARAMETROS_COTA_DEFAULTS.casasDecimaisPl,
      tipoCota: formValue.tipoCota ?? null,
      horarioCorte: formValue.horarioCorte ?? null,
      cotaInicial: formValue.cotaInicial ?? null,
      dataCotaInicial: formValue.dataCotaInicial ?? null,
      fusoHorario: fusoHorarioValue,
      permiteCotaEstimada: formValue.permiteCotaEstimada ?? false,
    };
  }

  private prepareDataForForm(data: Partial<ParametrosCotaFormData>): Partial<typeof this.form.value> {
    // Convert FusoHorario value back to FusoHorarioOption for typeahead
    const fusoHorarioOption = FUSO_HORARIO_OPTIONS.find(
      (o) => o.value === (data.fusoHorario ?? PARAMETROS_COTA_DEFAULTS.fusoHorario)
    ) ?? this.defaultFusoHorarioOption;

    return {
      casasDecimaisCota: data.casasDecimaisCota ?? PARAMETROS_COTA_DEFAULTS.casasDecimaisCota,
      casasDecimaisQuantidade: data.casasDecimaisQuantidade ?? PARAMETROS_COTA_DEFAULTS.casasDecimaisQuantidade,
      casasDecimaisPl: data.casasDecimaisPl ?? PARAMETROS_COTA_DEFAULTS.casasDecimaisPl,
      tipoCota: data.tipoCota ?? PARAMETROS_COTA_DEFAULTS.tipoCota,
      horarioCorte: data.horarioCorte ?? PARAMETROS_COTA_DEFAULTS.horarioCorte,
      cotaInicial: data.cotaInicial ?? PARAMETROS_COTA_DEFAULTS.cotaInicial,
      dataCotaInicial: data.dataCotaInicial ?? null,
      fusoHorario: fusoHorarioOption,
      permiteCotaEstimada: data.permiteCotaEstimada ?? false,
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

  // Quick value setters for cota inicial (RF-03)
  setCotaInicial(value: number): void {
    this.form.get('cotaInicial')?.setValue(value);
    this.form.get('cotaInicial')?.markAsTouched();
    this.form.get('cotaInicial')?.markAsDirty();
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
