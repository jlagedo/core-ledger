import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';

test.describe('Audit Logs API', () => {
    test('GET /api/auditlogs - Retrieve audit logs with pagination', async ({ request }) => {
        const response = await request.get('/api/auditlogs', {
            params: {
                limit: 10,
                offset: 0,
                sortBy: 'id',
                sortDirection: 'desc',
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

    test('GET /api/auditlogs - Filter audit logs by entityName', async ({ request }) => {
        const response = await request.get('/api/auditlogs', {
            params: {
                limit: 10,
                offset: 0,
                sortDirection: 'desc',
                filter: 'entityName=Account',
            },
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(data).toHaveProperty('items');
        expect(Array.isArray(data.items)).toBe(true);
    });

    test('GET /api/auditlogs - Sort audit logs by performedAt', async ({ request }) => {
        const response = await request.get('/api/auditlogs', {
            params: {
                limit: 10,
                offset: 0,
                sortBy: 'performedAt',
                sortDirection: 'desc',
            },
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(Array.isArray(data.items)).toBe(true);
    });

    test('GET /api/auditlogs - Test pagination offset', async ({ request }) => {
        const firstPageResponse = await request.get('/api/auditlogs', {
            params: {
                limit: 5,
                offset: 0,
                sortDirection: 'desc',
            },
            headers: authConfig.getAuthHeader(),
        });

        expect(firstPageResponse.status()).toBe(200);
        const firstPage = await firstPageResponse.json();

        const secondPageResponse = await request.get('/api/auditlogs', {
            params: {
                limit: 5,
                offset: 5,
                sortDirection: 'desc',
            },
            headers: authConfig.getAuthHeader(),
        });

        expect(secondPageResponse.status()).toBe(200);
        const secondPage = await secondPageResponse.json();

        expect(firstPage.offset).toBe(0);
        expect(secondPage.offset).toBe(5);
    });
});
