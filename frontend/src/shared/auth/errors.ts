export type AuthError =
  | { kind: 'plan-unavailable'; status?: number }
  | { kind: 'unknown-provider' }
  | { kind: 'invalid-config'; field: 'authority' | 'clientId' | 'scopes' }
  | { kind: 'untrusted-authority'; expectedOrigin: string | null }
  | { kind: 'provider-changed' }
  | { kind: 'provider-refused'; code: string }
  | { kind: 'no-pending-login' }
  | { kind: 'storage-unavailable'; message: string }
  | { kind: 'state-mismatch' }
  | { kind: 'missing-code' }
  | { kind: 'token-exchange'; status?: number }
  | { kind: 'silent-unavailable'; reason: string }
  | { kind: 'provider-sdk'; message: string }
  | { kind: 'unexpected'; message: string };

export function errorMessage(err: unknown): string {
  return err instanceof Error ? err.message : String(err);
}

export function unexpected(err: unknown): AuthError {
  return { kind: 'unexpected', message: errorMessage(err) };
}

const INVALID_CONFIG_MESSAGES: Record<'authority' | 'clientId' | 'scopes', string> = {
  authority: 'The Keycloak authority is not a valid absolute url',
  clientId: 'The Keycloak client id has an unexpected shape',
  scopes: 'The Keycloak scopes have an unexpected shape',
};

export function describeAuthError(error: AuthError): string {
  switch (error.kind) {
    case 'plan-unavailable':
      return error.status === undefined
        ? 'The login endpoint could not be reached'
        : `The login endpoint answered with status ${error.status}`;
    case 'unknown-provider':
      return 'The server selected an identity provider this build does not know';
    case 'invalid-config':
      return INVALID_CONFIG_MESSAGES[error.field];
    case 'untrusted-authority':
      return error.expectedOrigin === null
        ? 'VITE_KEYCLOAK_URL is not set, so this build trusts no Keycloak origin. ' +
            'Add it to frontend/.env and restart the dev server.'
        : `The Keycloak authority is not on the origin this build trusts (${error.expectedOrigin})`;
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
    case 'token-exchange':
      return error.status === undefined
        ? 'The token endpoint returned no usable token'
        : `The token endpoint answered with status ${error.status}`;
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
