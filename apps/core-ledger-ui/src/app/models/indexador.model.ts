// Enums matching API
export enum TipoIndexador {
  Juros = 1,
  Inflacao = 2,
  Cambio = 3,
  IndiceBolsa = 4,
  IndiceRendaFixa = 5,
  Crypto = 6,
  Outro = 7,
}

export enum Periodicidade {
  Diaria = 1,
  Mensal = 2,
  Anual = 3,
}

// Main DTO matching IndexadorDto from API
export interface Indexador {
  id: number;
  codigo: string;
  nome: string;
  tipo: TipoIndexador;
  tipoDescricao: string;
  fonte: string | null;
  periodicidade: Periodicidade;
  periodicidadeDescricao: string;
  fatorAcumulado: number | null;
  dataBase: string | null; // ISO datetime string
  urlFonte: string | null;
  importacaoAutomatica: boolean;
  ativo: boolean;
  createdAt: string;
  updatedAt: string | null;
  // Historico stats
  ultimoValor: number | null;
  ultimaData: string | null; // ISO datetime string
  historicoCount: number;
}

// For creating new entries (CreateIndexadorDto)
export interface CreateIndexador {
  codigo: string;
  nome: string;
  tipo: TipoIndexador;
  fonte: string | null;
  periodicidade: Periodicidade;
  fatorAcumulado: number | null;
  dataBase: string | null;
  urlFonte: string | null;
  importacaoAutomatica: boolean;
  ativo: boolean;
}

// For updating entries (UpdateIndexadorDto)
// Note: Tipo and Periodicidade are immutable after creation
export interface UpdateIndexador {
  nome: string;
  fonte: string | null;
  fatorAcumulado: number | null;
  dataBase: string | null;
  urlFonte: string | null;
  importacaoAutomatica: boolean;
  ativo: boolean;
}

// Historical data DTO matching HistoricoIndexadorDto from API
export interface HistoricoIndexador {
  id: number;
  indexadorId: number;
  indexadorCodigo: string;
  dataReferencia: string; // ISO datetime string
  valor: number;
  fatorDiario: number | null;
  variacaoPercentual: number | null;
  fonte: string | null;
  importacaoId: string | null;
  createdAt: string;
}

// For creating new history entries
export interface CreateHistoricoIndexador {
  indexadorId: number;
  dataReferencia: string;
  valor: number;
  fatorDiario: number | null;
  variacaoPercentual: number | null;
  fonte: string | null;
  importacaoId: string | null;
}

// Import result
export interface ImportHistoricoResult {
  totalRows: number;
  importedRows: number;
  skippedRows: number;
  overwrittenRows: number;
  errors: string[];
}

// Paginated response (reusable)
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  limit: number;
  offset: number;
}

// Type descriptors for dropdowns
export interface TipoIndexadorOption {
  value: TipoIndexador;
  name: string;
  description: string;
  icon: string;
  color: string;
}

export interface PeriodicidadeOption {
  value: Periodicidade;
  name: string;
  description: string;
}

export interface FonteOption {
  value: string;
  name: string;
  description: string;
}
