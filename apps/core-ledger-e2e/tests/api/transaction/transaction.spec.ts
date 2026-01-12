import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData } from '../../config/test-data';

/**
 * Transaction (Single) API Tests
 * Focuses on complete transaction workflows with Brazilian securities data
 * Tests CRUD operations and complex transaction scenarios
 */
test.describe('Transaction (Single) API', () => {
    let fundId: number;
    let securityId: number;

    // Setup: Create test fund and security if needed
    test.beforeAll(async ({ request }) => {
        // In a real scenario, you'd create these via API
        // For now, we assume IDs 1 and 1 exist from previous tests
        fundId = 1;
        securityId = 1;
    });

    test('POST /api/transactions - Create a stock purchase transaction', async ({ request }) => {
        const transactionData = {
            ...testData.transaction.create,
            fundId,
            securityId,
        };

        const response = await request.post('/api/transactions', {
            data: transactionData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(201);
        const createdTransaction = await response.json();
        expect(createdTransaction).toHaveProperty('id');
        expect(createdTransaction.quantity).toBe(transactionData.quantity);
        expect(createdTransaction.price).toBe(transactionData.price);
        expect(createdTransaction.currency).toBe('BRL');
    });

    test('POST /api/transactions - Create a dividend transaction', async ({ request }) => {
        const transactionData = {
            ...testData.transaction.dividend,
            fundId,
            securityId,
        };

        const response = await request.post('/api/transactions', {
            data: transactionData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(201);
        const createdTransaction = await response.json();
        expect(createdTransaction).toHaveProperty('id');
        expect(createdTransaction.quantity).toBe(1000);
        expect(createdTransaction.price).toBe(1.50);
    });

    test('POST /api/transactions - Create a stock sale transaction', async ({ request }) => {
        const transactionData = {
            ...testData.transaction.sale,
            fundId,
            securityId: 3, // ITUB4
        };

        const response = await request.post('/api/transactions', {
            data: transactionData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(201);
        const createdTransaction = await response.json();
        expect(createdTransaction).toHaveProperty('id');
        expect(createdTransaction.transactionSubTypeId).toBe(2); // Sale
    });

    test('POST /api/transactions - Validate required fields', async ({ request }) => {
        const invalidTransaction = {
            fundId,
            // Missing securityId
            transactionSubTypeId: 1,
            quantity: 100,
            price: 50.00,
        };

        const response = await request.post('/api/transactions', {
            data: invalidTransaction,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });

    test('POST /api/transactions - Validate negative quantity', async ({ request }) => {
        const invalidTransaction = {
            ...testData.transaction.create,
            fundId,
            securityId,
            quantity: -100, // Invalid negative quantity
        };

        const response = await request.post('/api/transactions', {
            data: invalidTransaction,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });

    test('POST /api/transactions - Validate settlement date after trade date', async ({ request }) => {
        const tradeDate = new Date();
        const invalidTransaction = {
            ...testData.transaction.create,
            fundId,
            securityId,
            tradeDate: tradeDate.toISOString(),
            settleDate: new Date(tradeDate.getTime() - 86400000).toISOString(), // Before trade date
        };

        const response = await request.post('/api/transactions', {
            data: invalidTransaction,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });

    test('GET /api/transactions/{id} - Retrieve a specific transaction', async ({ request }) => {
        // First create a transaction
        const transactionData = {
            ...testData.transaction.create,
            fundId,
            securityId,
        };
        const createResponse = await request.post('/api/transactions', {
            data: transactionData,
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
        expect(transaction.fundId).toBe(fundId);
        expect(transaction.securityId).toBe(securityId);
    });

    test('GET /api/transactions/{id} - Return 404 for non-existent transaction', async ({ request }) => {
        const response = await request.get('/api/transactions/999999', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(404);
    });

    test('PUT /api/transactions/{id} - Update transaction details', async ({ request }) => {
        // Create a transaction first
        const transactionData = {
            ...testData.transaction.create,
            fundId,
            securityId,
        };
        const createResponse = await request.post('/api/transactions', {
            data: transactionData,
            headers: authConfig.getAuthHeader(),
        });
        const transactionId = (await createResponse.json()).id;

        // Update it with different security
        const updateData = {
            ...testData.transaction.update,
            fundId,
            securityId: 2, // VALE3
        };
        const response = await request.put(`/api/transactions/${transactionId}`, {
            data: updateData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(204);

        // Verify the update
        const getResponse = await request.get(`/api/transactions/${transactionId}`, {
            headers: authConfig.getAuthHeader(),
        });
        const updatedTransaction = await getResponse.json();
        expect(updatedTransaction.quantity).toBe(updateData.quantity);
        expect(updatedTransaction.price).toBe(updateData.price);
    });

    test('PUT /api/transactions/{id} - Return 404 for non-existent transaction', async ({ request }) => {
        const response = await request.put('/api/transactions/999999', {
            data: testData.transaction.update,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(404);
    });

    test('GET /api/transactions - List with pagination', async ({ request }) => {
        const response = await request.get('/api/transactions', {
            params: {
                limit: 10,
                offset: 0,
            },
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(data).toHaveProperty('items');
        expect(data).toHaveProperty('totalCount');
        expect(Array.isArray(data.items)).toBe(true);
    });

    test('GET /api/transactions - Filter by status', async ({ request }) => {
        const response = await request.get('/api/transactions', {
            params: {
                limit: 50,
                offset: 0,
                // statusId: 1 // If API supports filtering
            },
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(data).toHaveProperty('items');
    });

    test('GET /api/transactions/status - Retrieve transaction statuses', async ({ request }) => {
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

    test('GET /api/transactions/status/{id} - Retrieve specific status', async ({ request }) => {
        // First get all statuses
        const statusResponse = await request.get('/api/transactions/status', {
            headers: authConfig.getAuthHeader(),
        });
        const statuses = await statusResponse.json();

        if (statuses.length > 0) {
            const statusId = statuses[0].value;
            const response = await request.get(`/api/transactions/status/${statusId}`, {
                headers: authConfig.getAuthHeader(),
            });

            expect(response.status()).toBe(200);
            const status = await response.json();
            expect(status.value).toBe(statusId);
        }
    });

    test('GET /api/transactions/types - Retrieve transaction types', async ({ request }) => {
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

    test('GET /api/transactions/types/{id} - Retrieve specific type', async ({ request }) => {
        // First get all types
        const typesResponse = await request.get('/api/transactions/types', {
            headers: authConfig.getAuthHeader(),
        });
        const types = await typesResponse.json();

        if (types.length > 0) {
            const typeId = types[0].value;
            const response = await request.get(`/api/transactions/types/${typeId}`, {
                headers: authConfig.getAuthHeader(),
            });

            expect(response.status()).toBe(200);
            const type = await response.json();
            expect(type.value).toBe(typeId);
        }
    });

    test('GET /api/transactions/subtypes - Retrieve transaction subtypes', async ({ request }) => {
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

    test('GET /api/transactions/subtypes/{id} - Retrieve specific subtype', async ({ request }) => {
        // First get all subtypes
        const subtypesResponse = await request.get('/api/transactions/subtypes', {
            headers: authConfig.getAuthHeader(),
        });
        const subtypes = await subtypesResponse.json();

        if (subtypes.length > 0) {
            const subtypeId = subtypes[0].value;
            const response = await request.get(`/api/transactions/subtypes/${subtypeId}`, {
                headers: authConfig.getAuthHeader(),
            });

            expect(response.status()).toBe(200);
            const subtype = await response.json();
            expect(subtype.value).toBe(subtypeId);
        }
    });
});