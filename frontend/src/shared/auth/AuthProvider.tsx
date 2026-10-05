import { useCallback, useEffect, useMemo, useReducer, useRef } from 'react';
import type { ReactNode } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth0 } from '@auth0/auth0-react';
import { createAuth0Session } from './auth0Session';
import { AuthContext } from './authContext';
import { describeAuthError } from './errors';
import type { AuthError } from './errors';
import { beginLogin, finishKeycloakLogin, restoreKeycloak } from './flow';
import { CALLBACK_PATH, createKeycloakSession } from './keycloak';
import { fail, type Result } from './result';
import { authReducer, initialAuthState, isBusy } from './state';
import type { AuthActions, AuthContextValue } from './types';

export function AuthProvider({ children }: Readonly<{ children: ReactNode }>) {
  const auth0 = useAuth0();
  const navigate = useNavigate();
  const location = useLocation();
  const currentPath = `${location.pathname}${location.search}`;

  const [state, dispatch] = useReducer(authReducer, location.pathname, initialAuthState);
  const initialized = useRef(false);

  useEffect(() => {
    if (initialized.current || auth0.isLoading) {
      return;
    }

    initialized.current = true;

    const signOut = () => dispatch({ type: 'signed-out' });
    const expire = () => dispatch({ type: 'failed', error: { kind: 'session-expired' } });

    if (location.pathname === CALLBACK_PATH) {
      void finishKeycloakLogin(location.search).then((result) => {
        if (result.ok) {
          dispatch({ type: 'signed-in', session: createKeycloakSession(result.value.session, expire) });
          navigate(result.value.returnTo, { replace: true });
        } else {
          dispatch({ type: 'failed', error: result.error });
          navigate('/', { replace: true });
        }
      });
      return;
    }

    if (auth0.isAuthenticated) {
      const session = createAuth0Session(auth0, expire);

      if (session.ok) {
        dispatch({ type: 'signed-in', session: session.value });
      } else {
        dispatch({ type: 'failed', error: session.error });
      }
      return;
    }

    if (auth0.error) {
      dispatch({ type: 'failed', error: { kind: 'provider-sdk', message: auth0.error.message } });
      return;
    }

    void restoreKeycloak().then((result) => {
      if (result.ok && result.value !== null) {
        dispatch({ type: 'signed-in', session: createKeycloakSession(result.value, expire) });
      } else {
        signOut();
      }
    });
  }, [auth0, location.pathname, location.search, navigate]);

  const session = state.status === 'authenticated' ? state.session : null;
  const canStartLogin = state.status === 'anonymous';

  const login = useCallback(async () => {
    if (!canStartLogin) {
      return;
    }

    dispatch({ type: 'login-requested' });

    const result = await beginLogin(currentPath, () => auth0.loginWithRedirect());

    if (!result.ok) {
      dispatch({ type: 'failed', error: result.error });
    }
  }, [auth0, canStartLogin, currentPath]);

  const logout = useCallback(() => {
    if (session === null) {
      return;
    }

    dispatch({ type: 'signed-out' });
    session.logout();
  }, [session]);

  const getAccessToken = useCallback(
    (): Promise<Result<string, AuthError>> =>
      session === null ? Promise.resolve(fail({ kind: 'not-signed-in' })) : session.getAccessToken(),
    [session],
  );

  const actions = useMemo<AuthActions>(
    () => ({ login, logout, getAccessToken }),
    [login, logout, getAccessToken],
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      isAuthenticated: session !== null,
      isLoading: isBusy(state),
      user: session?.user ?? null,
      error: state.status === 'anonymous' && state.error !== null ? describeAuthError(state.error) : null,
      provider: session?.provider ?? null,
      ...actions,
    }),
    [state, session, actions],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
