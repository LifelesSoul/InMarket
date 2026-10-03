import { fail, ok } from '../result';
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
        return fail({ kind: 'session-expired' });
      }

      return ok(session.accessToken);
    },
    logout() {
      window.location.assign(buildLogoutUrl(session));
    },
  };
}
