import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData } from '../../config/test-data';

test.describe('Jobs Ingestion API', () => {
    test('POST /api/jobs-ingestion/test-connection - Test API -> Queue -> Worker connection', async ({ request }) => {
        const response = await request.post('/api/jobs-ingestion/test-connection', {
            data: testData.jobsIngestion.testConnection,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(202);
        const result = await response.json();
        expect(result).toHaveProperty('coreJobId');
        expect(result).toHaveProperty('referenceId');
        expect(result).toHaveProperty('status');
        expect(result).toHaveProperty('message');
    });

    test('POST /api/jobs-ingestion/test-connection - Should accept test connection request', async ({ request }) => {
        const testRequest = {
            referenceId: `TEST-${Date.now()}`,
            jobDescription: 'Test Job',
        };

        const response = await request.post('/api/jobs-ingestion/test-connection', {
            data: testRequest,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(202);
        const result = await response.json();
        expect(result.referenceId).toBe(testRequest.referenceId);
    });

    test('POST /api/jobs-ingestion/import-b3-instruction-file - Import B3 instruction file', async ({ request }) => {
        const response = await request.post('/api/jobs-ingestion/import-b3-instruction-file', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(202);
        const result = await response.json();
        expect(result).toHaveProperty('coreJobId');
        expect(result).toHaveProperty('referenceId');
        expect(result).toHaveProperty('status');
        expect(result).toHaveProperty('message');
    });
});
