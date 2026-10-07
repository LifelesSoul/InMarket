import { useState } from 'react';
import type { FormEvent } from 'react';
import { useAuth } from '../../../shared/auth/useAuth';
import { createCategory } from '../api';
import type { Category } from '../types';
import { CATEGORY_NAME_MAX, readCategoryName } from '../validation';
import './AddCategoryForm.css';

interface AddCategoryFormProps {
  categories: readonly Category[];
  onAdded: () => void;
}

export function AddCategoryForm({ categories, onAdded }: Readonly<AddCategoryFormProps>) {
  const { getAccessToken } = useAuth();
  const [name, setName] = useState('');
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const checked = readCategoryName(name, categories);

    if (!checked.ok) {
      setError(checked.error);
      return;
    }

    setPending(true);
    setError(null);

    const result = await createCategory(getAccessToken, checked.value);

    setPending(false);

    if (!result.ok) {
      setError(result.error);
      return;
    }

    setName('');
    onAdded();
  };

  return (
    <form className="add-category" onSubmit={handleSubmit} noValidate>
      <input
        className="add-category-input"
        aria-label="New category name"
        placeholder="New category"
        maxLength={CATEGORY_NAME_MAX}
        value={name}
        onChange={(event) => setName(event.target.value)}
      />
      <button type="submit" className="add-category-button" disabled={pending}>
        {pending ? 'Adding...' : 'Add'}
      </button>
      {error && (
        <p className="add-category-error" role="alert">
          {error}
        </p>
      )}
    </form>
  );
}
