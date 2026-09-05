/**
 * User profile hook — STUB (CANDIDATE)
 * Returns placeholder data until backend MeClient profile endpoint is confirmed.
 * Marked CANDIDATE per research.md Area 6.
 */

interface UserProfile {
  name: string;
  email: string;
  role: string;
  isLoading: false;
}

export function useUserProfile(): UserProfile {
  return {
    name: 'مستخدم النظام',
    email: '',
    role: '',
    isLoading: false,
  };
}
