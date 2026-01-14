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
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, tap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { WizardStepConfig, WizardStepId } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';
import {
  BaseCalculo,
  BASE_CALCULO_OPTIONS,
  FormaCobranca,
  FORMA_COBRANCA_OPTIONS,
  FundoTaxa,
  Indexador,
  LIMITES_PERCENTUAL,
  MAX_TAXAS,
  TaxasFormData,
  TAXA_ADMINISTRACAO_DEFAULT,
  TipoTaxa,
  TIPO_TAXA_OPTIONS,
} from '../../models/taxas.model';
import { IndexadorService } from '../../../../../../services/indexador';

/**
 * Validador customizado: data fim >= data inicio
 */
function dataFimVigenciaValidator(group: AbstractControl): ValidationErrors | null {
  const dataInicio = group.get('dataInicioVigencia')?.value;
  const dataFim = group.get('dataFimVigencia')?.value;

  if (!dataInicio || !dataFim) return null;

  const inicio = new Date(dataInicio);
  const fim = new Date(dataFim);

  return fim >= inicio ? null : { dataFimAnterior: true };
}

/**
 * Componente para Etapa 5 do wizard: Taxas do Fundo
 * Gerencia lista dinamica de taxas com FormArray
 */
@Component({
  selector: 'app-taxas-step',
  imports: [ReactiveFormsModule],
  templateUrl: './taxas-step.html',
  styleUrl: './taxas-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaxasStep {
  stepConfig = input.required<WizardStepConfig>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly wizardStore = inject(WizardStore);
  private readonly destroyRef = inject(DestroyRef);
  private readonly indexadorService = inject(IndexadorService);

  // Enum options for template
  readonly tipoTaxaOptions = TIPO_TAXA_OPTIONS;
  readonly baseCalculoOptions = BASE_CALCULO_OPTIONS;
  readonly formaCobrancaOptions = FORMA_COBRANCA_OPTIONS;
  readonly maxTaxas = MAX_TAXAS;

  // Indexadores from API with search support
  readonly indexadores = signal<Indexador[]>([]);

  // Search subject for debounced API calls
  private readonly searchSubject = new Subject<string>();

  // Signals for UI state
  readonly searchingIndexadores = signal(false);

  // Track step ID to avoid re-loading
  private lastLoadedStepId: WizardStepId | null = null;

  // Main form with FormArray
  form = this.formBuilder.group({
    taxas: this.formBuilder.array<FormGroup>([]),
  });

  // Computed signals
  readonly taxasArray = computed(() => this.form.get('taxas') as FormArray);
  readonly taxasCount = computed(() => this.taxasArray().length);
  readonly canAddTaxa = computed(() => this.taxasCount() < MAX_TAXAS);
  readonly hasAdministracao = computed(() => {
    const taxas = this.taxasArray().controls;
    return taxas.some(
      (control) => control.get('tipoTaxa')?.value === TipoTaxa.ADMINISTRACAO
    );
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
    });

    // Setup search subscription with debounce (RF-04)
    this.searchSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        tap(() => this.searchingIndexadores.set(true)),
        switchMap((searchTerm) =>
          this.indexadorService
            .getIndexadores(50, 0, 'codigo', 'asc', {
              filter: searchTerm,
              ativo: 'true',
            })
            .pipe(
              catchError(() => {
                // On error, return empty result
                return of({ items: [], totalCount: 0, limit: 50, offset: 0 });
              })
            )
        ),
        tap(() => this.searchingIndexadores.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((response) => {
        this.indexadores.set(response.items);
      });

    // Load initial indexadores (active only, sorted by codigo)
    this.loadInitialIndexadores();

    // Effect: Load data when step changes
    effect(() => {
      const stepConfig = this.stepConfig();
      const stepId = stepConfig.id;

      if (this.lastLoadedStepId === stepId) {
        return;
      }
      this.lastLoadedStepId = stepId;

      // Reset form array
      const taxasArray = this.form.get('taxas') as FormArray;
      taxasArray.clear({ emitEvent: false });

      const stepData = untracked(
        () => this.wizardStore.stepData()[stepConfig.key] as TaxasFormData | undefined
      );

      if (stepData?.taxas && stepData.taxas.length > 0) {
        // Restore saved data
        stepData.taxas.forEach((taxa) => {
          taxasArray.push(this.createTaxaFormGroup(taxa), { emitEvent: false });
        });

        // Re-apply conditional validators for each restored taxa
        // (valueChanges subscriptions don't fire with emitEvent: false)
        taxasArray.controls.forEach((group) => {
          const tipoTaxa = group.get('tipoTaxa')?.value;
          this.applyTipoTaxaValidators(group as FormGroup, tipoTaxa);
        });
      } else {
        // RF-01: Add default administracao tax
        taxasArray.push(
          this.createTaxaFormGroup(this.getDefaultAdministracaoTaxa()),
          { emitEvent: false }
        );
      }

      // Re-validate the entire FormArray and form
      taxasArray.updateValueAndValidity({ emitEvent: false });
      this.form.updateValueAndValidity({ emitEvent: false });

      // Mark all fields as touched
      this.markAllAsTouched();

      untracked(() => this.updateStepValidation());
    });
  }

  /**
   * Get today's date in ISO format for default vigencia
   */
  private getTodayISODate(): string {
    return new Date().toISOString().split('T')[0];
  }

  /**
   * Get default administracao taxa with today's date
   */
  private getDefaultAdministracaoTaxa(): FundoTaxa {
    return {
      ...TAXA_ADMINISTRACAO_DEFAULT,
      dataInicioVigencia: this.getTodayISODate(),
    };
  }

  /**
   * Create a FormGroup for a single taxa
   */
  private createTaxaFormGroup(taxa?: Partial<FundoTaxa>): FormGroup {
    const group = this.formBuilder.group(
      {
        tipoTaxa: [taxa?.tipoTaxa ?? null, [Validators.required]],
        percentual: [
          taxa?.percentual ?? null,
          [Validators.required, Validators.min(0.000001), Validators.max(100)],
        ],
        percentualMinimo: [taxa?.percentualMinimo ?? null],
        percentualMaximo: [taxa?.percentualMaximo ?? null],
        baseCalculo: [taxa?.baseCalculo ?? null, [Validators.required]],
        formaCobranca: [taxa?.formaCobranca ?? null, [Validators.required]],
        dataInicioVigencia: [taxa?.dataInicioVigencia ?? null, [Validators.required]],
        dataFimVigencia: [taxa?.dataFimVigencia ?? null],
        benchmarkId: [taxa?.benchmarkId ?? null],
        percentualBenchmark: [taxa?.percentualBenchmark ?? 100],
        possuiHurdle: [taxa?.possuiHurdle ?? false],
        possuiHighWaterMark: [taxa?.possuiHighWaterMark ?? false],
        linhaDAguaGlobal: [taxa?.linhaDAguaGlobal ?? false],
        classeId: [taxa?.classeId ?? null],
        ativo: [taxa?.ativo ?? true],
      },
      { validators: dataFimVigenciaValidator }
    );

    // Setup conditional validators based on tipoTaxa
    this.setupConditionalValidators(group);

    return group;
  }

  /**
   * Setup conditional validators when tipoTaxa changes
   */
  private setupConditionalValidators(group: FormGroup): void {
    group
      .get('tipoTaxa')
      ?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((tipoTaxa: TipoTaxa | null) => {
        this.applyTipoTaxaValidators(group, tipoTaxa);
      });
  }

  /**
   * Apply validators based on tipoTaxa value
   * Called both from valueChanges subscription and after data restoration
   */
  private applyTipoTaxaValidators(group: FormGroup, tipoTaxa: TipoTaxa | null): void {
    const percentualControl = group.get('percentual');
    const benchmarkIdControl = group.get('benchmarkId');
    const percentualBenchmarkControl = group.get('percentualBenchmark');

    if (tipoTaxa) {
      // RF-05: Apply max percentual based on tipo
      const maxPercentual = LIMITES_PERCENTUAL[tipoTaxa];
      percentualControl?.setValidators([
        Validators.required,
        Validators.min(0.000001),
        Validators.max(maxPercentual),
      ]);

      // RF-03: Performance tax requires benchmark
      if (tipoTaxa === TipoTaxa.PERFORMANCE) {
        benchmarkIdControl?.setValidators([Validators.required]);
        percentualBenchmarkControl?.setValidators([
          Validators.min(0),
          Validators.max(200),
        ]);
        // Set default base calculo for performance
        if (!group.get('baseCalculo')?.value) {
          group
            .get('baseCalculo')
            ?.setValue(BaseCalculo.RENDIMENTO_ACIMA_BENCHMARK, { emitEvent: false });
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
  }

  /**
   * Add a new taxa to the list (RF-02)
   */
  addTaxa(): void {
    if (!this.canAddTaxa()) return;

    const taxasArray = this.form.get('taxas') as FormArray;
    const newTaxa: Partial<FundoTaxa> = {
      tipoTaxa: null,
      percentual: null,
      baseCalculo: BaseCalculo.PL_MEDIO,
      formaCobranca: FormaCobranca.DIARIA,
      dataInicioVigencia: this.getTodayISODate(),
      ativo: true,
    };

    taxasArray.push(this.createTaxaFormGroup(newTaxa));
  }

  /**
   * Remove a taxa from the list (RF-02)
   * Cannot remove administracao tax
   */
  removeTaxa(index: number): void {
    const taxasArray = this.form.get('taxas') as FormArray;
    const taxa = taxasArray.at(index);

    // RF-02: Cannot remove administracao
    if (taxa.get('tipoTaxa')?.value === TipoTaxa.ADMINISTRACAO) {
      return;
    }

    taxasArray.removeAt(index);
  }

  /**
   * Check if taxa can be removed (not administracao)
   */
  canRemoveTaxa(index: number): boolean {
    const taxasArray = this.form.get('taxas') as FormArray;
    const taxa = taxasArray.at(index);
    return taxa.get('tipoTaxa')?.value !== TipoTaxa.ADMINISTRACAO;
  }

  /**
   * Check if taxa is performance type (for conditional fields)
   */
  isPerformanceTaxa(index: number): boolean {
    const taxasArray = this.form.get('taxas') as FormArray;
    const taxa = taxasArray.at(index);
    return taxa.get('tipoTaxa')?.value === TipoTaxa.PERFORMANCE;
  }

  /**
   * Load initial indexadores from API
   */
  private loadInitialIndexadores(): void {
    this.searchingIndexadores.set(true);
    this.indexadorService
      .getIndexadores(50, 0, 'codigo', 'asc', { ativo: 'true' })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.indexadores.set(response.items);
          this.searchingIndexadores.set(false);
        },
        error: () => {
          this.indexadores.set([]);
          this.searchingIndexadores.set(false);
        },
      });
  }

  /**
   * Search indexadores with debounce (RF-04)
   * Called from template on input change
   */
  searchIndexadores(searchTerm: string): void {
    this.searchSubject.next(searchTerm);
  }

  /**
   * Get indexador nome by id
   */
  getIndexadorNome(benchmarkId: number | null): string {
    if (!benchmarkId) return '';
    const indexador = this.indexadores().find((i) => i.id === benchmarkId);
    return indexador ? `${indexador.codigo} - ${indexador.nome}` : '';
  }

  /**
   * Get max percentual for a tipo taxa
   */
  getMaxPercentual(tipoTaxa: TipoTaxa | null): number {
    return tipoTaxa ? LIMITES_PERCENTUAL[tipoTaxa] : 100;
  }

  /**
   * Format percentual for display (RF-06)
   */
  formatPercentual(value: number | null): string {
    if (value === null || value === undefined) return '';
    return `${value.toFixed(2)}% a.a.`;
  }

  private prepareDataForStore(formValue: any): TaxasFormData {
    return {
      taxas: (formValue.taxas || []).map((taxa: any) => ({
        tipoTaxa: taxa.tipoTaxa ?? null,
        percentual: taxa.percentual ?? null,
        percentualMinimo: taxa.percentualMinimo ?? null,
        percentualMaximo: taxa.percentualMaximo ?? null,
        baseCalculo: taxa.baseCalculo ?? null,
        formaCobranca: taxa.formaCobranca ?? null,
        dataInicioVigencia: taxa.dataInicioVigencia ?? null,
        dataFimVigencia: taxa.dataFimVigencia ?? null,
        benchmarkId: taxa.benchmarkId ?? null,
        percentualBenchmark: taxa.percentualBenchmark ?? null,
        possuiHurdle: taxa.possuiHurdle ?? false,
        possuiHighWaterMark: taxa.possuiHighWaterMark ?? false,
        linhaDAguaGlobal: taxa.linhaDAguaGlobal ?? false,
        classeId: taxa.classeId ?? null,
        ativo: taxa.ativo ?? true,
      })),
    };
  }

  private markAllAsTouched(): void {
    const taxasArray = this.form.get('taxas') as FormArray;
    taxasArray.controls.forEach((group) => {
      Object.keys((group as FormGroup).controls).forEach((key) => {
        group.get(key)?.markAsTouched();
      });
    });
  }

  private updateStepValidation(): void {
    const stepId = this.stepConfig().id;
    const taxasArray = this.form.get('taxas') as FormArray;

    // Validate: at least one administracao tax
    const hasAdmin = this.hasAdministracao();
    const isValid = this.form.valid && hasAdmin;

    this.wizardStore.setStepValidation(stepId, {
      isValid,
      isDirty: this.form.dirty || taxasArray.length > 0,
      errors: hasAdmin ? [] : ['Taxa de administracao e obrigatoria'],
    });

    if (isValid && (this.form.dirty || taxasArray.length > 0)) {
      this.wizardStore.markStepComplete(stepId);
    } else {
      this.wizardStore.markStepIncomplete(stepId);
    }
  }

  // Helper methods for template
  isFieldInvalid(index: number, fieldName: string): boolean {
    const taxasArray = this.form.get('taxas') as FormArray;
    const control = taxasArray.at(index)?.get(fieldName);
    return control ? control.touched && control.invalid : false;
  }

  isFieldValid(index: number, fieldName: string): boolean {
    const taxasArray = this.form.get('taxas') as FormArray;
    const control = taxasArray.at(index)?.get(fieldName);
    return control ? control.touched && control.valid : false;
  }

  getFieldError(index: number, fieldName: string, errorKey: string): boolean {
    const taxasArray = this.form.get('taxas') as FormArray;
    const control = taxasArray.at(index)?.get(fieldName);
    return control?.hasError(errorKey) ?? false;
  }

  getTaxaGroup(index: number): FormGroup {
    const taxasArray = this.form.get('taxas') as FormArray;
    return taxasArray.at(index) as FormGroup;
  }

  /**
   * Get label for tipo taxa
   */
  getTipoTaxaLabel(tipoTaxa: TipoTaxa): string {
    const option = TIPO_TAXA_OPTIONS.find((opt) => opt.value === tipoTaxa);
    return option?.label ?? tipoTaxa;
  }
}
