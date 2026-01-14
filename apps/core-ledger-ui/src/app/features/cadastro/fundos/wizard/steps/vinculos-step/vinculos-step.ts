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
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, filter, switchMap, catchError, of } from 'rxjs';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { WizardStepConfig, WizardStepId } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';
import { IdentificacaoFormData, TipoFundo } from '../../models/identificacao.model';
import {
  TipoVinculo,
  VinculoFormData,
  VinculosFormData,
  VINCULOS_OBRIGATORIOS,
  TIPO_VINCULO_OPTIONS,
  TipoVinculoOption,
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

  // Track step ID to avoid re-loading
  private lastLoadedStepId: WizardStepId | null = null;

  // Enum options for template
  readonly tipoVinculoOptions = TIPO_VINCULO_OPTIONS;
  readonly vinculosObrigatorios = VINCULOS_OBRIGATORIOS;

  // Main form with FormArray of vinculos
  form = this.formBuilder.group({
    vinculos: this.formBuilder.array<FormGroup>([], [vinculosObrigatoriosValidator]),
  });

  // Autocomplete state per vínculo (keyed by index)
  readonly autocompleteResults = signal<Map<number, InstituicaoAutocompleteItem[]>>(new Map());
  readonly autocompleteLoading = signal<Map<number, boolean>>(new Map());
  readonly autocompleteVisible = signal<Map<number, boolean>>(new Map());

  // Computed signals
  readonly vinculosArray = computed(() => this.form.get('vinculos') as FormArray);
  readonly vinculosCount = computed(() => this.vinculosArray().length);

  // Computed: Check if fund is FIDC type (RF-02)
  readonly isFidc = computed(() => {
    const identificacaoData = this.wizardStore.stepData()['identificacao'] as IdentificacaoFormData | undefined;
    const tipoFundo = identificacaoData?.tipoFundo;
    return tipoFundo === TipoFundo.FIDC || tipoFundo === TipoFundo.FIDC_NP;
  });

  // Computed: Get missing required vinculos
  readonly missingVinculos = computed(() => {
    const vinculosArray = this.form.get('vinculos') as FormArray;
    const vinculos = vinculosArray.controls.map((ctrl) => ({
      tipoVinculo: ctrl.get('tipoVinculo')?.value as TipoVinculo,
      cnpjInstituicao: ctrl.get('cnpjInstituicao')?.value as string,
      instituicaoId: ctrl.get('instituicaoId')?.value as string | null,
    })) as VinculoFormData[];
    return getMissingRequiredVinculos(vinculos);
  });

  // Computed: Check if FIDC has recommended vinculos (RF-02)
  readonly hasFidcRecommended = computed(() => {
    if (!this.isFidc()) return true;
    const vinculosArray = this.form.get('vinculos') as FormArray;
    const vinculos = vinculosArray.controls.map((ctrl) => ({
      tipoVinculo: ctrl.get('tipoVinculo')?.value as TipoVinculo,
      cnpjInstituicao: ctrl.get('cnpjInstituicao')?.value as string,
      instituicaoId: ctrl.get('instituicaoId')?.value as string | null,
    })) as VinculoFormData[];
    return hasFidcRecommendedVinculos(vinculos);
  });

  // Computed: Get optional vinculo types available to add
  readonly availableOptionalVinculos = computed(() => {
    const vinculosArray = this.form.get('vinculos') as FormArray;
    const existingTypes = vinculosArray.controls
      .map((ctrl) => ctrl.get('tipoVinculo')?.value as TipoVinculo)
      .filter(Boolean);
    return getAvailableTipoVinculos(existingTypes, false);
  });

  // Expanded details state
  readonly expandedDetails = signal<Set<number>>(new Set());

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

    // Effect: Load data when step changes
    effect(() => {
      const stepConfig = this.stepConfig();
      const stepId = stepConfig.id;

      if (this.lastLoadedStepId === stepId) {
        return;
      }
      this.lastLoadedStepId = stepId;

      // Reset form array
      const vinculosArray = this.form.get('vinculos') as FormArray;
      vinculosArray.clear({ emitEvent: false });

      const stepData = untracked(
        () => this.wizardStore.stepData()[stepConfig.key] as VinculosFormData | undefined
      );

      if (stepData && stepData.vinculos && stepData.vinculos.length > 0) {
        // Restore saved data
        stepData.vinculos.forEach((vinculo) => {
          vinculosArray.push(this.createVinculoFormGroup(vinculo), { emitEvent: false });
        });
      } else {
        // Initialize with required vinculos (RF-01)
        VINCULOS_OBRIGATORIOS.forEach((tipo) => {
          vinculosArray.push(
            this.createVinculoFormGroup({
              ...VINCULO_DEFAULT,
              tipoVinculo: tipo,
              dataInicio: this.getTodayISODate(),
            } as VinculoFormData),
            { emitEvent: false }
          );
        });
      }

      // Mark all fields as touched
      this.markAllAsTouched();

      untracked(() => this.updateStepValidation());
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
            catchError(() => of([]))
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
   * Setup cross-field date validation (RF-07)
   */
  private setupDateValidation(group: FormGroup): void {
    group.get('dataFim')?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      const dataInicio = group.get('dataInicio')?.value;
      const dataFim = group.get('dataFim')?.value;

      if (dataInicio && dataFim && dataFim < dataInicio) {
        group.get('dataFim')?.setErrors({ dataFimAnterior: true });
      }
    });

    group.get('dataInicio')?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      const dataInicio = group.get('dataInicio')?.value;
      const today = this.getTodayISODate();

      if (dataInicio && dataInicio > today) {
        group.get('dataInicio')?.setErrors({ dataInicioFutura: true });
      }
    });
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
    const vinculosArray = this.form.get('vinculos') as FormArray;
    const group = vinculosArray.at(index) as FormGroup;

    group.patchValue({
      instituicaoId: item.id,
      cnpjInstituicao: item.cnpj,
      nomeInstituicao: item.razaoSocial,
      searchTerm: '',
    });

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

  private markAllAsTouched(): void {
    const vinculosArray = this.form.get('vinculos') as FormArray;
    vinculosArray.controls.forEach((group) => {
      Object.keys((group as FormGroup).controls).forEach((key) => {
        group.get(key)?.markAsTouched();
      });
    });
  }

  private updateStepValidation(): void {
    const stepId = this.stepConfig().id;
    const vinculosArray = this.form.get('vinculos') as FormArray;

    const errors: string[] = [];
    let isValid = false;

    // Check for missing required vinculos
    const missing = this.missingVinculos();
    if (missing.length > 0) {
      const missingLabels = missing.map((t) => getTipoVinculoLabel(t)).join(', ');
      errors.push(`Vínculos obrigatórios faltando: ${missingLabels}`);
    }

    // Check for individual field errors
    let hasFieldErrors = false;
    vinculosArray.controls.forEach((ctrl) => {
      if (ctrl.invalid) {
        hasFieldErrors = true;
      }
    });

    if (hasFieldErrors && errors.length === 0) {
      errors.push('Preencha todos os campos obrigatórios');
    }

    // Check FIDC recommendation (RF-02) - warning only
    if (this.isFidc() && !this.hasFidcRecommended()) {
      // This is a warning, not an error - step is still valid
    }

    isValid = missing.length === 0 && !hasFieldErrors;

    this.wizardStore.setStepValidation(stepId, {
      isValid,
      isDirty: this.form.dirty || vinculosArray.length > 0,
      errors,
    });

    if (isValid && (this.form.dirty || vinculosArray.length > 0)) {
      this.wizardStore.markStepComplete(stepId);
    } else {
      this.wizardStore.markStepIncomplete(stepId);
    }
  }
}
