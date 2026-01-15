import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  input,
  DestroyRef,
  untracked,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs/operators';
import {
  NgbDatepickerModule,
  NgbDateStruct,
  NgbDateParserFormatter,
} from '@ng-bootstrap/ng-bootstrap';
import { WizardStepConfig, WizardStepId, InvalidFieldInfo } from '../../models/wizard.model';
import {
  IdentificacaoFormData,
  TipoFundo,
  TIPO_FUNDO_OPTIONS,
} from '../../models/identificacao.model';
import { WizardStore } from '../../wizard-store';
import { cnpjValidator } from '../../../../../../shared/validators/cnpj.validator';
import { CnpjUniqueValidatorService } from '../../../../../../shared/validators/cnpj-unique.validator';
import { CnpjMaskDirective } from '../../../../../../directives/cnpj-mask.directive';
import { NgbDateBRParserFormatter } from '../../../../../../shared/formatters/ngb-date-br-parser-formatter';

/**
 * Componente para Etapa 1 do wizard: Identificacao do Fundo
 * Captura dados basicos de identificacao fiscal e registro do fundo
 */
@Component({
  selector: 'app-identificacao-step',
  imports: [ReactiveFormsModule, NgbDatepickerModule, CnpjMaskDirective],
  providers: [{ provide: NgbDateParserFormatter, useClass: NgbDateBRParserFormatter }],
  templateUrl: './identificacao-step.html',
  styleUrl: './identificacao-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IdentificacaoStep {
  stepConfig = input.required<WizardStepConfig>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly wizardStore = inject(WizardStore);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cnpjUniqueValidator = inject(CnpjUniqueValidatorService);

  // Enum options for template
  readonly tipoFundoOptions = TIPO_FUNDO_OPTIONS;

  // Track step ID and dataVersion to avoid re-loading unless store data changes
  private lastLoadedStepId: WizardStepId | null = null;
  private lastDataVersion = -1;

  // Loading flag to prevent store updates during restoration
  private isRestoring = false;

  // Signal for CNPJ validation status
  readonly cnpjValidating = signal(false);

  form = this.formBuilder.group(
    {
      cnpj: [
        '',
        {
          validators: [Validators.required, cnpjValidator()],
          asyncValidators: [this.cnpjUniqueValidator.validate()],
        },
      ],
      razaoSocial: [
        '',
        [Validators.required, Validators.minLength(10), Validators.maxLength(200)],
      ],
      nomeFantasia: [
        '',
        [Validators.required, Validators.minLength(3), Validators.maxLength(100)],
      ],
      nomeCurto: ['', [Validators.maxLength(50)]],
      tipoFundo: [null as TipoFundo | null, [Validators.required]],
      dataConstituicao: [null as NgbDateStruct | null, [Validators.required]],
      dataInicioAtividade: [null as NgbDateStruct | null, [Validators.required]],
    },
    {
      validators: [this.dateRangeValidator.bind(this)],
    }
  );

  constructor() {
    // Track CNPJ validation pending state
    this.form
      .get('cnpj')!
      .statusChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((status) => {
        this.cnpjValidating.set(status === 'PENDING');
      });

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
        // Convert NgbDateStruct to ISO strings for storage
        const dataForStore = this.prepareDataForStore(value);
        this.wizardStore.setStepData(stepConfig.key, dataForStore);
      });

    // Auto-fill nome_curto prefix when tipo_fundo changes
    this.form
      .get('tipoFundo')!
      .valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((tipoFundo) => {
        if (tipoFundo) {
          const option = TIPO_FUNDO_OPTIONS.find((o) => o.value === tipoFundo);
          const currentNomeCurto = this.form.get('nomeCurto')?.value;

          // Only suggest if nome_curto is empty or starts with a previous prefix
          if (!currentNomeCurto || this.isExistingPrefix(currentNomeCurto)) {
            this.form.get('nomeCurto')?.setValue(option?.prefixoNomeCurto ?? '');
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
            | Partial<IdentificacaoFormData>
            | undefined
      );

      // Check if step was already validated (user navigating back or restoring draft)
      // We check completedSteps because stepValidation is not persisted in drafts
      const completedSteps = untracked(() => this.wizardStore.completedSteps());
      const wasAlreadyCompleted = completedSteps.has(stepId);
      const hasStepData = !!stepData;

      if (stepData) {
        const formValue = this.prepareDataForForm(stepData);

        // Patch form without emitting events during restoration
        // Following wizard_research.md pattern: avoid triggering watchers during restore
        this.form.patchValue(formValue, { emitEvent: false });
        this.form.markAsDirty();

        // Mark all fields as touched to show validation state
        Object.keys(this.form.controls).forEach((key) => {
          this.form.get(key)?.markAsTouched();
        });

        // If step was already completed (from draft or previous navigation), bypass async validation
        // Per Angular docs: clearAsyncValidators() + updateValueAndValidity() removes async validators
        if (wasAlreadyCompleted && hasStepData) {
          const cnpjControl = this.form.get('cnpj')!;
          const originalAsyncValidators = cnpjControl.asyncValidator;

          // Step 1: Clear async validators
          cnpjControl.clearAsyncValidators();

          // Step 2: Apply the validator removal by calling updateValueAndValidity on the control
          // Per Angular docs: "you must call updateValueAndValidity() for the new validation to take effect"
          cnpjControl.updateValueAndValidity({ emitEvent: false });

          // Step 3: Now update the entire form (sync validators only since async are cleared)
          this.form.updateValueAndValidity({ emitEvent: false });

          // Step 4: Restore async validators for future user interactions (when they edit the field)
          if (originalAsyncValidators) {
            cnpjControl.setAsyncValidators(originalAsyncValidators);
            // IMPORTANT: Don't call updateValueAndValidity() after restoring!
            // This keeps the control VALID without triggering async validators
          }

          // Directly set the store validation state
          this.wizardStore.setStepValidation(stepId, {
            isValid: this.form.valid,
            isDirty: true,
            errors: [],
            invalidFields: [],
          });

          if (this.form.valid) {
            this.wizardStore.markStepComplete(stepId);
          }
        } else {
          // Fresh load or incomplete data - run all validators normally
          this.form.updateValueAndValidity();
          // updateStepValidation will be called by statusChanges subscription
        }
      } else {
        // No data to restore - run validators
        this.form.updateValueAndValidity();
      }

      // Clear restoration flag
      this.isRestoring = false;
    });
  }

  /**
   * Cross-field validator: dataInicioAtividade >= dataConstituicao
   * and dataConstituicao <= today
   */
  private dateRangeValidator(control: any): { [key: string]: any } | null {
    const dataConstituicao = control.get('dataConstituicao')?.value as NgbDateStruct | null;
    const dataInicioAtividade = control.get('dataInicioAtividade')?.value as NgbDateStruct | null;

    const errors: { [key: string]: any } = {};

    if (dataConstituicao) {
      const constituicaoDate = new Date(
        dataConstituicao.year,
        dataConstituicao.month - 1,
        dataConstituicao.day
      );
      const today = new Date();
      today.setHours(0, 0, 0, 0);

      if (constituicaoDate > today) {
        errors['dataConstituicaoFutura'] = {
          message: 'Data de constituicao nao pode ser futura',
        };
      }
    }

    if (dataConstituicao && dataInicioAtividade) {
      const constituicaoDate = new Date(
        dataConstituicao.year,
        dataConstituicao.month - 1,
        dataConstituicao.day
      );
      const inicioDate = new Date(
        dataInicioAtividade.year,
        dataInicioAtividade.month - 1,
        dataInicioAtividade.day
      );

      if (inicioDate < constituicaoDate) {
        errors['dataInicioAnterior'] = {
          message: 'Data de inicio de atividade deve ser maior ou igual a data de constituicao',
        };
      }
    }

    return Object.keys(errors).length > 0 ? errors : null;
  }

  private isExistingPrefix(value: string): boolean {
    return TIPO_FUNDO_OPTIONS.some((opt) => value === opt.prefixoNomeCurto);
  }

  private prepareDataForStore(formValue: any): IdentificacaoFormData {
    return {
      cnpj: formValue.cnpj ?? '',
      razaoSocial: formValue.razaoSocial ?? '',
      nomeFantasia: formValue.nomeFantasia ?? '',
      nomeCurto: formValue.nomeCurto ?? null,
      tipoFundo: formValue.tipoFundo ?? null,
      dataConstituicao: formValue.dataConstituicao
        ? this.ngbDateToIsoString(formValue.dataConstituicao)
        : '',
      dataInicioAtividade: formValue.dataInicioAtividade
        ? this.ngbDateToIsoString(formValue.dataInicioAtividade)
        : '',
    };
  }

  private prepareDataForForm(data: Partial<IdentificacaoFormData>): any {
    return {
      cnpj: data.cnpj ?? '',
      razaoSocial: data.razaoSocial ?? '',
      nomeFantasia: data.nomeFantasia ?? '',
      nomeCurto: data.nomeCurto ?? '',
      tipoFundo: data.tipoFundo ?? null,
      dataConstituicao: data.dataConstituicao
        ? this.isoStringToNgbDate(data.dataConstituicao)
        : null,
      dataInicioAtividade: data.dataInicioAtividade
        ? this.isoStringToNgbDate(data.dataInicioAtividade)
        : null,
    };
  }

  private ngbDateToIsoString(date: NgbDateStruct): string {
    const month = String(date.month).padStart(2, '0');
    const day = String(date.day).padStart(2, '0');
    return `${date.year}-${month}-${day}`;
  }

  private isoStringToNgbDate(isoString: string): NgbDateStruct {
    const [year, month, day] = isoString.split('-').map(Number);
    return { year, month, day };
  }

  private updateStepValidation(): void {
    const stepId = this.stepConfig().id;
    const errors: string[] = [];

    // Collect form-level errors
    if (this.form.errors) {
      if (this.form.errors['dataConstituicaoFutura']) {
        errors.push(this.form.errors['dataConstituicaoFutura'].message);
      }
      if (this.form.errors['dataInicioAnterior']) {
        errors.push(this.form.errors['dataInicioAnterior'].message);
      }
    }

    // Collect field-level errors
    const cnpjErrors = this.form.get('cnpj')?.errors;
    if (cnpjErrors?.['cnpjDuplicate']) {
      errors.push(cnpjErrors['cnpjDuplicate'].message);
    }

    // Collect invalid fields for debug
    const invalidFields = this.collectInvalidFields();

    this.wizardStore.setStepValidation(stepId, {
      isValid: this.form.valid && this.form.get('cnpj')?.status !== 'PENDING',
      isDirty: this.form.dirty,
      errors,
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
  getControl(name: string) {
    return this.form.get(name)!;
  }

  isInvalid(name: string): boolean {
    const control = this.getControl(name);
    return control.touched && control.invalid;
  }

  isValid(name: string): boolean {
    const control = this.getControl(name);
    return control.touched && control.valid;
  }

  hasError(name: string, error: string): boolean {
    return this.getControl(name).errors?.[error] ?? false;
  }

  get hasDateRangeError(): boolean {
    return this.form.errors?.['dataInicioAnterior'] != null;
  }

  get hasDataConstituicaoFuturaError(): boolean {
    return this.form.errors?.['dataConstituicaoFutura'] != null;
  }
}
