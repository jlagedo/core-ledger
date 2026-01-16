/**
 * Wizard de Cadastro de Fundo - IndexedDB Database
 *
 * Uses Dexie.js for TypeScript-native IndexedDB access.
 * Handles schema versioning and migrations.
 */

import Dexie, { Table } from 'dexie';
import { WizardDraft, WizardFile } from '../models/persistence.model';

/**
 * IndexedDB database for wizard draft persistence.
 *
 * Tables:
 * - drafts: Wizard draft data (form state, current step, completed steps)
 * - files: File blobs associated with drafts (documents step)
 */
export class WizardDatabase extends Dexie {
  drafts!: Table<WizardDraft, string>;
  files!: Table<WizardFile, number>;

  constructor() {
    super('CoreLedgerWizardDB');

    // Version 1: Initial schema
    this.version(1).stores({
      drafts: 'id, updatedAt',
      files: '++id, draftId, stepKey, tempId, createdAt',
    });

    // Version 2: Add version field to drafts for future migrations
    this.version(2)
      .stores({
        drafts: 'id, updatedAt, version',
        files: '++id, draftId, stepKey, tempId, createdAt',
      })
      .upgrade((tx) => {
        return tx
          .table('drafts')
          .toCollection()
          .modify((draft) => {
            draft.version = draft.version || 1;
          });
      });

    // Version 3: Add compound index [draftId+tempId] for efficient lookups
    this.version(3).stores({
      drafts: 'id, updatedAt, version',
      files: '++id, draftId, stepKey, tempId, [draftId+tempId], createdAt',
    });
  }
}
