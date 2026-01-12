/**
 * Test data fixtures for API tests
 * Uses real Brazilian fund data, securities, and institutions
 * References: B3, Investidor10, ANBIMA, CVM
 * DTOs aligned with Swagger specification
 */

/**
 * Generates a valid Brazilian CNPJ with correct check digits
 * CNPJ format: XX.XXX.XXX/XXXX-YY (14 digits total)
 */
export function generateValidCnpj(): string {
    const base = Array.from({ length: 8 }, () => Math.floor(Math.random() * 10));
    const branch = [0, 0, 0, 1];
    const digits = [...base, ...branch];

    const weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    let sum1 = 0;
    for (let i = 0; i < 12; i++) {
        sum1 += digits[i] * weights1[i];
    }
    const remainder1 = sum1 % 11;
    const checkDigit1 = remainder1 < 2 ? 0 : 11 - remainder1;
    digits.push(checkDigit1);

    const weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    let sum2 = 0;
    for (let i = 0; i < 13; i++) {
        sum2 += digits[i] * weights2[i];
    }
    const remainder2 = sum2 % 11;
    const checkDigit2 = remainder2 < 2 ? 0 : 11 - remainder2;
    digits.push(checkDigit2);

    return digits.join('');
}

export const testData = {
    // CreateAccountDto - Account creation
    account: {
        create: {
            code: 1100,
            name: 'Caixa - Fundo de Ações',
            typeId: 1,
            status: 1, // AccountStatus enum: 1=Active, 2=Inactive
            normalBalance: 1, // NormalBalance enum: 1=Debit, 2=Credit
        },
        update: {
            code: 1101,
            name: 'Banco - Conta Corrente Investimentos',
            typeId: 1,
            status: 1,
            normalBalance: 1,
        },
    },

    // CreateAccountTypeDto - Account type creation
    accountType: {
        create: {
            description: 'Conta Bancária - Banco Itaú Unibanco',
        },
        update: {
            description: 'Conta Bancária - Banco Bradesco',
        },
    },

    // CreateFundDto - Fund creation (English API /api/funds)
    fund: {
        create: {
            code: 'XPML11',
            name: 'XP Malls Fundos de Investimento Imobiliário',
            baseCurrency: 'BRL',
            inceptionDate: '2012-11-28T00:00:00Z',
            valuationFrequency: 1, // ValuationFrequency enum: 1=Daily, 2=Monthly
        },
        update: {
            code: 'BTLG11',
            name: 'BTG Pactual Logística Fundo de Investimento Imobiliário',
            baseCurrency: 'BRL',
            inceptionDate: '2008-12-18T00:00:00Z',
            valuationFrequency: 1,
        },
    },

    // FundoCreateDto - Brazilian Fundo creation (/api/v1/fundos)
    fundo: {
        create: {
            cnpj: generateValidCnpj(),
            razaoSocial: 'Fundo de Investimento em Ações Teste',
            tipoFundo: 1, // TipoFundo enum: 1=FIA, 2=FIM, 3=FIRF, 4=FIC, 5=FIDC, 6=FIP, 7=FII
            classificacaoCVM: 1, // ClassificacaoCVM enum
            prazo: 1, // PrazoFundo enum: 1=CurtoPrazo, 2=LongoPrazo
            publicoAlvo: 1, // PublicoAlvo enum: 1=Geral, 2=Qualificado, 3=Profissional
            tributacao: 1, // TributacaoFundo enum: 1=CurtoPrazo, 2=LongoPrazo, 3=Acoes, 4=Isento
            condominio: 1, // TipoCondominio enum: 1=Aberto, 2=Fechado
            nomeFantasia: 'Fundo Teste FIA',
            nomeCurto: 'TESTE FIA',
            dataConstituicao: new Date().toISOString().split('T')[0],
            dataInicioAtividade: new Date().toISOString().split('T')[0],
            exclusivo: false,
            reservado: false,
            permiteAlavancagem: false,
            aceitaCripto: false,
            percentualExterior: 0,
        },
        update: {
            razaoSocial: 'Fundo de Investimento em Ações Atualizado',
            nomeFantasia: 'Fundo Atualizado FIA',
            nomeCurto: 'UPDT FIA',
            classificacaoCVM: 2,
            prazo: 2,
            publicoAlvo: 2,
            tributacao: 2,
            condominio: 1,
            exclusivo: true,
            reservado: false,
            permiteAlavancagem: true,
            aceitaCripto: false,
            percentualExterior: 10,
        },
    },

    // CreateSecurityDto - Security creation
    security: {
        create: {
            name: 'Petróleo Brasileiro S/A - Petrobras PN',
            ticker: 'PETR4',
            isin: 'BR0155900909',
            type: 1, // SecurityType enum: 1=Stock, 2=Bond, 3=ETF, etc.
            currency: 'BRL',
        },
        update: {
            name: 'Vale S/A ON',
            ticker: 'VALE3',
            isin: 'BR0110673005',
            type: 1,
            currency: 'BRL',
        },
    },

    // CreateTransactionDto - Transaction creation
    transaction: {
        create: {
            fundId: 1,
            securityId: 1,
            transactionSubTypeId: 1,
            tradeDate: new Date().toISOString(),
            settleDate: new Date(Date.now() + 86400000 * 2).toISOString(),
            quantity: 1000,
            price: 28.50,
            amount: 28500,
            currency: 'BRL',
        },
        update: {
            fundId: 1,
            securityId: 2,
            transactionSubTypeId: 1,
            tradeDate: new Date().toISOString(),
            settleDate: new Date(Date.now() + 86400000 * 2).toISOString(),
            quantity: 500,
            price: 62.75,
            amount: 31375,
            currency: 'BRL',
            statusId: 1,
        },
        dividend: {
            fundId: 1,
            securityId: 1,
            transactionSubTypeId: 3,
            tradeDate: new Date().toISOString(),
            settleDate: new Date().toISOString(),
            quantity: 1000,
            price: 1.50,
            amount: 1500,
            currency: 'BRL',
        },
        sale: {
            fundId: 1,
            securityId: 3,
            transactionSubTypeId: 2,
            tradeDate: new Date().toISOString(),
            settleDate: new Date(Date.now() + 86400000 * 2).toISOString(),
            quantity: 250,
            price: 35.20,
            amount: 8800,
            currency: 'BRL',
        },
    },

    // CreateInstituicaoDto - Institution creation
    instituicao: {
        create: {
            cnpj: generateValidCnpj(),
            razaoSocial: 'Itaú Unibanco S/A',
            nomeFantasia: 'Itaú',
            ativo: true,
        },
        update: {
            cnpj: generateValidCnpj(),
            razaoSocial: 'Banco Bradesco S/A',
            nomeFantasia: 'Bradesco',
            ativo: true,
        },
    },

    // CreateCalendarioDto - Calendar creation
    calendario: {
        create: {
            data: new Date(Date.now() + 86400000 * 30).toISOString().split('T')[0],
            tipoDia: 1, // TipoDia enum: 1=DiaUtil, 2=Feriado, etc.
            praca: 1, // Praca enum: 1=Nacional, 2=SP, 3=RJ, etc.
            descricao: 'Feriado Nacional',
        },
        update: {
            tipoDia: 2,
            descricao: 'Feriado Atualizado',
        },
    },

    // FundoClasseCreateDto - Fund class creation (nested under fundo)
    classe: {
        create: {
            codigoClasse: 'SENIOR',
            nomeClasse: 'Classe Sênior',
            cnpjClasse: generateValidCnpj(),
            tipoClasseFidc: 1, // TipoClasseFIDC enum: 1=Senior, 2=Subordinada, 3=Mezanino
            ordemSubordinacao: 1,
            rentabilidadeAlvo: 12.5,
            responsabilidadeLimitada: true,
            segregacaoPatrimonial: true,
            valorMinimoAplicacao: 1000.00,
        },
        update: {
            nomeClasse: 'Classe Sênior Atualizada',
            rentabilidadeAlvo: 15.0,
            responsabilidadeLimitada: true,
            segregacaoPatrimonial: true,
            valorMinimoAplicacao: 5000.00,
        },
    },

    // CreateIndexadorDto - Indexer creation
    indexador: {
        create: {
            codigo: 'IPCA',
            nome: 'Índice Nacional de Preços ao Consumidor Amplo',
            tipo: 1, // IndexadorTipo enum: 1=Inflacao, 2=Juros, etc.
            fonte: 'IBGE',
            periodicidade: 2, // Periodicidade enum: 1=Diario, 2=Mensal, 3=Anual
            fatorAcumulado: 1.0,
            dataBase: new Date().toISOString(),
            urlFonte: 'https://www.ibge.gov.br',
            importacaoAutomatica: false,
            ativo: true,
        },
        update: {
            nome: 'IPCA Atualizado',
            fonte: 'IBGE',
            fatorAcumulado: 1.05,
            importacaoAutomatica: true,
            ativo: true,
        },
    },

    // CreateHistoricoIndexadorDto - Index history creation
    historicoIndexador: {
        create: {
            indexadorId: 1,
            dataReferencia: new Date().toISOString(),
            valor: 4.50,
            fatorDiario: 1.0001,
            variacaoPercentual: 0.45,
            fonte: 'IBGE',
        },
    },

    // FundoTaxaCreateDto - Fund fee creation (nested under fundo)
    taxa: {
        create: {
            tipoTaxa: 1, // TipoTaxa enum: 1=Administracao, 2=Performance, etc.
            percentual: 2.0,
            baseCalculo: 1, // BaseCalculoTaxa enum: 1=PatrimonioLiquido, etc.
            periodicidadeProvisao: 1, // PeriodicidadeProvisao enum: 1=Diaria, 2=Mensal
            periodicidadePagamento: 2, // PeriodicidadePagamento enum
            dataInicioVigencia: new Date().toISOString().split('T')[0],
            diaPagamento: 15,
            valorMinimo: 1000.00,
            valorMaximo: 100000.00,
        },
        update: {
            percentual: 2.5,
            baseCalculo: 1,
            periodicidadeProvisao: 1,
            periodicidadePagamento: 2,
            diaPagamento: 20,
            valorMinimo: 2000.00,
            valorMaximo: 150000.00,
        },
    },

    // FundoPrazoCreateDto - Fund term creation (nested under fundo)
    prazo: {
        create: {
            tipoPrazo: 1, // TipoPrazoOperacional enum: 1=Aplicacao, 2=Resgate, 3=Cotizacao
            diasCotizacao: 1,
            diasLiquidacao: 2,
            horarioLimite: '14:00:00',
            diasUteis: true,
            diasCarencia: 0,
            permiteParcial: true,
            percentualMinimo: 0.01,
            valorMinimo: 100.00,
        },
        update: {
            diasCotizacao: 2,
            diasLiquidacao: 3,
            horarioLimite: '15:00:00',
            diasUteis: true,
            diasCarencia: 1,
            permiteParcial: false,
            percentualMinimo: 0.05,
            valorMinimo: 500.00,
        },
    },

    // CreateFundoVinculoDto - Fund institution link creation
    vinculo: {
        create: {
            fundoId: '550e8400-e29b-41d4-a716-446655440000',
            instituicaoId: 1,
            tipoVinculo: 1, // TipoVinculoInstitucional enum: 1=Administrador, 2=Gestor, 3=Custodiante, etc.
            dataInicio: new Date().toISOString().split('T')[0],
            contratoNumero: 'CONT001',
            observacao: 'Vínculo principal',
            principal: true,
        },
        encerrar: {
            dataFim: new Date().toISOString().split('T')[0],
        },
    },

    // TransactionProcessedNotification - Worker notification
    workerNotification: {
        transactionProcessed: {
            transactionId: 1,
            success: true,
            finalStatusId: 2, // 2=Executed, 8=Failed
            errorMessage: null,
            processedAt: new Date().toISOString(),
            correlationId: 'corr-123',
            createdByUserId: 'user-123',
        },
    },

    // TestConnectionRequest - Jobs ingestion
    jobsIngestion: {
        testConnection: {
            referenceId: 'TEST-B3-' + Date.now(),
            jobDescription: 'Test B3 Securities Import',
        },
    },
};
