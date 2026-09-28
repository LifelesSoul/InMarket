import type { AuthProviderName, AuthUser } from './types';

export interface ProviderSession {
  provider: AuthProviderName;
  user: AuthUser;
  getAccessToken(): Promise<string | null>;
  logout(): void;
}
