import { createChallenge, createState, createVerifier } from './pkce';
import type { AuthUser } from './types';

export interface KeycloakConfig {
  authority: string;
  clientId: string;
  scopes: string;
}

export interface KeycloakSession {
  accessToken: string;
  expiresAt: number;
  user: AuthUser;
  authority: string;
}

interface PendingLogin {
  verifier: string;
  state: string;
  returnTo: string;
  clientId: string;
  authority: string;
}

const PENDING_KEY = 'inmarket.auth.keycloak.pending';

export const CALLBACK_PATH = '/auth/callback';

function redirectUri(): string {
  return `${window.location.origin}${CALLBACK_PATH}`;
}

function authorizeEndpoint(authority: string): string {
  return `${authority}/protocol/openid-connect/auth`;
}

function tokenEndpoint(authority: string): string {
  return `${authority}/protocol/openid-connect/token`;
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

function decodeIdToken(idToken: string): AuthUser {
  const payload = idToken.split('.')[1];

  if (payload === undefined) {
    return {};
  }

  const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
  const claims = JSON.parse(decodeURIComponent(escape(atob(normalized)))) as Record<string, unknown>;

  return {
    name: typeof claims.name === 'string' ? claims.name : undefined,
    email: typeof claims.email === 'string' ? claims.email : undefined,
    picture: typeof claims.picture === 'string' ? claims.picture : undefined,
  };
}

export function isConfigured(config: KeycloakConfig): boolean {
  return config.authority.length > 0 && config.clientId.length > 0;
}

export async function startLogin(config: KeycloakConfig, returnTo: string): Promise<void> {
  const verifier = createVerifier();
  const state = createState();
  const challenge = await createChallenge(verifier);

  const pending: PendingLogin = {
    verifier,
    state,
    returnTo,
    clientId: config.clientId,
    authority: config.authority,
  };

  sessionStorage.setItem(PENDING_KEY, JSON.stringify(pending));

  const url = new URL(authorizeEndpoint(config.authority));
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

export async function completeLogin(params: URLSearchParams): Promise<CompletedLogin> {
  const pending = readPending();
  clearPending();

  const providerError = params.get('error');

  if (providerError !== null) {
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
    client_id: pending.clientId,
    code_verifier: pending.verifier,
  });

  const response = await fetch(tokenEndpoint(pending.authority), {
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
    throw new Error('The token endpoint returned no access token');
  }

  const lifetime = typeof payload.expires_in === 'number' ? payload.expires_in : 0;

  return {
    session: {
      accessToken: payload.access_token,
      expiresAt: Date.now() + lifetime * 1000,
      user: typeof payload.id_token === 'string' ? decodeIdToken(payload.id_token) : {},
      authority: pending.authority,
    },
    returnTo: pending.returnTo,
  };
}

export function buildLogoutUrl(authority: string): string {
  const url = new URL(`${authority}/protocol/openid-connect/logout`);
  url.searchParams.set('post_logout_redirect_uri', window.location.origin);

  return url.toString();
}
