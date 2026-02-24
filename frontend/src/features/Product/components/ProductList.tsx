import { useEffect, useState, useRef, useCallback } from 'react';
import type { Product, PagedResult } from '../types';

import styles from './ProductList.module.css';

function mergeProducts(existingProducts: Product[], newProducts: Product[]): Product[] {
  const existingIds = new Set(existingProducts.map(p => p.id));
  const uniqueNewProducts = newProducts.filter(p => !existingIds.has(p.id));
  
  return [...existingProducts, ...uniqueNewProducts];
}

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

      setProducts(prev => mergeProducts(prev, data.items));

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
    <div className={styles.container}>
      {error && <h3 className={styles.errorMessage}>Error: {error}</h3>}

      <div className={styles.grid}>
        {products.map((product, index) => {
          const isLastElement = products.length === index + 1;

          return (
            <div 
              key={product.id} 
              ref={isLastElement ? lastProductElementRef : null} 
              className={styles.card}
            >
              <h3 className={styles.cardTitle}>{product.title}</h3>
              <span className={styles.badge}>
                {product.categoryName}
              </span>
              <p className={styles.description}>{product.description || "No Options"}</p>
              <strong className={styles.price}>Cost: ${product.price}</strong>
            </div>
          );
        })}
      </div>

      {loading && <h3 className={styles.loading}>Loading more... ⏳</h3>}
      
      {!hasMore && products.length > 0 && (
        <p className={styles.endMessage}> 🎉</p>
      )}
    </div>
  );
}
