import { useState } from 'react';
import { useAuth } from '../../../shared/auth/useAuth';
import { deleteProduct } from '../api';
import './DeleteProductButton.css';

type DeleteState =
  | { step: 'idle' }
  | { step: 'confirming' }
  | { step: 'deleting' }
  | { step: 'failed'; error: string };

interface DeleteProductButtonProps {
  productId: string;
  onDeleted: () => void;
}

export function DeleteProductButton({ productId, onDeleted }: Readonly<DeleteProductButtonProps>) {
  const { getAccessToken } = useAuth();
  const [state, setState] = useState<DeleteState>({ step: 'idle' });

  const confirmDelete = async () => {
    setState({ step: 'deleting' });

    const result = await deleteProduct(getAccessToken, productId);

    if (result.ok) {
      onDeleted();
      return;
    }

    setState({ step: 'failed', error: result.error });
  };

  if (state.step === 'idle') {
    return (
      <button type="button" className="delete-product" onClick={() => setState({ step: 'confirming' })}>
        Delete
      </button>
    );
  }

  const deleting = state.step === 'deleting';

  return (
    <div className="delete-product-confirm">
      <span>Delete this product for good?</span>
      <button type="button" className="delete-product" onClick={confirmDelete} disabled={deleting}>
        {deleting ? 'Deleting...' : 'Yes, delete'}
      </button>
      <button
        type="button"
        className="delete-product-cancel"
        onClick={() => setState({ step: 'idle' })}
        disabled={deleting}
      >
        Cancel
      </button>
      {state.step === 'failed' && <p className="delete-product-error">{state.error}</p>}
    </div>
  );
}
