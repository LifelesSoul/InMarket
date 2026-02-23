import { ProductList } from './features/Product/components/ProductList';

function App() {
  return (
    <div style={{ padding: '40px', fontFamily: 'sans-serif' }}>
      <header style={{ marginBottom: '30px', borderBottom: '1px solid #eee', paddingBottom: '10px' }}>
        <h1>MarketPlace</h1>
      </header>
      
      <main>
        <ProductList />
      </main>
    </div>
  );
}

export default App;
