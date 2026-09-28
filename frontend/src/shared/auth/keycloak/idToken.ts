import type { AuthError } from '../errors';
import { fail, type Result } from '../result';
import type { AuthUser } from '../types';
import { toAuthUser } from '../user';

function decodeSegment(segment: string): string {
  const normalized = segment.replaceAll('-', '+').replaceAll('_', '/');
  const binary = atob(normalized);
  const bytes = Uint8Array.from(binary, (character) => character.codePointAt(0) ?? 0);

  return new TextDecoder().decode(bytes);
}

export function decodeIdToken(idToken: string): Result<AuthUser, AuthError> {
  const segment = idToken.split('.')[1];

  if (segment === undefined) {
    return fail({ kind: 'token-invalid' });
  }

  let claims: unknown;

  try {
    claims = JSON.parse(decodeSegment(segment));
  } catch {
    return fail({ kind: 'token-invalid' });
  }

  return toAuthUser(claims);
}
