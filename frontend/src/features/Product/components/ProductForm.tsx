import { useState } from 'react';
import type { FormEvent } from 'react';
import type { Result } from '../../../shared/auth/result';
import { EMPTY_PRODUCT_FORM, readProductForm, TITLE_MAX_LENGTH, type ProductFormValues } from '../form';
import { PRODUCT_STATUS_LABELS, ProductStatus, type Category, type NewProduct } from '../types';
import './ProductForm.css';

const EDITABLE_STATUSES: readonly ProductStatus[] = [ProductStatus.Draft, ProductStatus.Available, ProductStatus.Sold];

interface ProductFormProps {
  categories: readonly Category[];
  initialValues?: ProductFormValues;
  withStatus?: boolean;
  submitLabel: string;
  onSubmit: (product: NewProduct, status: ProductStatus) => Promise<Result<unknown, string>>;
  onCancel: () => void;
}

export function ProductForm({
  categories,
  initialValues = EMPTY_PRODUCT_FORM,
  withStatus = false,
  submitLabel,
  onSubmit,
  onCancel,
}: Readonly<ProductFormProps>) {
  const [values, setValues] = useState(initialValues);
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const change = <K extends keyof ProductFormValues>(key: K, value: ProductFormValues[K]) => {
    setValues((current) => ({ ...current, [key]: value }));
  };

  const changeStatus = (value: string) => {
    const status = EDITABLE_STATUSES.find((item) => String(item) === value);

    if (status !== undefined) {
      change('status', status);
    }
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const product = readProductForm(values);

    if (!product.ok) {
      setError(product.error);
      return;
    }

    setPending(true);
    setError(null);

    const result = await onSubmit(product.value, values.status);

    if (!result.ok) {
      setError(result.error);
      setPending(false);
    }
  };

  return (
    <form className="product-form" onSubmit={handleSubmit} noValidate>
      <label className="product-form-field">
        <span>Title</span>
        <input
          value={values.title}
          maxLength={TITLE_MAX_LENGTH}
          onChange={(event) => change('title', event.target.value)}
        />
      </label>

      <label className="product-form-field">
        <span>Price, $</span>
        <input
          inputMode="decimal"
          placeholder="0.00"
          value={values.price}
          onChange={(event) => change('price', event.target.value)}
        />
      </label>

      <label className="product-form-field">
        <span>Category</span>
        <select value={values.categoryId} onChange={(event) => change('categoryId', event.target.value)}>
          <option value="" disabled>
            Choose a category
          </option>
          {categories.map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </select>
      </label>

      {withStatus && (
        <label className="product-form-field">
          <span>Status</span>
          <select value={values.status} onChange={(event) => changeStatus(event.target.value)}>
            {EDITABLE_STATUSES.map((status) => (
              <option key={status} value={status}>
                {PRODUCT_STATUS_LABELS[status]}
              </option>
            ))}
          </select>
          <span className="product-form-hint">Draft and Sold products are hidden from the catalog.</span>
        </label>
      )}

      <label className="product-form-field">
        <span>Description</span>
        <textarea
          rows={5}
          value={values.description}
          onChange={(event) => change('description', event.target.value)}
        />
      </label>

      <label className="product-form-field">
        <span>Image links</span>
        <textarea
          rows={3}
          placeholder="https://..."
          value={values.imageUrls}
          onChange={(event) => change('imageUrls', event.target.value)}
        />
        <span className="product-form-hint">One link per line. The first one is the cover.</span>
      </label>

      {error && (
        <p className="product-form-error" role="alert">
          {error}
        </p>
      )}

      <div className="product-form-actions">
        <button type="submit" className="product-form-submit" disabled={pending}>
          {pending ? 'Saving...' : submitLabel}
        </button>
        <button type="button" className="product-form-cancel" onClick={onCancel} disabled={pending}>
          Cancel
        </button>
      </div>
    </form>
  );
}
