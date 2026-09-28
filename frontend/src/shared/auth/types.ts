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
  getAccessToken: () => Promise<string | null>;
}

export type AuthContextValue = Readonly<AuthSnapshot & AuthActions>;
