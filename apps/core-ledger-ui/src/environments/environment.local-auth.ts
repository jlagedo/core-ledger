// Development environment configuration
export const environment = {
  production: false,
  apiUrl: '/api',
  logLevel: 'debug' as const,
  enableSentry: true, // Enable Sentry for development environment

  // Sentry Configuration
  sentry: {
    dsn: 'http://b7728d237cc34ee99eecb03fc6213b80@localhost:8000/1',
    environment: 'development',
    release: '1.0.0',
  },

  // Auth0 / OIDC Configuration
  auth: {
    useMock: false, // Enable mock authentication for development environment
    mockUser: 'admin', // Mock user role: admin, trader, analyst
    authority: 'https://dev-7yj4txd3qg4xsckj.us.auth0.com',
    clientId: 'PDPnT19fdBAv5VLjg3OR4U6N5wqkGohT',
    scope: 'openid profile email offline_access',
    audience: 'https://core-ledger-api',
  },

  // API Mock Configuration
  api: {
    useMock: false, // Use real API with Auth0 authentication
    mockDelayMs: 0,
    mockErrorRate: 0,
  },

  // Toast Notification Delays (milliseconds)
  toast: {
    defaultDelay: 10000,
    successDelay: 5000,
    errorDelay: 8000,
    serviceUnavailableDelay: 15000,
  },

  // Pagination Defaults
  pagination: {
    defaultPageSize: 15,
    apiDefaultLimit: 100,
  },

  // Storage Keys
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
    useMock: true, // Use real SignalR with Auth0 authentication
    hubUrl: '/api/hub/notifications',
    reconnectDelays: [0, 2000, 5000, 10000, 30000],
    maxReconnectAttempts: 5,
  },
};
