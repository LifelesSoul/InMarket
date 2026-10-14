import { useEffect, useReducer, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../../shared/auth/useAuth';
import { fetchMyProducts, MY_PRODUCTS_PAGE_SIZE } from '../api';
import { formatPrice } from '../format';
import type { PagedResult, Product } from '../types';
import { ProductStatusBadge } from './ProductStatusBadge';
import './MyProducts.css';

interface MyProductsState {
  items: Product[];
  lastId: string | null;
  hasMore: boolean;
  loading: boolean;
  error: string | null;
}

type MyProductsEvent =
  | { type: 'requested' }
  | { type: 'received'; page: PagedResult<Product>; append: boolean }
  | { type: 'failed'; error: string };

const INITIAL_STATE: MyProductsState = { items: [], lastId: null, hasMore: false, loading: true, error: null };

function myProductsReducer(state: MyProductsState, event: MyProductsEvent): MyProductsState {
  switch (event.type) {
    case 'requested':
      return { ...state, loading: true, error: null };
    case 'received':
      return {
        items: event.append ? [...state.items, ...event.page.items] : event.page.items,
        lastId: event.page.lastId ?? null,
        hasMore: event.page.items.length === MY_PRODUCTS_PAGE_SIZE,
        loading: false,
        error: null,
      };
    case 'failed':
      return { ...state, loading: false, error: event.error };
  }
}

export function MyProducts() {
  const { getAccessToken } = useAuth();
  const [state, dispatch] = useReducer(myProductsReducer, INITIAL_STATE);
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    let cancelled = false;
    dispatch({ type: 'requested' });

    void fetchMyProducts(getAccessToken, null).then((result) => {
      if (cancelled) {
        return;
      }

      dispatch(
        result.ok
          ? { type: 'received', page: result.value, append: false }
          : { type: 'failed', error: result.error },
      );
    });

    return () => {
      cancelled = true;
    };
  }, [getAccessToken, attempt]);

  const loadMore = async () => {
    dispatch({ type: 'requested' });

    const result = await fetchMyProducts(getAccessToken, state.lastId);

    dispatch(
      result.ok
        ? { type: 'received', page: result.value, append: true }
        : { type: 'failed', error: result.error },
    );
  };

  const retry = () => {
    if (state.items.length === 0) {
      setAttempt((value) => value + 1);
    } else {
      void loadMore();
    }
  };

  const isEmpty = state.items.length === 0;

  return (
    <section className="my-products">
      <div className="my-products-header">
        <h3 className="my-products-title">My products</h3>
        <Link to="/products/new" className="my-products-add">
          + Add product
        </Link>
      </div>

      {isEmpty && state.loading && <p className="my-products-message">Loading your products...</p>}

      {isEmpty && !state.loading && state.error === null && (
        <p className="my-products-message">You have not published anything yet.</p>
      )}

      {!isEmpty && (
        <ul className="my-products-list">
          {state.items.map((product) => (
            <li key={product.id} className="my-products-item">
              {product.imageUrl ? (
                <img className="my-products-thumb" src={product.imageUrl} alt="" />
              ) : (
                <div className="my-products-thumb my-products-thumb-empty" aria-hidden="true" />
              )}
              <Link to={`/products/${product.id}`} className="my-products-name">
                {product.title}
              </Link>
              <ProductStatusBadge status={product.status} />
              <span className="my-products-price">{formatPrice(product.price)}</span>
              <Link to={`/products/${product.id}/edit`} className="my-products-edit">
                Edit
              </Link>
            </li>
          ))}
        </ul>
      )}

      {state.error !== null && (
        <div className="my-products-error">
          <span>{state.error}</span>
          <button type="button" onClick={retry}>
            Retry
          </button>
        </div>
      )}

      {state.hasMore && state.error === null && (
        <button type="button" className="my-products-more" onClick={loadMore} disabled={state.loading}>
          {state.loading ? 'Loading...' : 'Load more'}
        </button>
      )}
    </section>
  );
}
