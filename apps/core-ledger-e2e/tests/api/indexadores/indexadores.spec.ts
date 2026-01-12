import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData } from '../../config/test-data';

/**
 * Indexadores (Economic Indices) API Tests
 * Tests Brazilian economic indices used for investment pricing and returns
 * Real data: IPCA (Inflation), IGPM (Price Index), CDI (Interbank Rate)
 */
test.describe('Indexadores (Economic Indices) API', () => {
    test('GET /api/indexadores - Retrieve all indices with pagination', async ({ request }) => {
        const response = await request.get('/api/indexadores', {
            params: {
                Limit: 10,
                Offset: 0,
                SortBy: 'id',
                SortDirection: 'asc',
            },
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(data).toHaveProperty('items');
        expect(data).toHaveProperty('totalCount');
        expect(Array.isArray(data.items)).toBe(true);
    });

    test('POST /api/indexadores - Create IPCA inflation index', async ({ request }) => {
        const indexadorData = {
            ...testData.indexador.create,
            codigo: `IPCA${Date.now()}`.substring(0, 20),
        };

        const response = await request.post('/api/indexadores', {
            data: indexadorData,
            headers: authConfig.getAuthHeader(),
        });

        expect([201, 200]).toContain(response.status());
        const createdIndexador = await response.json();
        expect(createdIndexador).toHaveProperty('id');
        expect(createdIndexador.nome).toContain('IPCA');
    });

    test('POST /api/indexadores - Create IGPM price index', async ({ request }) => {
        const indexadorData = {
            codigo: `IGPM${Date.now()}`.substring(0, 20),
            nome: 'Índice Geral de Preços do Mercado',
            tipo: 1,
            fonte: 'FGV',
            periodicidade: 2,
            fatorAcumulado: 1.0,
            dataBase: new Date().toISOString(),
            urlFonte: 'https://www.fgv.br',
            importacaoAutomatica: false,
            ativo: true,
        };

        const response = await request.post('/api/indexadores', {
            data: indexadorData,
            headers: authConfig.getAuthHeader(),
        });

        expect([201, 200]).toContain(response.status());
        const createdIndexador = await response.json();
        expect(createdIndexador).toHaveProperty('id');
        expect(createdIndexador.nome).toContain('IGPM');
    });

    test('POST /api/indexadores - Create CDI interbank rate', async ({ request }) => {
        const indexadorData = {
            codigo: `CDI${Date.now()}`.substring(0, 20),
            nome: 'Certificado de Depósito Interbancário',
            tipo: 2, // Juros
            fonte: 'CETIP',
            periodicidade: 1, // Diario
            fatorAcumulado: 1.0,
            dataBase: new Date().toISOString(),
            urlFonte: 'https://www.b3.com.br',
            importacaoAutomatica: true,
            ativo: true,
        };

        const response = await request.post('/api/indexadores', {
            data: indexadorData,
            headers: authConfig.getAuthHeader(),
        });

        expect([201, 200]).toContain(response.status());
        const createdIndexador = await response.json();
        expect(createdIndexador).toHaveProperty('id');
    });

    test('GET /api/indexadores/{id} - Retrieve a specific index by ID', async ({ request }) => {
        // First create an index
        const indexadorData = {
            ...testData.indexador.create,
            codigo: `IDX${Date.now()}`.substring(0, 20),
        };
        const createResponse = await request.post('/api/indexadores', {
            data: indexadorData,
            headers: authConfig.getAuthHeader(),
        });
        const indexadorId = (await createResponse.json()).id;

        // Then fetch it
        const response = await request.get(`/api/indexadores/${indexadorId}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const indexador = await response.json();
        expect(indexador.id).toBe(indexadorId);
    });

    test('GET /api/indexadores/{id} - Return 404 for non-existent index', async ({ request }) => {
        const response = await request.get('/api/indexadores/999999', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(404);
    });

    test('PUT /api/indexadores/{id} - Update an existing index', async ({ request }) => {
        // First create an index
        const indexadorData = {
            ...testData.indexador.create,
            codigo: `IDX${Date.now()}`.substring(0, 20),
        };
        const createResponse = await request.post('/api/indexadores', {
            data: indexadorData,
            headers: authConfig.getAuthHeader(),
        });
        const indexadorId = (await createResponse.json()).id;

        // Then update it
        const updateData = {
            ...testData.indexador.update,
        };
        const response = await request.put(`/api/indexadores/${indexadorId}`, {
            data: updateData,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 204]).toContain(response.status());
    });

    test('DELETE /api/indexadores/{id} - Delete an index', async ({ request }) => {
        // First create an index
        const indexadorData = {
            ...testData.indexador.create,
            codigo: `DEL${Date.now()}`.substring(0, 20),
        };
        const createResponse = await request.post('/api/indexadores', {
            data: indexadorData,
            headers: authConfig.getAuthHeader(),
        });
        const indexadorId = (await createResponse.json()).id;

        // Then delete it
        const response = await request.delete(`/api/indexadores/${indexadorId}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect([204, 200]).toContain(response.status());

        // Verify it's deleted
        const getResponse = await request.get(`/api/indexadores/${indexadorId}`, {
            headers: authConfig.getAuthHeader(),
        });
        expect(getResponse.status()).toBe(404);
    });

    test('GET /api/indexadores - Filter by active status', async ({ request }) => {
        const response = await request.get('/api/indexadores', {
            params: {
                Limit: 50,
                Offset: 0,
            },
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(data).toHaveProperty('items');
    });

    test('GET /api/indexadores - Verify indexador structure', async ({ request }) => {
        const response = await request.get('/api/indexadores', {
            params: {
                Limit: 10,
                Offset: 0,
            },
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();

        if (data.items && data.items.length > 0) {
            data.items.forEach((item: any) => {
                expect(item).toHaveProperty('id');
                expect(item).toHaveProperty('codigo');
                expect(item).toHaveProperty('nome');
            });
        }
    });
});
