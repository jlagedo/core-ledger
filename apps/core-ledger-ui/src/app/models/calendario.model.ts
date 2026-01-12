// Enums matching API
export enum TipoDia {
  Util = 1,
  FeriadoNacional = 2,
  FeriadoEstadual = 3,
  FeriadoMunicipal = 4,
  FeriadoBancario = 5,
  FimDeSemana = 6,
  PontoFacultativo = 7,
}

export enum Praca {
  Nacional = 1,
  SaoPaulo = 2,
  RioDeJaneiro = 3,
  ExteriorEua = 4,
  ExteriorEur = 5,
}

// Main DTO matching CalendarioDto from API
export interface Calendario {
  id: number;
  data: string; // DateOnly as ISO string (yyyy-MM-dd)
  diaUtil: boolean;
  tipoDia: TipoDia;
  tipoDiaDescricao: string;
  praca: Praca;
  pracaDescricao: string;
  descricao: string | null;
  createdAt: string;
  updatedAt: string | null;
}

// For creating new entries (CreateCalendarioDto)
export interface CreateCalendario {
  data: string;
  tipoDia: TipoDia;
  praca: Praca;
  descricao: string | null;
}

// For updating entries (UpdateCalendarioDto)
export interface UpdateCalendario {
  tipoDia: TipoDia;
  descricao: string | null;
}

// Paginated response
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  limit: number;
  offset: number;
}

// Type descriptors for dropdowns
export interface TipoDiaOption {
  value: TipoDia;
  name: string;
  description: string;
}

export interface PracaOption {
  value: Praca;
  name: string;
  description: string;
}
