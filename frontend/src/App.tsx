import { ProductList } from './features/Product/components/ProductList';
import './App.css';

function App() {
  return (
    <div className="app-container">
      <header className="app-header">
        <h1>MarketPlace</h1>
      </header>
      
      <main>
        <ProductList />
      </main>
    </div>
  );
}

export default App;
