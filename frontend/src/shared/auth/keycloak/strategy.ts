import { createChallenge, createState, createVerifier } from '../pkce';
import { decodeIdToken } from './idToken';
import { savePending, takePending } from './pendingLogin';
import type { CompletedLogin, TrustedAuthority, ValidatedConfig } from './types';

export const CALLBACK_PATH = '/auth/callback';

function redirectUri(): string {
  return `${window.location.origin}${CALLBACK_PATH}`;
}

function endpoint(authority: TrustedAuthority, name: string): string {
  return new URL(`${authority.realmPath}/protocol/openid-connect/${name}`, authority.origin).toString();
}

function safeReturnTo(value: string): string {
  return value.startsWith('/') && !value.startsWith('//') ? value : '/';
}

export async function startLogin(config: ValidatedConfig, returnTo: string): Promise<void> {
  const verifier = createVerifier();
  const state = createState();
  const challenge = await createChallenge(verifier);

  savePending({
    verifier,
    state,
    returnTo: safeReturnTo(returnTo),
  });

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

export async function completeLogin(
  config: ValidatedConfig,
  params: URLSearchParams,
): Promise<CompletedLogin> {
  const pending = takePending();

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
