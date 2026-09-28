import type { AuthError } from './errors';
import type { KeycloakConfig } from './keycloak';
import { fail, ok } from './result';
import type { Result } from './result';

export type LoginPlan =
  | { provider: 'Auth0' }
  | { provider: 'Keycloak'; config: KeycloakConfig };

const LOGIN_PLAN_URL = `${import.meta.env.VITE_API_URL}/auth/login-url`;
const LOGIN_PLAN_TIMEOUT_MS = 10_000;

function str(v: unknown): string {
  return typeof v === 'string' ? v : '';
}

function toLoginPlan(data: unknown): Result<LoginPlan, AuthError> {
  const source = (data ?? {}) as Record<string, unknown>;

  if (source.provider === 'Auth0') {
    return ok({ provider: 'Auth0' });
  }

  if (source.provider === 'Keycloak') {
    return ok({
      provider: 'Keycloak',
      config: {
        authority: str(source.authority),
        clientId: str(source.clientId),
        scopes: str(source.scopes),
      },
    });
  }

  return fail({ kind: 'unknown-provider' });
}

export async function fetchLoginPlan(): Promise<Result<LoginPlan, AuthError>> {
  let response: Response;

  try {
    response = await fetch(LOGIN_PLAN_URL, { signal: AbortSignal.timeout(LOGIN_PLAN_TIMEOUT_MS) });
  } catch {
    return fail({ kind: 'plan-unavailable' });
  }

  if (!response.ok) {
    return fail({ kind: 'plan-unavailable', status: response.status });
  }

  let data: unknown;

  try {
    data = await response.json();
  } catch {
    return fail({ kind: 'plan-unavailable', status: response.status });
  }

  return toLoginPlan(data);
}
