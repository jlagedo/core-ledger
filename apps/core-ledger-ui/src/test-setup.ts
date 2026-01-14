/**
 * Test setup file for Vitest
 * Runs before all tests to set up global mocks and configurations.
 */

// Set up fake IndexedDB for tests that use Dexie.js
import 'fake-indexeddb/auto';
