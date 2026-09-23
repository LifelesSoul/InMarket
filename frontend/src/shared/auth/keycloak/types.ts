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
  expiresAt: number;
  user: AuthUser;
  authority: TrustedAuthority;
}

export interface CompletedLogin {
  session: KeycloakSession;
  returnTo: string;
}
