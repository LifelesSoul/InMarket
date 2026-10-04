import { apiRequest, describeApiError } from '../../shared/api/client';
import { describeAuthError } from '../../shared/auth/errors';
import type { AuthError } from '../../shared/auth/errors';
import { fail, ok, type Result } from '../../shared/auth/result';
import { isJsonObject } from '../../shared/json';
import type { MarketRole, MyProfile } from './types';

type TokenSource = () => Promise<Result<string, AuthError>>;

interface MyProfileResponse {
  id: string;
  username: string;
  email: string;
  registrationDate: string;
  avatarUrl?: string | null;
  biography?: string | null;
  ratingScore?: number;
  roles: string[];
}

const MARKET_ROLES = new Set<string>(['Buyer', 'Seller', 'Admin']);

function isMarketRole(role: string): role is MarketRole {
  return MARKET_ROLES.has(role);
}

function isFilledString(value: unknown): value is string {
  return typeof value === 'string' && value.length > 0;
}

function isOptionalString(value: unknown): value is string | null | undefined {
  return value === undefined || value === null || typeof value === 'string';
}

function isMyProfileResponse(value: unknown): value is MyProfileResponse {
  return isJsonObject(value)
    && isFilledString(value.id)
    && isFilledString(value.username)
    && isFilledString(value.email)
    && isFilledString(value.registrationDate)
    && isOptionalString(value.avatarUrl)
    && isOptionalString(value.biography)
    && (value.ratingScore === undefined || typeof value.ratingScore === 'number')
    && Array.isArray(value.roles)
    && value.roles.every((role) => typeof role === 'string');
}

function toMyProfile(response: MyProfileResponse): MyProfile {
  return {
    id: response.id,
    username: response.username,
    email: response.email,
    registrationDate: response.registrationDate,
    avatarUrl: response.avatarUrl || undefined,
    biography: response.biography || undefined,
    ratingScore: response.ratingScore ?? 0,
    roles: response.roles.filter(isMarketRole),
  };
}

async function requestProfile(
  path: string,
  method: 'GET' | 'POST',
  getToken: TokenSource,
): Promise<Result<MyProfile, string>> {
  const token = await getToken();

  if (!token.ok) {
    return fail(describeAuthError(token.error));
  }

  const response = await apiRequest(path, { method, token: token.value });

  if (!response.ok) {
    return fail(describeApiError(response.error));
  }

  if (!isMyProfileResponse(response.value)) {
    return fail('The server returned a profile in an unexpected format');
  }

  return ok(toMyProfile(response.value));
}

export function loadMyProfile(getToken: TokenSource): Promise<Result<MyProfile, string>> {
  return requestProfile('/profiles/me', 'GET', getToken);
}

export function requestSellerRole(getToken: TokenSource): Promise<Result<MyProfile, string>> {
  return requestProfile('/profiles/me/seller', 'POST', getToken);
}
