import type { AuthError } from './errors';
import type { Result } from './result';

export type AuthProviderName = 'Auth0' | 'Keycloak';

export interface AuthUser {
  name: string;
  email: string;
  picture?: string;
}

export interface AuthSnapshot {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: AuthUser | null;
  error: string | null;
  provider: AuthProviderName | null;
}

export interface AuthActions {
  login: () => Promise<void>;
  logout: () => void;
  getAccessToken: () => Promise<Result<string, AuthError>>;
}

export type AuthContextValue = Readonly<AuthSnapshot & AuthActions>;
