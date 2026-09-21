import { useAuth0 } from '@auth0/auth0-react';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import './UserProfile.css';

function toHost(value: string): string | null {
  try {
    return new URL(value.includes('://') ? value : `https://${value}`).host;
  } catch {
    return null;
  }
}

const LOGIN_HOST_SOURCES: Record<string, string | undefined> = {
  VITE_AUTH0_DOMAIN: import.meta.env.VITE_AUTH0_DOMAIN,
  VITE_KEYCLOAK_URL: import.meta.env.VITE_KEYCLOAK_URL,
};

const MISSING_LOGIN_HOST_VARS = Object.entries(LOGIN_HOST_SOURCES)
  .filter(([, value]) => typeof value !== 'string' || value.length === 0)
  .map(([name]) => name);

const ALLOWED_LOGIN_HOSTS = Object.values(LOGIN_HOST_SOURCES)
  .filter((value): value is string => typeof value === 'string' && value.length > 0)
  .map(toHost)
  .filter((host): host is string => host !== null);

if (MISSING_LOGIN_HOST_VARS.length > 0) {
  console.warn(
    `Login host allowlist is incomplete: ${MISSING_LOGIN_HOST_VARS.join(', ')} not set. ` +
      'Logging in through that provider will be refused. ' +
      'Add the variable to frontend/.env and restart the dev server.',
  );
}

function safeLoginUrl(raw: unknown): string | null {
  if (typeof raw !== 'string') {
    return null;
  }

  try {
    const parsed = new URL(raw);

    if (parsed.protocol !== 'https:' && parsed.protocol !== 'http:') {
      return null;
    }

    return ALLOWED_LOGIN_HOSTS.includes(parsed.host) ? parsed.toString() : null;
  } catch {
    return null;
  }
}

export function UserProfile() {
  const { logout, user, isAuthenticated, isLoading } = useAuth0();
  const navigate = useNavigate();

  const [isRedirecting, setIsRedirecting] = useState(false);

  const handleDynamicLogin = async () => {
    setIsRedirecting(true);
    try {
      const response = await fetch(`${import.meta.env.VITE_API_URL}/auth/login-url`);

      if (!response.ok) {
        throw new Error('Error retrieving login link');
      }

      const data = await response.json();
      const target = safeLoginUrl(data.url);

      if (!target) {
        throw new Error(
          'The login url returned by the server is not trusted. ' +
            `Allowed hosts: ${ALLOWED_LOGIN_HOSTS.join(', ') || '(none)'}`,
        );
      }

      window.location.href = target;
    } catch (error) {
      console.error(error);
      setIsRedirecting(false);
    }
  };

  if (isLoading) {
    return <div className="auth-container">Loading...</div>;
  }

  return (
    <div className="auth-container">
      {!isAuthenticated ? (
        <button
          className="auth-btn login" 
          onClick={handleDynamicLogin}
          disabled={isRedirecting}
        >
          {isRedirecting ? 'Connecting...' : 'Log In'}
        </button>
      ) : (
        <div className="profile-info">
          <img className="profile-avatar" src={user?.picture} alt={user?.name} />
          <div className="profile-details">
            <span className="profile-name">{user?.name}</span>
            <span className="profile-email">{user?.email}</span>
          </div>
          
          <button 
            className="auth-btn" 
            style={{ backgroundColor: '#198754', color: 'white' }}
            onClick={() => navigate('/profile')}
          >
            My Profile
          </button>

          <button 
            className="auth-btn logout" 
            onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}
          >
            Log Out
          </button>
        </div>
      )}
    </div>
  );
}
