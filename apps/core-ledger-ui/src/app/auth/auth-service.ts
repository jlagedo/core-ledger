import { Injectable, inject, computed, effect } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { UserService } from '../services/user-service';
import { environment } from '../../environments/environment';
import { MockAuthService } from './mock-auth.service';

export interface UserProfile {
  name: string;
  email: string;
  picture: string;
  sub: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly oidcSecurityService = inject(OidcSecurityService);
  private readonly mockAuthService = inject(MockAuthService, { optional: true });
  private readonly userService = inject(UserService);

  // Determine which auth service to use based on environment
  private readonly authServiceInUse = environment.auth.useMock
    ? this.mockAuthService!
    : this.oidcSecurityService;

  private readonly authState = toSignal(this.authServiceInUse.isAuthenticated$);

  private readonly userData = toSignal(this.authServiceInUse.userData$);

  readonly isLoggedIn = computed(() => this.authState()?.isAuthenticated ?? false);

  readonly userProfile = computed<UserProfile | null>(() => {
    const data = this.userData();
    if (!data?.userData) return null;

    return {
      name: data.userData.name ?? data.userData.nickname ?? 'User',
      email: data.userData.email ?? '',
      picture: data.userData.picture ?? '',
      sub: data.userData.sub ?? '',
    };
  });

  constructor() {
    // Reactive effect: fetch user from backend when login state changes to true
    effect(() => {
      if (this.isLoggedIn()) {
        // In mock mode, bypass backend call and use mock user data directly
        if (environment.auth.useMock && this.mockAuthService) {
          const mockBackendUser = this.mockAuthService.getMockBackendUser();
          this.userService.setUser(mockBackendUser);
        } else {
          // Real mode: fetch from backend API
          this.userService.fetchCurrentUser().subscribe({
            next: (user) => this.userService.setUser(user),
            error: () => {
              // Error already logged in UserService
              // App remains functional without database user data
            },
          });
        }
      }
    });
  }

  login(): void {
    this.authServiceInUse.authorize();
  }

  logout(): void {
    // Clear user data before logout
    this.userService.clearUser();
    this.authServiceInUse.logoff().subscribe();
  }
}
