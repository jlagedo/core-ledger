import { Routes } from '@angular/router';

export const INDEXADORES_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'list',
    pathMatch: 'full',
  },
  {
    path: 'list',
    data: { breadcrumb: 'Lista de Indexadores' },
    loadComponent: () => import('./indexador-list').then((m) => m.IndexadorList),
  },
  {
    path: 'new',
    data: { breadcrumb: 'Novo Indexador' },
    loadComponent: () => import('./indexador-form/indexador-form').then((m) => m.IndexadorForm),
  },
  {
    path: ':id',
    data: { breadcrumb: 'Detalhes' },
    loadComponent: () => import('./indexador-detail/indexador-detail').then((m) => m.IndexadorDetail),
  },
  {
    path: ':id/edit',
    data: { breadcrumb: 'Editar Indexador' },
    loadComponent: () => import('./indexador-form/indexador-form').then((m) => m.IndexadorForm),
  },
];
