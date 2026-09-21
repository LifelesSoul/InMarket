import { useAuth0 } from '@auth0/auth0-react';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import './UserProfile.css';

export function UserProfile() {
  const { logout, user, isAuthenticated, isLoading } = useAuth0();
  const navigate = useNavigate();

  const [isRedirecting, setIsRedirecting] = useState(false);

  const handleDynamicLogin = async () => {
    setIsRedirecting(true);
    try {
      const response = await fetch('https://localhost:7032/api/auth/login-url');
      
      if (!response.ok) {
        throw new Error('Error retrieving login link');
      }

      const data = await response.json();
      console.log(`Redirecting to: ${data.provider}`);

      window.location.href = data.url;
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
