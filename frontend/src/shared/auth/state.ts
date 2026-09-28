import type { AuthError } from './errors';
import { CALLBACK_PATH } from './keycloak';
import type { ProviderSession } from './session';

export type AuthState =
  | { status: 'restoring' }
  | { status: 'anonymous'; error: AuthError | null }
  | { status: 'selecting' }
  | { status: 'completing' }
  | { status: 'authenticated'; session: ProviderSession };

export type AuthEvent =
  | { type: 'login-requested' }
  | { type: 'signed-in'; session: ProviderSession }
  | { type: 'failed'; error: AuthError }
  | { type: 'signed-out' };

export function initialAuthState(pathname: string): AuthState {
  return pathname === CALLBACK_PATH ? { status: 'completing' } : { status: 'restoring' };
}

export function authReducer(state: AuthState, event: AuthEvent): AuthState {
  switch (event.type) {
    case 'login-requested':
      return state.status === 'anonymous' ? { status: 'selecting' } : state;
    case 'signed-in':
      return { status: 'authenticated', session: event.session };
    case 'failed':
      return { status: 'anonymous', error: event.error };
    case 'signed-out':
      return { status: 'anonymous', error: null };
    default:
      return state;
  }
}

export function isBusy(state: AuthState): boolean {
  return state.status === 'restoring' || state.status === 'selecting' || state.status === 'completing';
}
