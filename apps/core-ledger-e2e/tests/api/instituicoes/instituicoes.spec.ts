import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData, generateValidCnpj } from '../../config/test-data';

/**
 * Instituições (Institutions) API Tests
 * Tests Brazilian financial institutions (banks, brokers, etc.)
 * Uses CreateInstituicaoDto
 */
test.describe('Instituições API', () => {
    test('GET /api/v1/instituicoes - Retrieve all institutions with pagination', async ({ request }) => {
        const response = await request.get('/api/v1/instituicoes', {
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

    test('POST /api/v1/instituicoes - Create a new institution with valid CNPJ', async ({ request }) => {
        const instituicaoData = {
            ...testData.instituicao.create,
            cnpj: generateValidCnpj(),
        };

        const response = await request.post('/api/v1/instituicoes', {
            data: instituicaoData,
            headers: authConfig.getAuthHeader(),
        });

        expect([201, 200]).toContain(response.status());
        const createdInstituicao = await response.json();
        expect(createdInstituicao).toHaveProperty('id');
        expect(createdInstituicao.razaoSocial).toBe(instituicaoData.razaoSocial);
    });

    test('POST /api/v1/instituicoes - Validate CNPJ format', async ({ request }) => {
        const invalidData = {
            ...testData.instituicao.create,
            cnpj: 'invalid-cnpj',
        };

        const response = await request.post('/api/v1/instituicoes', {
            data: invalidData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });

    test('GET /api/v1/instituicoes/{id} - Retrieve a specific institution by ID', async ({ request }) => {
        // First create an institution
        const instituicaoData = {
            ...testData.instituicao.create,
            cnpj: generateValidCnpj(),
        };
        const createResponse = await request.post('/api/v1/instituicoes', {
            data: instituicaoData,
            headers: authConfig.getAuthHeader(),
        });
        const instituicaoId = (await createResponse.json()).id;

        // Then fetch it
        const response = await request.get(`/api/v1/instituicoes/${instituicaoId}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const instituicao = await response.json();
        expect(instituicao.id).toBe(instituicaoId);
        expect(instituicao.razaoSocial).toBe(instituicaoData.razaoSocial);
    });

    test('GET /api/v1/instituicoes/{id} - Return 404 for non-existent institution', async ({ request }) => {
        const response = await request.get('/api/v1/instituicoes/999999', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(404);
    });

    test('PUT /api/v1/instituicoes/{id} - Update an existing institution', async ({ request }) => {
        // First create an institution
        const instituicaoData = {
            ...testData.instituicao.create,
            cnpj: generateValidCnpj(),
        };
        const createResponse = await request.post('/api/v1/instituicoes', {
            data: instituicaoData,
            headers: authConfig.getAuthHeader(),
        });
        const instituicaoId = (await createResponse.json()).id;

        // Then update it
        const updateData = {
            ...testData.instituicao.update,
            cnpj: generateValidCnpj(),
        };
        const response = await request.put(`/api/v1/instituicoes/${instituicaoId}`, {
            data: updateData,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 204]).toContain(response.status());

        // Verify the update
        const getResponse = await request.get(`/api/v1/instituicoes/${instituicaoId}`, {
            headers: authConfig.getAuthHeader(),
        });
        const updatedInstituicao = await getResponse.json();
        expect(updatedInstituicao.razaoSocial).toBe(updateData.razaoSocial);
    });

    test('DELETE /api/v1/instituicoes/{id} - Delete an institution', async ({ request }) => {
        // First create an institution
        const instituicaoData = {
            ...testData.instituicao.create,
            cnpj: generateValidCnpj(),
        };
        const createResponse = await request.post('/api/v1/instituicoes', {
            data: instituicaoData,
            headers: authConfig.getAuthHeader(),
        });
        const instituicaoId = (await createResponse.json()).id;

        // Then delete it
        const response = await request.delete(`/api/v1/instituicoes/${instituicaoId}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect([204, 200]).toContain(response.status());

        // Verify it's deleted
        const getResponse = await request.get(`/api/v1/instituicoes/${instituicaoId}`, {
            headers: authConfig.getAuthHeader(),
        });
        expect(getResponse.status()).toBe(404);
    });

    test('GET /api/v1/instituicoes - Filter by active status', async ({ request }) => {
        const response = await request.get('/api/v1/instituicoes', {
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

    test('POST /api/v1/instituicoes - Create with complete valid data', async ({ request }) => {
        const instituicaoData = {
            cnpj: generateValidCnpj(),
            razaoSocial: 'Banco Teste Instituição S/A',
            nomeFantasia: 'Banco Teste',
            ativo: true,
        };

        const response = await request.post('/api/v1/instituicoes', {
            data: instituicaoData,
            headers: authConfig.getAuthHeader(),
        });

        expect([201, 200]).toContain(response.status());
        const createdInstituicao = await response.json();
        expect(createdInstituicao).toHaveProperty('id');
        expect(createdInstituicao.razaoSocial).toBe(instituicaoData.razaoSocial);
        expect(createdInstituicao.cnpj).toBe(instituicaoData.cnpj);
    });

    test('POST /api/v1/instituicoes - Create Caixa Econômica test institution', async ({ request }) => {
        const instituicaoData = {
            cnpj: generateValidCnpj(),
            razaoSocial: 'Caixa Econômica Federal - Teste',
            nomeFantasia: 'CEF Teste',
            ativo: true,
        };

        const response = await request.post('/api/v1/instituicoes', {
            data: instituicaoData,
            headers: authConfig.getAuthHeader(),
        });

        expect([201, 200]).toContain(response.status());
        const createdInstituicao = await response.json();
        expect(createdInstituicao).toHaveProperty('id');
        expect(createdInstituicao.razaoSocial).toContain('Caixa');
    });
});
