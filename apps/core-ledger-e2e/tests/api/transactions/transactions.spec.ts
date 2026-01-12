import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData } from '../../config/test-data';

test.describe('Transactions API', () => {
    test('GET /api/transactions - Retrieve all transactions with pagination', async ({ request }) => {
        const response = await request.get('/api/transactions', {
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

    test('POST /api/transactions - Create a new transaction', async ({ request }) => {
        const response = await request.post('/api/transactions', {
            data: testData.transaction.create,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 201]).toContain(response.status());
        const createdTransaction = await response.json();
        expect(createdTransaction).toHaveProperty('id');
        expect(createdTransaction.fundId).toBe(testData.transaction.create.fundId);
        expect(createdTransaction.quantity).toBe(testData.transaction.create.quantity);
        expect(createdTransaction.price).toBe(testData.transaction.create.price);
    });

    test('POST /api/transactions - Validate required fields', async ({ request }) => {
        const invalidData = {
            fundId: 1,
            // Missing other required fields
        };

        const response = await request.post('/api/transactions', {
            data: invalidData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });

    test('GET /api/transactions/{id} - Retrieve a specific transaction by ID', async ({ request }) => {
        // First create a transaction
        const createResponse = await request.post('/api/transactions', {
            data: testData.transaction.create,
            headers: authConfig.getAuthHeader(),
        });
        const transactionId = (await createResponse.json()).id;

        // Then fetch it
        const response = await request.get(`/api/transactions/${transactionId}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const transaction = await response.json();
        expect(transaction.id).toBe(transactionId);
    });

    test('GET /api/transactions/{id} - Return 404 for non-existent transaction', async ({ request }) => {
        const response = await request.get('/api/transactions/99999', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(404);
    });

    test('PUT /api/transactions/{id} - Update an existing transaction', async ({ request }) => {
        // First create a transaction
        const createResponse = await request.post('/api/transactions', {
            data: testData.transaction.create,
            headers: authConfig.getAuthHeader(),
        });
        const transactionId = (await createResponse.json()).id;

        // Then update it
        const response = await request.put(`/api/transactions/${transactionId}`, {
            data: testData.transaction.update,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 204]).toContain(response.status());

        // Verify the update
        const getResponse = await request.get(`/api/transactions/${transactionId}`, {
            headers: authConfig.getAuthHeader(),
        });
        const updatedTransaction = await getResponse.json();
        expect(updatedTransaction.quantity).toBe(testData.transaction.update.quantity);
        expect(updatedTransaction.price).toBe(testData.transaction.update.price);
    });

    test('GET /api/transactions/status - Retrieve all transaction statuses', async ({ request }) => {
        const response = await request.get('/api/transactions/status', {
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

    test('GET /api/transactions/types - Retrieve all transaction types', async ({ request }) => {
        const response = await request.get('/api/transactions/types', {
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

    test('GET /api/transactions/subtypes - Retrieve all transaction subtypes', async ({ request }) => {
        const response = await request.get('/api/transactions/subtypes', {
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

    test('POST /api/transactions - Create dividend transaction', async ({ request }) => {
        const response = await request.post('/api/transactions', {
            data: testData.transaction.dividend,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 201]).toContain(response.status());
        const createdTransaction = await response.json();
        expect(createdTransaction).toHaveProperty('id');
        expect(createdTransaction.transactionSubTypeId).toBe(testData.transaction.dividend.transactionSubTypeId);
    });

    test('POST /api/transactions - Create sale transaction', async ({ request }) => {
        const response = await request.post('/api/transactions', {
            data: testData.transaction.sale,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 201]).toContain(response.status());
        const createdTransaction = await response.json();
        expect(createdTransaction).toHaveProperty('id');
        expect(createdTransaction.transactionSubTypeId).toBe(testData.transaction.sale.transactionSubTypeId);
    });
});
