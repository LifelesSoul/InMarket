import { unexpected } from './errors';
import type { AuthError } from './errors';
import { completeLogin, restoreSession, startLogin, validateConfig } from './keycloak';
import type { CompletedLogin, KeycloakSession } from './keycloak';
import { fetchLoginPlan } from './loginPlan';
import { fail, ok } from './result';
import type { Result } from './result';

export async function restoreKeycloak(): Promise<Result<KeycloakSession | null, AuthError>> {
  const plan = await fetchLoginPlan();

  if (!plan.ok) {
    return plan;
  }

  if (plan.value.provider !== 'Keycloak') {
    return ok(null);
  }

  const config = validateConfig(plan.value.config);

  if (!config.ok) {
    return config;
  }

  return restoreSession(config.value);
}

export async function beginLogin(
  returnTo: string,
  loginWithAuth0: () => Promise<void>,
): Promise<Result<void, AuthError>> {
  const plan = await fetchLoginPlan();

  if (!plan.ok) {
    return plan;
  }

  if (plan.value.provider === 'Auth0') {
    try {
      await loginWithAuth0();
      return ok(undefined);
    } catch (err: unknown) {
      return fail(unexpected(err));
    }
  }

  const config = validateConfig(plan.value.config);

  if (!config.ok) {
    return config;
  }

  return startLogin(config.value, returnTo);
}

export async function finishKeycloakLogin(
  search: string,
): Promise<Result<CompletedLogin, AuthError>> {
  const plan = await fetchLoginPlan();

  if (!plan.ok) {
    return plan;
  }

  if (plan.value.provider !== 'Keycloak') {
    return fail({ kind: 'provider-changed' });
  }

  const config = validateConfig(plan.value.config);

  if (!config.ok) {
    return config;
  }

  return completeLogin(config.value, new URLSearchParams(search));
}
