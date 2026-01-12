import { ChangeDetectionStrategy, Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { IndexadorService } from '../../../../services/indexador';
import { Indexador, ImportHistoricoResult } from '../../../../models/indexador.model';

interface CsvPreviewRow {
  dataReferencia: string;
  valor: string;
  fatorDiario: string;
  variacaoPercentual: string;
  fonte: string;
}

@Component({
  selector: 'app-import-modal',
  imports: [FormsModule],
  templateUrl: './import-modal.html',
  styleUrl: './import-modal.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'dialog',
    'aria-modal': 'true',
    '[attr.aria-labelledby]': '"modal-title"',
    '(keydown.escape)': 'onEscape()',
  },
})
export class ImportModal {
  readonly activeModal = inject(NgbActiveModal);
  private readonly indexadorService = inject(IndexadorService);

  readonly indexador = signal<Indexador | null>(null);
  readonly importMode = signal<'csv' | 'api'>('csv');
  readonly isImporting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly importResult = signal<ImportHistoricoResult | null>(null);

  // CSV import state
  readonly selectedFile = signal<File | null>(null);
  readonly csvPreview = signal<CsvPreviewRow[]>([]);
  readonly sobrescrever = signal(false);
  readonly isDragOver = signal(false);

  // API import state (if importacaoAutomatica is enabled)
  readonly apiImportStatus = signal<'idle' | 'pending' | 'success' | 'error'>('idle');

  readonly canImport = computed(() => {
    if (this.importMode() === 'csv') {
      return this.selectedFile() !== null;
    }
    return this.indexador()?.importacaoAutomatica ?? false;
  });

  readonly showApiTab = computed(() => {
    return this.indexador()?.importacaoAutomatica ?? false;
  });

  onEscape(): void {
    if (!this.isImporting()) {
      this.activeModal.dismiss('escape');
    }
  }

  cancel(): void {
    this.activeModal.dismiss('cancel');
  }

  setImportMode(mode: 'csv' | 'api'): void {
    this.importMode.set(mode);
    this.errorMessage.set(null);
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
      this.handleFileSelect(files[0]);
    }
  }

  onFileInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFileSelect(input.files[0]);
    }
  }

  private handleFileSelect(file: File): void {
    if (!file.name.endsWith('.csv')) {
      this.errorMessage.set('Por favor, selecione um arquivo CSV');
      return;
    }

    this.selectedFile.set(file);
    this.errorMessage.set(null);
    this.parsePreview(file);
  }

  private parsePreview(file: File): void {
    const reader = new FileReader();
    reader.onload = (e) => {
      const content = e.target?.result as string;
      const lines = content.split('\n').filter(line => line.trim());

      // Skip header and get first 5 rows for preview
      const dataLines = lines.slice(1, 6);
      const preview: CsvPreviewRow[] = dataLines.map(line => {
        const parts = line.split(';');
        return {
          dataReferencia: parts[0] || '',
          valor: parts[1] || '',
          fatorDiario: parts[2] || '',
          variacaoPercentual: parts[3] || '',
          fonte: parts[4]?.trim() || '',
        };
      });

      this.csvPreview.set(preview);
    };
    reader.readAsText(file);
  }

  clearFile(): void {
    this.selectedFile.set(null);
    this.csvPreview.set([]);
    this.errorMessage.set(null);
  }

  import(): void {
    if (this.importMode() === 'csv') {
      this.importCsv();
    } else {
      this.importFromApi();
    }
  }

  private importCsv(): void {
    const idx = this.indexador();
    const file = this.selectedFile();
    if (!idx || !file) return;

    this.isImporting.set(true);
    this.errorMessage.set(null);
    this.importResult.set(null);

    this.indexadorService.importHistorico(idx.id, file, this.sobrescrever()).subscribe({
      next: (result) => {
        this.isImporting.set(false);
        this.importResult.set(result);
        if (result.errors && result.errors.length > 0) {
          this.errorMessage.set(`Importado com erros: ${result.errors.join(', ')}`);
        }
      },
      error: (err) => {
        this.isImporting.set(false);
        if (err.status === 400) {
          this.errorMessage.set('Formato de arquivo invalido');
        } else {
          this.errorMessage.set('Erro ao importar arquivo. Tente novamente.');
        }
      },
    });
  }

  private importFromApi(): void {
    const idx = this.indexador();
    if (!idx) return;

    this.isImporting.set(true);
    this.errorMessage.set(null);
    this.apiImportStatus.set('pending');

    this.indexadorService.triggerImport(idx.id).subscribe({
      next: () => {
        this.isImporting.set(false);
        this.apiImportStatus.set('success');
      },
      error: () => {
        this.isImporting.set(false);
        this.apiImportStatus.set('error');
        this.errorMessage.set('Erro ao iniciar importacao automatica');
      },
    });
  }

  closeWithResult(): void {
    this.activeModal.close('imported');
  }
}
