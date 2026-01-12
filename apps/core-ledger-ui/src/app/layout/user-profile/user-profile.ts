import { ChangeDetectionStrategy, Component, input, inject, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import {
  NgbDropdown,
  NgbDropdownToggle,
  NgbDropdownMenu,
  NgbDropdownItem,
} from '@ng-bootstrap/ng-bootstrap';
import { ThemeService } from '../../services/theme-service';
import { AuthService } from '../../auth/auth-service';
import { UserService } from '../../services/user-service';

@Component({
  selector: 'app-user-profile',
  imports: [RouterLink, DatePipe, NgbDropdown, NgbDropdownToggle, NgbDropdownMenu, NgbDropdownItem],
  templateUrl: './user-profile.html',
  styleUrl: './user-profile.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserProfile {
  private readonly themeService = inject(ThemeService);
  private readonly authService = inject(AuthService);
  private readonly userService = inject(UserService);

  isCollapsed = input.required<boolean>();

  readonly isDarkMode = this.themeService.isDarkMode;
  readonly userProfile = this.authService.userProfile;
  readonly currentUser = this.userService.currentUser;

  // Computed signal to merge Auth0 profile with database user
  readonly displayName = computed(() => {
    const dbUser = this.currentUser();
    const authProfile = this.userProfile();

    // Prefer database name if available, fallback to Auth0 profile
    return dbUser?.name || authProfile?.name || 'User';
  });

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  logout(): void {
    this.authService.logout();
  }
}
