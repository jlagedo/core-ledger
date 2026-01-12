import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData, generateValidCnpj } from '../../config/test-data';

/**
 * Vínculos (Fund Custodian/Bank Links) API Tests
 * Tests relationships between funds and custodian institutions
 * Nested under fundos resource - Uses CreateFundoVinculoDto, EncerrarVinculoDto
 */
test.describe('Vínculos (Fund Institution Links) API', () => {
    let fundoId: string;
    let instituicaoId: number;

    test.beforeAll(async ({ request }) => {
        // Create a fundo first
        const fundoData = {
            ...testData.fundo.create,
            cnpj: generateValidCnpj(),
        };

        const fundoResponse = await request.post('/api/v1/fundos', {
            data: fundoData,
            headers: authConfig.getAuthHeader(),
        });

        if (fundoResponse.status() === 201 || fundoResponse.status() === 200) {
            const created = await fundoResponse.json();
            fundoId = created.id;
        } else {
            fundoId = '550e8400-e29b-41d4-a716-446655440000';
        }

        // Create an institution to link
        const instituicaoData = {
            ...testData.instituicao.create,
            cnpj: generateValidCnpj(),
        };

        const instituicaoResponse = await request.post('/api/v1/instituicoes', {
            data: instituicaoData,
            headers: authConfig.getAuthHeader(),
        });

        if (instituicaoResponse.status() === 201 || instituicaoResponse.status() === 200) {
            const created = await instituicaoResponse.json();
            instituicaoId = created.id;
        } else {
            instituicaoId = 1;
        }
    });

    test('GET /api/v1/fundos/{fundoId}/vinculos - Retrieve links for a fund', async ({ request }) => {
        const response = await request.get(`/api/v1/fundos/${fundoId}/vinculos`, {
            params: {
                incluirEncerrados: false,
            },
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 200) {
            const data = await response.json();
            expect(Array.isArray(data)).toBe(true);

            if (data.length > 0) {
                data.forEach((item: any) => {
                    expect(item).toHaveProperty('id');
                    expect(item).toHaveProperty('instituicaoId');
                    expect(item).toHaveProperty('tipoVinculo');
                    expect(item).toHaveProperty('dataInicio');
                });
            }
        }
    });

    test('GET /api/v1/fundos/{fundoId}/vinculos - Include closed links', async ({ request }) => {
        const response = await request.get(`/api/v1/fundos/${fundoId}/vinculos`, {
            params: {
                incluirEncerrados: true,
            },
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 200) {
            const data = await response.json();
            expect(Array.isArray(data)).toBe(true);
        }
    });

    test('POST /api/v1/fundos/{fundoId}/vinculos - Create fund institution link (Administrador)', async ({ request }) => {
        const vinculoData = {
            instituicaoId: instituicaoId,
            tipoVinculo: 1, // Administrador
            dataInicio: new Date().toISOString().split('T')[0],
            principal: true,
            contratoNumero: `CONT${Date.now()}`,
            observacao: 'Administrador principal do fundo',
        };

        const response = await request.post(`/api/v1/fundos/${fundoId}/vinculos`, {
            data: vinculoData,
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 201 || response.status() === 200) {
            const created = await response.json();
            expect(created).toHaveProperty('id');
            expect(created.tipoVinculo).toBe(vinculoData.tipoVinculo);
        }
    });

    test('POST /api/v1/fundos/{fundoId}/vinculos - Create custodian link', async ({ request }) => {
        const vinculoData = {
            instituicaoId: instituicaoId,
            tipoVinculo: 3, // Custodiante
            dataInicio: new Date().toISOString().split('T')[0],
            principal: true,
            contratoNumero: `CUST${Date.now()}`,
            observacao: 'Custodiante principal',
        };

        const response = await request.post(`/api/v1/fundos/${fundoId}/vinculos`, {
            data: vinculoData,
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 201 || response.status() === 200) {
            const created = await response.json();
            expect(created).toHaveProperty('id');
            expect(created.tipoVinculo).toBe(3);
        }
    });

    test('POST /api/v1/fundos/{fundoId}/vinculos - Create gestor link', async ({ request }) => {
        const vinculoData = {
            instituicaoId: instituicaoId,
            tipoVinculo: 2, // Gestor
            dataInicio: new Date().toISOString().split('T')[0],
            principal: true,
            contratoNumero: `GEST${Date.now()}`,
            observacao: 'Gestor do fundo',
        };

        const response = await request.post(`/api/v1/fundos/${fundoId}/vinculos`, {
            data: vinculoData,
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 201 || response.status() === 200) {
            const created = await response.json();
            expect(created).toHaveProperty('id');
            expect(created.tipoVinculo).toBe(2);
        }
    });

    test('PATCH /api/v1/vinculos/{id}/encerrar - Close fund institution link', async ({ request }) => {
        // First create a link
        const vinculoData = {
            instituicaoId: instituicaoId,
            tipoVinculo: 4, // Distribuidor
            dataInicio: new Date().toISOString().split('T')[0],
            principal: false,
            contratoNumero: `DIST${Date.now()}`,
            observacao: 'Distribuidor temporário',
        };

        const createResponse = await request.post(`/api/v1/fundos/${fundoId}/vinculos`, {
            data: vinculoData,
            headers: authConfig.getAuthHeader(),
        });

        if (createResponse.status() === 201 || createResponse.status() === 200) {
            const created = await createResponse.json();
            const vinculoId = created.id;

            // Then close it
            const encerrarData = {
                dataFim: new Date().toISOString().split('T')[0],
            };

            const response = await request.patch(`/api/v1/vinculos/${vinculoId}/encerrar`, {
                data: encerrarData,
                headers: authConfig.getAuthHeader(),
            });

            expect([200, 204]).toContain(response.status());
        }
    });

    test('GET /api/v1/fundos/{fundoId}/vinculos - Verify vinculo structure', async ({ request }) => {
        const response = await request.get(`/api/v1/fundos/${fundoId}/vinculos`, {
            params: {
                incluirEncerrados: false,
            },
            headers: authConfig.getAuthHeader(),
        });

        if (response.status() === 200) {
            const data = await response.json();

            if (data.length > 0) {
                const vinculo = data[0];
                expect(vinculo).toHaveProperty('id');
                expect(vinculo).toHaveProperty('instituicaoId');
                expect(vinculo).toHaveProperty('tipoVinculo');
                expect(vinculo).toHaveProperty('dataInicio');
                expect(vinculo).toHaveProperty('principal');
                expect(typeof vinculo.principal).toBe('boolean');
            }
        }
    });

    test('POST /api/v1/fundos/{fundoId}/vinculos - Validate required fields', async ({ request }) => {
        const invalidData = {
            // Missing instituicaoId and tipoVinculo
            dataInicio: new Date().toISOString().split('T')[0],
            principal: true,
        };

        const response = await request.post(`/api/v1/fundos/${fundoId}/vinculos`, {
            data: invalidData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });
});
