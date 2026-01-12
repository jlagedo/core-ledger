// Production environment configuration
export const environment = {
  production: true,
  apiUrl: '/api', // This would be your production API URL
  logLevel: 'warn' as const,
  enableSentry: true,

  // Sentry Configuration
  sentry: {
    dsn: '', // TODO: Add your production Sentry DSN here
    environment: 'production',
    release: '1.0.0', // TODO: This should be updated from your CI/CD pipeline
  },

  // Auth0 / OIDC Configuration
  // TODO: Complete Auth0 production configuration before deployment
  // Required steps:
  // 1. Set up production Auth0 tenant at https://auth0.com
  // 2. Create production application in Auth0 dashboard
  // 3. Configure allowed callback URLs, logout URLs, and web origins
  // 4. Update authority with production Auth0 domain
  // 5. Update clientId with production Auth0 application client ID
  // 6. Update audience with production API identifier
  // 7. Verify scope matches backend API requirements
  auth: {
    useMock: false, // Mock auth must NEVER be enabled in production
    mockUser: 'admin', // Only used when useMock: true
    authority: '', // TODO: Production Auth0 authority URL (e.g., https://your-domain.auth0.com)
    clientId: '', // TODO: Production Auth0 client ID
    scope: 'openid profile email offline_access',
    audience: 'https://core-ledger-api', // TODO: Production API audience/identifier
  },

  // API Mock Configuration
  api: {
    useMock: false, // Mock API must NEVER be enabled in production
    mockDelayMs: 0,
    mockErrorRate: 0,
  },

  // Toast Notification Delays (milliseconds)
  toast: {
    defaultDelay: 8000, // Shorter in production
    successDelay: 4000,
    errorDelay: 10000, // Longer for errors in production
    serviceUnavailableDelay: 15000,
  },

  // Pagination Defaults
  pagination: {
    defaultPageSize: 15,
    apiDefaultLimit: 100,
  },

  // Storage Keys (same as dev to maintain user preferences)
  storage: {
    theme: 'core-ledger-theme',
    sidenavState: 'sidenav-state',
  },

  // Feature Flags
  features: {
    sidenavDefaultCollapsed: true,
  },

  // SignalR / Notification Hub Configuration
  signalr: {
    useMock: false, // Mock SignalR must NEVER be enabled in production
    hubUrl: '/api/hub/notifications',
    reconnectDelays: [0, 2000, 5000, 10000, 30000, 60000],
    maxReconnectAttempts: 10, // More attempts in production
  },
};
