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
import {
  NgbDatepickerModule,
  NgbDateStruct,
  NgbDateParserFormatter,
} from '@ng-bootstrap/ng-bootstrap';
import { WizardStepConfig, WizardStepId } from '../../models/wizard.model';
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

  // Track step ID to avoid re-loading
  private lastLoadedStepId: WizardStepId | null = null;

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

    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
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
            | Partial<IdentificacaoFormData>
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

        // Trigger async validation for CNPJ if it has a value
        if (formValue.cnpj) {
          this.form.get('cnpj')?.updateValueAndValidity();
        }
      }

      untracked(() => this.updateStepValidation());
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

    this.wizardStore.setStepValidation(stepId, {
      isValid: this.form.valid && this.form.get('cnpj')?.status !== 'PENDING',
      isDirty: this.form.dirty,
      errors,
    });

    if (this.form.valid && this.form.dirty) {
      this.wizardStore.markStepComplete(stepId);
    } else if (this.form.invalid && this.form.dirty) {
      this.wizardStore.markStepIncomplete(stepId);
    }
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
