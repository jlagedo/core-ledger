import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { getMockUser, MockUser } from './mock-users';
import { UserDto } from '../models/user.model';

/**
 * Authentication result structure matching angular-auth-oidc-client.
 */
export interface AuthenticatedResult {
  isAuthenticated: boolean;
  userData?: any;
  accessToken?: string;
  idToken?: string;
  configId?: string;
}

/**
 * Mock authentication service for development and testing.
 * Simulates the OidcSecurityService API without requiring Auth0 connection.
 *
 * IMPORTANT: Only for development. Will throw error if used in production.
 *
 * Usage:
 * 1. Set environment.auth.useMock = true
 * 2. Set environment.auth.mockUser to desired role (admin, trader, analyst)
 * 3. Restart dev server
 *
 * @remarks
 * Intentionally NOT using `providedIn: 'root'` because this service is
 * conditionally provided based on environment.auth.useMock flag.
 * See `src/app/app.config.ts` for conditional provider setup.
 *
 * @internal This service is replaced with real OidcSecurityService in production.
 */
@Injectable()
export class MockAuthService {
  private readonly mockUser: MockUser;
  private readonly _isAuthenticated$ = new BehaviorSubject<AuthenticatedResult>({
    isAuthenticated: true,
    accessToken: '',
    configId: 'mock',
  });
  private readonly _userData$ = new BehaviorSubject<any>({ userData: null });

  readonly isAuthenticated$ = this._isAuthenticated$.asObservable();
  readonly userData$ = this._userData$.asObservable();

  constructor() {
    if (!environment.auth.useMock || !environment.auth.mockUser) {
      throw new Error(
        'MockAuthService requires useMock: true and mockUser to be set in environment config'
      );
    }

    if (environment.production) {
      throw new Error('CRITICAL: Mock auth cannot be enabled in production!');
    }

    this.mockUser = getMockUser(environment.auth.mockUser);
    this.performMockLogin();
  }

  /**
   * Performs the mock login process by emitting authentication state.
   * Simulates async behavior with a small delay for realistic testing.
   */
  private performMockLogin(): void {
    setTimeout(() => {
      this._isAuthenticated$.next({
        isAuthenticated: true,
        userData: this.mockUser.oidcUserData,
        accessToken: this.mockUser.token,
        idToken: this.mockUser.token,
        configId: 'mock',
      });

      this._userData$.next({
        userData: this.mockUser.oidcUserData,
      });
    }, 100);
  }

  /**
   * Matches OidcSecurityService.checkAuth()
   * Called during app initialization to verify authentication state.
   */
  checkAuth(): Observable<AuthenticatedResult> {
    return of({
      isAuthenticated: true,
      userData: this.mockUser.oidcUserData,
      accessToken: this.mockUser.token,
      idToken: this.mockUser.token,
      configId: 'mock',
    }).pipe(delay(50));
  }

  /**
   * Matches OidcSecurityService.authorize()
   * Called when user clicks "login" button.
   * In mock mode, already logged in, so just emit state again.
   */
  authorize(): void {
    this.performMockLogin();
  }

  /**
   * Matches OidcSecurityService.logoff()
   * Called when user logs out.
   * Resets authentication state to logged out.
   */
  logoff(): Observable<any> {
    this._isAuthenticated$.next({
      isAuthenticated: false,
      configId: 'mock',
    });
    this._userData$.next({ userData: null });

    return of(null).pipe(delay(100));
  }

  /**
   * Get the current mock token for HTTP interceptor.
   * Returns the fake JWT token string.
   */
  getAccessToken(): string {
    return this.mockUser.token;
  }

  /**
   * Get the mock user's backend representation.
   * Used to bypass /api/users/me call in mock mode.
   * Returns what the backend would normally return from GET /api/users/me.
   */
  getMockBackendUser(): UserDto {
    return this.mockUser.backendUser;
  }
}
