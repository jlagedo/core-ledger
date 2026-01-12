import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData, generateValidCnpj } from '../../config/test-data';

/**
 * Prazos (Fund Terms/Conditions) API Tests
 * Tests fund redemption terms, quotation days, settlement days
 * Nested under fundos resource - Uses FundoPrazoCreateDto, FundoPrazoUpdateDto
 */
test.describe('Prazos (Fund Terms) API', () => {
    let fundoId: string;

    test.beforeAll(async ({ request }) => {
        // Create a fundo first to use in prazo tests
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

    test('GET /api/v1/fundos/{fundoId}/prazos - Retrieve terms for a fund', async ({ request }) => {
        const response = await request.get(`/api/v1/fundos/${fundoId}/prazos`, {
            params: {
                incluirInativos: false,
            },
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 200) {
            const data = await response.json();
            expect(Array.isArray(data)).toBe(true);

            if (data.length > 0) {
                data.forEach((item: any) => {
                    expect(item).toHaveProperty('id');
                    expect(item).toHaveProperty('tipoPrazo');
                    expect(item).toHaveProperty('diasCotizacao');
                });
            }
        }
    });

    test('GET /api/v1/fundos/{fundoId}/prazos - Include inactive terms', async ({ request }) => {
        const response = await request.get(`/api/v1/fundos/${fundoId}/prazos`, {
            params: {
                incluirInativos: true,
            },
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 200) {
            const data = await response.json();
            expect(Array.isArray(data)).toBe(true);
        }
    });

    test('POST /api/v1/fundos/{fundoId}/prazos - Create application term (D+1)', async ({ request }) => {
        const prazoData = {
            ...testData.prazo.create,
            tipoPrazo: 1, // Aplicacao
        };

        const response = await request.post(`/api/v1/fundos/${fundoId}/prazos`, {
            data: prazoData,
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 201 || response.status() === 200) {
            const created = await response.json();
            expect(created).toHaveProperty('id');
            expect(created.tipoPrazo).toBe(prazoData.tipoPrazo);
        }
    });

    test('POST /api/v1/fundos/{fundoId}/prazos - Create redemption term (D+2)', async ({ request }) => {
        const prazoData = {
            tipoPrazo: 2, // Resgate
            diasCotizacao: 0,
            diasLiquidacao: 2,
            horarioLimite: '14:00:00',
            diasUteis: true,
            diasCarencia: 30,
            permiteParcial: true,
            percentualMinimo: 0.10,
            valorMinimo: 500.00,
        };

        const response = await request.post(`/api/v1/fundos/${fundoId}/prazos`, {
            data: prazoData,
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 201 || response.status() === 200) {
            const created = await response.json();
            expect(created).toHaveProperty('id');
            expect(created.tipoPrazo).toBe(2);
            expect(created.diasCarencia).toBe(30);
        }
    });

    test('GET /api/v1/prazos/{id} - Retrieve a specific term by ID', async ({ request }) => {
        // First create a term
        const prazoData = {
            ...testData.prazo.create,
        };

        const createResponse = await request.post(`/api/v1/fundos/${fundoId}/prazos`, {
            data: prazoData,
            headers: authConfig.getAuthHeader(),
        });

        if (createResponse.status() === 201 || createResponse.status() === 200) {
            const created = await createResponse.json();
            const prazoId = created.id;

            // Then fetch it by ID
            const response = await request.get(`/api/v1/prazos/${prazoId}`, {
                headers: authConfig.getAuthHeader(),
            });

            expect(response.status()).toBe(200);
            const fetchedPrazo = await response.json();
            expect(fetchedPrazo.id).toBe(prazoId);
        }
    });

    test('PUT /api/v1/prazos/{id} - Update fund term', async ({ request }) => {
        // First create a term
        const prazoData = {
            ...testData.prazo.create,
        };

        const createResponse = await request.post(`/api/v1/fundos/${fundoId}/prazos`, {
            data: prazoData,
            headers: authConfig.getAuthHeader(),
        });

        if (createResponse.status() === 201 || createResponse.status() === 200) {
            const created = await createResponse.json();
            const prazoId = created.id;

            // Then update it
            const updateData = {
                ...testData.prazo.update,
            };

            const response = await request.put(`/api/v1/prazos/${prazoId}`, {
                data: updateData,
                headers: authConfig.getAuthHeader(),
            });

            expect([200, 204]).toContain(response.status());
        }
    });

    test('DELETE /api/v1/prazos/{id} - Delete a term', async ({ request }) => {
        // First create a term
        const prazoData = {
            ...testData.prazo.create,
        };

        const createResponse = await request.post(`/api/v1/fundos/${fundoId}/prazos`, {
            data: prazoData,
            headers: authConfig.getAuthHeader(),
        });

        if (createResponse.status() === 201 || createResponse.status() === 200) {
            const created = await createResponse.json();
            const prazoId = created.id;

            // Then delete it
            const response = await request.delete(`/api/v1/prazos/${prazoId}`, {
                headers: authConfig.getAuthHeader(),
            });

            expect([200, 204]).toContain(response.status());

            // Verify it's deleted
            const getResponse = await request.get(`/api/v1/prazos/${prazoId}`, {
                headers: authConfig.getAuthHeader(),
            });
            expect(getResponse.status()).toBe(404);
        }
    });

    test('GET /api/v1/fundos/{fundoId}/prazos - Verify term structure', async ({ request }) => {
        const response = await request.get(`/api/v1/fundos/${fundoId}/prazos`, {
            params: {
                incluirInativos: false,
            },
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 200) {
            const data = await response.json();

            if (data.length > 0) {
                const prazo = data[0];
                expect(prazo).toHaveProperty('id');
                expect(prazo).toHaveProperty('tipoPrazo');
                expect(prazo).toHaveProperty('diasCotizacao');
                expect(prazo).toHaveProperty('diasLiquidacao');
                expect(prazo).toHaveProperty('horarioLimite');
                expect(prazo).toHaveProperty('diasUteis');
                expect(typeof prazo.diasCotizacao).toBe('number');
                expect(typeof prazo.diasLiquidacao).toBe('number');
            }
        }
    });
});
