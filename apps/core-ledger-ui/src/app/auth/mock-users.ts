import { UserDto } from '../models/user.model';
import { MOCK_TOKENS } from './mock-tokens';

/**
 * Mock user definition matching OIDC and backend structures.
 * Used for development and testing without requiring Auth0.
 */
export interface MockUser {
  /** OIDC UserData format (matches angular-auth-oidc-client) */
  oidcUserData: {
    sub: string;
    name: string;
    email: string;
    nickname: string;
    picture: string;
  };
  /** User permissions for JWT token generation */
  permissions: string[];
  /** Fake JWT token for backend authentication */
  token: string;
  /** Backend user representation (what GET /api/users/me returns) */
  backendUser: UserDto;
}

/**
 * Predefined mock users with different roles for testing.
 * Tokens are simple base64-encoded JSON for development use only.
 */
export const MOCK_USERS: Record<string, MockUser> = {
  admin: {
    oidcUserData: {
      sub: 'mock|admin-001',
      name: 'Admin User',
      email: 'admin@coreledger.local',
      nickname: 'admin',
      picture: 'https://ui-avatars.com/api/?name=Admin+User&background=0D8ABC&color=fff',
    },
    permissions: [
      'ledger:transaction:manage',
      'ledger:transaction:read',
      'portfolio:manage',
      'portfolio:read',
      'nav:manage',
      'nav:read',
      'reports:read',
    ],
    token: MOCK_TOKENS['admin'],
    backendUser: {
      id: 2,
      authProviderId: 'mock|admin-001',
      provider: 'mock',
      email: 'admin@coreledger.local',
      name: 'Admin User',
      lastLoginAt: new Date().toISOString(),
      createdAt: new Date().toISOString(),
      updatedAt: null,
    },
  },
  trader: {
    oidcUserData: {
      sub: 'mock|trader-003',
      name: 'Trader User',
      email: 'trader@coreledger.local',
      nickname: 'trader',
      picture:
        'https://ui-avatars.com/api/?name=Trader+User&background=FD7E14&color=fff',
    },
    permissions: [
      'ledger:transaction:manage',
      'ledger:transaction:read',
      'portfolio:manage',
      'portfolio:read',
    ],
    token: MOCK_TOKENS['trader'],
    backendUser: {
      id: 4,
      authProviderId: 'mock|trader-003',
      provider: 'mock',
      email: 'trader@coreledger.local',
      name: 'Trader User',
      lastLoginAt: new Date().toISOString(),
      createdAt: new Date().toISOString(),
      updatedAt: null,
    },
  },
  analyst: {
    oidcUserData: {
      sub: 'mock|analyst-004',
      name: 'Analyst Viewer',
      email: 'analyst@coreledger.local',
      nickname: 'analyst',
      picture:
        'https://ui-avatars.com/api/?name=Analyst+Viewer&background=198754&color=fff',
    },
    permissions: [
      'ledger:transaction:read',
      'portfolio:read',
      'nav:read',
      'reports:read',
    ],
    token: MOCK_TOKENS['analyst'],
    backendUser: {
      id: 4,
      authProviderId: 'mock|analyst-004',
      provider: 'mock',
      email: 'analyst@coreledger.local',
      name: 'Analyst Viewer',
      lastLoginAt: new Date().toISOString(),
      createdAt: new Date().toISOString(),
      updatedAt: null,
    },
  },
};

/**
 * Helper function to get a mock user by role.
 * Throws an error if the role doesn't exist.
 *
 * @param role - User role key (admin, trader, analyst)
 * @returns MockUser object with OIDC and backend data
 * @throws Error if role is not found
 */
export function getMockUser(role: string): MockUser {
  const user = MOCK_USERS[role];
  if (!user) {
    const validRoles = Object.keys(MOCK_USERS).join(', ');
    throw new Error(`Unknown mock user role: ${role}. Valid roles: ${validRoles}`);
  }
  return user;
}
