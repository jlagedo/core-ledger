import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_URL } from '../config/api.config';
import {
  Calendario,
  CreateCalendario,
  UpdateCalendario,
  PaginatedResponse,
  TipoDiaOption,
  PracaOption,
  TipoDia,
  Praca,
} from '../models/calendario.model';

@Injectable({ providedIn: 'root' })
export class CalendarioService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);

  // Opções de enum estáticas para menus suspensos
  readonly tipoDiaOptions: TipoDiaOption[] = [
    { value: TipoDia.Util, name: 'Dia Útil', description: 'Dia Útil' },
    {
      value: TipoDia.FeriadoNacional,
      name: 'Feriado Nacional',
      description: 'Feriado Nacional',
    },
    {
      value: TipoDia.FeriadoEstadual,
      name: 'Feriado Estadual',
      description: 'Feriado Estadual',
    },
    {
      value: TipoDia.FeriadoMunicipal,
      name: 'Feriado Municipal',
      description: 'Feriado Municipal',
    },
    {
      value: TipoDia.FeriadoBancario,
      name: 'Feriado Bancário',
      description: 'Feriado Bancário',
    },
    { value: TipoDia.FimDeSemana, name: 'Fim de Semana', description: 'Fim de Semana' },
    {
      value: TipoDia.PontoFacultativo,
      name: 'Ponto Facultativo',
      description: 'Ponto Facultativo',
    },
  ];

  readonly pracaOptions: PracaOption[] = [
    { value: Praca.Nacional, name: 'Nacional', description: 'Nacional (B3/ANBIMA)' },
    { value: Praca.SaoPaulo, name: 'São Paulo', description: 'São Paulo' },
    { value: Praca.RioDeJaneiro, name: 'Rio de Janeiro', description: 'Rio de Janeiro' },
    { value: Praca.ExteriorEua, name: 'Exterior (EUA)', description: 'Mercado Americano' },
    { value: Praca.ExteriorEur, name: 'Exterior (EUR)', description: 'Mercado Europeu' },
  ];

  /**
   * Obtém lista paginada de calendários com filtro e ordenação opcionais
   */
  getCalendarios(
    limit: number = 100,
    offset: number = 0,
    sortBy?: string,
    sortDirection: 'asc' | 'desc' = 'desc',
    filters?: Record<string, string>
  ): Observable<PaginatedResponse<Calendario>> {
    const params: Record<string, string> = {
      limit: limit.toString(),
      offset: offset.toString(),
      sortDirection,
    };

    if (sortBy) params['sortBy'] = sortBy;

    // Adicionar parâmetros de filtro
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        params[key] = value;
      });
    }

    const queryString = new URLSearchParams(params).toString();
    return this.http.get<PaginatedResponse<Calendario>>(
      `${this.apiUrl}/v1/calendario?${queryString}`
    );
  }

  /**
   * Obtém um único calendário por ID
   */
  getCalendarioById(id: number): Observable<Calendario> {
    return this.http.get<Calendario>(`${this.apiUrl}/v1/calendario/${id}`);
  }

  /**
   * Cria uma nova entrada de calendário
   */
  createCalendario(calendario: CreateCalendario): Observable<Calendario> {
    return this.http.post<Calendario>(`${this.apiUrl}/v1/calendario`, calendario);
  }

  /**
   * Atualiza uma entrada de calendário existente
   */
  updateCalendario(id: number, calendario: UpdateCalendario): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/v1/calendario/${id}`, calendario);
  }

  /**
   * Deleta uma entrada de calendário
   */
  deleteCalendario(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/v1/calendario/${id}`);
  }
}
