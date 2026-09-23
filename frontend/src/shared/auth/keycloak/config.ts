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

export function toTrustedAuthority(rawAuthority: string): TrustedAuthority {
  const expected = trustedOrigin();

  if (expected === null) {
    throw new Error(
      'VITE_KEYCLOAK_URL is not set, so this build trusts no Keycloak origin. ' +
        'Add it to frontend/.env and restart the dev server.',
    );
  }

  let parsed: URL;

  try {
    parsed = new URL(rawAuthority);
  } catch {
    throw new TypeError('The Keycloak authority is not a valid absolute url');
  }

  if (parsed.origin !== expected) {
    throw new Error(`The Keycloak authority is not on the origin this build trusts (${expected})`);
  }

  const realmPath = parsed.pathname.endsWith('/')
    ? parsed.pathname.slice(0, -1)
    : parsed.pathname;

  return { origin: expected, realmPath };
}

export function validateConfig(config: KeycloakConfig): ValidatedConfig {
  if (!CLIENT_ID_PATTERN.test(config.clientId)) {
    throw new TypeError('The Keycloak client id has an unexpected shape');
  }

  if (!SCOPE_PATTERN.test(config.scopes)) {
    throw new TypeError('The Keycloak scopes have an unexpected shape');
  }

  return {
    authority: toTrustedAuthority(config.authority),
    clientId: config.clientId,
    scopes: config.scopes,
  };
}
