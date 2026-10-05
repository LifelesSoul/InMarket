import { errorMessage } from '../errors';
import type { AuthError } from '../errors';
import { fail, ok } from '../result';
import type { Result } from '../result';

export interface PendingLogin {
  verifier: string;
  state: string;
  returnTo: string;
}

const PENDING_KEY = 'inmarket.auth.keycloak.pending';

export function savePending(pending: PendingLogin): Result<void, AuthError> {
  const value = new URLSearchParams({
    verifier: pending.verifier,
    state: pending.state,
    returnTo: pending.returnTo,
  }).toString();

  try {
    sessionStorage.setItem(PENDING_KEY, value);
    return ok(undefined);
  } catch (err: unknown) {
    return fail({ kind: 'storage-unavailable', message: errorMessage(err) });
  }
}

function readPending(): Result<PendingLogin, AuthError> {
  let raw: string | null;

  try {
    raw = sessionStorage.getItem(PENDING_KEY);
  } catch (err: unknown) {
    return fail({ kind: 'storage-unavailable', message: errorMessage(err) });
  }

  if (raw === null) {
    return fail({ kind: 'no-pending-login' });
  }

  const params = new URLSearchParams(raw);
  const verifier = params.get('verifier');
  const state = params.get('state');
  const returnTo = params.get('returnTo');

  if (verifier === null || state === null || returnTo === null) {
    return fail({ kind: 'no-pending-login' });
  }

  return ok({ verifier, state, returnTo });
}

function clearPending(): Result<void, AuthError> {
  try {
    sessionStorage.removeItem(PENDING_KEY);
    return ok(undefined);
  } catch (err: unknown) {
    return fail({ kind: 'storage-unavailable', message: errorMessage(err) });
  }
}

export function takePending(): Result<PendingLogin, AuthError> {
  const pending = readPending();
  const cleared = clearPending();

  return cleared.ok ? pending : cleared;
}
