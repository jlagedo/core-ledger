import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData } from '../../config/test-data';

test.describe('Funds API', () => {
    test('GET /api/funds - Retrieve all funds with pagination', async ({ request }) => {
        const response = await request.get('/api/funds', {
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
        expect(data).toHaveProperty('limit');
        expect(data).toHaveProperty('offset');
        expect(Array.isArray(data.items)).toBe(true);
    });

    test('POST /api/funds - Create a new fund', async ({ request }) => {
        const fundData = {
            ...testData.fund.create,
            code: `FUND${Date.now()}`.substring(0, 10),
        };

        const response = await request.post('/api/funds', {
            data: fundData,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 201]).toContain(response.status());
        const createdFund = await response.json();
        expect(createdFund).toHaveProperty('id');
        expect(createdFund.code).toBe(fundData.code);
        expect(createdFund.name).toBe(fundData.name);
        expect(createdFund.baseCurrency).toBe(fundData.baseCurrency);
    });

    test('POST /api/funds - Validate required fields', async ({ request }) => {
        const invalidData = {
            name: 'Test Fund',
        };

        const response = await request.post('/api/funds', {
            data: invalidData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });

    test('GET /api/funds/{id} - Retrieve a specific fund by ID', async ({ request }) => {
        const fundData = {
            ...testData.fund.create,
            code: `FUND${Date.now()}`.substring(0, 10),
        };
        const createResponse = await request.post('/api/funds', {
            data: fundData,
            headers: authConfig.getAuthHeader(),
        });
        const fundId = (await createResponse.json()).id;

        const response = await request.get(`/api/funds/${fundId}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const fund = await response.json();
        expect(fund.id).toBe(fundId);
    });

    test('GET /api/funds/{id} - Return 404 for non-existent fund', async ({ request }) => {
        const response = await request.get('/api/funds/99999', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(404);
    });

    test('GET /api/funds/autocomplete - Autocomplete fund search', async ({ request }) => {
        const response = await request.get('/api/funds/autocomplete', {
            headers: authConfig.getAuthHeader(),
            params: {
                q: 'fund'
            }
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(Array.isArray(data)).toBe(true);
    });

    test('PUT /api/funds/{id} - Update an existing fund', async ({ request }) => {
        const fundData = {
            ...testData.fund.create,
            code: `FUND${Date.now()}`.substring(0, 10),
        };
        const createResponse = await request.post('/api/funds', {
            data: fundData,
            headers: authConfig.getAuthHeader(),
        });
        const fundId = (await createResponse.json()).id;

        const updateData = {
            ...testData.fund.update,
            code: `UPDT${Date.now()}`.substring(0, 10),
        };
        const response = await request.put(`/api/funds/${fundId}`, {
            data: updateData,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 204]).toContain(response.status());

        const getResponse = await request.get(`/api/funds/${fundId}`, {
            headers: authConfig.getAuthHeader(),
        });
        const updatedFund = await getResponse.json();
        expect(updatedFund.name).toBe(updateData.name);
    });

    test('PUT /api/funds/{id} - Return 404 for non-existent fund', async ({ request }) => {
        const response = await request.put('/api/funds/99999', {
            data: testData.fund.update,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(404);
    });

    test('PATCH /api/funds/{id}/deactivate - Deactivate a fund', async ({ request }) => {
        const fundData = {
            ...testData.fund.create,
            code: `FUND${Date.now()}`.substring(0, 10),
        };
        const createResponse = await request.post('/api/funds', {
            data: fundData,
            headers: authConfig.getAuthHeader(),
        });
        const fundId = (await createResponse.json()).id;

        const response = await request.patch(`/api/funds/${fundId}/deactivate`, {
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 204]).toContain(response.status());
    });
});
