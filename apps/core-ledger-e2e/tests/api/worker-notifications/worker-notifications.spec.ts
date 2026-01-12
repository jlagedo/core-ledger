import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';
import { testData } from '../../config/test-data';

/**
 * Worker Notifications API Tests
 * Tests transaction processed notification endpoint
 * Only endpoint available: POST /api/worker-notifications/transaction-processed
 */
test.describe('Worker Notifications API', () => {
    test('POST /api/worker-notifications/transaction-processed - Notify transaction processed successfully', async ({ request }) => {
        const notificationData = {
            ...testData.workerNotification.transactionProcessed,
            transactionId: Math.floor(Math.random() * 10000),
            correlationId: `corr-${Date.now()}`,
            processedAt: new Date().toISOString(),
        };

        const response = await request.post('/api/worker-notifications/transaction-processed', {
            data: notificationData,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 202, 204]).toContain(response.status());
    });

    test('POST /api/worker-notifications/transaction-processed - Notify transaction processed with failure', async ({ request }) => {
        const notificationData = {
            transactionId: Math.floor(Math.random() * 10000),
            success: false,
            finalStatusId: 8, // Failed
            errorMessage: 'Insufficient funds for transaction',
            processedAt: new Date().toISOString(),
            correlationId: `corr-fail-${Date.now()}`,
            createdByUserId: 'system',
        };

        const response = await request.post('/api/worker-notifications/transaction-processed', {
            data: notificationData,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 202, 204]).toContain(response.status());
    });

    test('POST /api/worker-notifications/transaction-processed - Validate required fields', async ({ request }) => {
        const invalidData = {
            // Missing transactionId
            success: true,
            finalStatusId: 2,
        };

        const response = await request.post('/api/worker-notifications/transaction-processed', {
            data: invalidData,
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(400);
    });

    test('POST /api/worker-notifications/transaction-processed - Notify with correlation ID', async ({ request }) => {
        const correlationId = `test-corr-${Date.now()}`;
        const notificationData = {
            transactionId: Math.floor(Math.random() * 10000),
            success: true,
            finalStatusId: 2,
            errorMessage: null,
            processedAt: new Date().toISOString(),
            correlationId: correlationId,
            createdByUserId: 'worker-1',
        };

        const response = await request.post('/api/worker-notifications/transaction-processed', {
            data: notificationData,
            headers: authConfig.getAuthHeader(),
        });

        expect([200, 202, 204]).toContain(response.status());
    });

    test('POST /api/worker-notifications/transaction-processed - Notify multiple transactions', async ({ request }) => {
        const notifications = [
            {
                transactionId: Math.floor(Math.random() * 10000),
                success: true,
                finalStatusId: 2,
                errorMessage: null,
                processedAt: new Date().toISOString(),
                correlationId: `batch-1-${Date.now()}`,
                createdByUserId: 'batch-processor',
            },
            {
                transactionId: Math.floor(Math.random() * 10000),
                success: true,
                finalStatusId: 2,
                errorMessage: null,
                processedAt: new Date().toISOString(),
                correlationId: `batch-2-${Date.now()}`,
                createdByUserId: 'batch-processor',
            },
        ];

        for (const notification of notifications) {
            const response = await request.post('/api/worker-notifications/transaction-processed', {
                data: notification,
                headers: authConfig.getAuthHeader(),
            });

            expect([200, 202, 204]).toContain(response.status());
        }
    });

    test('POST /api/worker-notifications/transaction-processed - Should require authentication', async ({ request }) => {
        const notificationData = {
            ...testData.workerNotification.transactionProcessed,
            transactionId: Math.floor(Math.random() * 10000),
        };

        const response = await request.post('/api/worker-notifications/transaction-processed', {
            data: notificationData,
            // No auth header
        });

        expect(response.status()).toBe(401);
    });
});
