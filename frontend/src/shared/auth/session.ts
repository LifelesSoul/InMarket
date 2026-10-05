import type { AuthActions, AuthProviderName, AuthUser } from './types';

export type ProviderSession = Pick<AuthActions, 'getAccessToken' | 'logout'> & {
  provider: AuthProviderName;
  user: AuthUser;
};
