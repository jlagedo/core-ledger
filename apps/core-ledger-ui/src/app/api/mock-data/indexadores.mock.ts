import {
  Indexador,
  HistoricoIndexador,
  TipoIndexador,
  Periodicidade,
} from '../../models/indexador.model';

/**
 * Mock indexador data for local development and testing.
 * Includes realistic Brazilian market indexes with various types and periodicities.
 * @internal
 */
export const MOCK_INDEXADORES: Indexador[] = [
  {
    id: 1,
    codigo: 'CDI',
    nome: 'Certificado de Deposito Interbancario',
    tipo: TipoIndexador.Juros,
    tipoDescricao: 'Juros',
    fonte: 'B3',
    periodicidade: Periodicidade.Diaria,
    periodicidadeDescricao: 'Diaria',
    fatorAcumulado: 1.000512374,
    dataBase: '2026-01-09T00:00:00Z',
    urlFonte: 'https://www.b3.com.br/pt_br/market-data-e-indices/servicos-de-dados/market-data/consultas/mercado-de-derivativos/indicadores/taxas-de-depositos-interfinanceiros-di/',
    importacaoAutomatica: true,
    ativo: true,
    createdAt: '2020-01-01T10:00:00Z',
    updatedAt: '2026-01-09T18:30:00Z',
    ultimoValor: 13.654823,
    ultimaData: '2026-01-09T00:00:00Z',
    historicoCount: 45,
  },
  {
    id: 2,
    codigo: 'SELIC',
    nome: 'Taxa SELIC',
    tipo: TipoIndexador.Juros,
    tipoDescricao: 'Juros',
    fonte: 'BACEN',
    periodicidade: Periodicidade.Diaria,
    periodicidadeDescricao: 'Diaria',
    fatorAcumulado: 1.000511854,
    dataBase: '2026-01-09T00:00:00Z',
    urlFonte: 'https://www.bcb.gov.br/controleinflacao/taxaselic',
    importacaoAutomatica: true,
    ativo: true,
    createdAt: '2020-01-01T10:00:00Z',
    updatedAt: '2026-01-09T18:30:00Z',
    ultimoValor: 13.251247,
    ultimaData: '2026-01-09T00:00:00Z',
    historicoCount: 45,
  },
  {
    id: 3,
    codigo: 'IPCA',
    nome: 'Indice de Precos ao Consumidor Amplo',
    tipo: TipoIndexador.Inflacao,
    tipoDescricao: 'Inflacao',
    fonte: 'IBGE',
    periodicidade: Periodicidade.Mensal,
    periodicidadeDescricao: 'Mensal',
    fatorAcumulado: null,
    dataBase: null,
    urlFonte: 'https://www.ibge.gov.br/indicadores',
    importacaoAutomatica: true,
    ativo: true,
    createdAt: '2020-01-01T10:00:00Z',
    updatedAt: '2025-12-15T10:00:00Z',
    ultimoValor: 0.5234,
    ultimaData: '2025-12-01T00:00:00Z',
    historicoCount: 24,
  },
  {
    id: 4,
    codigo: 'IGPM',
    nome: 'Indice Geral de Precos - Mercado',
    tipo: TipoIndexador.Inflacao,
    tipoDescricao: 'Inflacao',
    fonte: 'FGV',
    periodicidade: Periodicidade.Mensal,
    periodicidadeDescricao: 'Mensal',
    fatorAcumulado: null,
    dataBase: null,
    urlFonte: 'https://portalibre.fgv.br/',
    importacaoAutomatica: true,
    ativo: true,
    createdAt: '2020-01-01T10:00:00Z',
    updatedAt: '2025-12-15T10:00:00Z',
    ultimoValor: 0.4812,
    ultimaData: '2025-12-01T00:00:00Z',
    historicoCount: 24,
  },
  {
    id: 5,
    codigo: 'INPC',
    nome: 'Indice Nacional de Precos ao Consumidor',
    tipo: TipoIndexador.Inflacao,
    tipoDescricao: 'Inflacao',
    fonte: 'IBGE',
    periodicidade: Periodicidade.Mensal,
    periodicidadeDescricao: 'Mensal',
    fatorAcumulado: null,
    dataBase: null,
    urlFonte: null,
    importacaoAutomatica: false,
    ativo: true,
    createdAt: '2020-01-01T10:00:00Z',
    updatedAt: null,
    ultimoValor: null,
    ultimaData: null,
    historicoCount: 0,
  },
  {
    id: 6,
    codigo: 'PTAX',
    nome: 'Taxa de Cambio PTAX',
    tipo: TipoIndexador.Cambio,
    tipoDescricao: 'Cambio',
    fonte: 'BACEN',
    periodicidade: Periodicidade.Diaria,
    periodicidadeDescricao: 'Diaria',
    fatorAcumulado: null,
    dataBase: null,
    urlFonte: 'https://www.bcb.gov.br/estabilidadefinanceira/cotacoes',
    importacaoAutomatica: true,
    ativo: true,
    createdAt: '2020-01-01T10:00:00Z',
    updatedAt: '2026-01-09T18:30:00Z',
    ultimoValor: 5.853421,
    ultimaData: '2026-01-09T00:00:00Z',
    historicoCount: 45,
  },
  {
    id: 7,
    codigo: 'IBOVESPA',
    nome: 'Indice Bovespa',
    tipo: TipoIndexador.IndiceBolsa,
    tipoDescricao: 'Indice Bolsa',
    fonte: 'B3',
    periodicidade: Periodicidade.Diaria,
    periodicidadeDescricao: 'Diaria',
    fatorAcumulado: null,
    dataBase: null,
    urlFonte: 'https://www.b3.com.br/pt_br/market-data-e-indices/indices/indices-amplos/ibovespa.htm',
    importacaoAutomatica: true,
    ativo: true,
    createdAt: '2020-01-01T10:00:00Z',
    updatedAt: '2026-01-09T18:30:00Z',
    ultimoValor: 128543.12,
    ultimaData: '2026-01-09T00:00:00Z',
    historicoCount: 45,
  },
  {
    id: 8,
    codigo: 'IBRX100',
    nome: 'Indice Brasil 100',
    tipo: TipoIndexador.IndiceBolsa,
    tipoDescricao: 'Indice Bolsa',
    fonte: 'B3',
    periodicidade: Periodicidade.Diaria,
    periodicidadeDescricao: 'Diaria',
    fatorAcumulado: null,
    dataBase: null,
    urlFonte: null,
    importacaoAutomatica: false,
    ativo: true,
    createdAt: '2020-01-01T10:00:00Z',
    updatedAt: null,
    ultimoValor: null,
    ultimaData: null,
    historicoCount: 0,
  },
  {
    id: 9,
    codigo: 'SMLL',
    nome: 'Indice Small Cap',
    tipo: TipoIndexador.IndiceBolsa,
    tipoDescricao: 'Indice Bolsa',
    fonte: 'B3',
    periodicidade: Periodicidade.Diaria,
    periodicidadeDescricao: 'Diaria',
    fatorAcumulado: null,
    dataBase: null,
    urlFonte: null,
    importacaoAutomatica: false,
    ativo: false,
    createdAt: '2020-01-01T10:00:00Z',
    updatedAt: '2024-06-01T10:00:00Z',
    ultimoValor: null,
    ultimaData: null,
    historicoCount: 0,
  },
  {
    id: 10,
    codigo: 'IMAB',
    nome: 'Indice de Mercado ANBIMA - Indice Base',
    tipo: TipoIndexador.IndiceRendaFixa,
    tipoDescricao: 'Indice Renda Fixa',
    fonte: 'ANBIMA',
    periodicidade: Periodicidade.Diaria,
    periodicidadeDescricao: 'Diaria',
    fatorAcumulado: null,
    dataBase: null,
    urlFonte: 'https://www.anbima.com.br/pt_br/informar/ima-indice-de-mercado-anbima.htm',
    importacaoAutomatica: true,
    ativo: true,
    createdAt: '2020-01-01T10:00:00Z',
    updatedAt: '2026-01-09T18:30:00Z',
    ultimoValor: 8234.567891,
    ultimaData: '2026-01-09T00:00:00Z',
    historicoCount: 45,
  },
  {
    id: 11,
    codigo: 'IMAS',
    nome: 'Indice de Mercado ANBIMA - Selic',
    tipo: TipoIndexador.IndiceRendaFixa,
    tipoDescricao: 'Indice Renda Fixa',
    fonte: 'ANBIMA',
    periodicidade: Periodicidade.Diaria,
    periodicidadeDescricao: 'Diaria',
    fatorAcumulado: null,
    dataBase: null,
    urlFonte: 'https://www.anbima.com.br/pt_br/informar/ima-indice-de-mercado-anbima.htm',
    importacaoAutomatica: true,
    ativo: true,
    createdAt: '2020-01-01T10:00:00Z',
    updatedAt: '2026-01-09T18:30:00Z',
    ultimoValor: 5678.901234,
    ultimaData: '2026-01-09T00:00:00Z',
    historicoCount: 45,
  },
  {
    id: 12,
    codigo: 'BTC',
    nome: 'Bitcoin',
    tipo: TipoIndexador.Crypto,
    tipoDescricao: 'Crypto',
    fonte: 'MANUAL',
    periodicidade: Periodicidade.Diaria,
    periodicidadeDescricao: 'Diaria',
    fatorAcumulado: null,
    dataBase: null,
    urlFonte: null,
    importacaoAutomatica: false,
    ativo: true,
    createdAt: '2023-01-01T10:00:00Z',
    updatedAt: null,
    ultimoValor: null,
    ultimaData: null,
    historicoCount: 0,
  },
  {
    id: 13,
    codigo: 'TRPL',
    nome: 'Taxa Referencial Especial',
    tipo: TipoIndexador.Outro,
    tipoDescricao: 'Outro',
    fonte: 'BACEN',
    periodicidade: Periodicidade.Diaria,
    periodicidadeDescricao: 'Diaria',
    fatorAcumulado: null,
    dataBase: null,
    urlFonte: null,
    importacaoAutomatica: false,
    ativo: false,
    createdAt: '2020-01-01T10:00:00Z',
    updatedAt: '2023-12-31T10:00:00Z',
    ultimoValor: null,
    ultimaData: null,
    historicoCount: 0,
  },
];

/**
 * Helper function to generate daily history data for an indexador
 */
function generateDailyHistory(
  indexadorId: number,
  indexadorCodigo: string,
  baseValue: number,
  volatility: number,
  days: number,
  fonte: string
): HistoricoIndexador[] {
  const history: HistoricoIndexador[] = [];
  let currentValue = baseValue;
  const startDate = new Date('2026-01-10');

  for (let i = 0; i < days; i++) {
    const date = new Date(startDate);
    date.setDate(date.getDate() - i);

    // Skip weekends
    if (date.getDay() === 0 || date.getDay() === 6) continue;

    // Random daily variation
    const change = (Math.random() - 0.5) * volatility;
    currentValue = currentValue * (1 + change);

    const fatorDiario = 1 + Math.random() * 0.001;
    const variacaoPercentual = change * 100;

    history.push({
      id: history.length + 1 + indexadorId * 1000,
      indexadorId,
      indexadorCodigo,
      dataReferencia: date.toISOString().split('T')[0] + 'T00:00:00Z',
      valor: parseFloat(currentValue.toFixed(8)),
      fatorDiario: parseFloat(fatorDiario.toFixed(12)),
      variacaoPercentual: parseFloat(variacaoPercentual.toFixed(6)),
      fonte,
      importacaoId: null,
      createdAt: date.toISOString(),
    });
  }

  return history;
}

/**
 * Helper function to generate monthly history data for an indexador
 */
function generateMonthlyHistory(
  indexadorId: number,
  indexadorCodigo: string,
  baseValue: number,
  volatility: number,
  months: number,
  fonte: string
): HistoricoIndexador[] {
  const history: HistoricoIndexador[] = [];
  let currentValue = baseValue;
  const startDate = new Date('2026-01-01');

  for (let i = 0; i < months; i++) {
    const date = new Date(startDate);
    date.setMonth(date.getMonth() - i);

    // Random monthly variation
    const change = (Math.random() - 0.3) * volatility; // Slight positive bias for inflation
    currentValue = currentValue + change;

    history.push({
      id: history.length + 1 + indexadorId * 1000,
      indexadorId,
      indexadorCodigo,
      dataReferencia: date.toISOString().split('T')[0] + 'T00:00:00Z',
      valor: parseFloat(currentValue.toFixed(4)),
      fatorDiario: null,
      variacaoPercentual: parseFloat((change / (currentValue - change) * 100).toFixed(4)),
      fonte,
      importacaoId: null,
      createdAt: date.toISOString(),
    });
  }

  return history;
}

/**
 * Mock historical data for indexadores
 * @internal
 */
export const MOCK_HISTORICO_INDEXADORES: HistoricoIndexador[] = [
  // CDI - 30 days of daily data
  ...generateDailyHistory(1, 'CDI', 13.65, 0.001, 45, 'B3'),
  // SELIC - 30 days of daily data
  ...generateDailyHistory(2, 'SELIC', 13.25, 0.001, 45, 'BACEN'),
  // IPCA - 12 months of monthly data
  ...generateMonthlyHistory(3, 'IPCA', 0.52, 0.3, 24, 'IBGE'),
  // IGP-M - 12 months of monthly data
  ...generateMonthlyHistory(4, 'IGPM', 0.48, 0.4, 24, 'FGV'),
  // PTAX - 30 days of daily data
  ...generateDailyHistory(6, 'PTAX', 5.85, 0.02, 45, 'BACEN'),
  // IBOVESPA - 30 days of daily data
  ...generateDailyHistory(7, 'IBOVESPA', 128500, 0.015, 45, 'B3'),
  // IMA-B - 30 days of daily data
  ...generateDailyHistory(10, 'IMAB', 8234.56, 0.005, 45, 'ANBIMA'),
  // IMA-S - 30 days of daily data
  ...generateDailyHistory(11, 'IMAS', 5678.90, 0.003, 45, 'ANBIMA'),
];
