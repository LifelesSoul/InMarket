import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth0 } from '@auth0/auth0-react';
import { AuthContext } from './authContext';
import {
  CALLBACK_PATH,
  buildLogoutUrl,
  completeLogin,
  startLogin,
  validateConfig,
} from './keycloakStrategy';
import type { KeycloakSession } from './keycloakStrategy';
import type { AuthContextValue, AuthProviderName, AuthUser } from './types';

const KNOWN_PROVIDERS: AuthProviderName[] = ['Auth0', 'Keycloak'];

interface LoginPlan {
  provider: AuthProviderName;
  authority: string;
  clientId: string;
  scopes: string;
}

function readString(source: Record<string, unknown>, key: string): string {
  const value = source[key];

  return typeof value === 'string' ? value : '';
}

function toLoginPlan(data: unknown): LoginPlan {
  const source = (data ?? {}) as Record<string, unknown>;
  const provider = source.provider;

  if (typeof provider !== 'string' || !(KNOWN_PROVIDERS as string[]).includes(provider)) {
    throw new TypeError('The server selected an identity provider this build does not know');
  }

  return {
    provider: provider as AuthProviderName,
    authority: readString(source, 'authority'),
    clientId: readString(source, 'clientId'),
    scopes: readString(source, 'scopes') || 'openid profile email',
  };
}

function toMessage(err: unknown, fallback: string): string {
  return err instanceof Error ? err.message : fallback;
}

async function fetchLoginPlan(): Promise<LoginPlan> {
  const response = await fetch(`${import.meta.env.VITE_API_URL}/auth/login-url`);

  if (!response.ok) {
    throw new Error(`The login endpoint answered with status ${response.status}`);
  }

  return toLoginPlan(await response.json());
}

export function AuthProvider({ children }: Readonly<{ children: ReactNode }>) {
  const auth0 = useAuth0();
  const navigate = useNavigate();
  const location = useLocation();

  const [error, setError] = useState<string | null>(null);
  const [isStartingLogin, setIsStartingLogin] = useState(false);
  const [keycloak, setKeycloak] = useState<KeycloakSession | null>(null);

  const callbackHandled = useRef(false);

  useEffect(() => {
    if (location.pathname !== CALLBACK_PATH || callbackHandled.current) {
      return;
    }

    callbackHandled.current = true;

    const search = location.search;

    fetchLoginPlan()
      .then((plan) => {
        if (plan.provider !== 'Keycloak') {
          throw new Error('The sign-in was started with a provider that is no longer selected');
        }

        return completeLogin(validateConfig(plan), new URLSearchParams(search));
      })
      .then(({ session, returnTo }) => {
        setKeycloak(session);
        setError(null);
        navigate(returnTo, { replace: true });
      })
      .catch((err: unknown) => {
        setError(toMessage(err, 'The sign-in could not be completed'));
        navigate('/', { replace: true });
      });
  }, [location.pathname, location.search, navigate]);

  const login = useCallback(async () => {
    setError(null);
    setIsStartingLogin(true);

    try {
      const plan = await fetchLoginPlan();

      if (plan.provider === 'Auth0') {
        await auth0.loginWithRedirect();
        return;
      }

      await startLogin(validateConfig(plan), `${location.pathname}${location.search}`);
    } catch (err: unknown) {
      setError(toMessage(err, 'Could not start the sign-in'));
      setIsStartingLogin(false);
    }
  }, [auth0, location.pathname, location.search]);

  const logout = useCallback(() => {
    setError(null);

    if (keycloak !== null) {
      const authority = keycloak.authority;
      setKeycloak(null);
      window.location.assign(buildLogoutUrl(authority));
      return;
    }

    auth0.logout({ logoutParams: { returnTo: window.location.origin } });
  }, [auth0, keycloak]);

  const getAccessToken = useCallback(async (): Promise<string | null> => {
    if (keycloak !== null) {
      if (keycloak.expiresAt <= Date.now()) {
        setKeycloak(null);
        return null;
      }

      return keycloak.accessToken;
    }

    if (!auth0.isAuthenticated) {
      return null;
    }

    try {
      return await auth0.getAccessTokenSilently();
    } catch {
      return null;
    }
  }, [auth0, keycloak]);

  const user = useMemo<AuthUser | null>(() => {
    if (keycloak !== null) {
      return keycloak.user;
    }

    if (!auth0.isAuthenticated || !auth0.user) {
      return null;
    }

    return {
      name: auth0.user.name,
      email: auth0.user.email,
      picture: auth0.user.picture,
    };
  }, [keycloak, auth0.isAuthenticated, auth0.user]);

  const provider = useMemo<AuthProviderName | null>(() => {
    if (keycloak !== null) {
      return 'Keycloak';
    }

    return auth0.isAuthenticated ? 'Auth0' : null;
  }, [keycloak, auth0.isAuthenticated]);

  const value = useMemo<AuthContextValue>(() => ({
    isAuthenticated: keycloak !== null || auth0.isAuthenticated,
    isLoading: auth0.isLoading || isStartingLogin || location.pathname === CALLBACK_PATH,
    user,
    error: error ?? (auth0.error ? auth0.error.message : null),
    provider,
    login,
    logout,
    getAccessToken,
  }), [
    keycloak,
    auth0.isAuthenticated,
    auth0.isLoading,
    auth0.error,
    isStartingLogin,
    location.pathname,
    user,
    error,
    provider,
    login,
    logout,
    getAccessToken,
  ]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
