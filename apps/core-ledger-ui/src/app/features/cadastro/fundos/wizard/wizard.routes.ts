import { Routes } from '@angular/router';
import { unsavedChangesGuard } from './guards/unsaved-changes.guard';

export const WIZARD_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./wizard-container').then((m) => m.WizardContainer),
    data: { breadcrumb: 'Cadastro' },
    canDeactivate: [unsavedChangesGuard],
  },
];
