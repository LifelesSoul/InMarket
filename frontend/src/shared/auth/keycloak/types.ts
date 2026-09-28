import type { AuthUser } from '../types';

export interface KeycloakConfig {
  authority: string;
  clientId: string;
  scopes: string;
}

export interface TrustedAuthority {
  origin: string;
  realmPath: string;
}

export interface ValidatedConfig {
  authority: TrustedAuthority;
  clientId: string;
  scopes: string;
}

export interface KeycloakSession {
  accessToken: string;
  idToken: string | null;
  expiresAt: number;
  user: AuthUser;
  authority: TrustedAuthority;
  clientId: string;
}

export interface CompletedLogin {
  session: KeycloakSession;
  returnTo: string;
}
