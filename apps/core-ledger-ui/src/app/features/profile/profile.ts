import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe, KeyValuePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../layout/page-header/page-header';
import { AuthService } from '../../auth/auth-service';
import { KeyboardShortcutsService } from '../../services/keyboard-shortcuts.service';

// ============================================================
// INTERFACES
// ============================================================

interface Session {
  id: string;
  device: string;
  browser: string;
  location: string;
  ip: string;
  lastActive: Date;
  isCurrent: boolean;
}

interface ActivityLog {
  id: number;
  time: Date;
  action: string;
  entity?: string;
  type: 'login' | 'logout' | 'settings' | 'security' | 'data';
}

interface Preference {
  id: string;
  label: string;
  description: string;
  enabled: boolean;
}

interface QuickAction {
  key: string;
  label: string;
  icon: string;
  action: string;
  isDanger?: boolean;
}

// ============================================================
// MOCK DATA
// ============================================================

const MOCK_SESSIONS: Session[] = [
  {
    id: 'sess-001',
    device: 'MacBook Pro',
    browser: 'Chrome 120',
    location: 'New York, US',
    ip: '192.168.1.105',
    lastActive: new Date(),
    isCurrent: true,
  },
  {
    id: 'sess-002',
    device: 'iPhone 15 Pro',
    browser: 'Safari Mobile',
    location: 'New York, US',
    ip: '192.168.1.108',
    lastActive: new Date(Date.now() - 1000 * 60 * 45),
    isCurrent: false,
  },
  {
    id: 'sess-003',
    device: 'Windows Desktop',
    browser: 'Edge 119',
    location: 'Boston, US',
    ip: '10.0.0.24',
    lastActive: new Date(Date.now() - 1000 * 60 * 60 * 24 * 2),
    isCurrent: false,
  },
];

const MOCK_ACTIVITY: ActivityLog[] = [
  { id: 1, time: new Date(), action: 'Session started', type: 'login' },
  {
    id: 2,
    time: new Date(Date.now() - 1000 * 60 * 30),
    action: 'Viewed report',
    entity: 'Trial Balance Q4',
    type: 'data',
  },
  {
    id: 3,
    time: new Date(Date.now() - 1000 * 60 * 60),
    action: 'Updated preference',
    entity: 'Dark Mode',
    type: 'settings',
  },
  {
    id: 4,
    time: new Date(Date.now() - 1000 * 60 * 60 * 3),
    action: 'Password changed',
    type: 'security',
  },
  {
    id: 5,
    time: new Date(Date.now() - 1000 * 60 * 60 * 24),
    action: 'Session started',
    entity: 'iPhone 15 Pro',
    type: 'login',
  },
  {
    id: 6,
    time: new Date(Date.now() - 1000 * 60 * 60 * 24 * 2),
    action: 'Exported data',
    entity: 'Portfolio Report',
    type: 'data',
  },
  {
    id: 7,
    time: new Date(Date.now() - 1000 * 60 * 60 * 24 * 3),
    action: 'Session ended',
    type: 'logout',
  },
];

const MOCK_PREFERENCES: Preference[] = [
  {
    id: 'email-notifications',
    label: 'Email Notifications',
    description: 'Receive email alerts for important events',
    enabled: true,
  },
  {
    id: 'nav-alerts',
    label: 'NAV Calculation Alerts',
    description: 'Get notified when NAV calculations complete',
    enabled: true,
  },
  {
    id: 'period-reminders',
    label: 'Period Close Reminders',
    description: 'Reminder before posting period closes',
    enabled: false,
  },
  {
    id: 'session-timeout',
    label: 'Extended Session',
    description: 'Keep session active for 8 hours instead of 2',
    enabled: false,
  },
];

const QUICK_ACTIONS: QuickAction[] = [
  { key: 'F1', label: 'Change Password', icon: 'bi-key', action: 'password' },
  { key: 'F2', label: 'Two-Factor Auth', icon: 'bi-shield-lock', action: '2fa' },
  { key: 'F3', label: 'Export Data', icon: 'bi-download', action: 'export' },
  { key: 'F4', label: 'Sign Out All', icon: 'bi-box-arrow-right', action: 'signout', isDanger: true },
];

// ============================================================
// COMPONENT
// ============================================================

@Component({
  selector: 'app-profile',
  imports: [PageHeader, DatePipe, KeyValuePipe, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Profile {
  private readonly authService = inject(AuthService);
  private readonly keyboardService = inject(KeyboardShortcutsService);

  readonly userProfile = this.authService.userProfile;

  // Keyboard shortcuts from service
  readonly shortcutsByCategory = this.keyboardService.shortcutsByCategory;

  // Signals for mock data
  readonly sessions = signal<Session[]>(MOCK_SESSIONS);
  readonly activityLog = signal<ActivityLog[]>(MOCK_ACTIVITY);
  readonly preferences = signal<Preference[]>(MOCK_PREFERENCES);
  readonly quickActions = signal<QuickAction[]>(QUICK_ACTIONS);

  // Computed: user role from profile
  readonly userRole = computed(() => {
    const profile = this.userProfile();
    // Mock role extraction - in real app would come from claims
    return profile?.email?.includes('admin') ? 'Administrator' : 'Fund Manager';
  });

  // Computed: account age
  readonly memberSince = computed(() => {
    // Mock date - would come from user metadata
    return new Date('2023-06-15');
  });

  // Computed: last login
  readonly lastLogin = computed(() => {
    const current = this.sessions().find(s => s.isCurrent);
    return current?.lastActive ?? new Date();
  });

  // Format relative time
  formatRelativeTime(date: Date): string {
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays}d ago`;

    return date.toLocaleDateString();
  }

  // Get icon for activity type
  getActivityIcon(type: ActivityLog['type']): string {
    switch (type) {
      case 'login':
        return 'bi-box-arrow-in-right';
      case 'logout':
        return 'bi-box-arrow-left';
      case 'security':
        return 'bi-shield-check';
      case 'settings':
        return 'bi-gear';
      case 'data':
        return 'bi-database';
      default:
        return 'bi-circle';
    }
  }

  // Get icon class for activity type
  getActivityIconClass(type: ActivityLog['type']): string {
    switch (type) {
      case 'login':
        return 'activity-log__icon--success';
      case 'security':
        return 'activity-log__icon--warning';
      case 'logout':
        return 'activity-log__icon--danger';
      default:
        return '';
    }
  }

  // Get device icon
  getDeviceIcon(device: string): string {
    if (device.toLowerCase().includes('iphone') || device.toLowerCase().includes('android')) {
      return 'bi-phone';
    }
    if (device.toLowerCase().includes('ipad') || device.toLowerCase().includes('tablet')) {
      return 'bi-tablet';
    }
    return 'bi-laptop';
  }

  // Toggle preference
  togglePreference(prefId: string): void {
    this.preferences.update(prefs =>
      prefs.map(p => (p.id === prefId ? { ...p, enabled: !p.enabled } : p))
    );
  }

  // Handle quick action
  handleAction(action: string): void {
    console.log('Action triggered:', action);
    // In real app, would trigger appropriate action
  }

  // Revoke session
  revokeSession(sessionId: string): void {
    this.sessions.update(sessions => sessions.filter(s => s.id !== sessionId));
  }

  // Get keyboard shortcut key parts for display
  getShortcutKeyParts(keys: string): string[] {
    return this.keyboardService.getKeyParts(keys);
  }

  // Check if running on macOS (affects key display)
  get isMac(): boolean {
    return this.keyboardService.isMacPlatform;
  }

  // Get icon for shortcut category
  getCategoryIcon(category: string): string {
    switch (category.toLowerCase()) {
      case 'navigation':
        return 'bi-signpost-2';
      case 'focus':
        return 'bi-cursor';
      case 'help':
        return 'bi-question-circle';
      default:
        return 'bi-keyboard';
    }
  }
}
