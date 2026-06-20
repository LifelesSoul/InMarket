import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ProductList } from './features/Product/components/ProductList';
import { MyProfile } from './features/Auth/components/MyProfile';
import './App.css';

function App() {
  return (
    // Оборачиваем всё приложение в провайдер маршрутизации
    <BrowserRouter>
      <div className="app-container">
        
        {/* Шапка останется видимой на ВСЕХ страницах */}
        <header className="app-header">
          <h1>MarketPlace</h1>
        </header>
        
        <main>
          {/* Здесь React Router будет подменять компоненты */}
          <Routes>
            {/* Если путь "/", показываем товары */}
            <Route path="/" element={<ProductList />} />
            
            {/* Если путь "/profile", показываем профиль из БД */}
            <Route path="/profile" element={<MyProfile />} />
          </Routes>
        </main>

      </div>
    </BrowserRouter>
  );
}

export default App;