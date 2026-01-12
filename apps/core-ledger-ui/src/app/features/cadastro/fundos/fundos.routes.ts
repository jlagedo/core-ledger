import { Routes } from '@angular/router';

export const FUNDOS_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'novo',
    pathMatch: 'full',
  },
  {
    path: 'novo',
    data: { breadcrumb: 'Novo Fundo' },
    loadChildren: () => import('./wizard/wizard.routes').then((m) => m.WIZARD_ROUTES),
  },
];
