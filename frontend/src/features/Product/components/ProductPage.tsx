import { useCallback, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useResource } from '../../../shared/api/useResource';
import { useMyProfile } from '../../Profile/useMyProfile';
import { fetchProduct } from '../api';
import { formatPrice } from '../format';
import { canManageProduct, isProductOwner } from '../permissions';
import { ProductStatus } from '../types';
import { DeleteProductButton } from './DeleteProductButton';
import { ProductStatusBadge } from './ProductStatusBadge';
import './ProductPage.css';

export function ProductPage() {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const { state: profileState } = useMyProfile();
  const load = useCallback(() => fetchProduct(id), [id]);
  const { state, reload } = useResource(load);

  if (state.status === 'loading') {
    return <div className="product-page">Loading product...</div>;
  }

  if (state.status === 'failed') {
    return (
      <div className="product-page">
        <Link to="/" className="product-page-back">
          ← Back to products
        </Link>
        <p className="product-page-error">{state.error}</p>
        <button type="button" className="product-page-retry" onClick={reload}>
          Retry
        </button>
      </div>
    );
  }

  const product = state.data;
  const profile = profileState.status === 'loaded' ? profileState.profile : null;
  const canManage = profile !== null && canManageProduct(product, profile);
  const afterDelete = () => navigate(profile !== null && isProductOwner(product, profile) ? '/profile' : '/');

  return (
    <div className="product-page">
      <Link to="/" className="product-page-back">
        ← Back to products
      </Link>

      <article className="product-page-card">
        <ProductGallery key={product.id} urls={product.imageUrls} />

        <div className="product-page-heading">
          <h2 className="product-page-title">{product.title}</h2>
          {product.status !== ProductStatus.Available && <ProductStatusBadge status={product.status} />}
        </div>

        <span className="product-page-category">{product.categoryName}</span>
        <strong className="product-page-price">{formatPrice(product.price)}</strong>
        <p className="product-page-description">{product.description ?? 'No description.'}</p>
        <p className="product-page-meta">
          Sold by {product.seller.username} · Listed {new Date(product.createdAt).toLocaleDateString()}
        </p>
      </article>

      {canManage && (
        <section className="product-page-actions">
          <Link to={`/products/${product.id}/edit`} className="product-page-edit">
            Edit
          </Link>
          <DeleteProductButton productId={product.id} onDeleted={afterDelete} />
        </section>
      )}
    </div>
  );
}

function ProductGallery({ urls }: Readonly<{ urls: readonly string[] }>) {
  const [selected, setSelected] = useState(0);

  if (urls.length === 0) {
    return null;
  }

  const current = urls[Math.min(selected, urls.length - 1)];

  return (
    <div className="product-gallery">
      <img className="product-gallery-main" src={current} alt="" />
      {urls.length > 1 && (
        <div className="product-gallery-thumbs">
          {urls.map((url, index) => (
            <button
              key={`${index}-${url}`}
              type="button"
              className={index === selected ? 'product-gallery-thumb selected' : 'product-gallery-thumb'}
              onClick={() => setSelected(index)}
              aria-label={`Show image ${index + 1}`}
            >
              <img src={url} alt="" />
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
