import { UserDto } from '../models/user.model';

/**
 * Production stub for mock users.
 * This file replaces mock-users.ts in production builds.
 *
 * ⛔ PRODUCTION BUILD - MOCK USERS DISABLED
 */

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

export const MOCK_USERS: Record<string, MockUser> = new Proxy(
  {},
  {
    get() {
      throw new Error(
        'CRITICAL SECURITY ERROR: Mock users are not available in production. ' +
          'This indicates a build configuration error. Please check your Angular build settings.'
      );
    },
  }
);

/**
 * Helper function to get a mock user by role.
 * Throws an error in production builds.
 *
 * @param role - User role key (admin, trader, analyst)
 * @returns MockUser object with OIDC and backend data
 * @throws Error always in production
 */
export function getMockUser(role: string): MockUser {
  throw new Error(
    'CRITICAL SECURITY ERROR: Mock authentication is not available in production. ' +
      'This indicates a build configuration error. Please check your Angular build settings.'
  );
}
