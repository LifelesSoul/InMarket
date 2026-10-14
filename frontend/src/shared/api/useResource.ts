import { useCallback, useEffect, useReducer, useState } from 'react';
import type { Result } from '../auth/result';

export type ResourceState<T> =
  | { status: 'loading' }
  | { status: 'failed'; error: string }
  | { status: 'loaded'; data: T };

type ResourceEvent<T> =
  | { type: 'started' }
  | { type: 'finished'; result: Result<T, string> };

function resourceReducer<T>(_state: ResourceState<T>, event: ResourceEvent<T>): ResourceState<T> {
  switch (event.type) {
    case 'started':
      return { status: 'loading' };
    case 'finished':
      return event.result.ok
        ? { status: 'loaded', data: event.result.value }
        : { status: 'failed', error: event.result.error };
  }
}

export function useResource<T>(load: () => Promise<Result<T, string>>) {
  const [state, dispatch] = useReducer(resourceReducer<T>, { status: 'loading' });
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    let cancelled = false;
    dispatch({ type: 'started' });

    void load().then((result) => {
      if (!cancelled) {
        dispatch({ type: 'finished', result });
      }
    });

    return () => {
      cancelled = true;
    };
  }, [load, attempt]);

  const reload = useCallback(() => {
    setAttempt((value) => value + 1);
  }, []);

  return { state, reload };
}
