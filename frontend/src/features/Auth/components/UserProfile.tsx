import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../shared/auth/useAuth';
import './UserProfile.css';

export function UserProfile() {
  const { isAuthenticated, isLoading, user, error, login, logout } = useAuth();
  const navigate = useNavigate();

  if (isLoading) {
    return <div className="auth-container">Loading...</div>;
  }

  return (
    <div className="auth-container">
      {!isAuthenticated ? (
        <>
          <button className="auth-btn login" onClick={login}>
            Log In
          </button>
          {error && <span className="auth-error">{error}</span>}
        </>
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

          <button className="auth-btn logout" onClick={logout}>
            Log Out
          </button>
        </div>
      )}
    </div>
  );
}
