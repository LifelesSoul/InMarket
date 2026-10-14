import { PRODUCT_STATUS_LABELS, type ProductStatus } from '../types';
import './ProductStatusBadge.css';

export function ProductStatusBadge({ status }: Readonly<{ status: ProductStatus }>) {
  const label = PRODUCT_STATUS_LABELS[status];

  return <span className={`product-status product-status-${label.toLowerCase()}`}>{label}</span>;
}
