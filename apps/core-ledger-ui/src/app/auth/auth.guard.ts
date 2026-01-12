import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AuthService } from './auth-service';

/**
 * Universal auth guard that works with both OIDC and mock authentication.
 * Uses AuthService which abstracts the underlying auth mechanism.
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Use signal-based check (synchronous and works with both auth modes)
  if (authService.isLoggedIn()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
