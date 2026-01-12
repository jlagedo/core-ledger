import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData } from '../../config/test-data';

/**
 * Calendário (Business Days/Holidays) API Tests
 * Tests Brazilian financial market calendar management
 * Used for calculating business days, D+2 settlements, holidays
 * Uses CreateCalendarioDto, UpdateCalendarioDto
 */
test.describe('Calendário (Business Calendar) API', () => {
    test('GET /api/v1/calendario - Retrieve all calendar entries with pagination', async ({ request }) => {
        const response = await request.get('/api/v1/calendario', {
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

    test('GET /api/v1/calendario/dia-util/{data} - Check if date is business day', async ({ request }) => {
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        const dateStr = tomorrow.toISOString().split('T')[0];

        const response = await request.get(`/api/v1/calendario/dia-util/${dateStr}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(data).toHaveProperty('isDiaUtil');
        expect(typeof data.isDiaUtil).toBe('boolean');
    });

    test('GET /api/v1/calendario/proximo-dia-util/{data} - Get next business day', async ({ request }) => {
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        const dateStr = tomorrow.toISOString().split('T')[0];

        const response = await request.get(`/api/v1/calendario/proximo-dia-util/${dateStr}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(data).toHaveProperty('data');
        expect(typeof data.data).toBe('string');
    });

    test('GET /api/v1/calendario/calcular-d-mais/{data}/{dias} - Calculate D+N business days', async ({ request }) => {
        const today = new Date();
        const dateStr = today.toISOString().split('T')[0];

        const response = await request.get(`/api/v1/calendario/calcular-d-mais/${dateStr}/2`, {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(data).toHaveProperty('dateLiquidacao');
        expect(typeof data.dateLiquidacao).toBe('string');
    });

    test('GET /api/v1/calendario/health - Check calendar health', async ({ request }) => {
        const response = await request.get('/api/v1/calendario/health', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const data = await response.json();
        expect(data).toHaveProperty('status');
    });

    test('POST /api/v1/calendario - Create new calendar entry', async ({ request }) => {
        const newDate = new Date();
        newDate.setDate(newDate.getDate() + Math.floor(Math.random() * 30) + 30);

        const calendarData = {
            ...testData.calendario.create,
            data: newDate.toISOString().split('T')[0],
            descricao: `Holiday ${Date.now()}`,
        };

        const response = await request.post('/api/v1/calendario', {
            data: calendarData,
            headers: authConfig.getAuthHeader(),
        });

        expect([201, 200]).toContain(response.status());
        const created = await response.json();
        expect(created).toHaveProperty('id');
        expect(created.data).toBe(calendarData.data);
    });

    test('GET /api/v1/calendario/{id} - Retrieve specific calendar entry', async ({ request }) => {
        // First create an entry
        const newDate = new Date();
        newDate.setDate(newDate.getDate() + Math.floor(Math.random() * 30) + 35);

        const calendarData = {
            ...testData.calendario.create,
            data: newDate.toISOString().split('T')[0],
            descricao: `Test ${Date.now()}`,
        };

        const createResponse = await request.post('/api/v1/calendario', {
            data: calendarData,
            headers: authConfig.getAuthHeader(),
        });
        const entryId = (await createResponse.json()).id;

        // Then fetch it
        const response = await request.get(`/api/v1/calendario/${entryId}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const entry = await response.json();
        expect(entry.id).toBe(entryId);
    });

    test('PUT /api/v1/calendario/{id} - Update calendar entry', async ({ request }) => {
        // Create an entry
        const newDate = new Date();
        newDate.setDate(newDate.getDate() + Math.floor(Math.random() * 30) + 40);

        const calendarData = {
            ...testData.calendario.create,
            data: newDate.toISOString().split('T')[0],
            descricao: `Original ${Date.now()}`,
        };

        const createResponse = await request.post('/api/v1/calendario', {
            data: calendarData,
            headers: authConfig.getAuthHeader(),
        });
        const entryId = (await createResponse.json()).id;

        // Update it
        const updateData = {
            ...testData.calendario.update,
        };

        const updateResponse = await request.put(`/api/v1/calendario/${entryId}`, {
            data: updateData,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 204]).toContain(updateResponse.status());
    });

    test('DELETE /api/v1/calendario/{id} - Delete calendar entry', async ({ request }) => {
        // Create an entry
        const newDate = new Date();
        newDate.setDate(newDate.getDate() + Math.floor(Math.random() * 30) + 45);

        const calendarData = {
            ...testData.calendario.create,
            data: newDate.toISOString().split('T')[0],
            descricao: `Delete Test ${Date.now()}`,
        };

        const createResponse = await request.post('/api/v1/calendario', {
            data: calendarData,
            headers: authConfig.getAuthHeader(),
        });
        const entryId = (await createResponse.json()).id;

        // Delete it
        const deleteResponse = await request.delete(`/api/v1/calendario/${entryId}`, {
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 204]).toContain(deleteResponse.status());

        // Verify it's deleted
        const getResponse = await request.get(`/api/v1/calendario/${entryId}`, {
            headers: authConfig.getAuthHeader(),
        });
        expect(getResponse.status()).toBe(404);
    });

    test('POST /api/v1/calendario - Validate date format', async ({ request }) => {
        const invalidData = {
            data: 'invalid-date',
            tipoDia: 1,
            praca: 1,
            descricao: 'Test',
        };

        const response = await request.post('/api/v1/calendario', {
            data: invalidData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });
});
