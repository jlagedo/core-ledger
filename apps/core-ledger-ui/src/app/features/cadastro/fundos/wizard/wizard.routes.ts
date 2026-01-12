import { Routes } from '@angular/router';

export const WIZARD_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./wizard-container').then((m) => m.WizardContainer),
    data: { breadcrumb: 'Cadastro' },
  },
];
