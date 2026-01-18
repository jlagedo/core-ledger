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
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { map, startWith } from 'rxjs/operators';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { WizardStepConfig, InvalidFieldInfo } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';
import {
  createRestorationEffect,
  withRestorationGuard,
  markFormArrayAsTouched,
  collectFormArrayInvalidFields,
} from '../../shared';
import { IdentificacaoFormData, TipoFundo } from '../../models/identificacao.model';
import {
  ClassesFormData,
  CLASSE_DEFAULT,
  CLASSE_SENIOR_DEFAULT,
  ClasseTemplate,
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

// Sub-components
import { ClassOverviewPanel } from './components/class-overview-panel';
import { ClassDetailPanel } from './components/class-detail-panel';
import { FidcWaterfall } from './components/fidc-waterfall';
import { ClassTemplateModal } from './components/class-template-modal';

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
 * Validador customizado: FIDC deve ter pelo menos uma classe SENIOR
 */
function seniorObrigatorioValidator(isFidc: () => boolean): (formArray: AbstractControl) => ValidationErrors | null {
  return (formArray: AbstractControl): ValidationErrors | null => {
    if (!isFidc()) return null;

    const array = formArray as FormArray;
    const hasSenior = array.controls.some((ctrl) => ctrl.get('tipoClasseFidc')?.value === TipoClasseFidc.SENIOR);

    if (!hasSenior && array.length > 0) {
      return { seniorObrigatorio: true };
    }

    return null;
  };
}

/**
 * Componente para Etapa 7 do wizard: Classes CVM 175
 * Redesigned with two-panel layout for better UX
 */
@Component({
  selector: 'app-classes-step',
  imports: [ReactiveFormsModule, ClassOverviewPanel, ClassDetailPanel, FidcWaterfall],
  templateUrl: './classes-step.html',
  styleUrl: './classes-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClassesStep {
  stepConfig = input.required<WizardStepConfig>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly wizardStore = inject(WizardStore);
  private readonly destroyRef = inject(DestroyRef);
  private readonly modalService = inject(NgbModal);

  // Enum options for template
  readonly tipoClasseFidcOptions = TIPO_CLASSE_FIDC_OPTIONS;
  readonly publicoAlvoOptions = PUBLICO_ALVO_OPTIONS;
  readonly maxClasses = MAX_CLASSES;

  // Computed: Check if fund is FIDC type (RF-02)
  // IMPORTANT: Must be defined before the form, as the validator references this.isFidc()
  readonly isFidc = computed(() => {
    const identificacaoData = this.wizardStore.stepData()['identificacao'] as IdentificacaoFormData | undefined;
    const tipoFundo = identificacaoData?.tipoFundo;
    return tipoFundo === TipoFundo.FIDC || tipoFundo === TipoFundo.FIDC_NP;
  });

  // Main form
  form = this.formBuilder.group({
    multiclasse: [false],
    classes: this.formBuilder.array<FormGroup>([], [
      ordemSubordinacaoValidator,
      codigoUnicoValidator,
      seniorObrigatorioValidator(() => this.isFidc()),
    ]),
  });

  // UI State
  readonly selectedClassIndex = signal<number | null>(null);
  readonly isMulticlasse = signal(false);

  // Convert form valueChanges to signal for reactive computed dependencies
  // Fixes bug: computed() doesn't track Reactive Forms values directly
  // See: docs/aidebug/computed-signal-form-values.md
  private readonly classesFormValue = toSignal(
    this.form.valueChanges.pipe(
      startWith(this.form.value),
      map((value) => (value.classes ?? []) as Partial<FundoClasse>[])
    ),
    { initialValue: [] as Partial<FundoClasse>[] }
  );

  // Convert form statusChanges to signal for validation error tracking
  private readonly formStatus = toSignal(
    this.form.statusChanges.pipe(startWith(this.form.status)),
    { initialValue: this.form.status }
  );

  // Computed signals using the signal for proper reactivity
  readonly classesArray = computed(() => this.form.get('classes') as FormArray);
  readonly classesCount = computed(() => this.classesFormValue().length);
  readonly canAddClasse = computed(() => this.classesCount() < MAX_CLASSES);

  // Computed: Get FormGroup array for sub-components
  // IMPORTANT: Must read classesFormValue() to establish reactive dependency
  // on form changes. Without this, the computed won't update when FormArray
  // is modified (e.g., when applying a template).
  readonly classesFormGroups = computed(() => {
    // Read classesFormValue to trigger reactivity when form changes
    this.classesFormValue();
    return this.classesArray().controls as FormGroup[];
  });

  // Computed: Get selected class FormGroup
  readonly selectedClassFormGroup = computed(() => {
    const index = this.selectedClassIndex();
    if (index === null) return null;
    const groups = this.classesFormGroups();
    return groups[index] ?? null;
  });

  // Computed: Classes are mandatory for FIDC (RF-02)
  readonly classesObrigatorias = computed(() => this.isFidc());

  // Computed: Has at least one class when multiclasse is enabled
  readonly hasMinimumClasses = computed(() => {
    if (!this.isMulticlasse()) return true;
    return this.classesCount() >= 1;
  });

  // Computed: Check for subordination order validation errors
  // Triggers on formStatus changes to ensure reactivity
  readonly hasOrdemGapError = computed(() => {
    // Reading formStatus ensures this computed re-runs on validation changes
    this.formStatus();
    const classesControl = this.form.get('classes');
    return classesControl?.hasError('ordemGap') ?? false;
  });

  // Computed: Check for duplicate codigo error
  readonly hasCodigoDuplicadoError = computed(() => {
    // Reading formStatus ensures this computed re-runs on validation changes
    this.formStatus();
    const classesControl = this.form.get('classes');
    return classesControl?.hasError('codigoDuplicado') ?? false;
  });

  // Computed: Check for senior required error
  readonly hasSeniorObrigatorioError = computed(() => {
    this.formStatus();
    const classesControl = this.form.get('classes');
    return classesControl?.hasError('seniorObrigatorio') ?? false;
  });

  // Computed: Get list of subordination types present
  readonly tiposSubordinacaoPresentes = computed(() => {
    const tipos: TipoClasseFidc[] = [];
    const classes = this.classesFormValue();
    classes.forEach((classe) => {
      const tipo = classe.tipoClasseFidc;
      if (tipo && !tipos.includes(tipo)) {
        tipos.push(tipo);
      }
    });
    return tipos;
  });

  // Computed: Show FIDC waterfall when FIDC and has multiple classes
  readonly showFidcWaterfall = computed(() => {
    return this.isFidc() && this.classesCount() > 0;
  });

  constructor() {
    // Create restoration effect
    const { isRestoring } = createRestorationEffect<ClassesFormData>({
      stepConfig: () => this.stepConfig(),
      wizardStore: this.wizardStore,
      form: this.form,
      resetForm: () => {
        const classesArray = this.form.get('classes') as FormArray;
        classesArray.clear();
        this.selectedClassIndex.set(null);
      },
      restoreData: (data) => {
        const classesArray = this.form.get('classes') as FormArray;
        const isFidc = this.isFidc();

        // Restore saved data - use emitEvent: false to prevent handleMulticlasseChange from triggering
        // and adding default classes before the saved classes are restored
        // See: docs/aidebug/wizard-multiclasse-restoration-defaults.md
        this.form.get('multiclasse')?.setValue(data.multiclasse, { emitEvent: false });
        this.isMulticlasse.set(data.multiclasse);

        if (data.classes && data.classes.length > 0) {
          // Restore saved data - setupTipoClasseChangeHandler runs automatically via startWith
          data.classes.forEach((classe) => {
            classesArray.push(this.createClasseFormGroup(classe, isFidc));
          });
          // Select first class by default
          this.selectedClassIndex.set(0);
        }
      },
      createDefaultData: () => {
        // RF-02: For FIDC, pre-create SENIOR class and enable multiclasse
        if (this.isFidc()) {
          const classesArray = this.form.get('classes') as FormArray;
          // Use emitEvent: false since we're manually adding the class right after
          this.form.get('multiclasse')?.setValue(true, { emitEvent: false });
          this.isMulticlasse.set(true);
          classesArray.push(this.createClasseFormGroup(this.getDefaultSeniorClasse(), true));
          this.selectedClassIndex.set(0);
        }
      },
      markAllAsTouched: () => this.markAllAsTouched(),
      updateStepValidation: () => this.updateStepValidation(),
    });

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
      this.selectedClassIndex.set(0);
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
   * Setup tipoClasseFidc change handler for automatic defaults (RF-06).
   * Uses startWith to apply defaults immediately for initial value.
   */
  private setupTipoClasseChangeHandler(group: FormGroup): void {
    const tipoClasseControl = group.get('tipoClasseFidc');
    tipoClasseControl?.valueChanges
      .pipe(
        startWith(tipoClasseControl.value),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((tipoClasse: TipoClasseFidc | null) => {
        this.applyTipoClasseDefaults(group, tipoClasse);
      });
  }

  /**
   * Apply defaults based on tipoClasseFidc value.
   * Called from valueChanges subscription (including startWith for initial value).
   */
  private applyTipoClasseDefaults(group: FormGroup, tipoClasse: TipoClasseFidc | null): void {
    if (tipoClasse) {
      // Set default responsabilidade limitada based on tipo
      const defaultResp = getDefaultResponsabilidadeLimitada(tipoClasse);
      group.get('responsabilidadeLimitada')?.setValue(defaultResp, { emitEvent: false });

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

  // ============================================================
  // PUBLIC METHODS FOR TEMPLATE/SUB-COMPONENTS
  // ============================================================

  /**
   * Handle class selection from overview panel
   */
  onClassSelected(index: number): void {
    this.selectedClassIndex.set(index);
  }

  /**
   * Handle add class from overview panel
   */
  onAddClass(event: { parentIndex?: number }): void {
    if (!this.canAddClasse()) return;

    const classesArray = this.form.get('classes') as FormArray;
    const isFidc = this.isFidc();

    const newClasse: Partial<FundoClasse> = {
      ...CLASSE_DEFAULT,
      dataInicio: this.getTodayISODate(),
    };

    classesArray.push(this.createClasseFormGroup(newClasse, isFidc));

    // Select the new class
    this.selectedClassIndex.set(classesArray.length - 1);
  }

  /**
   * Handle remove class from overview panel
   */
  onRemoveClass(index: number): void {
    const classesArray = this.form.get('classes') as FormArray;

    if (!this.canRemoveClasse(index)) return;

    classesArray.removeAt(index);

    // Update selection
    const newLength = classesArray.length;
    if (newLength === 0) {
      this.selectedClassIndex.set(null);
    } else if (this.selectedClassIndex() !== null) {
      const currentSelection = this.selectedClassIndex()!;
      if (currentSelection >= newLength) {
        this.selectedClassIndex.set(newLength - 1);
      } else if (currentSelection > index) {
        this.selectedClassIndex.set(currentSelection - 1);
      }
    }
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
   * Open template selection modal
   */
  openTemplateModal(): void {
    const modalRef = this.modalService.open(ClassTemplateModal, {
      centered: true,
      size: 'lg',
      backdrop: 'static',
    });

    // Set the signal value - isFidc is a writable signal on the modal
    modalRef.componentInstance.isFidc.set(this.isFidc());

    modalRef.result.then(
      (template: ClasseTemplate) => {
        this.applyTemplate(template);
      },
      (reason) => {
        if (reason === 'scratch') {
          // User chose to create from scratch - just add a default class
          this.onAddClass({});
        }
        // Otherwise cancelled - do nothing
      }
    );
  }

  /**
   * Apply a template configuration
   */
  private applyTemplate(template: ClasseTemplate): void {
    const classesArray = this.form.get('classes') as FormArray;
    const isFidc = this.isFidc();

    // Clear existing classes
    classesArray.clear();
    this.selectedClassIndex.set(null);

    // Add classes from template
    template.classes.forEach((classeTemplate) => {
      const classe: Partial<FundoClasse> = {
        ...classeTemplate,
        dataInicio: this.getTodayISODate(),
      };
      classesArray.push(this.createClasseFormGroup(classe, isFidc));
    });

    // Select first class
    if (classesArray.length > 0) {
      this.selectedClassIndex.set(0);
    }
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
    markFormArrayAsTouched(classesArray);
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
        if (this.hasSeniorObrigatorioError()) {
          errors.push('FIDC deve ter pelo menos uma classe Senior');
        }
        if (errors.length === 0) {
          errors.push('Preencha todos os campos obrigatorios');
        }
      } else {
        isValid = true;
      }
    }

    const invalidFields = this.collectInvalidFields();

    this.wizardStore.setStepValidation(stepId, {
      isValid,
      isDirty: this.form.dirty || classesArray.length > 0,
      errors,
      invalidFields,
    });

    if (isValid && (this.form.dirty || !multiclasse || classesArray.length > 0)) {
      this.wizardStore.markStepComplete(stepId);
    } else {
      this.wizardStore.markStepIncomplete(stepId);
    }
  }

  private collectInvalidFields(): InvalidFieldInfo[] {
    const classesArray = this.form.get('classes') as FormArray;
    return collectFormArrayInvalidFields(classesArray, 'classes');
  }
}
