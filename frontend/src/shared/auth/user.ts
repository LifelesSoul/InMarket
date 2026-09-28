import type { AuthError } from './errors';
import { fail, ok, type Result } from './result';
import type { AuthUser } from './types';

function readClaim(claims: Record<string, unknown>, key: string): string | null {
  const value = claims[key];
  return typeof value === 'string' && value.length > 0 ? value : null;
}

export function toAuthUser(data: unknown): Result<AuthUser, AuthError> {
  const claims = (data ?? {}) as Record<string, unknown>;
  const name = readClaim(claims, 'name') ?? readClaim(claims, 'preferred_username');
  const email = readClaim(claims, 'email');
  const picture = readClaim(claims, 'picture');

  if (name === null) {
    return fail({ kind: 'incomplete-profile', claim: 'name' });
  }

  if (email === null) {
    return fail({ kind: 'incomplete-profile', claim: 'email' });
  }

  return ok(picture === null ? { name, email } : { name, email, picture });
}
