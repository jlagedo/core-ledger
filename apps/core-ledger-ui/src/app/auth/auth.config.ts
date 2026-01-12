import {PassedInitialConfig} from 'angular-auth-oidc-client';
import {environment} from '../../environments/environment';

export const authConfig: PassedInitialConfig = {
  config: {
    authority: environment.auth.authority,
    redirectUrl: window.location.origin,
    postLogoutRedirectUri: window.location.origin + '/login',
    clientId: environment.auth.clientId,
    scope: environment.auth.scope,
    responseType: 'code',
    silentRenew: true,
    useRefreshToken: true,
    secureRoutes: ['/api'],
    customParamsAuthRequest: {
      audience: environment.auth.audience,
    },
  },
};
