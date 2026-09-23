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
} from './keycloak';
import type { KeycloakConfig, KeycloakSession } from './keycloak';
import type { AuthContextValue, AuthProviderName, AuthUser } from './types';

type LoginPlan =
  | { provider: 'Auth0' }
  | { provider: 'Keycloak'; config: KeycloakConfig };

const LOGIN_PLAN_URL = `${import.meta.env.VITE_API_URL}/auth/login-url`;

function str(v: unknown): string {
  return typeof v === 'string' ? v : '';
}

function toLoginPlan(data: unknown): LoginPlan {
  const source = (data ?? {}) as Record<string, unknown>;

  if (source.provider === 'Auth0') {
    return { provider: 'Auth0' };
  }

  if (source.provider === 'Keycloak') {
    return {
      provider: 'Keycloak',
      config: {
        authority: str(source.authority),
        clientId: str(source.clientId),
        scopes: str(source.scopes),
      },
    };
  }

  throw new TypeError('The server selected an identity provider this build does not know');
}

function toMessage(err: unknown, fallback: string): string {
  return err instanceof Error ? err.message : fallback;
}

async function fetchLoginPlan(): Promise<LoginPlan> {
  const response = await fetch(LOGIN_PLAN_URL);

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

        return completeLogin(validateConfig(plan.config), new URLSearchParams(search));
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

      await startLogin(validateConfig(plan.config), `${location.pathname}${location.search}`);
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

  const isAuthenticated = keycloak !== null || auth0.isAuthenticated;
  const isLoading = auth0.isLoading || isStartingLogin || location.pathname === CALLBACK_PATH;
  const resolvedError = error ?? auth0.error?.message ?? null;

  const actions = useMemo(
    () => ({ login, logout, getAccessToken }),
    [login, logout, getAccessToken],
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      isAuthenticated,
      isLoading,
      user,
      error: resolvedError,
      provider,
      ...actions,
    }),
    [isAuthenticated, isLoading, user, resolvedError, provider, actions],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
