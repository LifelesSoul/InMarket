import { Link } from 'react-router-dom';
import { useResource } from '../../../shared/api/useResource';
import { RequireProfile } from '../../Profile/components/RequireProfile';
import { useMyProfile } from '../../Profile/useMyProfile';
import { fetchCategories } from '../api';
import { AddCategoryForm } from './AddCategoryForm';
import { CategoryRow } from './CategoryRow';
import './CategoriesAdminPage.css';

export function CategoriesAdminPage() {
  return (
    <div className="categories-admin">
      <Link to="/" className="categories-admin-back">
        ← Back to products
      </Link>
      <h2 className="categories-admin-title">Categories</h2>
      <RequireProfile>{() => <CategoriesAdminContent />}</RequireProfile>
    </div>
  );
}

function CategoriesAdminContent() {
  const { hasRole } = useMyProfile();
  const { state, reload } = useResource(fetchCategories);

  if (!hasRole('Admin')) {
    return (
      <div className="categories-admin-denied" role="alert">
        <p className="categories-admin-denied-title">Access denied</p>
        <p className="categories-admin-denied-text">
          You don&apos;t have admin rights. Ask an administrator to give you the Admin role.
        </p>
      </div>
    );
  }

  if (state.status === 'loading') {
    return <p className="categories-admin-message">Loading categories...</p>;
  }

  if (state.status === 'failed') {
    return (
      <>
        <p className="categories-admin-error">{state.error}</p>
        <button type="button" className="categories-admin-retry" onClick={reload}>
          Retry
        </button>
      </>
    );
  }

  const categories = state.data;

  return (
    <section className="categories-admin-card">
      <AddCategoryForm categories={categories} onAdded={reload} />

      {categories.length === 0 ? (
        <p className="categories-admin-message">No categories yet. Add the first one above.</p>
      ) : (
        <ul className="categories-admin-list">
          {categories.map((category) => (
            <CategoryRow key={category.id} category={category} categories={categories} onChanged={reload} />
          ))}
        </ul>
      )}

      <p className="categories-admin-hint">
        Names are saved in sentence case. A category that still has products cannot be deleted.
      </p>
    </section>
  );
}
