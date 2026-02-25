import { useAuth0 } from '@auth0/auth0-react';
import styles from './ProfileHeader.module.css';

export function ProfileHeader() {
  const { loginWithRedirect, logout, user, isAuthenticated, isLoading } = useAuth0();

  if (isLoading) {
    return <div>Loading profile...</div>;
  }

  return (
    <div className={styles.container}>
      
      {!isAuthenticated && (
        <>
          <h2>Guest</h2>
          <button 
            onClick={() => loginWithRedirect()}
            className={styles.loginButton}
          >
            Login / Registration
          </button>
        </>
      )}

      {isAuthenticated && user && (
        <>
          <div className={styles.userInfo}>
            <img 
              src={user.picture} 
              alt={user.name} 
              className={styles.avatar}
            />
            <div>
              <h3 className={styles.userName}>{user.name}</h3>
              <p className={styles.userEmail}>{user.email}</p>
            </div>
          </div>

          <div className={styles.buttonGroup}>
            <button className={styles.addButton}>
              Add product
            </button>
            <button className={styles.lotsButton}>
              My lots
            </button>
            <button 
              onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}
              className={styles.exitButton}
            >
              Exit
            </button>
          </div>
        </>
      )}

    </div>
  );
}
