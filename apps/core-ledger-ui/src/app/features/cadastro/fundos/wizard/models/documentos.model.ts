/**
 * Wizard de Cadastro de Fundo - Modelos de Documentos
 */

/**
 * Tipos de documento permitidos
 */
export enum TipoDocumento {
  REGULAMENTO = 'REGULAMENTO',
  LAMINA = 'LAMINA',
  FIC = 'FIC',
  PROSPECTO = 'PROSPECTO',
  TERMO_ADESAO = 'TERMO_ADESAO',
  POLITICA_INVESTIMENTO = 'POLITICA_INVESTIMENTO',
  MANUAL_COMPLIANCE = 'MANUAL_COMPLIANCE',
  CONTRATO_CUSTODIA = 'CONTRATO_CUSTODIA',
  CONTRATO_GESTAO = 'CONTRATO_GESTAO',
  ATA_ASSEMBLEIA = 'ATA_ASSEMBLEIA',
  PARECER_AUDITOR = 'PARECER_AUDITOR',
}

/**
 * Opções de tipo de documento para dropdown
 */
export interface TipoDocumentoOption {
  value: TipoDocumento;
  label: string;
  description: string;
  recommended?: boolean;
  icon: string;
  color: string;
}

export const TIPO_DOCUMENTO_OPTIONS: TipoDocumentoOption[] = [
  {
    value: TipoDocumento.REGULAMENTO,
    label: 'Regulamento',
    description: 'Regulamento do Fundo',
    recommended: true,
    icon: 'bi-file-earmark-text',
    color: '#0d6efd', // primary
  },
  {
    value: TipoDocumento.LAMINA,
    label: 'Lâmina',
    description: 'Lâmina de Informações',
    icon: 'bi-file-earmark-richtext',
    color: '#6610f2', // indigo
  },
  {
    value: TipoDocumento.FIC,
    label: 'FIC',
    description: 'Formulário Info. Complementares',
    icon: 'bi-file-earmark-spreadsheet',
    color: '#6f42c1', // purple
  },
  {
    value: TipoDocumento.PROSPECTO,
    label: 'Prospecto',
    description: 'Prospecto do Fundo',
    icon: 'bi-file-earmark-text',
    color: '#d63384', // pink
  },
  {
    value: TipoDocumento.TERMO_ADESAO,
    label: 'Termo de Adesão',
    description: 'Termo de Adesão',
    icon: 'bi-file-earmark-check',
    color: '#dc3545', // danger
  },
  {
    value: TipoDocumento.POLITICA_INVESTIMENTO,
    label: 'Política de Investimento',
    description: 'Política de Investimento',
    icon: 'bi-file-earmark-ruled',
    color: '#fd7e14', // orange
  },
  {
    value: TipoDocumento.MANUAL_COMPLIANCE,
    label: 'Manual de Compliance',
    description: 'Manual de Compliance',
    icon: 'bi-file-earmark-lock',
    color: '#ffc107', // warning
  },
  {
    value: TipoDocumento.CONTRATO_CUSTODIA,
    label: 'Contrato de Custódia',
    description: 'Contrato de Custódia',
    icon: 'bi-file-earmark-lock2',
    color: '#198754', // success
  },
  {
    value: TipoDocumento.CONTRATO_GESTAO,
    label: 'Contrato de Gestão',
    description: 'Contrato de Gestão',
    icon: 'bi-file-earmark-person',
    color: '#20c997', // teal
  },
  {
    value: TipoDocumento.ATA_ASSEMBLEIA,
    label: 'Ata de Assembleia',
    description: 'Ata de Assembleia',
    icon: 'bi-file-earmark-post',
    color: '#0dcaf0', // info
  },
  {
    value: TipoDocumento.PARECER_AUDITOR,
    label: 'Parecer do Auditor',
    description: 'Parecer do Auditor',
    icon: 'bi-file-earmark-check2',
    color: '#6c757d', // secondary
  },
];

/**
 * Documento anexado ao fundo
 */
export interface DocumentoFundo {
  // IDs temporários para gerenciamento no frontend
  tempId: string;

  // Metadados do documento
  tipoDocumento: TipoDocumento;
  versao: number;
  dataVigencia: string; // ISO 8601 date string
  dataFimVigencia?: string | null;
  observacoes?: string | null;

  // Dados do arquivo
  arquivoNome: string;
  arquivoTamanho: number; // bytes
  arquivoTipo: string; // MIME type
  arquivoConteudo?: File; // File object (apenas no upload)
  arquivoHash?: string; // SHA-256 (após upload no backend)

  // Status de upload
  uploadStatus: 'pending' | 'uploading' | 'uploaded' | 'error';
  uploadProgress?: number; // 0-100
  uploadError?: string;

  // Timestamps
  createdAt?: string;
}

/**
 * Dados do formulário de novo documento
 */
export interface NovoDocumentoFormData {
  tipoDocumento: TipoDocumento | null;
  dataVigencia: string;
  dataFimVigencia: string;
  observacoes: string;
}

/**
 * Dados salvos no wizard store (array de documentos)
 */
export type DocumentosStepData = DocumentoFundo[];

/**
 * Tipos de arquivo permitidos
 */
export const ALLOWED_FILE_TYPES = [
  'application/pdf',
  'application/msword',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
];

export const ALLOWED_FILE_EXTENSIONS = ['.pdf', '.doc', '.docx'];

/**
 * Tamanho máximo por arquivo (10MB)
 */
export const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB em bytes

/**
 * Converte bytes para formato legível
 */
export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
}

/**
 * Obtém a opção de tipo de documento pelo valor
 */
export function getTipoDocumentoOption(tipo: TipoDocumento): TipoDocumentoOption | undefined {
  return TIPO_DOCUMENTO_OPTIONS.find((opt) => opt.value === tipo);
}

/**
 * Valida tipo MIME do arquivo
 */
export function isValidFileType(file: File): boolean {
  return ALLOWED_FILE_TYPES.includes(file.type);
}

/**
 * Valida tamanho do arquivo
 */
export function isValidFileSize(file: File): boolean {
  return file.size <= MAX_FILE_SIZE;
}

/**
 * Valida extensão do arquivo
 */
export function isValidFileExtension(fileName: string): boolean {
  return ALLOWED_FILE_EXTENSIONS.some((ext) => fileName.toLowerCase().endsWith(ext));
}
