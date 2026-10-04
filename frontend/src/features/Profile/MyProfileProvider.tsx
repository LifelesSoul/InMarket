import { useCallback, useEffect, useMemo, useReducer, useState } from 'react';
import type { ReactNode } from 'react';
import { ok, type Result } from '../../shared/auth/result';
import { useAuth } from '../../shared/auth/useAuth';
import { loadMyProfile, requestSellerRole } from './api';
import { MyProfileContext } from './myProfileContext';
import type { MarketRole, MyProfile, MyProfileContextValue, MyProfileState } from './types';

type MyProfileEvent =
  | { type: 'signed-out' }
  | { type: 'load-started' }
  | { type: 'loaded'; profile: MyProfile }
  | { type: 'failed'; error: string };

function myProfileReducer(state: MyProfileState, event: MyProfileEvent): MyProfileState {
  switch (event.type) {
    case 'signed-out':
      return { status: 'anonymous' };
    case 'load-started':
      return { status: 'loading' };
    case 'loaded':
      return { status: 'loaded', profile: event.profile };
    case 'failed':
      return { status: 'failed', error: event.error };
    default:
      return state;
  }
}

export function MyProfileProvider({ children }: Readonly<{ children: ReactNode }>) {
  const { isAuthenticated, getAccessToken } = useAuth();
  const [state, dispatch] = useReducer(myProfileReducer, { status: 'anonymous' });
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    if (!isAuthenticated) {
      dispatch({ type: 'signed-out' });
      return;
    }

    let cancelled = false;
    dispatch({ type: 'load-started' });

    void loadMyProfile(getAccessToken).then((result) => {
      if (cancelled) {
        return;
      }

      dispatch(result.ok ? { type: 'loaded', profile: result.value } : { type: 'failed', error: result.error });
    });

    return () => {
      cancelled = true;
    };
  }, [isAuthenticated, getAccessToken, attempt]);

  const reload = useCallback(() => {
    setAttempt((value) => value + 1);
  }, []);

  const becomeSeller = useCallback(async (): Promise<Result<void, string>> => {
    const result = await requestSellerRole(getAccessToken);

    if (!result.ok) {
      return result;
    }

    dispatch({ type: 'loaded', profile: result.value });
    return ok(undefined);
  }, [getAccessToken]);

  const hasRole = useCallback(
    (role: MarketRole) => state.status === 'loaded' && state.profile.roles.includes(role),
    [state],
  );

  const value = useMemo<MyProfileContextValue>(
    () => ({ state, hasRole, reload, becomeSeller }),
    [state, hasRole, reload, becomeSeller],
  );

  return <MyProfileContext.Provider value={value}>{children}</MyProfileContext.Provider>;
}
