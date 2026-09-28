import type { AuthError } from '../errors';
import { fail, ok, type Result } from '../result';
import type { KeycloakConfig, TrustedAuthority, ValidatedConfig } from './types';

const CLIENT_ID_PATTERN = /^[\w-]{1,128}$/;
const SCOPE_PATTERN = /^[\w .:/-]{1,256}$/;

function trustedOrigin(): string | null {
  const configured = import.meta.env.VITE_KEYCLOAK_URL;

  if (typeof configured !== 'string' || configured.length === 0) {
    return null;
  }

  try {
    return new URL(configured).origin;
  } catch {
    return null;
  }
}

export function toTrustedAuthority(rawAuthority: string): Result<TrustedAuthority, AuthError> {
  const expected = trustedOrigin();

  if (expected === null) {
    return fail({ kind: 'untrusted-authority', expectedOrigin: null });
  }

  let parsed: URL;

  try {
    parsed = new URL(rawAuthority);
  } catch {
    return fail({ kind: 'invalid-config', field: 'authority' });
  }

  if (parsed.origin !== expected) {
    return fail({ kind: 'untrusted-authority', expectedOrigin: expected });
  }

  const realmPath = parsed.pathname.endsWith('/')
    ? parsed.pathname.slice(0, -1)
    : parsed.pathname;

  return ok({ origin: expected, realmPath });
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
