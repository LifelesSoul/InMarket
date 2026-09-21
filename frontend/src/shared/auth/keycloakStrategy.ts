import { createChallenge, createState, createVerifier } from './pkce';
import type { AuthUser } from './types';

export interface KeycloakConfig {
  authority: string;
  clientId: string;
  scopes: string;
}

export interface TrustedAuthority {
  origin: string;
  realmPath: string;
}

export interface KeycloakSession {
  accessToken: string;
  expiresAt: number;
  user: AuthUser;
  authority: TrustedAuthority;
}

interface PendingLogin {
  verifier: string;
  state: string;
  returnTo: string;
}

const PENDING_KEY = 'inmarket.auth.keycloak.pending';
const CLIENT_ID_PATTERN = /^[\w-]{1,128}$/;
const SCOPE_PATTERN = /^[\w .:/-]{1,256}$/;

export const CALLBACK_PATH = '/auth/callback';

function redirectUri(): string {
  return `${window.location.origin}${CALLBACK_PATH}`;
}

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

function endpoint(authority: TrustedAuthority, name: string): string {
  return new URL(`${authority.realmPath}/protocol/openid-connect/${name}`, authority.origin).toString();
}

function safeReturnTo(value: string): string {
  return value.startsWith('/') && !value.startsWith('//') ? value : '/';
}

function readPending(): PendingLogin | null {
  try {
    const raw = sessionStorage.getItem(PENDING_KEY);

    return raw === null ? null : (JSON.parse(raw) as PendingLogin);
  } catch {
    return null;
  }
}

function clearPending(): void {
  try {
    sessionStorage.removeItem(PENDING_KEY);
  } catch {
    return;
  }
}

function decodeSegment(segment: string): string {
  const normalized = segment.replaceAll('-', '+').replaceAll('_', '/');
  const binary = atob(normalized);
  const bytes = Uint8Array.from(binary, (character) => character.codePointAt(0) ?? 0);

  return new TextDecoder().decode(bytes);
}

function decodeIdToken(idToken: string): AuthUser {
  const segment = idToken.split('.')[1];

  if (segment === undefined) {
    return {};
  }

  try {
    const claims = JSON.parse(decodeSegment(segment)) as Record<string, unknown>;

    return {
      name: typeof claims.name === 'string' ? claims.name : undefined,
      email: typeof claims.email === 'string' ? claims.email : undefined,
      picture: typeof claims.picture === 'string' ? claims.picture : undefined,
    };
  } catch {
    return {};
  }
}

export interface ValidatedConfig {
  authority: TrustedAuthority;
  clientId: string;
  scopes: string;
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

export async function startLogin(config: ValidatedConfig, returnTo: string): Promise<void> {
  const verifier = createVerifier();
  const state = createState();
  const challenge = await createChallenge(verifier);

  const pending: PendingLogin = {
    verifier,
    state,
    returnTo: safeReturnTo(returnTo),
  };

  sessionStorage.setItem(PENDING_KEY, JSON.stringify(pending));

  const url = new URL(endpoint(config.authority, 'auth'));
  url.searchParams.set('client_id', config.clientId);
  url.searchParams.set('response_type', 'code');
  url.searchParams.set('redirect_uri', redirectUri());
  url.searchParams.set('scope', config.scopes);
  url.searchParams.set('state', state);
  url.searchParams.set('code_challenge', challenge);
  url.searchParams.set('code_challenge_method', 'S256');

  window.location.assign(url.toString());
}

export interface CompletedLogin {
  session: KeycloakSession;
  returnTo: string;
}

export async function completeLogin(
  config: ValidatedConfig,
  params: URLSearchParams,
): Promise<CompletedLogin> {
  const pending = readPending();
  clearPending();

  if (params.get('error') !== null) {
    throw new Error('The identity provider refused the sign-in');
  }

  if (pending === null) {
    throw new Error('No sign-in was started from this tab');
  }

  const returnedState = params.get('state');

  if (returnedState === null || returnedState !== pending.state) {
    throw new Error('The sign-in response does not match the request that started it');
  }

  const code = params.get('code');

  if (code === null) {
    throw new Error('The identity provider returned no authorization code');
  }

  const body = new URLSearchParams({
    grant_type: 'authorization_code',
    code,
    redirect_uri: redirectUri(),
    client_id: config.clientId,
    code_verifier: pending.verifier,
  });

  const response = await fetch(endpoint(config.authority, 'token'), {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body,
  });

  if (!response.ok) {
    throw new Error(`The token endpoint answered with status ${response.status}`);
  }

  const payload = (await response.json()) as {
    access_token?: unknown;
    id_token?: unknown;
    expires_in?: unknown;
  };

  if (typeof payload.access_token !== 'string') {
    throw new TypeError('The token endpoint returned no access token');
  }

  const lifetime = typeof payload.expires_in === 'number' ? payload.expires_in : 0;

  return {
    session: {
      accessToken: payload.access_token,
      expiresAt: Date.now() + lifetime * 1000,
      user: typeof payload.id_token === 'string' ? decodeIdToken(payload.id_token) : {},
      authority: config.authority,
    },
    returnTo: safeReturnTo(pending.returnTo),
  };
}

export function buildLogoutUrl(authority: TrustedAuthority): string {
  const url = new URL(endpoint(authority, 'logout'));
  url.searchParams.set('post_logout_redirect_uri', window.location.origin);

  return url.toString();
}
