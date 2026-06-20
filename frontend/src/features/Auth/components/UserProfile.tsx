import { useAuth0 } from '@auth0/auth0-react';
import { useNavigate } from 'react-router-dom';
import './UserProfile.css';

export function UserProfile() {
  const { loginWithRedirect, logout, user, isAuthenticated, isLoading } = useAuth0();
  const navigate = useNavigate();

  if (isLoading) {
    return <div className="auth-container">Loading...</div>;
  }

  return (
    <div className="auth-container">
      {!isAuthenticated ? (
        <button className="auth-btn login" onClick={() => loginWithRedirect()}>
          Log In
        </button>
      ) : (
        <div className="profile-info">
          <img className="profile-avatar" src={user?.picture} alt={user?.name} />
          <div className="profile-details">
            <span className="profile-name">{user?.name}</span>
            <span className="profile-email">{user?.email}</span>
          </div>
          
          {/* Новая кнопка для перехода в профиль */}
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