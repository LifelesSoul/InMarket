import { createContext } from 'react';
import type { MyProfileContextValue } from './types';

export const MyProfileContext = createContext<MyProfileContextValue | null>(null);
