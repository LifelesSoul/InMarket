import { Routes, Route } from 'react-router-dom';
import { ProductList } from './features/Product/components/ProductList';
import { ProductPage } from './features/Product/components/ProductPage';
import { NewProductPage } from './features/Product/components/NewProductPage';
import { EditProductPage } from './features/Product/components/EditProductPage';
import { ProfilePage } from './features/Profile/components/ProfilePage';
import { AuthCallback } from './features/Auth/components/AuthCallback';
import { CategoriesAdminPage } from './features/Category/components/CategoriesAdminPage';
import './App.css';

function App() {
  return (
    <div className="app-container">

      <header className="app-header">
        <h1>MarketPlace</h1>
      </header>

      <main>
        <Routes>
          <Route path="/" element={<ProductList />} />
          <Route path="/products/new" element={<NewProductPage />} />
          <Route path="/products/:id" element={<ProductPage />} />
          <Route path="/products/:id/edit" element={<EditProductPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/admin/categories" element={<CategoriesAdminPage />} />
          <Route path="/auth/callback" element={<AuthCallback />} />
        </Routes>
      </main>

    </div>
  );
}

export default App;
