import { CanDeactivateFn } from '@angular/router';

/**
 * Interface for components that track dirty state.
 * Components implementing this interface can be used with the unsavedChangesGuard.
 */
export interface DirtyComponent {
  /**
   * Returns true if the component has unsaved changes.
   */
  isDirty(): boolean;
}

/**
 * Functional guard to prevent navigation with unsaved changes.
 * Shows confirmation dialog when component has dirty state.
 *
 * Usage in routes:
 * ```typescript
 * {
 *   path: '',
 *   loadComponent: () => import('./wizard-container').then(m => m.WizardContainer),
 *   canDeactivate: [unsavedChangesGuard],
 * }
 * ```
 */
export const unsavedChangesGuard: CanDeactivateFn<DirtyComponent> = (component) => {
  if (!component.isDirty()) {
    return true;
  }

  return confirm(
    'Existem alterações não salvas. Seu progresso foi salvo automaticamente.\n\n' +
      'Deseja realmente sair?'
  );
};
