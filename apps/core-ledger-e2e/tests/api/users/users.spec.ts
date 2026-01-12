import { test, expect } from '@playwright/test';
import { authConfig } from '../../config/auth.config';

test.describe('Users API', () => {
    test('GET /api/users/me - Retrieve current authenticated user profile', async ({ request }) => {
        const response = await request.get('/api/users/me', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const userProfile = await response.json();
        expect(userProfile).toHaveProperty('id');
        expect(userProfile).toHaveProperty('authProviderId');
        expect(userProfile).toHaveProperty('provider');
        expect(userProfile).toHaveProperty('email');
        expect(userProfile).toHaveProperty('name');
        expect(userProfile).toHaveProperty('lastLoginAt');
        expect(userProfile).toHaveProperty('createdAt');
    });

    test('GET /api/users/me - Should have valid user data', async ({ request }) => {
        const response = await request.get('/api/users/me', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const userProfile = await response.json();
        expect(typeof userProfile.id).toBe('number');
        expect(typeof userProfile.email).toBe('string');
        expect(typeof userProfile.name).toBe('string');
    });

    test('GET /api/users/me - Should fail without authentication', async ({ request }) => {
        const response = await request.get('/api/users/me');

        expect(response.status()).toBe(401);
    });

    test('GET /api/users/me - Verify profile structure', async ({ request }) => {
        const response = await request.get('/api/users/me', {
            headers: authConfig.getAuthHeader(),
        });

        expect(response.status()).toBe(200);
        const userProfile = await response.json();

        // Check all expected properties exist
        expect(userProfile).toBeDefined();
        expect(userProfile.email).toBeTruthy();
        expect(userProfile.name).toBeTruthy();
    });
});
