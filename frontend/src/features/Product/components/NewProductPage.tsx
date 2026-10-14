import { useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useResource } from '../../../shared/api/useResource';
import { useAuth } from '../../../shared/auth/useAuth';
import { RequireProfile } from '../../Profile/components/RequireProfile';
import { useMyProfile } from '../../Profile/useMyProfile';
import { createProduct, fetchCategories } from '../api';
import type { NewProduct } from '../types';
import { ProductForm } from './ProductForm';
import './ProductFormPage.css';

export function NewProductPage() {
  return (
    <div className="product-form-page">
      <Link to="/profile" className="product-form-page-back">
        ← Back to profile
      </Link>
      <h2 className="product-form-page-title">New product</h2>
      <RequireProfile>{() => <NewProductContent />}</RequireProfile>
    </div>
  );
}

function NewProductContent() {
  const { getAccessToken } = useAuth();
  const { hasRole } = useMyProfile();
  const navigate = useNavigate();
  const { state, reload } = useResource(fetchCategories);

  const publish = useCallback(
    async (product: NewProduct) => {
      const result = await createProduct(getAccessToken, product);

      if (result.ok) {
        navigate(`/products/${result.value.id}`);
      }

      return result;
    },
    [getAccessToken, navigate],
  );

  if (!hasRole('Seller')) {
    return (
      <p className="product-form-page-message">
        Only sellers can publish products. <Link to="/profile">Become a seller in your profile.</Link>
      </p>
    );
  }

  if (state.status === 'loading') {
    return <p className="product-form-page-message">Loading categories...</p>;
  }

  if (state.status === 'failed') {
    return (
      <>
        <p className="product-form-page-error">{state.error}</p>
        <button type="button" className="product-form-page-retry" onClick={reload}>
          Retry
        </button>
      </>
    );
  }

  if (state.data.length === 0) {
    return <p className="product-form-page-message">There are no categories yet. Ask an admin to add one.</p>;
  }

  return (
    <ProductForm
      categories={state.data}
      submitLabel="Publish"
      onSubmit={publish}
      onCancel={() => navigate('/profile')}
    />
  );
}
