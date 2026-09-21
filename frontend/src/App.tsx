import { Routes, Route } from 'react-router-dom';
import { ProductList } from './features/Product/components/ProductList';
import { MyProfile } from './features/Auth/components/MyProfile';
import { AuthCallback } from './features/Auth/components/AuthCallback';
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
          <Route path="/profile" element={<MyProfile />} />
          <Route path="/auth/callback" element={<AuthCallback />} />
        </Routes>
      </main>

    </div>
  );
}

export default App;
