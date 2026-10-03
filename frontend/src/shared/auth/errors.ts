export type AuthError =
  | { kind: 'plan-unreachable'; message: string }
  | { kind: 'plan-timeout' }
  | { kind: 'plan-unavailable'; status: number }
  | { kind: 'plan-invalid' }
  | { kind: 'unknown-provider' }
  | { kind: 'invalid-config'; field: 'authority' | 'clientId' | 'scopes' }
  | { kind: 'trusted-origin-missing' }
  | { kind: 'untrusted-authority'; expectedOrigin: string }
  | { kind: 'provider-changed' }
  | { kind: 'provider-refused'; code: string }
  | { kind: 'no-pending-login' }
  | { kind: 'storage-unavailable'; message: string }
  | { kind: 'state-mismatch' }
  | { kind: 'missing-code' }
  | { kind: 'token-unreachable'; message: string }
  | { kind: 'token-exchange'; status: number }
  | { kind: 'token-invalid' }
  | { kind: 'incomplete-profile'; claim: 'name' | 'email' }
  | { kind: 'not-signed-in' }
  | { kind: 'session-expired' }
  | { kind: 'silent-unavailable'; reason: string }
  | { kind: 'provider-sdk'; message: string }
  | { kind: 'unexpected'; message: string };

export function errorMessage(err: unknown): string {
  return err instanceof Error ? err.message : String(err);
}

export function unexpected(err: unknown): AuthError {
  return { kind: 'unexpected', message: errorMessage(err) };
}

const INTERACTION_REQUIRED = new Set([
  'login_required',
  'interaction_required',
  'consent_required',
  'account_selection_required',
]);

export function isInteractionRequired(code: string): boolean {
  return INTERACTION_REQUIRED.has(code);
}

const INVALID_CONFIG_MESSAGES: Record<'authority' | 'clientId' | 'scopes', string> = {
  authority: 'The Keycloak authority is not a valid absolute url',
  clientId: 'The Keycloak client id has an unexpected shape',
  scopes: 'The Keycloak scopes have an unexpected shape',
};

export function describeAuthError(error: AuthError): string {
  switch (error.kind) {
    case 'plan-unreachable':
      return `The login endpoint could not be reached (${error.message})`;
    case 'plan-timeout':
      return 'The login endpoint did not answer in time';
    case 'plan-unavailable':
      return `The login endpoint answered with status ${error.status}`;
    case 'plan-invalid':
      return 'The login endpoint returned a response in an unexpected format';
    case 'unknown-provider':
      return 'The server selected an identity provider this build does not know';
    case 'invalid-config':
      return INVALID_CONFIG_MESSAGES[error.field];
    case 'trusted-origin-missing':
      return 'VITE_KEYCLOAK_URL is not set or is not a valid url, so this build trusts no Keycloak origin. ' +
        'Add it to frontend/.env and restart the dev server.';
    case 'untrusted-authority':
      return `The Keycloak authority is not on the origin this build trusts (${error.expectedOrigin})`;
    case 'provider-changed':
      return 'The sign-in was started with a provider that is no longer selected';
    case 'provider-refused':
      return 'The identity provider refused the sign-in';
    case 'no-pending-login':
      return 'No sign-in was started from this tab';
    case 'storage-unavailable':
      return `The browser blocked session storage, so the sign-in cannot continue (${error.message})`;
    case 'state-mismatch':
      return 'The sign-in response does not match the request that started it';
    case 'missing-code':
      return 'The identity provider returned no authorization code';
    case 'token-unreachable':
      return `The token endpoint could not be reached (${error.message})`;
    case 'token-exchange':
      return `The token endpoint answered with status ${error.status}`;
    case 'token-invalid':
      return 'The token endpoint returned no usable token';
    case 'incomplete-profile':
      return `The identity provider did not return the user's ${error.claim}`;
    case 'not-signed-in':
      return 'You need to log in to do this';
    case 'session-expired':
      return 'Your session has expired. Please log in again.';
    case 'silent-unavailable':
      return 'There is no identity provider session to restore';
    case 'provider-sdk':
    case 'unexpected':
      return error.message;
    default:
      return assertNever(error);
  }
}

function assertNever(value: never): never {
  throw new Error(`Unhandled auth error: ${JSON.stringify(value)}`);
}
