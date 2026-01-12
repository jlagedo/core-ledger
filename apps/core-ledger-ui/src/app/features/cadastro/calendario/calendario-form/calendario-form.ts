import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { map } from 'rxjs/operators';
import { NgbDatepickerModule, NgbDateStruct, NgbDateParserFormatter } from '@ng-bootstrap/ng-bootstrap';
import { CalendarioService } from '../../../../services/calendario';
import { ToastService } from '../../../../services/toast-service';
import { CreateCalendario, TipoDia, UpdateCalendario } from '../../../../models/calendario.model';
import { PageHeader } from '../../../../layout/page-header/page-header';
import { NgbDateBRParserFormatter } from '../../../../shared/formatters/ngb-date-br-parser-formatter';

@Component({
  selector: 'app-calendario-form',
  imports: [ReactiveFormsModule, NgbDatepickerModule, PageHeader],
  providers: [{ provide: NgbDateParserFormatter, useClass: NgbDateBRParserFormatter }],
  templateUrl: './calendario-form.html',
  styleUrl: './calendario-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CalendarioForm {
  // Signal-based route parameter (replaces ngOnInit snapshot)
  private readonly route = inject(ActivatedRoute);
  private readonly paramId = toSignal(this.route.paramMap.pipe(map((params) => params.get('id'))));

  readonly calendarioId = computed(() => {
    const id = this.paramId();
    return id ? +id : null;
  });

  isSubmitting = signal(false);
  submitStatus = signal<'idle' | 'loading' | 'success' | 'error'>('idle');
  errorMessage = signal<string>('');

  private formBuilder = inject(FormBuilder);
  calendarioForm = this.formBuilder.group({
    data: [null as NgbDateStruct | null, [Validators.required]],
    tipoDia: [null as number | null, [Validators.required]],
    praca: [null as number | null, [Validators.required]],
    descricao: ['', [Validators.maxLength(100)]],
  });

  calendarioService = inject(CalendarioService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  constructor() {
    // React to route parameter changes
    effect(() => {
      const id = this.calendarioId();
      if (id) {
        this.loadCalendario(id);
        // Disable data and praca for edit mode (per API - UpdateCalendarioDto only has tipoDia and descricao)
        this.calendarioForm.get('data')?.disable();
        this.calendarioForm.get('praca')?.disable();
      }
    });
  }

  loadCalendario(id: number): void {
    this.calendarioService.getCalendarioById(id).subscribe({
      next: (calendario) => {
        this.calendarioForm.patchValue({
          data: this.isoStringToNgbDate(calendario.data),
          tipoDia: calendario.tipoDia,
          praca: calendario.praca,
          descricao: calendario.descricao,
        });
      },
      error: (err) => {
        this.toastService.error('Falha ao carregar calendário');
        this.router.navigate(['/cadastro/calendario']);
      },
    });
  }

  onSubmit(): void {
    if (this.isSubmitting()) return;

    this.calendarioForm.markAllAsTouched();
    if (this.calendarioForm.invalid) return;

    this.isSubmitting.set(true);
    this.submitStatus.set('loading');

    const formValue = this.calendarioForm.getRawValue();

    const id = this.calendarioId();
    if (id) {
      // Update existing calendario
      const dto: UpdateCalendario = {
        tipoDia: formValue.tipoDia! as TipoDia,
        descricao: formValue.descricao || null,
      };

      this.calendarioService.updateCalendario(id, dto).subscribe({
        next: () => {
          this.submitStatus.set('success');
          this.toastService.success('Calendário atualizado com sucesso');
          setTimeout(() => {
            this.router.navigate(['/cadastro/calendario']);
          }, 1500);
        },
        error: (error) => {
          this.isSubmitting.set(false);
          this.submitStatus.set('error');
          this.errorMessage.set(error.error?.message || 'Falha ao atualizar calendário');
        },
      });
    } else {
      // Create new calendario
      const dto: CreateCalendario = {
        data: this.ngbDateToISOString(formValue.data!),
        tipoDia: formValue.tipoDia! as TipoDia,
        praca: formValue.praca!,
        descricao: formValue.descricao || null,
      };

      this.calendarioService.createCalendario(dto).subscribe({
        next: (calendario) => {
          this.submitStatus.set('success');
          this.toastService.success('Calendário criado com sucesso');
          setTimeout(() => {
            this.router.navigate(['/cadastro/calendario']);
          }, 1500);
        },
        error: (error) => {
          this.isSubmitting.set(false);
          this.submitStatus.set('error');
          this.errorMessage.set(error.error?.message || 'Falha ao criar calendário');
        },
      });
    }
  }

  private isoStringToNgbDate(isoString: string): NgbDateStruct {
    const [year, month, day] = isoString.split('-').map(Number);
    return { year, month, day };
  }

  private ngbDateToISOString(date: NgbDateStruct): string {
    const month = String(date.month).padStart(2, '0');
    const day = String(date.day).padStart(2, '0');
    return `${date.year}-${month}-${day}`;
  }

  onCancel(): void {
    this.router.navigate(['/cadastro/calendario']);
  }

  retrySubmit(): void {
    this.submitStatus.set('idle');
    this.errorMessage.set('');
    this.onSubmit();
  }

  clearForm(): void {
    this.calendarioForm.reset();
    this.calendarioForm.markAsUntouched();
    this.calendarioForm.markAsPristine();
    this.submitStatus.set('idle');
    this.errorMessage.set('');
  }

  getControl(name: string) {
    return this.calendarioForm.get(name)!;
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
}
