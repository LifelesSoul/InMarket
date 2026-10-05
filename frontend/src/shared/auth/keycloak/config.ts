import type { AuthError } from '../errors';
import { fail, ok, type Result } from '../result';
import type { KeycloakConfig, TrustedAuthority, ValidatedConfig } from './types';

const CLIENT_ID_PATTERN = /^[\w-]{1,128}$/;
const SCOPE_PATTERN = /^[\w .:/-]{1,256}$/;

function trustedOrigin(): Result<string, AuthError> {
  const configured = import.meta.env.VITE_KEYCLOAK_URL;

  if (typeof configured !== 'string' || !URL.canParse(configured)) {
    return fail({ kind: 'trusted-origin-missing' });
  }

  return ok(new URL(configured).origin);
}

export function toTrustedAuthority(rawAuthority: string): Result<TrustedAuthority, AuthError> {
  const expected = trustedOrigin();

  if (!expected.ok) {
    return expected;
  }

  if (!URL.canParse(rawAuthority)) {
    return fail({ kind: 'invalid-config', field: 'authority' });
  }

  const parsed = new URL(rawAuthority);

  if (parsed.origin !== expected.value) {
    return fail({ kind: 'untrusted-authority', expectedOrigin: expected.value });
  }

  const realmPath = parsed.pathname.endsWith('/')
    ? parsed.pathname.slice(0, -1)
    : parsed.pathname;

  return ok({ origin: expected.value, realmPath });
}

export function validateConfig(config: KeycloakConfig): Result<ValidatedConfig, AuthError> {
  if (!CLIENT_ID_PATTERN.test(config.clientId)) {
    return fail({ kind: 'invalid-config', field: 'clientId' });
  }

  if (!SCOPE_PATTERN.test(config.scopes)) {
    return fail({ kind: 'invalid-config', field: 'scopes' });
  }

  const authority = toTrustedAuthority(config.authority);

  if (!authority.ok) {
    return authority;
  }

  return ok({
    authority: authority.value,
    clientId: config.clientId,
    scopes: config.scopes,
  });
}
