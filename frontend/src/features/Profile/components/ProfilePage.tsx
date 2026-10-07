import { Link } from 'react-router-dom';
import { useAuth } from '../../../shared/auth/useAuth';
import { useMyProfile } from '../useMyProfile';
import { BecomeSellerCard } from './BecomeSellerCard';
import { RoleBadges } from './RoleBadges';
import './ProfilePage.css';

export function ProfilePage() {
  const { isAuthenticated, isLoading: isAuthLoading } = useAuth();
  const { state, hasRole, reload } = useMyProfile();

  if (isAuthLoading) {
    return <div className="profile-page">Loading profile...</div>;
  }

  if (!isAuthenticated) {
    return <div className="profile-page">Please log in to view your profile.</div>;
  }

  if (state.status === 'anonymous' || state.status === 'loading') {
    return <div className="profile-page">Loading profile...</div>;
  }

  if (state.status === 'failed') {
    return (
      <div className="profile-page">
        <p className="profile-error">{state.error}</p>
        <button type="button" className="profile-retry" onClick={reload}>
          Retry
        </button>
      </div>
    );
  }

  const { profile } = state;
  const isSeller = hasRole('Seller');

  return (
    <div className="profile-page">
      <Link to="/" className="profile-back">
        ← Back to products
      </Link>

      <section className="profile-card profile-header">
        {profile.avatarUrl ? (
          <img className="profile-avatar" src={profile.avatarUrl} alt="" />
        ) : (
          <div className="profile-avatar profile-avatar-empty" aria-hidden="true">
            {profile.username.charAt(0).toUpperCase()}
          </div>
        )}

        <div>
          <h2 className="profile-name">{profile.username}</h2>
          <p className="profile-email">{profile.email}</p>
          <div className="profile-meta">
            <RoleBadges roles={profile.roles} />
            {isSeller && <span className="profile-rating">★ {profile.ratingScore}</span>}
          </div>
        </div>
      </section>

      <section className="profile-card">
        <h3 className="profile-section-title">About me</h3>
        <p className="profile-bio">{profile.biography ?? 'Nothing here yet.'}</p>
        <p className="profile-since">
          Member since {new Date(profile.registrationDate).toLocaleDateString()}
        </p>
      </section>

      {!isSeller && <BecomeSellerCard />}
    </div>
  );
}
