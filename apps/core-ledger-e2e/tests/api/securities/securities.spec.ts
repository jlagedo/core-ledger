import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData } from '../../config/test-data';

test.describe('Securities API', () => {
    test('GET /api/securities - Retrieve all securities with pagination', async ({ request }) => {
        const response = await request.get('/api/securities', {
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

    test('POST /api/securities - Create a new security', async ({ request }) => {
        const securityData = {
            ...testData.security.create,
            ticker: `TEST${Date.now()}`.substring(0, 20),
        };

        const response = await request.post('/api/securities', {
            data: securityData,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 201]).toContain(response.status());
        const createdSecurity = await response.json();
        expect(createdSecurity).toHaveProperty('id');
        expect(createdSecurity.name).toBe(securityData.name);
        expect(createdSecurity.ticker).toBe(securityData.ticker);
        expect(createdSecurity.currency).toBe(securityData.currency);
    });

    test('POST /api/securities - Invalid security type should fail', async ({ request }) => {
        const invalidData = {
            ...testData.security.create,
            ticker: `INV${Date.now()}`.substring(0, 20),
            type: 999,
        };

        const response = await request.post('/api/securities', {
            data: invalidData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });

    test('GET /api/securities/{id} - Retrieve a specific security by ID', async ({ request }) => {
        // First create a security
        const securityData = {
            ...testData.security.create,
            ticker: `TEST${Date.now()}`.substring(0, 20),
        };
        const createResponse = await request.post('/api/securities', {
            data: securityData,
            headers: authConfig.getAuthHeader(),
        });
        const securityId = (await createResponse.json()).id;

        // Then fetch it
        const response = await request.get(`/api/securities/${securityId}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const security = await response.json();
        expect(security.id).toBe(securityId);
    });

    test('GET /api/securities/{id} - Return 404 for non-existent security', async ({ request }) => {
        const response = await request.get('/api/securities/99999', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(404);
    });

    test('GET /api/securities/autocomplete - Autocomplete search', async ({ request }) => {
        const response = await request.get('/api/securities/autocomplete', {
            headers: authConfig.getAuthHeader(),
            params: {
                q: 'PETR'
            }
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(Array.isArray(data)).toBe(true);
    });

    test('PUT /api/securities/{id} - Update an existing security', async ({ request }) => {
        // First create a security
        const securityData = {
            ...testData.security.create,
            ticker: `TEST${Date.now()}`.substring(0, 20),
        };
        const createResponse = await request.post('/api/securities', {
            data: securityData,
            headers: authConfig.getAuthHeader(),
        });
        const securityId = (await createResponse.json()).id;

        // Then update it
        const updateData = {
            ...testData.security.update,
            ticker: `UPDT${Date.now()}`.substring(0, 20),
        };
        const response = await request.put(`/api/securities/${securityId}`, {
            data: updateData,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 204]).toContain(response.status());

        // Verify the update
        const getResponse = await request.get(`/api/securities/${securityId}`, {
            headers: authConfig.getAuthHeader(),
        });
        const updatedSecurity = await getResponse.json();
        expect(updatedSecurity.name).toBe(updateData.name);
    });

    test('PATCH /api/securities/{id}/deactivate - Deactivate a security', async ({ request }) => {
        // First create a security
        const securityData = {
            ...testData.security.create,
            ticker: `TEST${Date.now()}`.substring(0, 20),
        };
        const createResponse = await request.post('/api/securities', {
            data: securityData,
            headers: authConfig.getAuthHeader(),
        });
        const securityId = (await createResponse.json()).id;

        // Then deactivate it
        const response = await request.patch(`/api/securities/${securityId}/deactivate`, {
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 204]).toContain(response.status());

        // Verify the security is deactivated
        const getResponse = await request.get(`/api/securities/${securityId}`, {
            headers: authConfig.getAuthHeader(),
        });
        const security = await getResponse.json();
        expect(security.deactivatedAt).toBeTruthy();
    });

    test('GET /api/securitytypes - Retrieve all security types', async ({ request }) => {
        const response = await request.get('/api/securitytypes', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(Array.isArray(data)).toBe(true);
        if (data.length > 0) {
            expect(data[0]).toHaveProperty('value');
            expect(data[0]).toHaveProperty('name');
        }
    });
});
