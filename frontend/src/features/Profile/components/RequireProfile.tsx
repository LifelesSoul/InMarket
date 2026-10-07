import type { ReactNode } from 'react';
import { useAuth } from '../../../shared/auth/useAuth';
import type { MyProfile } from '../types';
import { useMyProfile } from '../useMyProfile';
import './RequireProfile.css';

interface RequireProfileProps {
  children: (profile: MyProfile) => ReactNode;
}

export function RequireProfile({ children }: Readonly<RequireProfileProps>) {
  const { isAuthenticated, isLoading, login } = useAuth();
  const { state, reload } = useMyProfile();

  if (isLoading) {
    return <p className="require-profile">Loading...</p>;
  }

  if (!isAuthenticated) {
    return (
      <div className="require-profile">
        <p>Please log in to continue.</p>
        <button type="button" className="require-profile-button" onClick={login}>
          Log In
        </button>
      </div>
    );
  }

  if (state.status === 'anonymous' || state.status === 'loading') {
    return <p className="require-profile">Loading profile...</p>;
  }

  if (state.status === 'failed') {
    return (
      <div className="require-profile">
        <p className="require-profile-error">{state.error}</p>
        <button type="button" className="require-profile-button" onClick={reload}>
          Retry
        </button>
      </div>
    );
  }

  return <>{children(state.profile)}</>;
}
