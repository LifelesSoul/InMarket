import { useContext } from 'react';
import { MyProfileContext } from './myProfileContext';
import type { MyProfileContextValue } from './types';

export function useMyProfile(): MyProfileContextValue {
  const context = useContext(MyProfileContext);

  if (context === null) {
    throw new Error('useMyProfile must be used inside MyProfileProvider');
  }

  return context;
}
