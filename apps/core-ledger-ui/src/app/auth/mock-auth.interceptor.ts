import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MockAuthService } from './mock-auth.service';

/**
 * Functional HTTP interceptor to attach mock JWT tokens to API requests.
 * Only active when mock auth is enabled in environment config.
 *
 * This interceptor:
 * - Only intercepts requests to /api/** routes
 * - Adds Authorization header with mock JWT token
 * - Allows requests to other domains to pass through unchanged
 *
 * Usage: Automatically registered in app.config.ts when environment.auth.useMock is true
 */
export const mockAuthInterceptor: HttpInterceptorFn = (req, next) => {
  // Only intercept /api/** requests
  if (!req.url.includes('/api/')) {
    return next(req);
  }

  const mockAuthService = inject(MockAuthService);
  const token = mockAuthService.getAccessToken();

  // Clone request and add Authorization header
  const authReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });

  return next(authReq);
};
