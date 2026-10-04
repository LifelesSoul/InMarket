import { useState } from 'react';
import { useMyProfile } from '../useMyProfile';
import './BecomeSellerCard.css';

export function BecomeSellerCard() {
  const { becomeSeller } = useMyProfile();
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleClick = async () => {
    setPending(true);
    setError(null);

    const result = await becomeSeller();

    if (!result.ok) {
      setError(result.error);
      setPending(false);
    }
  };

  return (
    <section className="become-seller">
      <h3 className="become-seller-title">Want to sell?</h3>
      <p className="become-seller-text">Become a seller to publish products and manage them from your profile.</p>
      <button type="button" className="become-seller-button" onClick={handleClick} disabled={pending}>
        {pending ? 'Saving...' : 'Become a seller'}
      </button>
      {error && <p className="become-seller-error">{error}</p>}
    </section>
  );
}
