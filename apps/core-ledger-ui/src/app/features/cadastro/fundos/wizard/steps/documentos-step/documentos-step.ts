import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  input,
  DestroyRef,
  untracked,
  signal,
  computed,
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
  DocumentoFundo,
  TipoDocumento,
  TIPO_DOCUMENTO_OPTIONS,
  NovoDocumentoFormData,
  formatFileSize,
  getTipoDocumentoOption,
  isValidFileType,
  isValidFileSize,
  isValidFileExtension,
  ALLOWED_FILE_EXTENSIONS,
  MAX_FILE_SIZE,
} from '../../models/documentos.model';
import { WizardStore } from '../../wizard-store';
import { NgbDateBRParserFormatter } from '../../../../../../shared/formatters/ngb-date-br-parser-formatter';

/**
 * Componente para Etapa 10 do wizard: Upload de Documentos
 * Upload de documentos do fundo (regulamento, lâmina, etc.)
 * Esta etapa é OPCIONAL - não requer documentos para avançar
 */
@Component({
  selector: 'app-documentos-step',
  imports: [ReactiveFormsModule, NgbDatepickerModule],
  providers: [{ provide: NgbDateParserFormatter, useClass: NgbDateBRParserFormatter }],
  templateUrl: './documentos-step.html',
  styleUrl: './documentos-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DocumentosStep {
  stepConfig = input.required<WizardStepConfig>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly wizardStore = inject(WizardStore);
  private readonly destroyRef = inject(DestroyRef);

  // Enum options for template
  readonly tipoDocumentoOptions = TIPO_DOCUMENTO_OPTIONS;

  // Track step ID to avoid re-loading
  private lastLoadedStepId: WizardStepId | null = null;

  // Signal for uploaded documents
  readonly documentos = signal<DocumentoFundo[]>([]);

  // Signal for drag-over state
  readonly isDragOver = signal(false);

  // Signal for upload in progress
  readonly isUploading = signal(false);

  // Signal for error messages
  readonly errorMessage = signal<string | null>(null);

  // Computed: check if REGULAMENTO is missing
  readonly missingRegulamento = computed(() => {
    const docs = this.documentos();
    return !docs.some((doc) => doc.tipoDocumento === TipoDocumento.REGULAMENTO);
  });

  // Computed: document count
  readonly documentCount = computed(() => this.documentos().length);

  // Form for new document
  form = this.formBuilder.group({
    tipoDocumento: [null as TipoDocumento | null, [Validators.required]],
    dataVigencia: [null as NgbDateStruct | null, [Validators.required]],
    dataFimVigencia: [null as NgbDateStruct | null],
    observacoes: ['', [Validators.maxLength(500)]],
  });

  constructor() {
    // Setup form subscriptions
    this.form.statusChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.updateStepValidation());

    // Effect to load data when step changes
    effect(() => {
      const stepConfig = this.stepConfig();
      const stepId = stepConfig.id;

      if (this.lastLoadedStepId === stepId) {
        return;
      }
      this.lastLoadedStepId = stepId;

      // Load documentos from store
      const stepData = untracked(
        () => this.wizardStore.stepData()[stepConfig.key] as DocumentoFundo[] | undefined
      );

      if (stepData && Array.isArray(stepData)) {
        this.documentos.set(stepData);
      } else {
        this.documentos.set([]);
      }

      untracked(() => this.updateStepValidation());
    });

    // Watch documentos changes and update store
    effect(() => {
      const docs = this.documentos();
      const stepConfig = untracked(() => this.stepConfig());
      this.wizardStore.setStepData(stepConfig.key, docs);
      untracked(() => this.updateStepValidation());
    });
  }

  // Drag and drop handlers
  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(false);

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFileSelect(Array.from(files));
    }
  }

  onFileInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFileSelect(Array.from(input.files));
    }
  }

  private handleFileSelect(files: File[]): void {
    this.errorMessage.set(null);

    // Validate form first
    if (this.form.invalid) {
      this.errorMessage.set('Preencha o tipo e a data de vigência antes de anexar o arquivo');
      // Mark all fields as touched to show validation errors
      Object.keys(this.form.controls).forEach((key) => {
        this.form.get(key)?.markAsTouched();
      });
      return;
    }

    // Validate files
    for (const file of files) {
      if (!isValidFileExtension(file.name)) {
        this.errorMessage.set(
          `Arquivo "${file.name}" tem extensão inválida. Permitido: ${ALLOWED_FILE_EXTENSIONS.join(', ')}`
        );
        continue;
      }

      if (!isValidFileType(file)) {
        this.errorMessage.set(
          `Arquivo "${file.name}" tem tipo inválido. Apenas PDF, DOC e DOCX são permitidos.`
        );
        continue;
      }

      if (!isValidFileSize(file)) {
        this.errorMessage.set(
          `Arquivo "${file.name}" excede o tamanho máximo de ${formatFileSize(MAX_FILE_SIZE)}`
        );
        continue;
      }

      // Add document
      this.addDocumento(file);
    }
  }

  private addDocumento(file: File): void {
    const formValue = this.form.value;
    const tipoDocumento = formValue.tipoDocumento!;

    // Calculate next version for this document type
    const existingDocs = this.documentos().filter((d) => d.tipoDocumento === tipoDocumento);
    const nextVersion = existingDocs.length + 1;

    const documento: DocumentoFundo = {
      tempId: crypto.randomUUID(),
      tipoDocumento,
      versao: nextVersion,
      dataVigencia: formValue.dataVigencia
        ? this.ngbDateToIsoString(formValue.dataVigencia)
        : '',
      dataFimVigencia: formValue.dataFimVigencia
        ? this.ngbDateToIsoString(formValue.dataFimVigencia)
        : null,
      observacoes: formValue.observacoes || null,
      arquivoNome: file.name,
      arquivoTamanho: file.size,
      arquivoTipo: file.type,
      arquivoConteudo: file,
      uploadStatus: 'uploaded', // Mock: assume uploaded (no backend yet)
      createdAt: new Date().toISOString(),
    };

    // Add to list
    this.documentos.update((docs) => [...docs, documento]);

    // Reset form (keep tipo_documento selected for convenience)
    this.form.patchValue({
      dataVigencia: null,
      dataFimVigencia: null,
      observacoes: '',
    });
    this.form.markAsPristine();
  }

  removeDocumento(tempId: string): void {
    this.documentos.update((docs) => docs.filter((d) => d.tempId !== tempId));
  }

  // Helper: format file size for display
  formatSize(bytes: number): string {
    return formatFileSize(bytes);
  }

  // Helper: get document type option
  getDocumentoOption(tipo: TipoDocumento) {
    return getTipoDocumentoOption(tipo);
  }

  // Helper: format date for display (DD/MM/YYYY)
  formatDate(isoDate: string): string {
    if (!isoDate) return '';
    const [year, month, day] = isoDate.split('-');
    return `${day}/${month}/${year}`;
  }

  private ngbDateToIsoString(date: NgbDateStruct): string {
    const month = String(date.month).padStart(2, '0');
    const day = String(date.day).padStart(2, '0');
    return `${date.year}-${month}-${day}`;
  }

  private updateStepValidation(): void {
    const stepId = this.stepConfig().id;
    const errors: string[] = [];

    // This step is always valid (optional step)
    // But we can mark as dirty if documents were added
    const isDirty = this.documentos().length > 0;

    this.wizardStore.setStepValidation(stepId, {
      isValid: true,
      isDirty,
      errors,
    });

    // Mark step complete if any documents added (but not required)
    if (isDirty) {
      this.wizardStore.markStepComplete(stepId);
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
}
