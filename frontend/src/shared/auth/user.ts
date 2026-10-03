import { isJsonObject } from '../json';
import type { AuthError } from './errors';
import { fail, ok, type Result } from './result';
import type { AuthUser } from './types';

export interface UserClaims {
  name?: string;
  preferred_username?: string;
  email?: string;
  picture?: string;
}

const CLAIM_KEYS = ['name', 'preferred_username', 'email', 'picture'] as const;

export function isUserClaims(value: unknown): value is UserClaims {
  return isJsonObject(value)
    && CLAIM_KEYS.every((key) => value[key] === undefined || typeof value[key] === 'string');
}

function isFilled(value: string | undefined): value is string {
  return value !== undefined && value.length > 0;
}

export function toAuthUser(claims: UserClaims): Result<AuthUser, AuthError> {
  const name = isFilled(claims.name) ? claims.name : claims.preferred_username;

  if (!isFilled(name)) {
    return fail({ kind: 'incomplete-profile', claim: 'name' });
  }

  if (!isFilled(claims.email)) {
    return fail({ kind: 'incomplete-profile', claim: 'email' });
  }

  const user: AuthUser = { name, email: claims.email };

  return ok(isFilled(claims.picture) ? { ...user, picture: claims.picture } : user);
}
