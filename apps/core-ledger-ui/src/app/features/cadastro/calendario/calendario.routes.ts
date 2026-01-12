import { Routes } from '@angular/router';
import { CalendarioList } from './calendario-list';
import { CalendarioForm } from './calendario-form/calendario-form';

export const CALENDARIO_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'list',
    pathMatch: 'full',
  },
  {
    path: 'list',
    data: { breadcrumb: 'Lista de Calendário' },
    component: CalendarioList,
  },
  {
    path: 'new',
    data: { breadcrumb: 'Novo Calendário' },
    component: CalendarioForm,
  },
  {
    path: ':id/edit',
    data: { breadcrumb: 'Editar Calendário' },
    component: CalendarioForm,
  },
];
