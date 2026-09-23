import { useEffect, useState } from 'react';
import { useAuth } from '../../../shared/auth/useAuth';
import { useNavigate } from 'react-router-dom';

interface UserProfileData {
  username: string;
  email: string;
  avatarUrl: string;
  biography: string;
  ratingScore: number;
}

export function MyProfile() {
  const { getAccessToken, isAuthenticated, isLoading: isAuthLoading } = useAuth();
  const navigate = useNavigate();
  
  const [profile, setProfile] = useState<UserProfileData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchMyProfile = async () => {
      if (!isAuthenticated) {
        setIsLoading(false);
        return;
      }

      try {
        const token = await getAccessToken();

        if (token === null) {
          throw new Error('Could not obtain an access token');
        }

        const response = await fetch(`${import.meta.env.VITE_API_URL}/profiles/me`, {
          headers: {
            Authorization: `Bearer ${token}`
          }
        });

        if (!response.ok) throw new Error('Failed to load profile');

        const data = await response.json();
        setProfile(data);
      } catch (err: unknown) {
        setError(err instanceof Error ? err.message : 'Unknown error');
      } finally {
        setIsLoading(false);
      }
    };

    fetchMyProfile();
  }, [getAccessToken, isAuthenticated]);

  if (isAuthLoading || isLoading) return <div>Loading profile... ⏳</div>;
  if (error) return <div style={{ color: 'red' }}>Error: {error}</div>;
  if (!isAuthenticated) return <div>Please log in to view your profile.</div>;
  if (!profile) return <div>Profile not found.</div>;

  return (
    <div className="db-profile-card">
      <button onClick={() => navigate('/')} style={{ marginBottom: '20px' }}>
        ← Back to Products
      </button>

      <h2>My Profile</h2>
      <div className="profile-header">
        <img src={profile.avatarUrl} alt="Avatar" className="db-avatar" />
        <div className="profile-main-info">
          <h3>{profile.username}</h3>
          <p>{profile.email}</p>
          <span className="rating-badge">Rating: {profile.ratingScore}</span>
        </div>
      </div>
      
      <div className="profile-bio">
        <h4>About me:</h4>
        <p>{profile.biography}</p>
      </div>
    </div>
  );
}
