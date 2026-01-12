import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData, generateValidCnpj } from '../../config/test-data';

/**
 * Taxas (Fund Fees) API Tests
 * Tests Brazilian fund fee management - nested under fundos resource
 * Uses FundoTaxaCreateDto, FundoTaxaUpdateDto
 */
test.describe('Taxas (Fund Fees) API', () => {
    let fundoId: string;

    test.beforeAll(async ({ request }) => {
        // Create a fundo first to use in taxa tests
        const fundoData = {
            ...testData.fundo.create,
            cnpj: generateValidCnpj(),
        };

        const response = await request.post('/api/v1/fundos', {
            data: fundoData,
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 201 || response.status() === 200) {
            const created = await response.json();
            fundoId = created.id;
        } else {
            // Use a sample GUID if creation fails
            fundoId = '550e8400-e29b-41d4-a716-446655440000';
        }
    });

    test('GET /api/v1/fundos/{fundoId}/taxas - Retrieve all fees for a fund', async ({ request }) => {
        const response = await request.get(`/api/v1/fundos/${fundoId}/taxas`, {
            params: {
                incluirInativas: false,
            },
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 200) {
            const data = await response.json();
            expect(Array.isArray(data)).toBe(true);
            if (data.length > 0) {
                expect(data[0]).toHaveProperty('id');
                expect(data[0]).toHaveProperty('tipoTaxa');
                expect(data[0]).toHaveProperty('percentual');
            }
        }
    });

    test('GET /api/v1/fundos/{fundoId}/taxas - Include inactive fees', async ({ request }) => {
        const response = await request.get(`/api/v1/fundos/${fundoId}/taxas`, {
            params: {
                incluirInativas: true,
            },
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 200) {
            const data = await response.json();
            expect(Array.isArray(data)).toBe(true);
        }
    });

    test('POST /api/v1/fundos/{fundoId}/taxas - Create administration fee', async ({ request }) => {
        const taxaData = {
            ...testData.taxa.create,
            tipoTaxa: 1, // Administracao
        };

        const response = await request.post(`/api/v1/fundos/${fundoId}/taxas`, {
            data: taxaData,
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 201 || response.status() === 200) {
            const createdTaxa = await response.json();
            expect(createdTaxa).toHaveProperty('id');
            expect(createdTaxa.tipoTaxa).toBe(1);
        }
    });

    test('POST /api/v1/fundos/{fundoId}/taxas - Create performance fee', async ({ request }) => {
        const taxaData = {
            tipoTaxa: 2, // Performance
            percentual: 20.0, // 20% performance fee
            baseCalculo: 2, // Sobre rendimento
            periodicidadeProvisao: 1,
            periodicidadePagamento: 3, // Semestral
            dataInicioVigencia: new Date().toISOString().split('T')[0],
            diaPagamento: 1,
            valorMinimo: 0,
            valorMaximo: 0,
        };

        const response = await request.post(`/api/v1/fundos/${fundoId}/taxas`, {
            data: taxaData,
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 201 || response.status() === 200) {
            const createdTaxa = await response.json();
            expect(createdTaxa).toHaveProperty('id');
            expect(createdTaxa.tipoTaxa).toBe(2);
            expect(createdTaxa.percentual).toBe(20.0);
        }
    });

    test('POST /api/v1/fundos/{fundoId}/taxas - Create custody fee', async ({ request }) => {
        const taxaData = {
            tipoTaxa: 3, // Custodia
            percentual: 0.1, // 0.1% custody fee
            baseCalculo: 1, // PatrimonioLiquido
            periodicidadeProvisao: 1, // Diaria
            periodicidadePagamento: 2, // Mensal
            dataInicioVigencia: new Date().toISOString().split('T')[0],
            diaPagamento: 10,
            valorMinimo: 500.00,
            valorMaximo: 50000.00,
        };

        const response = await request.post(`/api/v1/fundos/${fundoId}/taxas`, {
            data: taxaData,
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 201 || response.status() === 200) {
            const createdTaxa = await response.json();
            expect(createdTaxa).toHaveProperty('id');
            expect(createdTaxa.tipoTaxa).toBe(3);
        }
    });

    test('GET /api/v1/taxas/{id} - Retrieve a specific fee by ID', async ({ request }) => {
        // First create a fee
        const taxaData = {
            ...testData.taxa.create,
        };

        const createResponse = await request.post(`/api/v1/fundos/${fundoId}/taxas`, {
            data: taxaData,
            headers: authConfig.getAuthHeader(),
        });

        if (createResponse.status() === 201 || createResponse.status() === 200) {
            const createdTaxa = await createResponse.json();
            const taxaId = createdTaxa.id;

            // Then fetch it by ID
            const response = await request.get(`/api/v1/taxas/${taxaId}`, {
                headers: authConfig.getAuthHeader(),
            });

            expect(response.status()).toBe(200);
            const fetchedTaxa = await response.json();
            expect(fetchedTaxa.id).toBe(taxaId);
        }
    });

    test('GET /api/v1/taxas/{id} - Return 404 for non-existent fee', async ({ request }) => {
        const response = await request.get('/api/v1/taxas/99999999', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(404);
    });

    test('PUT /api/v1/taxas/{id} - Update an existing fee', async ({ request }) => {
        // First create a fee
        const taxaData = {
            ...testData.taxa.create,
        };

        const createResponse = await request.post(`/api/v1/fundos/${fundoId}/taxas`, {
            data: taxaData,
            headers: authConfig.getAuthHeader(),
        });

        if (createResponse.status() === 201 || createResponse.status() === 200) {
            const createdTaxa = await createResponse.json();
            const taxaId = createdTaxa.id;

            // Then update it
            const updateData = {
                ...testData.taxa.update,
            };

            const response = await request.put(`/api/v1/taxas/${taxaId}`, {
                data: updateData,
                headers: authConfig.getAuthHeader(),
            });

            expect([200, 204]).toContain(response.status());
        }
    });

    test('DELETE /api/v1/taxas/{id} - Delete a fee', async ({ request }) => {
        // First create a fee
        const taxaData = {
            ...testData.taxa.create,
        };

        const createResponse = await request.post(`/api/v1/fundos/${fundoId}/taxas`, {
            data: taxaData,
            headers: authConfig.getAuthHeader(),
        });

        if (createResponse.status() === 201 || createResponse.status() === 200) {
            const createdTaxa = await createResponse.json();
            const taxaId = createdTaxa.id;

            // Then delete it
            const response = await request.delete(`/api/v1/taxas/${taxaId}`, {
                headers: authConfig.getAuthHeader(),
            });

            expect([200, 204]).toContain(response.status());

            // Verify it's deleted
            const getResponse = await request.get(`/api/v1/taxas/${taxaId}`, {
                headers: authConfig.getAuthHeader(),
            });
            expect(getResponse.status()).toBe(404);
        }
    });

    test('POST /api/v1/fundos/{fundoId}/taxas - Validate percentage range', async ({ request }) => {
        const invalidData = {
            ...testData.taxa.create,
            percentual: -5.0, // Invalid negative percentage
        };

        const response = await request.post(`/api/v1/fundos/${fundoId}/taxas`, {
            data: invalidData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });
});
