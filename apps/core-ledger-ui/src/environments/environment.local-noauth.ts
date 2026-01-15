// Development environment configuration
export const environment = {
  production: false,
  apiUrl: '/api',
  logLevel: 'debug' as const,
  enableSentry: false, // Enable Sentry for development environment

  // Sentry Configuration
  sentry: {
    dsn: 'http://b7728d237cc34ee99eecb03fc6213b80@localhost:8000/1',
    environment: 'development',
    release: '1.0.0',
  },

  // Auth0 / OIDC Configuration
  auth: {
    useMock: true, // Enable mock authentication for development environment
    mockUser: 'admin', // Mock user role: admin, trader, analyst
    authority: 'https://dev-7yj4txd3qg4xsckj.us.auth0.com',
    clientId: 'PDPnT19fdBAv5VLjg3OR4U6N5wqkGohT',
    scope: 'openid profile email offline_access',
    audience: 'https://core-ledger-api',
  },

  // API Mock Configuration
  api: {
    useMock: true, // Enable mock API for local development
    mockDelayMs: 300, // Simulate network latency (300ms)
    mockErrorRate: 0, // Probability of random errors (0 = none, 0.1 = 10%)
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
    useMock: true, // Enable mock SignalR for local development
    hubUrl: '/hubs/notifications',
    reconnectDelays: [0, 2000, 5000, 10000, 30000],
    maxReconnectAttempts: 5,
  },
};
