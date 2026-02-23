import { useEffect, useState, useRef, useCallback } from 'react';
import type { Product, PagedResult } from '../types';

export function ProductList() {
  const [products, setProducts] = useState<Product[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  
  const [hasMore, setHasMore] = useState<boolean>(true);
  const [cursor, setCursor] = useState<string | null>(null);

  const observer = useRef<IntersectionObserver | null>(null);

  const lastProductElementRef = useCallback((node: HTMLDivElement) => {
    if (loading) return;
    if (observer.current) observer.current.disconnect();

    observer.current = new IntersectionObserver(entries => {
      if (entries[0].isIntersecting && hasMore) {
        fetchProducts(cursor);
      }
    });

    if (node) observer.current.observe(node);
  }, [loading, hasMore, cursor]);

  const fetchProducts = async (currentCursor: string | null) => {
    setLoading(true);
    try {
      const url = new URL(`${import.meta.env.VITE_API_URL}/products`);
      url.searchParams.append("limit", "10");
      
      if (currentCursor) {
        url.searchParams.append("lastId", currentCursor); 
      }

      const response = await fetch(url.toString());
      if (!response.ok) throw new Error(`Server error: ${response.status}`);

      const data: PagedResult<Product> = await response.json();

      const nextCursor = data.lastId ?? null;

      setProducts(prev => {
        const newItems = data.items.filter(newItem => !prev.some(p => p.id === newItem.id));
        return [...prev, ...newItems];
      });

      setCursor(nextCursor);
      setHasMore(nextCursor !== null && data.items.length > 0);

    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts(null);
  }, []);

  return (
    <div style={{ maxWidth: '800px', margin: '0 auto' }}>
      {error && <h3 style={{ color: 'red', backgroundColor: '#fee', padding: '10px' }}>Ошибка: {error}</h3>}

      <div style={{ display: 'grid', gap: '20px' }}>
        {products.map((product, index) => {
          const isLastElement = products.length === index + 1;

          return (
            <div 
              key={product.id} 
              ref={isLastElement ? lastProductElementRef : null} 
              style={{ border: '1px solid #ccc', padding: '15px', borderRadius: '8px', boxShadow: '0 2px 5px rgba(0,0,0,0.1)' }}
            >
              <h3 style={{ margin: '0 0 10px 0' }}>{product.title}</h3>
              <span style={{ backgroundColor: '#eee', padding: '3px 8px', borderRadius: '12px', fontSize: '0.8em' }}>
                {product.categoryName}
              </span>
              <p style={{ color: '#555' }}>{product.description || "No Options"}</p>
              <strong style={{ fontSize: '1.2em', color: '#2a9d8f' }}>Цена: ${product.price}</strong>
            </div>
          );
        })}
      </div>

      {loading && <h3 style={{ textAlign: 'center', margin: '20px 0' }}>Loading more... ⏳</h3>}
      
      {!hasMore && products.length > 0 && (
        <p style={{ textAlign: 'center', color: '#888', marginTop: '20px' }}> 🎉</p>
      )}
    </div>
  );
}
