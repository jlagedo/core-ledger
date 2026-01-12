import { Routes } from '@angular/router';

export const CADASTRO_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'calendario',
    pathMatch: 'full',
  },
  {
    path: 'calendario',
    data: { breadcrumb: 'Calendário' },
    loadChildren: () =>
      import('./calendario/calendario.routes').then((m) => m.CALENDARIO_ROUTES),
  },
  {
    path: 'indexadores',
    data: { breadcrumb: 'Indexadores' },
    loadChildren: () =>
      import('./indexadores/indexadores.routes').then((m) => m.INDEXADORES_ROUTES),
  },
  {
    path: 'fundos',
    data: { breadcrumb: 'Fundos' },
    loadChildren: () => import('./fundos/fundos.routes').then((m) => m.FUNDOS_ROUTES),
  },
];
