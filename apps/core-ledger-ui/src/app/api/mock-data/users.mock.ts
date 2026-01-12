import { UserDto } from '../../models/user.model';

/**
 * Mock user data for local development and testing.
 * Matches mock users from auth/mock-users.ts
 * @internal
 */
export const MOCK_USERS: UserDto[] = [
    {
        id: 1,
        authProviderId: 'auth0|admin',
        provider: 'auth0',
        email: 'admin@coreledger.com',
        name: 'Admin User',
        lastLoginAt: '2024-12-01T10:00:00Z',
        createdAt: '2020-01-01T00:00:00Z',
        updatedAt: '2024-12-01T10:00:00Z',
    },
    {
        id: 2,
        authProviderId: 'auth0|trader',
        provider: 'auth0',
        email: 'trader@coreledger.com',
        name: 'Trader User',
        lastLoginAt: '2024-12-01T08:45:00Z',
        createdAt: '2020-01-01T00:00:00Z',
        updatedAt: '2024-12-01T08:45:00Z',
    },
    {
        id: 3,
        authProviderId: 'auth0|analyst',
        provider: 'auth0',
        email: 'analyst@coreledger.com',
        name: 'Analyst User',
        lastLoginAt: '2024-12-01T11:15:00Z',
        createdAt: '2020-01-01T00:00:00Z',
        updatedAt: '2024-12-01T11:15:00Z',
    },
];
