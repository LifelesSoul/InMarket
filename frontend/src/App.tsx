import { ProductList } from './features/Product/components/ProductList';
import { ProfileHeader } from './features/Auth/components/ProfileHeader';
import styles from './App.module.css';

function App() {
  return (
    <div className={styles.mainContainer}>
      <ProfileHeader />
      <ProductList />
    </div>
  );
}

export default App;
