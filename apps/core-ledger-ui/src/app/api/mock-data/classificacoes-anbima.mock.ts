import {
  ClassificacaoAnbimaOption,
  ClassificacaoCvm,
} from '../../features/cadastro/fundos/wizard/models/classificacao.model';

/**
 * Mock ANBIMA classifications data organized by CVM classification.
 * Based on ANBIMA's official fund classification taxonomy.
 * Used for local development and testing without backend dependency.
 * @internal
 */
export const MOCK_CLASSIFICACOES_ANBIMA: Record<ClassificacaoCvm, ClassificacaoAnbimaOption[]> = {
  [ClassificacaoCvm.RENDA_FIXA]: [
    {
      codigo: 'RF_SIMPLES',
      nome: 'Renda Fixa Simples',
      nivel1: 'Renda Fixa',
      nivel2: 'Simples',
    },
    {
      codigo: 'RF_INDEXADO',
      nome: 'Renda Fixa Indexado',
      nivel1: 'Renda Fixa',
      nivel2: 'Indexado',
    },
    {
      codigo: 'RF_ATIVO_BAIXA',
      nome: 'Renda Fixa Ativo Duracao Baixa',
      nivel1: 'Renda Fixa',
      nivel2: 'Ativo',
      nivel3: 'Duracao Baixa',
    },
    {
      codigo: 'RF_ATIVO_MED',
      nome: 'Renda Fixa Ativo Duracao Media',
      nivel1: 'Renda Fixa',
      nivel2: 'Ativo',
      nivel3: 'Duracao Media',
    },
    {
      codigo: 'RF_ATIVO_ALTA',
      nome: 'Renda Fixa Ativo Duracao Alta',
      nivel1: 'Renda Fixa',
      nivel2: 'Ativo',
      nivel3: 'Duracao Alta',
    },
    {
      codigo: 'RF_ATIVO_LIVRE',
      nome: 'Renda Fixa Ativo Duracao Livre',
      nivel1: 'Renda Fixa',
      nivel2: 'Ativo',
      nivel3: 'Duracao Livre',
    },
    {
      codigo: 'RF_CREDITO_GI',
      nome: 'Renda Fixa Credito Grau de Investimento',
      nivel1: 'Renda Fixa',
      nivel2: 'Credito',
      nivel3: 'Grau de Investimento',
    },
    {
      codigo: 'RF_CREDITO_LIVRE',
      nome: 'Renda Fixa Credito Livre',
      nivel1: 'Renda Fixa',
      nivel2: 'Credito',
      nivel3: 'Livre',
    },
    {
      codigo: 'RF_SOBERANO',
      nome: 'Renda Fixa Divida Externa',
      nivel1: 'Renda Fixa',
      nivel2: 'Divida Externa',
    },
  ],

  [ClassificacaoCvm.ACOES]: [
    {
      codigo: 'AC_INDEXADO',
      nome: 'Acoes Indexado',
      nivel1: 'Acoes',
      nivel2: 'Indexado',
    },
    {
      codigo: 'AC_ATIVO',
      nome: 'Acoes Ativo',
      nivel1: 'Acoes',
      nivel2: 'Ativo',
    },
    {
      codigo: 'AC_VALOR',
      nome: 'Acoes Valor/Crescimento',
      nivel1: 'Acoes',
      nivel2: 'Valor/Crescimento',
    },
    {
      codigo: 'AC_SETORIAL',
      nome: 'Acoes Setoriais',
      nivel1: 'Acoes',
      nivel2: 'Setoriais',
    },
    {
      codigo: 'AC_DIVIDENDOS',
      nome: 'Acoes Dividendos',
      nivel1: 'Acoes',
      nivel2: 'Dividendos',
    },
    {
      codigo: 'AC_SMALL',
      nome: 'Acoes Small Caps',
      nivel1: 'Acoes',
      nivel2: 'Small Caps',
    },
    {
      codigo: 'AC_FMP',
      nome: 'Acoes FMP-FGTS',
      nivel1: 'Acoes',
      nivel2: 'FMP-FGTS',
    },
    {
      codigo: 'AC_LIVRE',
      nome: 'Acoes Livre',
      nivel1: 'Acoes',
      nivel2: 'Livre',
    },
    {
      codigo: 'AC_EXT',
      nome: 'Acoes Investimento no Exterior',
      nivel1: 'Acoes',
      nivel2: 'Investimento no Exterior',
    },
  ],

  [ClassificacaoCvm.MULTIMERCADO]: [
    {
      codigo: 'MM_MACRO',
      nome: 'Multimercado Macro',
      nivel1: 'Multimercado',
      nivel2: 'Macro',
    },
    {
      codigo: 'MM_TRADING',
      nome: 'Multimercado Trading',
      nivel1: 'Multimercado',
      nivel2: 'Trading',
    },
    {
      codigo: 'MM_LONG_SHORT',
      nome: 'Multimercado Long Short Neutro',
      nivel1: 'Multimercado',
      nivel2: 'Long Short',
      nivel3: 'Neutro',
    },
    {
      codigo: 'MM_LONG_SHORT_DIR',
      nome: 'Multimercado Long Short Direcional',
      nivel1: 'Multimercado',
      nivel2: 'Long Short',
      nivel3: 'Direcional',
    },
    {
      codigo: 'MM_JUROS',
      nome: 'Multimercado Juros e Moedas',
      nivel1: 'Multimercado',
      nivel2: 'Juros e Moedas',
    },
    {
      codigo: 'MM_LIVRE',
      nome: 'Multimercado Livre',
      nivel1: 'Multimercado',
      nivel2: 'Livre',
    },
    {
      codigo: 'MM_CAPITAL',
      nome: 'Multimercado Capital Protegido',
      nivel1: 'Multimercado',
      nivel2: 'Capital Protegido',
    },
    {
      codigo: 'MM_ESTRATEGIA',
      nome: 'Multimercado Estrategia Especifica',
      nivel1: 'Multimercado',
      nivel2: 'Estrategia Especifica',
    },
    {
      codigo: 'MM_EXTERIOR',
      nome: 'Multimercado Investimento no Exterior',
      nivel1: 'Multimercado',
      nivel2: 'Investimento no Exterior',
    },
  ],

  [ClassificacaoCvm.CAMBIAL]: [
    {
      codigo: 'CAMBIAL',
      nome: 'Cambial',
      nivel1: 'Cambial',
      nivel2: 'Cambial',
    },
  ],

  [ClassificacaoCvm.PREVIDENCIA]: [
    {
      codigo: 'PREV_RF',
      nome: 'Previdencia Renda Fixa',
      nivel1: 'Previdencia',
      nivel2: 'Renda Fixa',
    },
    {
      codigo: 'PREV_BAL_15',
      nome: 'Previdencia Balanceados ate 15%',
      nivel1: 'Previdencia',
      nivel2: 'Balanceados',
      nivel3: 'Ate 15%',
    },
    {
      codigo: 'PREV_BAL_30',
      nome: 'Previdencia Balanceados 15 a 30%',
      nivel1: 'Previdencia',
      nivel2: 'Balanceados',
      nivel3: '15 a 30%',
    },
    {
      codigo: 'PREV_MM',
      nome: 'Previdencia Multimercados',
      nivel1: 'Previdencia',
      nivel2: 'Multimercados',
    },
    {
      codigo: 'PREV_ACOES',
      nome: 'Previdencia Acoes',
      nivel1: 'Previdencia',
      nivel2: 'Acoes',
    },
    {
      codigo: 'PREV_DATA',
      nome: 'Previdencia Data Alvo',
      nivel1: 'Previdencia',
      nivel2: 'Data Alvo',
    },
  ],

  // Classifications without ANBIMA options (empty arrays)
  [ClassificacaoCvm.FIDC]: [],
  [ClassificacaoCvm.FIP]: [],
  [ClassificacaoCvm.FII]: [],
  [ClassificacaoCvm.FIAGRO]: [],
  [ClassificacaoCvm.FI_INFRA]: [],
  [ClassificacaoCvm.ETF]: [],
};

/**
 * Get ANBIMA classifications filtered by CVM classification.
 * Returns empty array if CVM classification has no ANBIMA options.
 * @param classificacaoCvm - The CVM classification to filter by
 * @returns Array of ANBIMA classification options
 * @internal
 */
export function getClassificacoesAnbimaByCvm(
  classificacaoCvm: ClassificacaoCvm
): ClassificacaoAnbimaOption[] {
  return MOCK_CLASSIFICACOES_ANBIMA[classificacaoCvm] || [];
}
