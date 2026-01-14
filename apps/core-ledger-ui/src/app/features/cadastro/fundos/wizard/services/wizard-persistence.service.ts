/**
 * Wizard de Cadastro de Fundo - Persistence Service
 *
 * Service for persisting wizard state to IndexedDB.
 * Handles both form data and file uploads.
 */

import { Injectable } from '@angular/core';
import { WizardDatabase } from './wizard-database';
import {
  CURRENT_DRAFT_VERSION,
  WizardDraft,
  WizardFile,
} from '../models/persistence.model';
import { WizardStepId } from '../models/wizard.model';

/**
 * Service for persisting wizard state to IndexedDB.
 *
 * Provides methods for:
 * - Draft CRUD operations (create, read, update, delete)
 * - File storage and retrieval (blobs)
 * - Cleanup of stale drafts
 *
 * @example
 * ```typescript
 * const persistence = inject(WizardPersistenceService);
 *
 * // Save draft
 * await persistence.saveDraft('draft-123', formData, currentStep, completedSteps);
 *
 * // Load draft
 * const draft = await persistence.loadDraft('draft-123');
 *
 * // Save file
 * await persistence.saveFile('draft-123', 'documentos', file, 'temp-1', metadata);
 * ```
 */
@Injectable({ providedIn: 'root' })
export class WizardPersistenceService {
  private readonly db = new WizardDatabase();

  /**
   * Save or update a draft.
   * Uses upsert semantics - creates if not exists, updates if exists.
   */
  async saveDraft(
    draftId: string,
    formData: Record<string, unknown>,
    currentStep: WizardStepId,
    completedSteps: WizardStepId[]
  ): Promise<void> {
    const existing = await this.db.drafts.get(draftId);
    const now = new Date();

    await this.db.drafts.put({
      id: draftId,
      formData,
      currentStep,
      completedSteps,
      version: CURRENT_DRAFT_VERSION,
      createdAt: existing?.createdAt ?? now,
      updatedAt: now,
    });
  }

  /**
   * Save a file associated with a draft.
   * Returns the auto-generated file ID.
   */
  async saveFile(
    draftId: string,
    stepKey: string,
    file: File,
    tempId: string,
    metadata: Record<string, unknown>
  ): Promise<number> {
    // Check if file already exists with same tempId
    const existing = await this.db.files
      .where({ draftId, tempId })
      .first();

    if (existing?.id) {
      // Update existing file
      await this.db.files.update(existing.id, {
        fileName: file.name,
        fileType: file.type,
        blob: file,
        metadata,
      });
      return existing.id;
    }

    // Add new file
    return this.db.files.add({
      draftId,
      stepKey,
      tempId,
      fileName: file.name,
      fileType: file.type,
      blob: file,
      metadata,
      createdAt: new Date(),
    });
  }

  /**
   * Load a draft by ID.
   * Returns null if draft not found.
   */
  async loadDraft(draftId: string): Promise<WizardDraft | null> {
    const draft = await this.db.drafts.get(draftId);
    return draft ?? null;
  }

  /**
   * Load files for a draft, optionally filtered by step key.
   */
  async loadFiles(draftId: string, stepKey?: string): Promise<WizardFile[]> {
    const query = this.db.files.where('draftId').equals(draftId);

    if (stepKey) {
      const files = await query.toArray();
      return files.filter((f) => f.stepKey === stepKey);
    }

    return query.toArray();
  }

  /**
   * Load a single file by its tempId within a draft.
   */
  async loadFileByTempId(
    draftId: string,
    tempId: string
  ): Promise<WizardFile | null> {
    const file = await this.db.files.where({ draftId, tempId }).first();
    return file ?? null;
  }

  /**
   * Check if a draft exists.
   */
  async hasDraft(draftId: string): Promise<boolean> {
    const count = await this.db.drafts.where('id').equals(draftId).count();
    return count > 0;
  }

  /**
   * Delete a draft and all its associated files.
   */
  async deleteDraft(draftId: string): Promise<void> {
    await this.db.files.where('draftId').equals(draftId).delete();
    await this.db.drafts.delete(draftId);
  }

  /**
   * Delete a specific file by ID.
   */
  async deleteFile(fileId: number): Promise<void> {
    await this.db.files.delete(fileId);
  }

  /**
   * Delete a file by its tempId within a draft.
   */
  async deleteFileByTempId(draftId: string, tempId: string): Promise<void> {
    await this.db.files.where({ draftId, tempId }).delete();
  }

  /**
   * Cleanup drafts older than maxAgeDays.
   * Returns the number of drafts deleted.
   */
  async cleanupStaleDrafts(maxAgeDays = 7): Promise<number> {
    const cutoff = new Date(Date.now() - maxAgeDays * 24 * 60 * 60 * 1000);
    const staleDrafts = await this.db.drafts
      .where('updatedAt')
      .below(cutoff)
      .toArray();

    for (const draft of staleDrafts) {
      await this.deleteDraft(draft.id);
    }

    return staleDrafts.length;
  }

  /**
   * Get all draft IDs (for recovery detection).
   */
  async getAllDraftIds(): Promise<string[]> {
    const drafts = await this.db.drafts.toArray();
    return drafts.map((d) => d.id);
  }

  /**
   * Get the most recent draft.
   * Returns null if no drafts exist.
   */
  async getMostRecentDraft(): Promise<WizardDraft | null> {
    const drafts = await this.db.drafts
      .orderBy('updatedAt')
      .reverse()
      .limit(1)
      .toArray();
    return drafts[0] ?? null;
  }

  /**
   * Get total storage usage in bytes for a draft (form data + files).
   * Useful for quota management.
   */
  async getDraftStorageSize(draftId: string): Promise<number> {
    const files = await this.loadFiles(draftId);
    return files.reduce((total, file) => total + (file.blob?.size ?? 0), 0);
  }

  /**
   * Clear all wizard data from IndexedDB.
   * USE WITH CAUTION - deletes all drafts and files.
   */
  async clearAllData(): Promise<void> {
    await this.db.drafts.clear();
    await this.db.files.clear();
  }
}
