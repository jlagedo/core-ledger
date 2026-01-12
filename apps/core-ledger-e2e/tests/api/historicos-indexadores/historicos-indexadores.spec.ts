import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData } from '../../config/test-data';

/**
 * Históricos Indexadores (Index History) API Tests
 * Tests historical values of Brazilian economic indices
 * Used for calculating returns and performance on indexed investments
 */
test.describe('Históricos Indexadores (Index History) API', () => {
    test('POST /api/historicos-indexadores - Create IPCA historical record', async ({ request }) => {
        const historicoData = {
            ...testData.historicoIndexador.create,
            dataReferencia: new Date().toISOString(),
        };

        const response = await request.post('/api/historicos-indexadores', {
            data: historicoData,
            headers: authConfig.getAuthHeader(),
        });

        expect([201, 200]).toContain(response.status());
        const createdHistorico = await response.json();
        expect(createdHistorico).toHaveProperty('id');
        expect(createdHistorico.valor).toBe(historicoData.valor);
    });

    test('POST /api/historicos-indexadores - Validate required fields', async ({ request }) => {
        const invalidData = {
            // Missing indexadorId
            dataReferencia: new Date().toISOString(),
            valor: 4.50,
        };

        const response = await request.post('/api/historicos-indexadores', {
            data: invalidData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });

    test('POST /api/historicos-indexadores - Create with fator diario', async ({ request }) => {
        const historicoData = {
            indexadorId: 1,
            dataReferencia: new Date().toISOString(),
            valor: 4.75,
            fatorDiario: 1.00015,
            variacaoPercentual: 0.015,
            fonte: 'IBGE',
        };

        const response = await request.post('/api/historicos-indexadores', {
            data: historicoData,
            headers: authConfig.getAuthHeader(),
        });

        expect([201, 200]).toContain(response.status());
    });

    test('POST /api/historicos-indexadores - Create multiple monthly records', async ({ request }) => {
        const baseDate = new Date('2024-01-01');
        const records = [];

        for (let i = 0; i < 3; i++) {
            const date = new Date(baseDate);
            date.setMonth(date.getMonth() + i);

            records.push({
                indexadorId: 1,
                dataReferencia: date.toISOString(),
                valor: 4.00 + (i * 0.5),
                fatorDiario: 1.0001,
                variacaoPercentual: 0.40 + (i * 0.05),
                fonte: 'IBGE',
            });
        }

        for (const record of records) {
            const response = await request.post('/api/historicos-indexadores', {
                data: record,
                headers: authConfig.getAuthHeader(),
            });

            expect([201, 200]).toContain(response.status());
        }
    });

    test('POST /api/historicos-indexadores - Create CDI historical record', async ({ request }) => {
        const historicoData = {
            indexadorId: 2, // CDI
            dataReferencia: new Date().toISOString(),
            valor: 13.75, // Current Selic/CDI rate
            fatorDiario: 1.000509,
            variacaoPercentual: 0.05,
            fonte: 'CETIP',
        };

        const response = await request.post('/api/historicos-indexadores', {
            data: historicoData,
            headers: authConfig.getAuthHeader(),
        });

        expect([201, 200]).toContain(response.status());
    });

    test('POST /api/historicos-indexadores - Validate non-negative value', async ({ request }) => {
        const invalidData = {
            indexadorId: 1,
            dataReferencia: new Date().toISOString(),
            valor: -5.0, // Invalid negative value
            fatorDiario: 1.0,
            fonte: 'IBGE',
        };

        const response = await request.post('/api/historicos-indexadores', {
            data: invalidData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });
});
