import { useCallback } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useResource } from '../../../shared/api/useResource';
import { ok, type Result } from '../../../shared/auth/result';
import { useAuth } from '../../../shared/auth/useAuth';
import { RequireProfile } from '../../Profile/components/RequireProfile';
import type { MyProfile } from '../../Profile/types';
import { fetchCategories, fetchProduct, updateProduct } from '../api';
import { toFormValues } from '../form';
import { canManageProduct } from '../permissions';
import type { Category, NewProduct, Product, ProductStatus } from '../types';
import { ProductForm } from './ProductForm';
import './ProductFormPage.css';

interface EditData {
  product: Product;
  categories: Category[];
}

async function loadEditData(id: string): Promise<Result<EditData, string>> {
  const [product, categories] = await Promise.all([fetchProduct(id), fetchCategories()]);

  if (!product.ok) {
    return product;
  }

  if (!categories.ok) {
    return categories;
  }

  return ok({ product: product.value, categories: categories.value });
}

export function EditProductPage() {
  const { id = '' } = useParams();

  return (
    <div className="product-form-page">
      <Link to={`/products/${id}`} className="product-form-page-back">
        ← Back to product
      </Link>
      <h2 className="product-form-page-title">Edit product</h2>
      <RequireProfile>{(profile) => <EditProductContent id={id} profile={profile} />}</RequireProfile>
    </div>
  );
}

function EditProductContent({ id, profile }: Readonly<{ id: string; profile: MyProfile }>) {
  const { getAccessToken } = useAuth();
  const navigate = useNavigate();
  const load = useCallback(() => loadEditData(id), [id]);
  const { state, reload } = useResource(load);

  const save = useCallback(
    async (product: NewProduct, status: ProductStatus) => {
      const result = await updateProduct(getAccessToken, { ...product, id, status });

      if (result.ok) {
        navigate(`/products/${id}`);
      }

      return result;
    },
    [getAccessToken, id, navigate],
  );

  if (state.status === 'loading') {
    return <p className="product-form-page-message">Loading product...</p>;
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

  const { product, categories } = state.data;

  if (!canManageProduct(product, profile)) {
    return <p className="product-form-page-message">You can edit only your own products.</p>;
  }

  return (
    <ProductForm
      categories={categories}
      initialValues={toFormValues(product)}
      withStatus
      submitLabel="Save changes"
      onSubmit={save}
      onCancel={() => navigate(`/products/${id}`)}
    />
  );
}
