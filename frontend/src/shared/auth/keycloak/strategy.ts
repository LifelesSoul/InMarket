import { errorMessage, unexpected } from '../errors';
import type { AuthError } from '../errors';
import { createChallenge, createState, createVerifier } from '../pkce';
import { fail, ok } from '../result';
import type { Result } from '../result';
import { decodeIdToken } from './idToken';
import { savePending, takePending } from './pendingLogin';
import type { CompletedLogin, KeycloakSession, TrustedAuthority, ValidatedConfig } from './types';

export const CALLBACK_PATH = '/auth/callback';
export const CALLBACK_URI = `${window.location.origin}${CALLBACK_PATH}`;

export interface AuthorizeRequest {
  config: ValidatedConfig;
  redirectUri: string;
  state: string;
  challenge: string;
  prompt?: 'none';
}

interface TokenPayload {
  access_token?: unknown;
  id_token?: unknown;
  expires_in?: unknown;
}

function endpoint(authority: TrustedAuthority, name: string): string {
  return new URL(`${authority.realmPath}/protocol/openid-connect/${name}`, authority.origin).toString();
}

function safeReturnTo(value: string): string {
  return value.startsWith('/') && !value.startsWith('//') ? value : '/';
}

export function buildAuthorizeUrl(request: AuthorizeRequest): string {
  const url = new URL(endpoint(request.config.authority, 'auth'));
  url.searchParams.set('client_id', request.config.clientId);
  url.searchParams.set('response_type', 'code');
  url.searchParams.set('redirect_uri', request.redirectUri);
  url.searchParams.set('scope', request.config.scopes);
  url.searchParams.set('state', request.state);
  url.searchParams.set('code_challenge', request.challenge);
  url.searchParams.set('code_challenge_method', 'S256');

  if (request.prompt !== undefined) {
    url.searchParams.set('prompt', request.prompt);
  }

  return url.toString();
}

export async function exchangeCode(
  config: ValidatedConfig,
  code: string,
  verifier: string,
  redirectUri: string,
): Promise<Result<KeycloakSession, AuthError>> {
  const body = new URLSearchParams({
    grant_type: 'authorization_code',
    code,
    redirect_uri: redirectUri,
    client_id: config.clientId,
    code_verifier: verifier,
  });

  let response: Response;

  try {
    response = await fetch(endpoint(config.authority, 'token'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body,
    });
  } catch (err: unknown) {
    return fail({ kind: 'token-unreachable', message: errorMessage(err) });
  }

  if (!response.ok) {
    return fail({ kind: 'token-exchange', status: response.status });
  }

  let payload: TokenPayload;

  try {
    payload = (await response.json()) as TokenPayload;
  } catch {
    return fail({ kind: 'token-invalid' });
  }

  if (typeof payload.access_token !== 'string' || typeof payload.id_token !== 'string') {
    return fail({ kind: 'token-invalid' });
  }

  const user = decodeIdToken(payload.id_token);

  if (!user.ok) {
    return user;
  }

  const lifetime = typeof payload.expires_in === 'number' ? payload.expires_in : 0;

  return ok({
    accessToken: payload.access_token,
    idToken: payload.id_token,
    expiresAt: Date.now() + lifetime * 1000,
    user: user.value,
    authority: config.authority,
  });
}

export async function startLogin(
  config: ValidatedConfig,
  returnTo: string,
): Promise<Result<void, AuthError>> {
  let url: string;

  try {
    const verifier = createVerifier();
    const state = createState();
    const challenge = await createChallenge(verifier);

    const saved = savePending({ verifier, state, returnTo: safeReturnTo(returnTo) });

    if (!saved.ok) {
      return saved;
    }

    url = buildAuthorizeUrl({ config, redirectUri: CALLBACK_URI, state, challenge });
  } catch (err: unknown) {
    return fail(unexpected(err));
  }

  window.location.assign(url);

  return ok(undefined);
}

export async function completeLogin(
  config: ValidatedConfig,
  params: URLSearchParams,
): Promise<Result<CompletedLogin, AuthError>> {
  const pending = takePending();
  const error = params.get('error');

  if (error !== null) {
    return fail({ kind: 'provider-refused', code: error });
  }

  if (!pending.ok) {
    return pending;
  }

  if (params.get('state') !== pending.value.state) {
    return fail({ kind: 'state-mismatch' });
  }

  const code = params.get('code');

  if (code === null) {
    return fail({ kind: 'missing-code' });
  }

  const session = await exchangeCode(config, code, pending.value.verifier, CALLBACK_URI);

  if (!session.ok) {
    return session;
  }

  return ok({ session: session.value, returnTo: safeReturnTo(pending.value.returnTo) });
}

export function buildLogoutUrl(session: KeycloakSession): string {
  const url = new URL(endpoint(session.authority, 'logout'));
  url.searchParams.set('post_logout_redirect_uri', window.location.origin);
  url.searchParams.set('id_token_hint', session.idToken);

  return url.toString();
}
