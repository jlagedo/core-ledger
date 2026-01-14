/**
 * Wizard de Cadastro de Fundo - Modelos de Persistência IndexedDB
 */

import { WizardStepId } from './wizard.model';

/**
 * Represents a saved wizard draft in IndexedDB
 */
export interface WizardDraft {
  /** UUID identifying this draft */
  id: string;
  /** All step data keyed by step key (e.g., 'identificacao', 'classificacao') */
  formData: Record<string, unknown>;
  /** Current step ID (1-11) */
  currentStep: WizardStepId;
  /** Array of completed step IDs */
  completedSteps: WizardStepId[];
  /** Schema version for migrations */
  version: number;
  /** When draft was first created */
  createdAt: Date;
  /** When draft was last updated */
  updatedAt: Date;
}

/**
 * Represents a file stored in IndexedDB
 */
export interface WizardFile {
  /** Auto-incremented ID */
  id?: number;
  /** Reference to parent draft */
  draftId: string;
  /** Step key (e.g., 'documentos') */
  stepKey: string;
  /** Client-side temp ID matching DocumentoFundo.tempId */
  tempId: string;
  /** Original filename */
  fileName: string;
  /** MIME type */
  fileType: string;
  /** File content as Blob */
  blob: Blob;
  /** Additional metadata (tipoDocumento, versao, dataVigencia, etc.) */
  metadata: Record<string, unknown>;
  /** When file was added */
  createdAt: Date;
}

/**
 * Auto-save status indicator states
 */
export type SaveStatus = 'idle' | 'saving' | 'saved' | 'error';

/**
 * Current schema version for drafts
 */
export const CURRENT_DRAFT_VERSION = 2;
