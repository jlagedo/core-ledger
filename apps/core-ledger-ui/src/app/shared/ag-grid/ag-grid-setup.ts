/**
 * AG Grid module registration for lazy-loaded components.
 *
 * This file registers AG Grid modules when imported, ensuring the library
 * is only bundled with components that use it (lazy-loaded routes).
 *
 * IMPORTANT: Import this file in any component that uses AG Grid:
 *   import '../../../shared/ag-grid/ag-grid-setup';
 *
 * This replaces the global registration that was in app.config.ts,
 * reducing the initial bundle size by ~1MB.
 */
import { ModuleRegistry, AllCommunityModule } from 'ag-grid-community';

// Register AG Grid modules (runs once when this module is first imported)
ModuleRegistry.registerModules([AllCommunityModule]);
