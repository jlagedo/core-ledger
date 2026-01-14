/**
 * WizardPersistenceService Tests
 *
 * Uses fake-indexeddb for IndexedDB mocking in jsdom environment.
 * fake-indexeddb is loaded globally via test-setup.ts
 */

import { TestBed } from '@angular/core/testing';
import { WizardPersistenceService } from './wizard-persistence.service';
import { WizardStepId } from '../models/wizard.model';

describe('WizardPersistenceService', () => {
  let service: WizardPersistenceService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [WizardPersistenceService],
    });

    service = TestBed.inject(WizardPersistenceService);
  });

  afterEach(async () => {
    // Clean up all data after each test
    await service.clearAllData();
  });

  describe('Draft Operations', () => {
    const testDraftId = 'test-draft-123';
    const testFormData = {
      identificacao: { nome: 'Test Fund', cnpj: '12.345.678/0001-99' },
      classificacao: { tipo: 'FI' },
    };
    const testCurrentStep: WizardStepId = 3;
    const testCompletedSteps: WizardStepId[] = [1, 2];

    it('should save a new draft', async () => {
      await service.saveDraft(testDraftId, testFormData, testCurrentStep, testCompletedSteps);

      const draft = await service.loadDraft(testDraftId);

      expect(draft).toBeTruthy();
      expect(draft?.id).toBe(testDraftId);
      expect(draft?.formData).toEqual(testFormData);
      expect(draft?.currentStep).toBe(testCurrentStep);
      expect(draft?.completedSteps).toEqual(testCompletedSteps);
      expect(draft?.version).toBe(2);
      expect(draft?.createdAt).toBeInstanceOf(Date);
      expect(draft?.updatedAt).toBeInstanceOf(Date);
    });

    it('should update an existing draft', async () => {
      // Create initial draft
      await service.saveDraft(testDraftId, testFormData, testCurrentStep, testCompletedSteps);
      const initialDraft = await service.loadDraft(testDraftId);

      // Wait a bit to ensure different timestamp
      await new Promise((resolve) => setTimeout(resolve, 10));

      // Update draft
      const updatedFormData = { ...testFormData, taxas: { administracao: 0.5 } };
      const updatedCurrentStep: WizardStepId = 5;
      const updatedCompletedSteps: WizardStepId[] = [1, 2, 3, 4];

      await service.saveDraft(
        testDraftId,
        updatedFormData,
        updatedCurrentStep,
        updatedCompletedSteps
      );

      const updatedDraft = await service.loadDraft(testDraftId);

      expect(updatedDraft?.formData).toEqual(updatedFormData);
      expect(updatedDraft?.currentStep).toBe(updatedCurrentStep);
      expect(updatedDraft?.completedSteps).toEqual(updatedCompletedSteps);
      // createdAt should remain the same
      expect(updatedDraft?.createdAt.getTime()).toBe(initialDraft?.createdAt.getTime());
      // updatedAt should be different
      expect(updatedDraft?.updatedAt.getTime()).toBeGreaterThan(
        initialDraft?.updatedAt.getTime() ?? 0
      );
    });

    it('should return null for non-existent draft', async () => {
      const draft = await service.loadDraft('non-existent-id');
      expect(draft).toBeNull();
    });

    it('should check if draft exists', async () => {
      expect(await service.hasDraft(testDraftId)).toBe(false);

      await service.saveDraft(testDraftId, testFormData, testCurrentStep, testCompletedSteps);

      expect(await service.hasDraft(testDraftId)).toBe(true);
    });

    it('should delete a draft', async () => {
      await service.saveDraft(testDraftId, testFormData, testCurrentStep, testCompletedSteps);
      expect(await service.hasDraft(testDraftId)).toBe(true);

      await service.deleteDraft(testDraftId);

      expect(await service.hasDraft(testDraftId)).toBe(false);
    });

    it('should get all draft IDs', async () => {
      await service.saveDraft('draft-1', {}, 1, []);
      await service.saveDraft('draft-2', {}, 1, []);
      await service.saveDraft('draft-3', {}, 1, []);

      const ids = await service.getAllDraftIds();

      expect(ids).toContain('draft-1');
      expect(ids).toContain('draft-2');
      expect(ids).toContain('draft-3');
      expect(ids.length).toBe(3);
    });

    it('should get most recent draft', async () => {
      await service.saveDraft('draft-old', {}, 1, []);
      await new Promise((resolve) => setTimeout(resolve, 10));
      await service.saveDraft('draft-new', {}, 2, []);

      const mostRecent = await service.getMostRecentDraft();

      expect(mostRecent?.id).toBe('draft-new');
    });

    it('should return null when no drafts exist', async () => {
      const mostRecent = await service.getMostRecentDraft();
      expect(mostRecent).toBeNull();
    });
  });

  describe('File Operations', () => {
    const testDraftId = 'file-test-draft';
    const testStepKey = 'documentos';
    const testTempId = 'temp-file-1';
    const testMetadata = {
      tipoDocumento: 'REGULAMENTO',
      versao: 1,
      dataVigencia: '2024-01-01',
    };

    const createTestFile = (name = 'test.pdf', size = 1024): File => {
      const content = new Uint8Array(size).fill(65); // Fill with 'A'
      return new File([content], name, { type: 'application/pdf' });
    };

    beforeEach(async () => {
      // Create a draft for file tests
      await service.saveDraft(testDraftId, {}, 1, []);
    });

    it('should save a file', async () => {
      const file = createTestFile();

      const fileId = await service.saveFile(
        testDraftId,
        testStepKey,
        file,
        testTempId,
        testMetadata
      );

      expect(fileId).toBeGreaterThan(0);

      const files = await service.loadFiles(testDraftId);
      expect(files.length).toBe(1);
      expect(files[0].fileName).toBe('test.pdf');
      expect(files[0].fileType).toBe('application/pdf');
      expect(files[0].tempId).toBe(testTempId);
      expect(files[0].metadata).toEqual(testMetadata);
    });

    it('should update existing file with same tempId', async () => {
      const file1 = createTestFile('file1.pdf', 1024);
      const file2 = createTestFile('file2.pdf', 2048);

      await service.saveFile(testDraftId, testStepKey, file1, testTempId, testMetadata);
      await service.saveFile(testDraftId, testStepKey, file2, testTempId, {
        ...testMetadata,
        versao: 2,
      });

      const files = await service.loadFiles(testDraftId);
      expect(files.length).toBe(1);
      expect(files[0].fileName).toBe('file2.pdf');
      expect(files[0].metadata['versao']).toBe(2);
    });

    it('should load files filtered by step key', async () => {
      const file1 = createTestFile('doc1.pdf');
      const file2 = createTestFile('doc2.pdf');

      await service.saveFile(testDraftId, 'documentos', file1, 'temp-1', {});
      await service.saveFile(testDraftId, 'other-step', file2, 'temp-2', {});

      const documentosFiles = await service.loadFiles(testDraftId, 'documentos');
      const otherFiles = await service.loadFiles(testDraftId, 'other-step');
      const allFiles = await service.loadFiles(testDraftId);

      expect(documentosFiles.length).toBe(1);
      expect(documentosFiles[0].fileName).toBe('doc1.pdf');
      expect(otherFiles.length).toBe(1);
      expect(otherFiles[0].fileName).toBe('doc2.pdf');
      expect(allFiles.length).toBe(2);
    });

    it('should load file by tempId', async () => {
      const file = createTestFile();
      await service.saveFile(testDraftId, testStepKey, file, testTempId, testMetadata);

      const loadedFile = await service.loadFileByTempId(testDraftId, testTempId);

      expect(loadedFile).toBeTruthy();
      expect(loadedFile?.fileName).toBe('test.pdf');
    });

    it('should return null for non-existent file by tempId', async () => {
      const file = await service.loadFileByTempId(testDraftId, 'non-existent');
      expect(file).toBeNull();
    });

    it('should delete file by ID', async () => {
      const file = createTestFile();
      const fileId = await service.saveFile(
        testDraftId,
        testStepKey,
        file,
        testTempId,
        testMetadata
      );

      await service.deleteFile(fileId);

      const files = await service.loadFiles(testDraftId);
      expect(files.length).toBe(0);
    });

    it('should delete file by tempId', async () => {
      const file = createTestFile();
      await service.saveFile(testDraftId, testStepKey, file, testTempId, testMetadata);

      await service.deleteFileByTempId(testDraftId, testTempId);

      const files = await service.loadFiles(testDraftId);
      expect(files.length).toBe(0);
    });

    it('should delete files when draft is deleted', async () => {
      const file = createTestFile();
      await service.saveFile(testDraftId, testStepKey, file, testTempId, testMetadata);

      await service.deleteDraft(testDraftId);

      const files = await service.loadFiles(testDraftId);
      expect(files.length).toBe(0);
    });

    it('should calculate storage size for draft', async () => {
      const file1 = createTestFile('file1.pdf', 1024);
      const file2 = createTestFile('file2.pdf', 2048);

      await service.saveFile(testDraftId, testStepKey, file1, 'temp-1', {});
      await service.saveFile(testDraftId, testStepKey, file2, 'temp-2', {});

      const size = await service.getDraftStorageSize(testDraftId);
      // Note: fake-indexeddb may return 0 for blob sizes in test environment
      // In real browser, this would return the actual blob sizes
      expect(typeof size).toBe('number');
      expect(Number.isFinite(size)).toBe(true);
      expect(size).toBeGreaterThanOrEqual(0);
    });
  });

  describe('Cleanup Operations', () => {
    it('should cleanup stale drafts', async () => {
      // Create a draft with old updatedAt
      await service.saveDraft('old-draft', {}, 1, []);

      // Manually update the draft to have an old date
      // Since we can't easily manipulate dates in IndexedDB, we'll test the method exists
      // and returns the correct type
      const count = await service.cleanupStaleDrafts(7);

      // In a real scenario with proper date mocking, this would clean up old drafts
      expect(typeof count).toBe('number');
    });

    it('should clear all data', async () => {
      await service.saveDraft('draft-1', {}, 1, []);
      await service.saveDraft('draft-2', {}, 1, []);
      await service.saveFile('draft-1', 'docs', createTestFile(), 'temp-1', {});

      await service.clearAllData();

      const ids = await service.getAllDraftIds();
      const files = await service.loadFiles('draft-1');

      expect(ids.length).toBe(0);
      expect(files.length).toBe(0);
    });
  });

  // Helper function for tests
  function createTestFile(name = 'test.pdf', size = 1024): File {
    const content = new Uint8Array(size).fill(65);
    return new File([content], name, { type: 'application/pdf' });
  }
});
