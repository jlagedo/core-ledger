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
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, filter, map, startWith, switchMap, catchError, of } from 'rxjs';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { WizardStepConfig, InvalidFieldInfo } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';
import { createRestorationEffect, withRestorationGuard, markFormArrayAsTouched, collectFormArrayInvalidFields } from '../../shared';
import { IdentificacaoFormData, TipoFundo } from '../../models/identificacao.model';
import {
  TipoVinculo,
  VinculoFormData,
  VinculosFormData,
  VINCULOS_OBRIGATORIOS,
  TIPO_VINCULO_OPTIONS,
  VINCULO_DEFAULT,
  getTipoVinculoLabel,
  isVinculoObrigatorio,
  isVinculoFidcRecomendado,
  formatCnpj,
  getMissingRequiredVinculos,
  hasFidcRecommendedVinculos,
  InstituicaoAutocompleteItem,
  getAvailableTipoVinculos,
} from '../../models/vinculos.model';
import { InstituicoesService } from '../../../../../../services/instituicoes-service';
import { InstituicaoModal } from './instituicao-modal/instituicao-modal';

/**
 * Validador customizado: verifica se todos os vínculos obrigatórios estão preenchidos (RF-01)
 */
function vinculosObrigatoriosValidator(formArray: AbstractControl): ValidationErrors | null {
  const array = formArray as FormArray;
  const tiposPreenchidos = array.controls
    .filter((ctrl) => {
      const cnpj = ctrl.get('cnpjInstituicao')?.value;
      return cnpj && cnpj.length > 0;
    })
    .map((ctrl) => ctrl.get('tipoVinculo')?.value as TipoVinculo)
    .filter(Boolean);

  const faltantes = VINCULOS_OBRIGATORIOS.filter((tipo) => !tiposPreenchidos.includes(tipo));

  if (faltantes.length > 0) {
    return { vinculosFaltantes: { tipos: faltantes } };
  }

  return null;
}

/**
 * Componente para Etapa 9 do wizard: Vínculos Institucionais
 * Gerencia vínculos com instituições (administrador, gestor, custodiante, etc.)
 */
@Component({
  selector: 'app-vinculos-step',
  imports: [ReactiveFormsModule],
  templateUrl: './vinculos-step.html',
  styleUrl: './vinculos-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VinculosStep {
  stepConfig = input.required<WizardStepConfig>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly wizardStore = inject(WizardStore);
  private readonly destroyRef = inject(DestroyRef);
  private readonly instituicoesService = inject(InstituicoesService);
  private readonly modalService = inject(NgbModal);
  private readonly router = inject(Router);

  // Enum options for template
  readonly tipoVinculoOptions = TIPO_VINCULO_OPTIONS;
  readonly vinculosObrigatorios = VINCULOS_OBRIGATORIOS;

  // Main form with FormArray of vinculos
  form = this.formBuilder.group({
    vinculos: this.formBuilder.array<FormGroup>([], [vinculosObrigatoriosValidator]),
  });

  // Convert form valueChanges to signal for reactive computed dependencies
  // Fixes bug: computed() doesn't track Reactive Forms values directly
  // See: docs/aidebug/computed-signal-form-values.md
  private readonly vinculosFormValue = toSignal(
    this.form.valueChanges.pipe(
      startWith(this.form.value),
      map((value) => (value.vinculos ?? []) as Partial<VinculoFormData>[])
    ),
    { initialValue: [] as Partial<VinculoFormData>[] }
  );

  // Autocomplete state per vínculo (keyed by index)
  readonly autocompleteResults = signal<Map<number, InstituicaoAutocompleteItem[]>>(new Map());
  readonly autocompleteLoading = signal<Map<number, boolean>>(new Map());
  readonly autocompleteVisible = signal<Map<number, boolean>>(new Map());

  // Computed signals using the signal for proper reactivity
  readonly vinculosArray = computed(() => this.form.get('vinculos') as FormArray);
  readonly vinculosCount = computed(() => this.vinculosFormValue().length);

  // Computed: Check if fund is FIDC type (RF-02)
  readonly isFidc = computed(() => {
    const identificacaoData = this.wizardStore.stepData()['identificacao'] as IdentificacaoFormData | undefined;
    const tipoFundo = identificacaoData?.tipoFundo;
    return tipoFundo === TipoFundo.FIDC || tipoFundo === TipoFundo.FIDC_NP;
  });

  // Computed: Get missing required vinculos
  readonly missingVinculos = computed(() => {
    const vinculos = this.vinculosFormValue().map((v) => ({
      tipoVinculo: v.tipoVinculo as TipoVinculo,
      cnpjInstituicao: v.cnpjInstituicao as string,
      instituicaoId: v.instituicaoId as string | null,
    })) as VinculoFormData[];
    return getMissingRequiredVinculos(vinculos);
  });

  // Computed: Check if FIDC has recommended vinculos (RF-02)
  readonly hasFidcRecommended = computed(() => {
    if (!this.isFidc()) return true;
    const vinculos = this.vinculosFormValue().map((v) => ({
      tipoVinculo: v.tipoVinculo as TipoVinculo,
      cnpjInstituicao: v.cnpjInstituicao as string,
      instituicaoId: v.instituicaoId as string | null,
    })) as VinculoFormData[];
    return hasFidcRecommendedVinculos(vinculos);
  });

  // Computed: Get optional vinculo types available to add
  readonly availableOptionalVinculos = computed(() => {
    const existingTypes = this.vinculosFormValue()
      .map((v) => v.tipoVinculo as TipoVinculo)
      .filter(Boolean);
    return getAvailableTipoVinculos(existingTypes, false);
  });

  // Expanded details state
  readonly expandedDetails = signal<Set<number>>(new Set());

  constructor() {
    // Create restoration effect (centralizes deduplication, isRestoring flag, and form restore)
    const { isRestoring } = createRestorationEffect<VinculosFormData>({
      stepConfig: () => this.stepConfig(),
      wizardStore: this.wizardStore,
      form: this.form,
      resetForm: () => {
        const vinculosArray = this.form.get('vinculos') as FormArray;
        vinculosArray.clear();
      },
      restoreData: (data) => {
        const vinculosArray = this.form.get('vinculos') as FormArray;
        // Restore saved vinculos - setupDateValidation runs automatically via startWith
        data.vinculos.forEach((vinculo) => {
          vinculosArray.push(this.createVinculoFormGroup(vinculo));
        });
        this.form.markAsDirty();
      },
      createDefaultData: () => {
        // Initialize with required vinculos (RF-01)
        const vinculosArray = this.form.get('vinculos') as FormArray;
        VINCULOS_OBRIGATORIOS.forEach((tipo) => {
          vinculosArray.push(
            this.createVinculoFormGroup({
              ...VINCULO_DEFAULT,
              tipoVinculo: tipo,
              dataInicio: this.getTodayISODate(),
            } as VinculoFormData)
          );
        });
      },
      markAllAsTouched: () => {
        const vinculosArray = this.form.get('vinculos') as FormArray;
        markFormArrayAsTouched(vinculosArray);
      },
      updateStepValidation: () => this.updateStepValidation(),
    });

    // Setup form subscriptions
    this.form.statusChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.updateStepValidation());

    this.form.valueChanges
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        withRestorationGuard(isRestoring)
      )
      .subscribe((value) => {
        const stepConfig = untracked(() => this.stepConfig());
        const dataForStore = this.prepareDataForStore(value);
        this.wizardStore.setStepData(stepConfig.key, dataForStore);
      });
  }

  /**
   * Get today's date in ISO format
   */
  private getTodayISODate(): string {
    return new Date().toISOString().split('T')[0];
  }

  /**
   * Create a FormGroup for a single vinculo
   */
  private createVinculoFormGroup(vinculo: Partial<VinculoFormData>): FormGroup {
    const group = this.formBuilder.group({
      tipoVinculo: [vinculo.tipoVinculo ?? null, [Validators.required]],
      instituicaoId: [vinculo.instituicaoId ?? null],
      cnpjInstituicao: [vinculo.cnpjInstituicao ?? '', [Validators.required]],
      nomeInstituicao: [vinculo.nomeInstituicao ?? ''],
      dataInicio: [vinculo.dataInicio ?? '', [Validators.required]],
      dataFim: [vinculo.dataFim ?? null],
      motivoFim: [vinculo.motivoFim ?? null],
      responsavelNome: [vinculo.responsavelNome ?? null, [Validators.maxLength(100)]],
      responsavelEmail: [vinculo.responsavelEmail ?? null, [Validators.email, Validators.maxLength(100)]],
      responsavelTelefone: [vinculo.responsavelTelefone ?? null, [Validators.maxLength(20)]],
      // Autocomplete search term (not saved)
      searchTerm: [''],
    });

    // Setup autocomplete for this vinculo
    this.setupAutocomplete(group);

    // Setup date validation (RF-07)
    this.setupDateValidation(group);

    return group;
  }

  /**
   * Setup autocomplete behavior for a vinculo form group
   */
  private setupAutocomplete(group: FormGroup): void {
    group
      .get('searchTerm')
      ?.valueChanges.pipe(
        takeUntilDestroyed(this.destroyRef),
        debounceTime(300),
        distinctUntilChanged(),
        filter((term) => typeof term === 'string' && term.length >= 3),
        switchMap((term) => {
          const index = this.getVinculoIndex(group);
          this.setAutocompleteLoading(index, true);
          return this.instituicoesService.search(term).pipe(
            catchError((err) => {
              console.error('🔴 Autocomplete search error:', err);
              return of([]);
            })
          );
        })
      )
      .subscribe((results) => {
        const index = this.getVinculoIndex(group);
        this.setAutocompleteLoading(index, false);
        this.setAutocompleteResults(index, results);
        this.setAutocompleteVisible(index, results.length > 0);
      });
  }

  /**
   * Setup cross-field date validation (RF-07).
   * Uses startWith to apply validation immediately for initial values.
   */
  private setupDateValidation(group: FormGroup): void {
    const dataFimControl = group.get('dataFim');
    dataFimControl?.valueChanges
      .pipe(
        startWith(dataFimControl.value),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.validateDataFim(group);
      });

    const dataInicioControl = group.get('dataInicio');
    dataInicioControl?.valueChanges
      .pipe(
        startWith(dataInicioControl.value),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.validateDataInicio(group);
      });
  }

  /**
   * Validate dataFim is not before dataInicio.
   * Called from valueChanges subscription (including startWith for initial value).
   */
  private validateDataFim(group: FormGroup): void {
    const dataInicio = group.get('dataInicio')?.value;
    const dataFim = group.get('dataFim')?.value;
    const dataFimControl = group.get('dataFim');

    if (dataInicio && dataFim && dataFim < dataInicio) {
      dataFimControl?.setErrors({ dataFimAnterior: true });
    } else if (dataFimControl?.hasError('dataFimAnterior')) {
      // Clear custom error if date is now valid
      const errors = { ...dataFimControl.errors };
      delete errors['dataFimAnterior'];
      dataFimControl.setErrors(Object.keys(errors).length > 0 ? errors : null);
    }
  }

  /**
   * Validate dataInicio is not in the future.
   * Called from valueChanges subscription (including startWith for initial value).
   */
  private validateDataInicio(group: FormGroup): void {
    const dataInicio = group.get('dataInicio')?.value;
    const today = this.getTodayISODate();
    const dataInicioControl = group.get('dataInicio');

    if (dataInicio && dataInicio > today) {
      dataInicioControl?.setErrors({ dataInicioFutura: true });
    } else if (dataInicioControl?.hasError('dataInicioFutura')) {
      // Clear custom error if date is now valid
      const errors = { ...dataInicioControl.errors };
      delete errors['dataInicioFutura'];
      dataInicioControl.setErrors(Object.keys(errors).length > 0 ? errors : null);
    }
  }

  /**
   * Get the index of a vinculo FormGroup in the array
   */
  private getVinculoIndex(group: FormGroup): number {
    const vinculosArray = this.form.get('vinculos') as FormArray;
    return vinculosArray.controls.indexOf(group);
  }

  /**
   * Set autocomplete loading state
   */
  private setAutocompleteLoading(index: number, loading: boolean): void {
    this.autocompleteLoading.update((map) => {
      const newMap = new Map(map);
      newMap.set(index, loading);
      return newMap;
    });
  }

  /**
   * Set autocomplete results
   */
  private setAutocompleteResults(index: number, results: InstituicaoAutocompleteItem[]): void {
    this.autocompleteResults.update((map) => {
      const newMap = new Map(map);
      newMap.set(index, results);
      return newMap;
    });
  }

  /**
   * Set autocomplete visibility
   */
  private setAutocompleteVisible(index: number, visible: boolean): void {
    this.autocompleteVisible.update((map) => {
      const newMap = new Map(map);
      newMap.set(index, visible);
      return newMap;
    });
  }

  /**
   * Get autocomplete results for a vinculo
   */
  getAutocompleteResults(index: number): InstituicaoAutocompleteItem[] {
    return this.autocompleteResults().get(index) ?? [];
  }

  /**
   * Check if autocomplete is loading for a vinculo
   */
  isAutocompleteLoading(index: number): boolean {
    return this.autocompleteLoading().get(index) ?? false;
  }

  /**
   * Check if autocomplete is visible for a vinculo
   */
  isAutocompleteVisible(index: number): boolean {
    return this.autocompleteVisible().get(index) ?? false;
  }

  /**
   * Handle autocomplete item selection
   */
  selectInstituicao(index: number, item: InstituicaoAutocompleteItem): void {
    if (!item || !item.id || !item.cnpj) {
      return;
    }

    const vinculosArray = this.form.get('vinculos') as FormArray;
    const group = vinculosArray.at(index) as FormGroup;

    group.patchValue({
      instituicaoId: item.id,
      cnpjInstituicao: item.cnpj,
      nomeInstituicao: item.razaoSocial || item.nomeFantasia || '',
      searchTerm: '',
    });

    // Ensure form control is marked as touched and validated
    const cnpjControl = group.get('cnpjInstituicao');
    const nomeControl = group.get('nomeInstituicao');
    const idControl = group.get('instituicaoId');

    if (cnpjControl) {
      cnpjControl.markAsTouched();
      cnpjControl.markAsDirty();
      cnpjControl.updateValueAndValidity();
    }

    if (nomeControl) {
      nomeControl.markAsTouched();
      nomeControl.markAsDirty();
      nomeControl.updateValueAndValidity();
    }

    if (idControl) {
      idControl.markAsTouched();
      idControl.markAsDirty();
      idControl.updateValueAndValidity();
    }

    group.markAsTouched();
    group.markAsDirty();
    group.updateValueAndValidity();

    // Force form array to re-validate
    vinculosArray.updateValueAndValidity();
    this.form.updateValueAndValidity();

    // Explicitly update wizard validation state
    this.updateStepValidation();

    this.setAutocompleteVisible(index, false);
    this.setAutocompleteResults(index, []);
  }

  /**
   * Clear selected instituicao
   */
  clearInstituicao(index: number): void {
    const vinculosArray = this.form.get('vinculos') as FormArray;
    const group = vinculosArray.at(index) as FormGroup;

    group.patchValue({
      instituicaoId: null,
      cnpjInstituicao: '',
      nomeInstituicao: '',
      searchTerm: '',
    });
  }

  /**
   * Open modal for quick institution registration (RF-04)
   */
  openInstituicaoModal(index: number): void {
    const vinculosArray = this.form.get('vinculos') as FormArray;
    const group = vinculosArray.at(index) as FormGroup;
    const searchTerm = group.get('searchTerm')?.value || '';

    const modalRef = this.modalService.open(InstituicaoModal, {
      centered: true,
      backdrop: 'static',
      size: 'md',
    });

    // Pass the search term to pre-fill the form
    modalRef.componentInstance.initialSearchTerm.set(searchTerm);

    modalRef.result.then(
      (result: InstituicaoAutocompleteItem | string) => {
        if (typeof result === 'object') {
          // Auto-select the created institution
          this.selectInstituicao(index, result);
        }
      },
      (reason) => {
        if (reason === 'full-registration') {
          // Navigate to full registration page
          this.router.navigate(['/cadastros/instituicoes/novo']);
        }
        // Other dismissals (escape, cancel) are ignored
      }
    );
  }

  /**
   * Navigate to full institution registration page (RF-04)
   */
  goToFullRegistration(): void {
    this.router.navigate(['/cadastros/instituicoes/novo']);
  }

  /**
   * Hide autocomplete dropdown
   */
  hideAutocomplete(index: number): void {
    // Delay to allow click event to fire first
    setTimeout(() => {
      this.setAutocompleteVisible(index, false);
    }, 200);
  }

  /**
   * Show autocomplete dropdown (if results exist)
   */
  showAutocomplete(index: number): void {
    const results = this.getAutocompleteResults(index);
    if (results.length > 0) {
      this.setAutocompleteVisible(index, true);
    }
  }

  /**
   * Add an optional vinculo
   */
  addOptionalVinculo(tipo: TipoVinculo): void {
    const vinculosArray = this.form.get('vinculos') as FormArray;
    vinculosArray.push(
      this.createVinculoFormGroup({
        ...VINCULO_DEFAULT,
        tipoVinculo: tipo,
        dataInicio: this.getTodayISODate(),
      } as VinculoFormData)
    );
  }

  /**
   * Remove an optional vinculo (RF-01: cannot remove required vinculos)
   */
  removeVinculo(index: number): void {
    const vinculosArray = this.form.get('vinculos') as FormArray;
    const vinculo = vinculosArray.at(index);
    const tipoVinculo = vinculo.get('tipoVinculo')?.value as TipoVinculo;

    // Cannot remove required vinculos
    if (isVinculoObrigatorio(tipoVinculo)) {
      return;
    }

    vinculosArray.removeAt(index);
  }

  /**
   * Check if a vinculo can be removed (RF-01)
   */
  canRemoveVinculo(index: number): boolean {
    const vinculosArray = this.form.get('vinculos') as FormArray;
    const vinculo = vinculosArray.at(index);
    const tipoVinculo = vinculo.get('tipoVinculo')?.value as TipoVinculo;
    return !isVinculoObrigatorio(tipoVinculo);
  }

  /**
   * Toggle details expansion
   */
  toggleDetails(index: number): void {
    this.expandedDetails.update((set) => {
      const newSet = new Set(set);
      if (newSet.has(index)) {
        newSet.delete(index);
      } else {
        newSet.add(index);
      }
      return newSet;
    });
  }

  /**
   * Check if details are expanded
   */
  isDetailsExpanded(index: number): boolean {
    return this.expandedDetails().has(index);
  }

  /**
   * Get tipo vinculo label
   */
  getTipoVinculoLabel(tipo: TipoVinculo): string {
    return getTipoVinculoLabel(tipo);
  }

  /**
   * Check if tipo vinculo is obrigatorio
   */
  isObrigatorio(tipo: TipoVinculo): boolean {
    return isVinculoObrigatorio(tipo);
  }

  /**
   * Check if tipo vinculo is FIDC recommended
   */
  isFidcRecomendado(tipo: TipoVinculo): boolean {
    return isVinculoFidcRecomendado(tipo);
  }

  /**
   * Format CNPJ for display
   */
  formatCnpj(cnpj: string): string {
    return formatCnpj(cnpj);
  }

  /**
   * Get vinculo FormGroup
   */
  getVinculoGroup(index: number): FormGroup {
    const vinculosArray = this.form.get('vinculos') as FormArray;
    return vinculosArray.at(index) as FormGroup;
  }

  /**
   * Check if a field is invalid
   */
  isFieldInvalid(index: number, fieldName: string): boolean {
    const control = this.getVinculoGroup(index).get(fieldName);
    return control ? control.touched && control.invalid : false;
  }

  /**
   * Check if a field is valid
   */
  isFieldValid(index: number, fieldName: string): boolean {
    const control = this.getVinculoGroup(index).get(fieldName);
    return control ? control.touched && control.valid && control.value : false;
  }

  /**
   * Check if a field has a specific error
   */
  getFieldError(index: number, fieldName: string, errorKey: string): boolean {
    const control = this.getVinculoGroup(index).get(fieldName);
    return control?.hasError(errorKey) ?? false;
  }

  /**
   * Check if vinculo has instituicao selected
   */
  hasInstituicaoSelected(index: number): boolean {
    const group = this.getVinculoGroup(index);
    return !!group.get('cnpjInstituicao')?.value;
  }

  private prepareDataForStore(formValue: any): VinculosFormData {
    return {
      vinculos: (formValue.vinculos || []).map((vinculo: any) => ({
        tipoVinculo: vinculo.tipoVinculo,
        instituicaoId: vinculo.instituicaoId ?? null,
        cnpjInstituicao: vinculo.cnpjInstituicao ?? '',
        nomeInstituicao: vinculo.nomeInstituicao ?? '',
        dataInicio: vinculo.dataInicio ?? '',
        dataFim: vinculo.dataFim ?? null,
        motivoFim: vinculo.motivoFim ?? null,
        responsavelNome: vinculo.responsavelNome ?? null,
        responsavelEmail: vinculo.responsavelEmail ?? null,
        responsavelTelefone: vinculo.responsavelTelefone ?? null,
      })),
    };
  }

  private updateStepValidation(): void {
    const stepId = this.stepConfig().id;
    const vinculosArray = this.form.get('vinculos') as FormArray;

    const errors: string[] = [];

    // Use form's actual validity status (much more reliable)
    const isValid = this.form.valid;

    // If form is invalid, extract error messages
    if (this.form.invalid) {
      // Check FormArray errors from validator
      if (vinculosArray.errors && vinculosArray.errors['vinculosFaltantes']) {
        const faltantes = vinculosArray.errors['vinculosFaltantes'].tipos as TipoVinculo[];
        const missingLabels = faltantes.map((t) => getTipoVinculoLabel(t)).join(', ');
        errors.push(`Vínculos obrigatórios faltando: ${missingLabels}`);
      }

      // Check for individual field errors
      vinculosArray.controls.forEach((ctrl) => {
        if (ctrl.invalid) {
          const group = ctrl as FormGroup;
          Object.keys(group.controls).forEach((key) => {
            const control = group.get(key);
            if (control?.invalid && control?.errors && !errors.includes('Preencha todos os campos obrigatórios')) {
              errors.push('Preencha todos os campos obrigatórios');
            }
          });
        }
      });
    }

    const invalidFields = this.collectInvalidFields();

    this.wizardStore.setStepValidation(stepId, {
      isValid,
      isDirty: this.form.dirty || vinculosArray.length > 0,
      errors,
      invalidFields,
    });

    if (isValid && (this.form.dirty || vinculosArray.length > 0)) {
      this.wizardStore.markStepComplete(stepId);
    } else {
      this.wizardStore.markStepIncomplete(stepId);
    }
  }

  private collectInvalidFields(): InvalidFieldInfo[] {
    const vinculosArray = this.form.get('vinculos') as FormArray;
    // Exclude searchTerm field from validation as it's just for autocomplete
    return collectFormArrayInvalidFields(vinculosArray, 'vinculos', ['searchTerm']);
  }
}
