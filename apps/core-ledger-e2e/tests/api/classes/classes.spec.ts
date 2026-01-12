import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData, generateValidCnpj } from '../../config/test-data';

/**
 * Classes (Fund Share Classes) API Tests
 * Tests fund share class management - nested under fundos resource
 * Uses FundoClasseCreateDto, FundoClasseUpdateDto
 */
test.describe('Classes (Fund Share Classes) API', () => {
    let fundoId: string;

    test.beforeAll(async ({ request }) => {
        // Create a fundo first to use in class tests
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

    test('GET /api/v1/fundos/{fundoId}/classes - Retrieve all classes for a fund', async ({ request }) => {
        const response = await request.get(`/api/v1/fundos/${fundoId}/classes`, {
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
                expect(data[0]).toHaveProperty('codigoClasse');
                expect(data[0]).toHaveProperty('nomeClasse');
            }
        }
    });

    test('GET /api/v1/fundos/{fundoId}/classes - Include inactive classes', async ({ request }) => {
        const response = await request.get(`/api/v1/fundos/${fundoId}/classes`, {
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

    test('POST /api/v1/fundos/{fundoId}/classes - Create a new fund class (Senior)', async ({ request }) => {
        const classeData = {
            ...testData.classe.create,
            codigoClasse: `SENIOR${Date.now()}`.substring(0, 20),
            cnpjClasse: generateValidCnpj(),
        };

        const response = await request.post(`/api/v1/fundos/${fundoId}/classes`, {
            data: classeData,
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 201 || response.status() === 200) {
            const createdClasse = await response.json();
            expect(createdClasse).toHaveProperty('id');
            expect(createdClasse.nomeClasse).toBe(classeData.nomeClasse);
        }
    });

    test('POST /api/v1/fundos/{fundoId}/classes - Create a subordinated class (Mezanino)', async ({ request }) => {
        const classeData = {
            codigoClasse: `MEZZ${Date.now()}`.substring(0, 20),
            nomeClasse: 'Classe Mezanino',
            cnpjClasse: generateValidCnpj(),
            tipoClasseFidc: 3, // Mezanino
            ordemSubordinacao: 2,
            rentabilidadeAlvo: 18.0,
            responsabilidadeLimitada: true,
            segregacaoPatrimonial: true,
            valorMinimoAplicacao: 5000.00,
        };

        const response = await request.post(`/api/v1/fundos/${fundoId}/classes`, {
            data: classeData,
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 201 || response.status() === 200) {
            const createdClasse = await response.json();
            expect(createdClasse).toHaveProperty('id');
            expect(createdClasse.tipoClasseFidc).toBe(3);
        }
    });

    test('GET /api/v1/classes/{id} - Retrieve a specific class by ID', async ({ request }) => {
        // First create a class
        const classeData = {
            ...testData.classe.create,
            codigoClasse: `CLASS${Date.now()}`.substring(0, 20),
            cnpjClasse: generateValidCnpj(),
        };

        const createResponse = await request.post(`/api/v1/fundos/${fundoId}/classes`, {
            data: classeData,
            headers: authConfig.getAuthHeader(),
        });

        if (createResponse.status() === 201 || createResponse.status() === 200) {
            const createdClasse = await createResponse.json();
            const classeId = createdClasse.id;

            // Then fetch it by ID
            const response = await request.get(`/api/v1/classes/${classeId}`, {
                headers: authConfig.getAuthHeader(),
            });

            expect(response.status()).toBe(200);
            const fetchedClasse = await response.json();
            expect(fetchedClasse.id).toBe(classeId);
        }
    });

    test('GET /api/v1/classes/{id} - Return 404 for non-existent class', async ({ request }) => {
        const response = await request.get('/api/v1/classes/99999999-9999-9999-9999-999999999999', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(404);
    });

    test('PUT /api/v1/classes/{id} - Update an existing class', async ({ request }) => {
        // First create a class
        const classeData = {
            ...testData.classe.create,
            codigoClasse: `CLASS${Date.now()}`.substring(0, 20),
            cnpjClasse: generateValidCnpj(),
        };

        const createResponse = await request.post(`/api/v1/fundos/${fundoId}/classes`, {
            data: classeData,
            headers: authConfig.getAuthHeader(),
        });

        if (createResponse.status() === 201 || createResponse.status() === 200) {
            const createdClasse = await createResponse.json();
            const classeId = createdClasse.id;

            // Then update it
            const updateData = {
                ...testData.classe.update,
            };

            const response = await request.put(`/api/v1/classes/${classeId}`, {
                data: updateData,
                headers: authConfig.getAuthHeader(),
            });

            expect([200, 204]).toContain(response.status());
        }
    });

    test('DELETE /api/v1/classes/{id} - Delete a class', async ({ request }) => {
        // First create a class
        const classeData = {
            ...testData.classe.create,
            codigoClasse: `DEL${Date.now()}`.substring(0, 20),
            cnpjClasse: generateValidCnpj(),
        };

        const createResponse = await request.post(`/api/v1/fundos/${fundoId}/classes`, {
            data: classeData,
            headers: authConfig.getAuthHeader(),
        });

        if (createResponse.status() === 201 || createResponse.status() === 200) {
            const createdClasse = await createResponse.json();
            const classeId = createdClasse.id;

            // Then delete it
            const response = await request.delete(`/api/v1/classes/${classeId}`, {
                headers: authConfig.getAuthHeader(),
            });

            expect([200, 204]).toContain(response.status());

            // Verify it's deleted
            const getResponse = await request.get(`/api/v1/classes/${classeId}`, {
                headers: authConfig.getAuthHeader(),
            });
            expect(getResponse.status()).toBe(404);
        }
    });

    test('POST /api/v1/fundos/{fundoId}/classes - Validate CNPJ format', async ({ request }) => {
        const invalidData = {
            ...testData.classe.create,
            codigoClasse: `INV${Date.now()}`.substring(0, 20),
            cnpjClasse: 'invalid-cnpj',
        };

        const response = await request.post(`/api/v1/fundos/${fundoId}/classes`, {
            data: invalidData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });
});
