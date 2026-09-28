import type { ProviderSession } from '../session';
import { buildLogoutUrl } from './strategy';
import type { KeycloakSession } from './types';

export function createKeycloakSession(
  session: KeycloakSession,
  onExpired: () => void,
): ProviderSession {
  return {
    provider: 'Keycloak',
    user: session.user,
    async getAccessToken() {
      if (session.expiresAt <= Date.now()) {
        onExpired();
        return null;
      } 

      return session.accessToken;
    },
    logout() {
      window.location.assign(buildLogoutUrl(session));
    },
  };
}
