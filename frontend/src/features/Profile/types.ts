import type { Result } from '../../shared/auth/result';

export type MarketRole = 'Buyer' | 'Seller' | 'Admin';

export interface MyProfile {
  id: string;
  username: string;
  email: string;
  avatarUrl?: string;
  biography?: string;
  ratingScore: number;
  registrationDate: string;
  roles: MarketRole[];
}

export type MyProfileState =
  | { status: 'anonymous' }
  | { status: 'loading' }
  | { status: 'loaded'; profile: MyProfile }
  | { status: 'failed'; error: string };

export type MyProfileContextValue = Readonly<{
  state: MyProfileState;
  hasRole: (role: MarketRole) => boolean;
  reload: () => void;
  becomeSeller: () => Promise<Result<void, string>>;
}>;
