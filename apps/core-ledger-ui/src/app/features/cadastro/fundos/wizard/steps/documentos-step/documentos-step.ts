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
  OnDestroy,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  NgbDatepickerModule,
  NgbDateStruct,
  NgbDateParserFormatter,
} from '@ng-bootstrap/ng-bootstrap';
import { WizardStepConfig, WizardStepId, InvalidFieldInfo } from '../../models/wizard.model';
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
import { WizardPersistenceService } from '../../services/wizard-persistence.service';
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
export class DocumentosStep implements OnDestroy {
  stepConfig = input.required<WizardStepConfig>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly wizardStore = inject(WizardStore);
  private readonly persistenceService = inject(WizardPersistenceService);
  private readonly destroyRef = inject(DestroyRef);

  // Enum options for template
  readonly tipoDocumentoOptions = TIPO_DOCUMENTO_OPTIONS;

  // Track step ID and dataVersion to avoid re-loading unless store data changes
  private lastLoadedStepId: WizardStepId | null = null;
  private lastDataVersion = -1;

  // Flag to prevent store updates during data restoration
  private isRestoring = false;

  // Track object URLs for cleanup to prevent memory leaks
  private readonly objectUrls: string[] = [];

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

      // Load documentos from store
      const stepData = untracked(
        () => this.wizardStore.stepData()[stepConfig.key] as DocumentoFundo[] | undefined
      );

      if (stepData && Array.isArray(stepData)) {
        this.documentos.set(stepData);
        // Load file blobs from IndexedDB to hydrate arquivoConteudo
        // Use IIFE to properly await async operation before clearing isRestoring
        untracked(() => {
          this.loadFilesFromIndexedDB().finally(() => {
            this.isRestoring = false;
            this.updateStepValidation();
          });
        });
      } else {
        this.documentos.set([]);
        // Clear restoration flag
        this.isRestoring = false;
        untracked(() => this.updateStepValidation());
      }
    });

    // NOTE: Removed the effect that watched documentos() changes.
    // It caused an infinite loop because reading documentos() and calling setStepData()
    // triggered reactive updates. Instead, we now call saveToStore() explicitly
    // from addDocumento() and removeDocumento().
  }

  /**
   * Save current documentos to the wizard store.
   * Called explicitly after adding/removing documents (not via effect).
   */
  private saveToStore(): void {
    if (this.isRestoring) {
      return;
    }
    const docs = this.documentos();
    const stepConfig = this.stepConfig();
    this.wizardStore.setStepData(stepConfig.key, docs);
    this.updateStepValidation();
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

    // Save to wizard store (explicit call instead of effect)
    this.saveToStore();

    // Save file to IndexedDB for persistence
    this.saveFileToIndexedDB(documento, file);

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

    // Save to wizard store (explicit call instead of effect)
    this.saveToStore();

    // Delete file from IndexedDB
    this.deleteFileFromIndexedDB(tempId);
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
    const invalidFields = this.collectInvalidFields();

    this.wizardStore.setStepValidation(stepId, {
      isValid: true,
      isDirty,
      errors,
      invalidFields,
    });

    // Mark step complete if any documents added (but not required)
    if (isDirty) {
      this.wizardStore.markStepComplete(stepId);
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

  // ========== IndexedDB File Persistence ==========

  /**
   * Save a file to IndexedDB for persistence across page refreshes.
   * Called when a new document is added.
   */
  private async saveFileToIndexedDB(documento: DocumentoFundo, file: File): Promise<void> {
    const draftId = this.wizardStore.draftId();
    if (!draftId) {
      console.warn('Cannot save file: no draft ID available');
      return;
    }

    try {
      await this.persistenceService.saveFile(draftId, 'documentos', file, documento.tempId, {
        tipoDocumento: documento.tipoDocumento,
        versao: documento.versao,
        dataVigencia: documento.dataVigencia,
        dataFimVigencia: documento.dataFimVigencia,
        observacoes: documento.observacoes,
      });
    } catch (error) {
      console.error('Failed to save file to IndexedDB:', error);
      this.errorMessage.set('Erro ao salvar arquivo localmente. Tente novamente.');
    }
  }

  /**
   * Load files from IndexedDB and hydrate the documentos signal with file blobs.
   * Called when the step is initialized or after restoring a draft.
   */
  private async loadFilesFromIndexedDB(): Promise<void> {
    const draftId = this.wizardStore.draftId();
    if (!draftId) {
      return;
    }

    try {
      const files = await this.persistenceService.loadFiles(draftId, 'documentos');

      if (files.length === 0) {
        return;
      }

      // Update documentos with file blobs
      this.documentos.update((docs) => {
        return docs.map((doc) => {
          const storedFile = files.find((f) => f.tempId === doc.tempId);
          if (storedFile) {
            // Create a File object from the stored Blob
            const file = new File([storedFile.blob], storedFile.fileName, {
              type: storedFile.fileType,
            });
            // Create object URL for preview
            const url = URL.createObjectURL(storedFile.blob);
            this.objectUrls.push(url);

            return {
              ...doc,
              arquivoConteudo: file,
              arquivoUrl: url,
            };
          }
          return doc;
        });
      });
    } catch (error) {
      console.error('Failed to load files from IndexedDB:', error);
    }
  }

  /**
   * Delete a file from IndexedDB.
   * Called when a document is removed.
   */
  private async deleteFileFromIndexedDB(tempId: string): Promise<void> {
    const draftId = this.wizardStore.draftId();
    if (!draftId) {
      return;
    }

    try {
      await this.persistenceService.deleteFileByTempId(draftId, tempId);
    } catch (error) {
      console.error('Failed to delete file from IndexedDB:', error);
    }
  }

  /**
   * Cleanup object URLs to prevent memory leaks.
   */
  ngOnDestroy(): void {
    this.objectUrls.forEach((url) => URL.revokeObjectURL(url));
  }
}
