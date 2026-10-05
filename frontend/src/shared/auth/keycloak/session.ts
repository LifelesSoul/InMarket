import type { AuthError } from '../errors';
import { fail, ok, type Result } from '../result';
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
    getAccessToken(): Promise<Result<string, AuthError>> {
      if (session.expiresAt <= Date.now()) {
        onExpired();
        return Promise.resolve(fail({ kind: 'session-expired' }));
      }

      return Promise.resolve(ok(session.accessToken));
    },
    logout() {
      window.location.assign(buildLogoutUrl(session));
    },
  };
}
