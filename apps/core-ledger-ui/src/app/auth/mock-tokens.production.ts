/**
 * Production stub for mock tokens.
 * This file replaces mock-tokens.ts in production builds.
 *
 * ⛔ PRODUCTION BUILD - MOCK TOKENS DISABLED
 */

export const MOCK_TOKENS: Record<string, string> = new Proxy(
  {},
  {
    get() {
      throw new Error(
        'CRITICAL SECURITY ERROR: Mock authentication tokens are not available in production. ' +
          'This indicates a build configuration error. Please check your Angular build settings.'
      );
    },
  }
);
