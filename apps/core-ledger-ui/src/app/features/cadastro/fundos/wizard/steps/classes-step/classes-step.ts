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
import { WizardStepConfig, WizardStepId } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';
import { IdentificacaoFormData, TipoFundo } from '../../models/identificacao.model';
import {
  ClassesFormData,
  CLASSE_DEFAULT,
  CLASSE_SENIOR_DEFAULT,
  FundoClasse,
  getDefaultOrdemSubordinacao,
  getDefaultResponsabilidadeLimitada,
  LIMITES_TAXA_CLASSE,
  MAX_CLASSES,
  PublicoAlvo,
  PUBLICO_ALVO_OPTIONS,
  TipoClasseFidc,
  TIPO_CLASSE_FIDC_OPTIONS,
} from '../../models/classes.model';

/**
 * Validador customizado: verifica gaps na ordem de subordinacao (RF-05)
 */
function ordemSubordinacaoValidator(formArray: AbstractControl): ValidationErrors | null {
  const array = formArray as FormArray;
  const ordens = array.controls
    .map((ctrl) => ctrl.get('ordemSubordinacao')?.value)
    .filter((v): v is number => v !== null && v !== undefined)
    .sort((a, b) => a - b);

  if (ordens.length === 0) return null;

  // Check for gaps
  for (let i = 0; i < ordens.length; i++) {
    if (ordens[i] !== i + 1) {
      return { ordemGap: true };
    }
  }

  return null;
}

/**
 * Validador customizado: codigo unico por fundo
 */
function codigoUnicoValidator(formArray: AbstractControl): ValidationErrors | null {
  const array = formArray as FormArray;
  const codigos = array.controls.map((ctrl) => ctrl.get('codigoClasse')?.value?.toUpperCase()).filter(Boolean);

  const uniqueCodigos = new Set(codigos);
  if (uniqueCodigos.size !== codigos.length) {
    return { codigoDuplicado: true };
  }

  return null;
}

/**
 * Componente para Etapa 7 do wizard: Classes CVM 175
 * Gerencia estrutura multiclasse com lista dinamica de classes
 */
@Component({
  selector: 'app-classes-step',
  imports: [ReactiveFormsModule],
  templateUrl: './classes-step.html',
  styleUrl: './classes-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClassesStep {
  stepConfig = input.required<WizardStepConfig>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly wizardStore = inject(WizardStore);
  private readonly destroyRef = inject(DestroyRef);

  // Enum options for template
  readonly tipoClasseFidcOptions = TIPO_CLASSE_FIDC_OPTIONS;
  readonly publicoAlvoOptions = PUBLICO_ALVO_OPTIONS;
  readonly maxClasses = MAX_CLASSES;

  // Track step ID to avoid re-loading
  private lastLoadedStepId: WizardStepId | null = null;

  // Main form
  form = this.formBuilder.group({
    multiclasse: [false],
    classes: this.formBuilder.array<FormGroup>([], [ordemSubordinacaoValidator, codigoUnicoValidator]),
  });

  // Computed signals
  readonly classesArray = computed(() => this.form.get('classes') as FormArray);
  readonly classesCount = computed(() => this.classesArray().length);
  readonly canAddClasse = computed(() => this.classesCount() < MAX_CLASSES);
  readonly isMulticlasse = signal(false);

  // Computed: Check if fund is FIDC type (RF-02)
  readonly isFidc = computed(() => {
    const identificacaoData = this.wizardStore.stepData()['identificacao'] as IdentificacaoFormData | undefined;
    const tipoFundo = identificacaoData?.tipoFundo;
    return tipoFundo === TipoFundo.FIDC || tipoFundo === TipoFundo.FIDC_NP;
  });

  // Computed: Classes are mandatory for FIDC (RF-02)
  readonly classesObrigatorias = computed(() => this.isFidc());

  // Computed: Has at least one class when multiclasse is enabled
  readonly hasMinimumClasses = computed(() => {
    if (!this.isMulticlasse()) return true;
    return this.classesCount() >= 1;
  });

  // Computed: Check for subordination order validation errors
  readonly hasOrdemGapError = computed(() => {
    const classesControl = this.form.get('classes');
    return classesControl?.hasError('ordemGap') ?? false;
  });

  // Computed: Check for duplicate codigo error
  readonly hasCodigoDuplicadoError = computed(() => {
    const classesControl = this.form.get('classes');
    return classesControl?.hasError('codigoDuplicado') ?? false;
  });

  // Computed: Get list of subordination types present
  readonly tiposSubordinacaoPresentes = computed(() => {
    const tipos: TipoClasseFidc[] = [];
    const classesArray = this.form.get('classes') as FormArray;
    classesArray.controls.forEach((ctrl) => {
      const tipo = ctrl.get('tipoClasseFidc')?.value;
      if (tipo && !tipos.includes(tipo)) {
        tipos.push(tipo);
      }
    });
    return tipos;
  });

  constructor() {
    // Watch multiclasse toggle
    this.form
      .get('multiclasse')
      ?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        this.isMulticlasse.set(value ?? false);
        this.handleMulticlasseChange(value ?? false);
      });

    // Setup form subscriptions
    this.form.statusChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.updateStepValidation());

    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      const stepConfig = untracked(() => this.stepConfig());
      const dataForStore = this.prepareDataForStore(value);
      this.wizardStore.setStepData(stepConfig.key, dataForStore);
    });

    // Effect: Load data when step changes
    effect(() => {
      const stepConfig = this.stepConfig();
      const stepId = stepConfig.id;
      const isFidc = this.isFidc();

      if (this.lastLoadedStepId === stepId) {
        return;
      }
      this.lastLoadedStepId = stepId;

      // Reset form array
      const classesArray = this.form.get('classes') as FormArray;
      classesArray.clear({ emitEvent: false });

      const stepData = untracked(() => this.wizardStore.stepData()[stepConfig.key] as ClassesFormData | undefined);

      if (stepData) {
        // Restore saved data
        this.form.get('multiclasse')?.setValue(stepData.multiclasse, { emitEvent: false });
        this.isMulticlasse.set(stepData.multiclasse);

        if (stepData.classes && stepData.classes.length > 0) {
          stepData.classes.forEach((classe) => {
            classesArray.push(this.createClasseFormGroup(classe, isFidc), { emitEvent: false });
          });

          // Re-apply FIDC validators and tipoClasse defaults for each restored classe
          // (valueChanges subscriptions don't fire with emitEvent: false)
          classesArray.controls.forEach((group) => {
            const tipoClasse = group.get('tipoClasseFidc')?.value;
            // Apply defaults but skip setting responsabilidadeLimitada since data is restored
            this.applyTipoClasseDefaults(group as FormGroup, tipoClasse, true);
          });
        }
      } else if (isFidc) {
        // RF-02: For FIDC, pre-create SENIOR class and enable multiclasse
        this.form.get('multiclasse')?.setValue(true, { emitEvent: false });
        this.isMulticlasse.set(true);
        classesArray.push(this.createClasseFormGroup(this.getDefaultSeniorClasse(), true), { emitEvent: false });
      }

      // Re-validate the entire FormArray and form
      classesArray.updateValueAndValidity({ emitEvent: false });
      this.form.updateValueAndValidity({ emitEvent: false });

      // Mark all fields as touched
      this.markAllAsTouched();

      untracked(() => this.updateStepValidation());
    });
  }

  /**
   * Handle multiclasse toggle change
   */
  private handleMulticlasseChange(multiclasse: boolean): void {
    const classesArray = this.form.get('classes') as FormArray;
    const isFidc = this.isFidc();

    if (multiclasse && classesArray.length === 0) {
      // Add default class when enabling multiclasse
      if (isFidc) {
        classesArray.push(this.createClasseFormGroup(this.getDefaultSeniorClasse(), true));
      } else {
        classesArray.push(this.createClasseFormGroup(this.getDefaultClasse(), false));
      }
    }
  }

  /**
   * Get today's date in ISO format
   */
  private getTodayISODate(): string {
    return new Date().toISOString().split('T')[0];
  }

  /**
   * Get default SENIOR classe for FIDC
   */
  private getDefaultSeniorClasse(): FundoClasse {
    return {
      ...CLASSE_SENIOR_DEFAULT,
      dataInicio: this.getTodayISODate(),
    };
  }

  /**
   * Get default generic classe
   */
  private getDefaultClasse(): FundoClasse {
    return {
      ...CLASSE_DEFAULT,
      dataInicio: this.getTodayISODate(),
    };
  }

  /**
   * Create a FormGroup for a single classe
   */
  private createClasseFormGroup(classe?: Partial<FundoClasse>, isFidc?: boolean): FormGroup {
    const group = this.formBuilder.group({
      codigoClasse: [classe?.codigoClasse ?? '', [Validators.required, Validators.maxLength(20)]],
      nomeClasse: [classe?.nomeClasse ?? '', [Validators.required, Validators.maxLength(100)]],
      cnpjClasse: [classe?.cnpjClasse ?? null, [Validators.pattern(/^\d{14}$/)]],
      publicoAlvo: [classe?.publicoAlvo ?? null, [Validators.required]],
      tipoClasseFidc: [classe?.tipoClasseFidc ?? null],
      ordemSubordinacao: [classe?.ordemSubordinacao ?? null],
      rentabilidadeAlvo: [classe?.rentabilidadeAlvo ?? null, [Validators.min(0), Validators.max(100)]],
      valorMinimoAplicacao: [classe?.valorMinimoAplicacao ?? null, [Validators.min(0)]],
      valorMinimoPermanencia: [classe?.valorMinimoPermanencia ?? null, [Validators.min(0)]],
      responsabilidadeLimitada: [classe?.responsabilidadeLimitada ?? true],
      segregacaoPatrimonial: [classe?.segregacaoPatrimonial ?? true],
      taxaAdministracao: [
        classe?.taxaAdministracao ?? null,
        [Validators.min(0), Validators.max(LIMITES_TAXA_CLASSE.taxaAdministracao)],
      ],
      taxaGestao: [classe?.taxaGestao ?? null, [Validators.min(0), Validators.max(LIMITES_TAXA_CLASSE.taxaGestao)]],
      taxaPerformance: [
        classe?.taxaPerformance ?? null,
        [Validators.min(0), Validators.max(LIMITES_TAXA_CLASSE.taxaPerformance)],
      ],
      permiteResgateAntecipado: [classe?.permiteResgateAntecipado ?? false],
      dataInicio: [classe?.dataInicio ?? null, [Validators.required]],
      ativo: [classe?.ativo ?? true],
    });

    // Setup conditional validators for FIDC (RF-04)
    if (isFidc ?? this.isFidc()) {
      this.setupFidcValidators(group);
    }

    // Setup tipoClasseFidc change handler for defaults (RF-06)
    this.setupTipoClasseChangeHandler(group);

    return group;
  }

  /**
   * Setup FIDC-specific validators (RF-04)
   */
  private setupFidcValidators(group: FormGroup): void {
    group.get('tipoClasseFidc')?.setValidators([Validators.required]);
    group.get('ordemSubordinacao')?.setValidators([Validators.required, Validators.min(1)]);
    group.get('tipoClasseFidc')?.updateValueAndValidity({ emitEvent: false });
    group.get('ordemSubordinacao')?.updateValueAndValidity({ emitEvent: false });
  }

  /**
   * Setup tipoClasseFidc change handler for automatic defaults (RF-06)
   */
  private setupTipoClasseChangeHandler(group: FormGroup): void {
    group
      .get('tipoClasseFidc')
      ?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((tipoClasse: TipoClasseFidc | null) => {
        this.applyTipoClasseDefaults(group, tipoClasse, false);
      });
  }

  /**
   * Apply defaults based on tipoClasseFidc value
   * Called both from valueChanges subscription and after data restoration
   * @param skipOrdemIfSet When restoring data, skip setting ordemSubordinacao if already set
   */
  private applyTipoClasseDefaults(group: FormGroup, tipoClasse: TipoClasseFidc | null, skipOrdemIfSet: boolean): void {
    if (tipoClasse) {
      // Set default responsabilidade limitada based on tipo (only if not restoring)
      if (!skipOrdemIfSet) {
        const defaultResp = getDefaultResponsabilidadeLimitada(tipoClasse);
        group.get('responsabilidadeLimitada')?.setValue(defaultResp, { emitEvent: false });
      }

      // Set default ordem subordinacao if not already set
      if (!group.get('ordemSubordinacao')?.value) {
        const defaultOrdem = getDefaultOrdemSubordinacao(tipoClasse);
        group.get('ordemSubordinacao')?.setValue(defaultOrdem, { emitEvent: false });
      }
    }

    // Update validity
    group.get('ordemSubordinacao')?.updateValueAndValidity({ emitEvent: false });
    group.get('responsabilidadeLimitada')?.updateValueAndValidity({ emitEvent: false });
  }

  /**
   * Add a new classe to the list (RF-03)
   */
  addClasse(): void {
    if (!this.canAddClasse()) return;

    const classesArray = this.form.get('classes') as FormArray;
    const isFidc = this.isFidc();

    const newClasse: Partial<FundoClasse> = {
      ...CLASSE_DEFAULT,
      dataInicio: this.getTodayISODate(),
    };

    classesArray.push(this.createClasseFormGroup(newClasse, isFidc));
  }

  /**
   * Remove a classe from the list (RF-03)
   * For FIDC, cannot remove the SENIOR class if it's the only one
   */
  removeClasse(index: number): void {
    const classesArray = this.form.get('classes') as FormArray;

    if (!this.canRemoveClasse(index)) return;

    classesArray.removeAt(index);
  }

  /**
   * Check if classe can be removed
   * - For FIDC: cannot remove SENIOR if it's the only class
   * - Always need at least 1 class when multiclasse is enabled
   */
  canRemoveClasse(index: number): boolean {
    const classesArray = this.form.get('classes') as FormArray;

    // Always need at least 1 class
    if (classesArray.length <= 1) return false;

    // For FIDC, check if trying to remove the SENIOR class
    if (this.isFidc()) {
      const classe = classesArray.at(index);
      const tipoClasse = classe.get('tipoClasseFidc')?.value;

      // Check if this is the only SENIOR class
      if (tipoClasse === TipoClasseFidc.SENIOR) {
        const seniorCount = classesArray.controls.filter(
          (ctrl) => ctrl.get('tipoClasseFidc')?.value === TipoClasseFidc.SENIOR
        ).length;
        if (seniorCount <= 1) return false;
      }
    }

    return true;
  }

  /**
   * Check if classe is SENIOR type (for locked icon display)
   */
  isSeniorClasse(index: number): boolean {
    const classesArray = this.form.get('classes') as FormArray;
    const classe = classesArray.at(index);
    return classe.get('tipoClasseFidc')?.value === TipoClasseFidc.SENIOR;
  }

  /**
   * Get tipo classe label
   */
  getTipoClasseLabel(tipoClasse: TipoClasseFidc | null): string {
    if (!tipoClasse) return '';
    const option = TIPO_CLASSE_FIDC_OPTIONS.find((opt) => opt.value === tipoClasse);
    return option?.label ?? tipoClasse;
  }

  /**
   * Get publico alvo label
   */
  getPublicoAlvoLabel(publicoAlvo: PublicoAlvo | null): string {
    if (!publicoAlvo) return '';
    const option = PUBLICO_ALVO_OPTIONS.find((opt) => opt.value === publicoAlvo);
    return option?.label ?? publicoAlvo;
  }

  private prepareDataForStore(formValue: any): ClassesFormData {
    return {
      multiclasse: formValue.multiclasse ?? false,
      classes: (formValue.classes || []).map((classe: any) => ({
        codigoClasse: classe.codigoClasse ?? '',
        nomeClasse: classe.nomeClasse ?? '',
        cnpjClasse: classe.cnpjClasse ?? null,
        classePaiId: null,
        nivel: 1,
        publicoAlvo: classe.publicoAlvo ?? null,
        tipoClasseFidc: classe.tipoClasseFidc ?? null,
        ordemSubordinacao: classe.ordemSubordinacao ?? null,
        rentabilidadeAlvo: classe.rentabilidadeAlvo ?? null,
        indiceSubordinacaoMinimo: null,
        valorMinimoAplicacao: classe.valorMinimoAplicacao ?? null,
        valorMinimoPermanencia: classe.valorMinimoPermanencia ?? null,
        responsabilidadeLimitada: classe.responsabilidadeLimitada ?? true,
        segregacaoPatrimonial: classe.segregacaoPatrimonial ?? true,
        taxaAdministracao: classe.taxaAdministracao ?? null,
        taxaGestao: classe.taxaGestao ?? null,
        taxaPerformance: classe.taxaPerformance ?? null,
        benchmarkId: null,
        permiteResgateAntecipado: classe.permiteResgateAntecipado ?? false,
        dataInicio: classe.dataInicio ?? null,
        dataEncerramento: null,
        motivoEncerramento: null,
        ativo: classe.ativo ?? true,
      })),
    };
  }

  private markAllAsTouched(): void {
    const classesArray = this.form.get('classes') as FormArray;
    classesArray.controls.forEach((group) => {
      Object.keys((group as FormGroup).controls).forEach((key) => {
        group.get(key)?.markAsTouched();
      });
    });
  }

  private updateStepValidation(): void {
    const stepId = this.stepConfig().id;
    const multiclasse = this.form.get('multiclasse')?.value ?? false;
    const classesArray = this.form.get('classes') as FormArray;

    let isValid = false;
    const errors: string[] = [];

    if (!multiclasse && !this.classesObrigatorias()) {
      // No multiclasse and not required = valid
      isValid = true;
    } else {
      // Multiclasse enabled or FIDC = need valid classes
      if (classesArray.length === 0) {
        errors.push('Pelo menos uma classe e obrigatoria');
      } else if (!this.form.valid) {
        // Check specific errors
        if (this.hasOrdemGapError()) {
          errors.push('Ordem de subordinacao deve ser sequencial (1, 2, 3...)');
        }
        if (this.hasCodigoDuplicadoError()) {
          errors.push('Codigos de classe devem ser unicos');
        }
        if (errors.length === 0) {
          errors.push('Preencha todos os campos obrigatorios');
        }
      } else {
        isValid = true;
      }
    }

    this.wizardStore.setStepValidation(stepId, {
      isValid,
      isDirty: this.form.dirty || classesArray.length > 0,
      errors,
    });

    if (isValid && (this.form.dirty || !multiclasse || classesArray.length > 0)) {
      this.wizardStore.markStepComplete(stepId);
    } else {
      this.wizardStore.markStepIncomplete(stepId);
    }
  }

  // Helper methods for template
  isFieldInvalid(index: number, fieldName: string): boolean {
    const classesArray = this.form.get('classes') as FormArray;
    const control = classesArray.at(index)?.get(fieldName);
    return control ? control.touched && control.invalid : false;
  }

  isFieldValid(index: number, fieldName: string): boolean {
    const classesArray = this.form.get('classes') as FormArray;
    const control = classesArray.at(index)?.get(fieldName);
    return control ? control.touched && control.valid : false;
  }

  getFieldError(index: number, fieldName: string, errorKey: string): boolean {
    const classesArray = this.form.get('classes') as FormArray;
    const control = classesArray.at(index)?.get(fieldName);
    return control?.hasError(errorKey) ?? false;
  }

  getClasseGroup(index: number): FormGroup {
    const classesArray = this.form.get('classes') as FormArray;
    return classesArray.at(index) as FormGroup;
  }
}
