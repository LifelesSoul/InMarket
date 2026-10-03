import { isJsonObject } from '../json';
import { errorMessage } from './errors';
import type { AuthError } from './errors';
import type { KeycloakConfig } from './keycloak';
import { fail, ok } from './result';
import type { Result } from './result';

export type LoginPlan =
  | { provider: 'Auth0' }
  | { provider: 'Keycloak'; config: KeycloakConfig };

interface LoginPlanResponse {
  provider: string;
  authority?: string;
  clientId?: string;
  scopes?: string;
}

const LOGIN_PLAN_URL = `${import.meta.env.VITE_API_URL}/auth/login-url`;
const LOGIN_PLAN_TIMEOUT_MS = 10_000;
const KEYCLOAK_FIELDS = ['authority', 'clientId', 'scopes'] as const;

function isTimeout(err: unknown): boolean {
  return err instanceof DOMException && err.name === 'TimeoutError';
}

function isLoginPlanResponse(value: unknown): value is LoginPlanResponse {
  return isJsonObject(value)
    && typeof value.provider === 'string'
    && KEYCLOAK_FIELDS.every((key) => value[key] === undefined || typeof value[key] === 'string');
}

function toLoginPlan(response: LoginPlanResponse): Result<LoginPlan, AuthError> {
  if (response.provider === 'Auth0') {
    return ok({ provider: 'Auth0' });
  }

  if (response.provider !== 'Keycloak') {
    return fail({ kind: 'unknown-provider' });
  }

  const { authority, clientId, scopes } = response;

  if (authority === undefined || clientId === undefined || scopes === undefined) {
    return fail({ kind: 'plan-invalid' });
  }

  return ok({ provider: 'Keycloak', config: { authority, clientId, scopes } });
}

export async function fetchLoginPlan(): Promise<Result<LoginPlan, AuthError>> {
  let response: Response;

  try {
    response = await fetch(LOGIN_PLAN_URL, { signal: AbortSignal.timeout(LOGIN_PLAN_TIMEOUT_MS) });
  } catch (err: unknown) {
    return fail(
      isTimeout(err)
        ? { kind: 'plan-timeout' }
        : { kind: 'plan-unreachable', message: errorMessage(err) },
    );
  }

  if (!response.ok) {
    return fail({ kind: 'plan-unavailable', status: response.status });
  }

  let body: unknown;

  try {
    body = await response.json();
  } catch {
    return fail({ kind: 'plan-invalid' });
  }

  if (!isLoginPlanResponse(body)) {
    return fail({ kind: 'plan-invalid' });
  }

  return toLoginPlan(body);
}
