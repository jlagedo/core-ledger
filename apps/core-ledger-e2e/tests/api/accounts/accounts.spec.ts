import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData } from '../../config/test-data';

test.describe('Accounts API', () => {
    test('GET /api/accounts - Retrieve all accounts with pagination', async ({ request }) => {
        const response = await request.get('/api/accounts', {
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

    test('POST /api/accounts - Create a new account', async ({ request }) => {
        const accountData = {
            ...testData.account.create,
            code: testData.account.create.code + Math.floor(Math.random() * 10000),
        };

        const response = await request.post('/api/accounts', {
            data: accountData,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 201]).toContain(response.status());
        const createdAccount = await response.json();
        expect(createdAccount).toHaveProperty('id');
        expect(createdAccount.code).toBe(accountData.code);
        expect(createdAccount.name).toBe(accountData.name);
    });

    test('POST /api/accounts - Validate required fields', async ({ request }) => {
        const invalidData = {
            name: 'Test Account',
        };

        const response = await request.post('/api/accounts', {
            data: invalidData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });

    test('GET /api/accounts/{id} - Retrieve a specific account by ID', async ({ request }) => {
        const accountData = {
            ...testData.account.create,
            code: testData.account.create.code + Math.floor(Math.random() * 10000),
        };
        const createResponse = await request.post('/api/accounts', {
            data: accountData,
            headers: authConfig.getAuthHeader(),
        });
        const accountId = (await createResponse.json()).id;

        const response = await request.get(`/api/accounts/${accountId}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const account = await response.json();
        expect(account.id).toBe(accountId);
    });

    test('GET /api/accounts/{id} - Return 404 for non-existent account', async ({ request }) => {
        const response = await request.get('/api/accounts/99999', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(404);
    });

    test('PUT /api/accounts/{id} - Update an existing account', async ({ request }) => {
        const accountData = {
            ...testData.account.create,
            code: testData.account.create.code + Math.floor(Math.random() * 10000),
        };
        const createResponse = await request.post('/api/accounts', {
            data: accountData,
            headers: authConfig.getAuthHeader(),
        });
        const accountId = (await createResponse.json()).id;

        const updateData = {
            ...testData.account.update,
            code: testData.account.update.code + Math.floor(Math.random() * 10000),
        };
        const response = await request.put(`/api/accounts/${accountId}`, {
            data: updateData,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 204]).toContain(response.status());
    });

    test('PATCH /api/accounts/{id}/deactivate - Deactivate an account', async ({ request }) => {
        const accountData = {
            ...testData.account.create,
            code: testData.account.create.code + Math.floor(Math.random() * 10000),
        };
        const createResponse = await request.post('/api/accounts', {
            data: accountData,
            headers: authConfig.getAuthHeader(),
        });
        const accountId = (await createResponse.json()).id;

        const response = await request.patch(`/api/accounts/${accountId}/deactivate`, {
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 204]).toContain(response.status());

        const getResponse = await request.get(`/api/accounts/${accountId}`, {
            headers: authConfig.getAuthHeader(),
        });
        const account = await getResponse.json();
        expect(account.deactivatedAt).toBeTruthy();
    });

    test('GET /api/accounts/reports/by-type - Get accounts report by type', async ({ request }) => {
        const response = await request.get('/api/accounts/reports/by-type', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(Array.isArray(data)).toBe(true);
    });
});
