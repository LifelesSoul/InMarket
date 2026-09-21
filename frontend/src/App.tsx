import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ProductList } from './features/Product/components/ProductList';
import { MyProfile } from './features/Auth/components/MyProfile';
import './App.css';

function App() {
  return (
    <BrowserRouter>
      <div className="app-container">
        
        <header className="app-header">
          <h1>MarketPlace</h1>
        </header>
        
        <main>
          <Routes>
            <Route path="/" element={<ProductList />} />
            <Route path="/profile" element={<MyProfile />} />
          </Routes>
        </main>

      </div>
    </BrowserRouter>
  );
}

export default App;