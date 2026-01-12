import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbDatepickerModule, NgbDateStruct, NgbDateParserFormatter } from '@ng-bootstrap/ng-bootstrap';
import { IndexadorService } from '../../../../services/indexador';
import { ToastService } from '../../../../services/toast-service';
import {
  CreateIndexador,
  UpdateIndexador,
  TipoIndexador,
  Periodicidade,
  PeriodicidadeOption,
} from '../../../../models/indexador.model';
import { PageHeader } from '../../../../layout/page-header/page-header';
import { NgbDateBRParserFormatter } from '../../../../shared/formatters/ngb-date-br-parser-formatter';

/**
 * Mapping of allowed periodicidades for each tipo (IDX-004 validation rule)
 */
const TIPO_PERIODICIDADE_MAP: Record<TipoIndexador, Periodicidade[]> = {
  [TipoIndexador.Juros]: [Periodicidade.Diaria],
  [TipoIndexador.Inflacao]: [Periodicidade.Mensal],
  [TipoIndexador.Cambio]: [Periodicidade.Diaria],
  [TipoIndexador.IndiceBolsa]: [Periodicidade.Diaria],
  [TipoIndexador.IndiceRendaFixa]: [Periodicidade.Diaria],
  [TipoIndexador.Crypto]: [Periodicidade.Diaria],
  [TipoIndexador.Outro]: [Periodicidade.Diaria, Periodicidade.Mensal, Periodicidade.Anual],
};

@Component({
  selector: 'app-indexador-form',
  imports: [ReactiveFormsModule, NgbDatepickerModule, PageHeader],
  providers: [{ provide: NgbDateParserFormatter, useClass: NgbDateBRParserFormatter }],
  templateUrl: './indexador-form.html',
  styleUrl: './indexador-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IndexadorForm {
  // Signal-based route parameter (replaces ngOnInit snapshot)
  private readonly route = inject(ActivatedRoute);
  private readonly paramId = toSignal(this.route.paramMap.pipe(map((params) => params.get('id'))));

  readonly indexadorId = computed(() => {
    const id = this.paramId();
    return id ? +id : null;
  });

  isSubmitting = signal(false);
  submitStatus = signal<'idle' | 'loading' | 'success' | 'error'>('idle');
  errorMessage = signal<string>('');

  // Signal to track the currently selected tipo
  private selectedTipo = signal<TipoIndexador | null>(null);

  // Computed signal for allowed periodicidade options based on selected tipo
  readonly allowedPeriodicidades = computed<PeriodicidadeOption[]>(() => {
    const tipo = this.selectedTipo();
    if (tipo === null) {
      // No tipo selected - return all options
      return this.indexadorService.periodicidadeOptions;
    }
    const allowedValues = TIPO_PERIODICIDADE_MAP[tipo];
    return this.indexadorService.periodicidadeOptions.filter((opt) =>
      allowedValues.includes(opt.value)
    );
  });

  // Computed signal to check if only one periodicidade is allowed
  readonly hasFixedPeriodicidade = computed(() => {
    const tipo = this.selectedTipo();
    if (tipo === null) return false;
    return TIPO_PERIODICIDADE_MAP[tipo].length === 1;
  });

  private formBuilder = inject(FormBuilder);
  indexadorForm = this.formBuilder.group(
    {
      // Dados Basicos
      codigo: ['', [Validators.required, Validators.maxLength(20)]],
      nome: ['', [Validators.required, Validators.maxLength(200)]],
      // Classificacao
      tipo: [null as TipoIndexador | null, [Validators.required]],
      periodicidade: [null as Periodicidade | null, [Validators.required]],
      fonte: [null as string | null],
      // Fator Acumulado (Optional)
      dataBase: [null as NgbDateStruct | null],
      fatorAcumulado: [null as number | null],
      // Importacao
      importacaoAutomatica: [false],
      urlFonte: ['', [Validators.maxLength(500)]],
      // Status
      ativo: [true],
    },
    {
      validators: [this.tipoPeriodicidadeValidator],
    }
  );

  indexadorService = inject(IndexadorService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  /**
   * Cross-field validator for tipo + periodicidade compatibility (IDX-004)
   */
  private tipoPeriodicidadeValidator(control: AbstractControl): ValidationErrors | null {
    const tipo = control.get('tipo')?.value as TipoIndexador | null;
    const periodicidade = control.get('periodicidade')?.value as Periodicidade | null;

    if (tipo === null || periodicidade === null) {
      return null; // Let required validators handle missing values
    }

    const allowedPeriodicidades = TIPO_PERIODICIDADE_MAP[tipo];
    if (!allowedPeriodicidades.includes(periodicidade)) {
      return { tipoPeriodicidadeIncompatible: true };
    }

    return null;
  }

  constructor() {
    // React to route parameter changes
    effect(() => {
      const id = this.indexadorId();
      if (id) {
        this.loadIndexador(id);
        // Disable fields that cannot be changed in edit mode
        this.indexadorForm.get('codigo')?.disable();
        this.indexadorForm.get('tipo')?.disable();
        this.indexadorForm.get('periodicidade')?.disable();
      }
    });

    // Setup tipo change handler for auto-selecting periodicidade (IDX-004)
    this.indexadorForm.get('tipo')?.valueChanges.subscribe((tipo) => {
      this.selectedTipo.set(tipo);

      if (tipo !== null) {
        const allowedPeriodicidades = TIPO_PERIODICIDADE_MAP[tipo];
        const currentPeriodicidade = this.indexadorForm.get('periodicidade')?.value;

        // If only one periodicidade is allowed, auto-select it
        if (allowedPeriodicidades.length === 1) {
          this.indexadorForm.get('periodicidade')?.setValue(allowedPeriodicidades[0]);
        }
        // If current periodicidade is not allowed, reset it
        else if (
          currentPeriodicidade !== null &&
          currentPeriodicidade !== undefined &&
          !allowedPeriodicidades.includes(currentPeriodicidade)
        ) {
          this.indexadorForm.get('periodicidade')?.setValue(null);
        }
      }
    });

    // Setup conditional validation for urlFonte
    this.indexadorForm.get('importacaoAutomatica')?.valueChanges.subscribe((value) => {
      const urlFonteControl = this.indexadorForm.get('urlFonte');
      if (value) {
        urlFonteControl?.setValidators([Validators.required, Validators.maxLength(500)]);
      } else {
        urlFonteControl?.setValidators([Validators.maxLength(500)]);
      }
      urlFonteControl?.updateValueAndValidity();
    });
  }

  loadIndexador(id: number): void {
    this.indexadorService.getIndexadorById(id).subscribe({
      next: (indexador) => {
        // Set the selectedTipo signal for computed periodicidade options
        this.selectedTipo.set(indexador.tipo);

        this.indexadorForm.patchValue({
          codigo: indexador.codigo,
          nome: indexador.nome,
          tipo: indexador.tipo,
          periodicidade: indexador.periodicidade,
          fonte: indexador.fonte,
          dataBase: indexador.dataBase ? this.isoStringToNgbDate(indexador.dataBase) : null,
          fatorAcumulado: indexador.fatorAcumulado,
          importacaoAutomatica: indexador.importacaoAutomatica,
          urlFonte: indexador.urlFonte || '',
          ativo: indexador.ativo,
        });
      },
      error: () => {
        this.toastService.error('Falha ao carregar indexador');
        this.router.navigate(['/cadastro/indexadores']);
      },
    });
  }

  onSubmit(): void {
    if (this.isSubmitting()) return;

    this.indexadorForm.markAllAsTouched();
    if (this.indexadorForm.invalid) return;

    this.isSubmitting.set(true);
    this.submitStatus.set('loading');

    const formValue = this.indexadorForm.getRawValue();

    const id = this.indexadorId();
    if (id) {
      // Update existing indexador
      const dto: UpdateIndexador = {
        nome: formValue.nome!,
        fonte: formValue.fonte,
        fatorAcumulado: formValue.fatorAcumulado,
        dataBase: formValue.dataBase ? this.ngbDateToISOString(formValue.dataBase) : null,
        urlFonte: formValue.urlFonte || null,
        importacaoAutomatica: formValue.importacaoAutomatica ?? false,
        ativo: formValue.ativo ?? true,
      };

      this.indexadorService.updateIndexador(id, dto).subscribe({
        next: () => {
          this.submitStatus.set('success');
          this.toastService.success('Indexador atualizado com sucesso');
          setTimeout(() => {
            this.router.navigate(['/cadastro/indexadores']);
          }, 1500);
        },
        error: (error) => {
          this.isSubmitting.set(false);
          this.submitStatus.set('error');
          this.errorMessage.set(error.error?.message || 'Falha ao atualizar indexador');
        },
      });
    } else {
      // Create new indexador
      const dto: CreateIndexador = {
        codigo: formValue.codigo!,
        nome: formValue.nome!,
        tipo: formValue.tipo!,
        periodicidade: formValue.periodicidade!,
        fonte: formValue.fonte,
        fatorAcumulado: formValue.fatorAcumulado,
        dataBase: formValue.dataBase ? this.ngbDateToISOString(formValue.dataBase) : null,
        urlFonte: formValue.urlFonte || null,
        importacaoAutomatica: formValue.importacaoAutomatica ?? false,
        ativo: formValue.ativo ?? true,
      };

      this.indexadorService.createIndexador(dto).subscribe({
        next: () => {
          this.submitStatus.set('success');
          this.toastService.success('Indexador criado com sucesso');
          setTimeout(() => {
            this.router.navigate(['/cadastro/indexadores']);
          }, 1500);
        },
        error: (error) => {
          this.isSubmitting.set(false);
          this.submitStatus.set('error');
          this.errorMessage.set(error.error?.message || 'Falha ao criar indexador');
        },
      });
    }
  }

  private isoStringToNgbDate(isoString: string): NgbDateStruct {
    const datePart = isoString.split('T')[0];
    const [year, month, day] = datePart.split('-').map(Number);
    return { year, month, day };
  }

  private ngbDateToISOString(date: NgbDateStruct): string {
    const month = String(date.month).padStart(2, '0');
    const day = String(date.day).padStart(2, '0');
    return `${date.year}-${month}-${day}T00:00:00Z`;
  }

  onCancel(): void {
    this.router.navigate(['/cadastro/indexadores']);
  }

  retrySubmit(): void {
    this.submitStatus.set('idle');
    this.errorMessage.set('');
    this.onSubmit();
  }

  clearForm(): void {
    this.indexadorForm.reset({
      importacaoAutomatica: false,
      ativo: true,
    });
    this.indexadorForm.markAsUntouched();
    this.indexadorForm.markAsPristine();
    this.submitStatus.set('idle');
    this.errorMessage.set('');
  }

  getControl(name: string) {
    return this.indexadorForm.get(name)!;
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
    return this.getControl(name).errors?.[error] || false;
  }

  /**
   * Check if the form has the tipo/periodicidade incompatibility error
   */
  get hasTipoPeriodicidadeError(): boolean {
    return this.indexadorForm.errors?.['tipoPeriodicidadeIncompatible'] === true;
  }

  get showUrlFonte(): boolean {
    return this.indexadorForm.get('importacaoAutomatica')?.value === true;
  }
}
