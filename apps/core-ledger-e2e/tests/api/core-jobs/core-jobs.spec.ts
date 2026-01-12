import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';

test.describe('Core Jobs API', () => {
    test('GET /api/corejobs - Retrieve all core jobs with pagination', async ({ request }) => {
        const response = await request.get('/api/corejobs', {
            params: {
                limit: 10,
                offset: 0,
                sortBy: 'id',
                sortDirection: 'asc',
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

    test('GET /api/corejobs - Filter core jobs by status', async ({ request }) => {
        const response = await request.get('/api/corejobs', {
            params: {
                limit: 10,
                offset: 0,
                sortDirection: 'asc',
                filter: 'status=1',
            },
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(data).toHaveProperty('items');
        expect(Array.isArray(data.items)).toBe(true);
    });

    test('GET /api/corejobs - Sort core jobs by creationDate', async ({ request }) => {
        const response = await request.get('/api/corejobs', {
            params: {
                limit: 10,
                offset: 0,
                sortBy: 'creationDate',
                sortDirection: 'desc',
            },
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(Array.isArray(data.items)).toBe(true);
    });

    test('GET /api/corejobs - Test pagination', async ({ request }) => {
        const response = await request.get('/api/corejobs', {
            params: {
                limit: 5,
                offset: 0,
                sortDirection: 'asc',
            },
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(data.items.length).toBeLessThanOrEqual(5);
        expect(data.limit).toBe(5);
        expect(data.offset).toBe(0);
    });
});
