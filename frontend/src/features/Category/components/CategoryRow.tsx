import { useState } from 'react';
import { useAuth } from '../../../shared/auth/useAuth';
import { deleteCategory, renameCategory } from '../api';
import type { Category } from '../types';
import { CATEGORY_NAME_MAX, readCategoryName } from '../validation';
import './CategoryRow.css';

type RowState =
  | { mode: 'view' }
  | { mode: 'editing'; name: string; saving: boolean; error: string | null }
  | { mode: 'confirming'; deleting: boolean; error: string | null };

interface CategoryRowProps {
  category: Category;
  categories: readonly Category[];
  onChanged: () => void;
}

export function CategoryRow({ category, categories, onChanged }: Readonly<CategoryRowProps>) {
  const { getAccessToken } = useAuth();
  const [state, setState] = useState<RowState>({ mode: 'view' });

  const save = async (name: string) => {
    const checked = readCategoryName(name, categories, category.id);

    if (!checked.ok) {
      setState({ mode: 'editing', name, saving: false, error: checked.error });
      return;
    }

    if (checked.value === category.name) {
      setState({ mode: 'view' });
      return;
    }

    setState({ mode: 'editing', name, saving: true, error: null });

    const result = await renameCategory(getAccessToken, { id: category.id, name: checked.value });

    if (result.ok) {
      onChanged();
      return;
    }

    setState({ mode: 'editing', name, saving: false, error: result.error });
  };

  const remove = async () => {
    setState({ mode: 'confirming', deleting: true, error: null });

    const result = await deleteCategory(getAccessToken, category.id);

    if (result.ok) {
      onChanged();
      return;
    }

    setState({ mode: 'confirming', deleting: false, error: result.error });
  };

  if (state.mode === 'editing') {
    return (
      <li className="category-row">
        <input
          className="category-row-input"
          aria-label={`New name for ${category.name}`}
          maxLength={CATEGORY_NAME_MAX}
          value={state.name}
          onChange={(event) => setState({ ...state, name: event.target.value })}
        />
        <button
          type="button"
          className="category-row-primary"
          onClick={() => save(state.name)}
          disabled={state.saving}
        >
          {state.saving ? 'Saving...' : 'Save'}
        </button>
        <button type="button" onClick={() => setState({ mode: 'view' })} disabled={state.saving}>
          Cancel
        </button>
        {state.error && <p className="category-row-error">{state.error}</p>}
      </li>
    );
  }

  if (state.mode === 'confirming') {
    return (
      <li className="category-row">
        <span className="category-row-name">Delete “{category.name}”?</span>
        <button type="button" className="category-row-danger" onClick={remove} disabled={state.deleting}>
          {state.deleting ? 'Deleting...' : 'Yes, delete'}
        </button>
        <button type="button" onClick={() => setState({ mode: 'view' })} disabled={state.deleting}>
          Cancel
        </button>
        {state.error && <p className="category-row-error">{state.error}</p>}
      </li>
    );
  }

  return (
    <li className="category-row">
      <span className="category-row-name">{category.name}</span>
      <button
        type="button"
        onClick={() => setState({ mode: 'editing', name: category.name, saving: false, error: null })}
      >
        Rename
      </button>
      <button type="button" onClick={() => setState({ mode: 'confirming', deleting: false, error: null })}>
        Delete
      </button>
    </li>
  );
}
