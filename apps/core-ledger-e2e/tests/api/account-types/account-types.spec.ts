import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData } from '../../config/test-data';

test.describe('Account Types API', () => {
    test('GET /api/accounttypes - Retrieve all account types', async ({ request }) => {
        const response = await request.get('/api/accounttypes', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(Array.isArray(data)).toBe(true);
        if (data.length > 0) {
            expect(data[0]).toHaveProperty('id');
            expect(data[0]).toHaveProperty('description');
        }
    });

    test('POST /api/accounttypes - Create a new account type', async ({ request }) => {
        const response = await request.post('/api/accounttypes', {
            data: testData.accountType.create,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 201]).toContain(response.status());
        const data = await response.json();
        expect(data).toHaveProperty('id');
        expect(data.description).toBe(testData.accountType.create.description);
    });

    test('GET /api/accounttypes/{id} - Retrieve a specific account type by ID', async ({ request }) => {
        // First create an account type
        const createResponse = await request.post('/api/accounttypes', {
            data: testData.accountType.create,
            headers: authConfig.getAuthHeader(),
        });
        const typeId = (await createResponse.json()).id;

        // Then fetch it
        const response = await request.get(`/api/accounttypes/${typeId}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const type = await response.json();
        expect(type.id).toBe(typeId);
    });

    test('GET /api/accounttypes/{id} - Return 404 for non-existent account type', async ({ request }) => {
        const response = await request.get('/api/accounttypes/99999', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(404);
    });

    test('PUT /api/accounttypes/{id} - Update an existing account type', async ({ request }) => {
        // First create an account type
        const createResponse = await request.post('/api/accounttypes', {
            data: testData.accountType.create,
            headers: authConfig.getAuthHeader(),
        });
        const typeId = (await createResponse.json()).id;

        // Then update it
        const response = await request.put(`/api/accounttypes/${typeId}`, {
            data: testData.accountType.update,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 204]).toContain(response.status());

        // Verify the update
        const getResponse = await request.get(`/api/accounttypes/${typeId}`, {
            headers: authConfig.getAuthHeader(),
        });
        const updatedType = await getResponse.json();
        expect(updatedType.description).toBe(testData.accountType.update.description);
    });

    test('DELETE /api/accounttypes/{id} - Delete an account type', async ({ request }) => {
        // First create an account type
        const createResponse = await request.post('/api/accounttypes', {
            data: testData.accountType.create,
            headers: authConfig.getAuthHeader(),
        });
        const typeId = (await createResponse.json()).id;

        // Then delete it
        const response = await request.delete(`/api/accounttypes/${typeId}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 204]).toContain(response.status());

        // Verify deletion
        const getResponse = await request.get(`/api/accounttypes/${typeId}`, {
            headers: authConfig.getAuthHeader(),
        });
        expect(getResponse.status()).toBe(404);
    });
});
