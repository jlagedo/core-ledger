import { Injectable, signal } from '@angular/core';
import { MenuItem } from '../models/menu-item.model';

@Injectable({
  providedIn: 'root',
})
export class MenuService {
  private readonly _menuItems = signal<MenuItem[]>([
    {
      label: 'Visão Geral',
      icon: 'bi-grid-fill',
      route: '/dashboard',
      exactMatch: true,
    },
    {
      label: 'Cadastro',
      icon: 'bi-folder-plus',
      children: [
        {
          label: 'Calendário',
          icon: '',
          route: '/cadastro/calendario',
        },
        {
          label: 'Indexadores',
          icon: '',
          route: '/cadastro/indexadores',
        },
        {
          label: 'Fundos',
          icon: '',
          route: '/cadastro/fundos',
        },
      ],
    },
  ]);

  readonly menuItems = this._menuItems.asReadonly();
}
