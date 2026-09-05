import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import {
  getToken, setToken, removeToken,
  getAuthUser, setAuthUser, isAuthenticated as checkTokenValid,
} from '../utils/auth-token';
import { authFetch } from '../utils/auth-fetch';

interface AuthContextValue {
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    setIsAuthenticated(checkTokenValid());
    setIsLoading(false);
  }, []);

  const login = async (email: string, password: string) => {
    const res = await fetch('/api/Users/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
      body: JSON.stringify({ email, password }),
    });

    if (!res.ok) {
      const body = await res.json().catch(() => ({}));
      throw new Error(body.error || `Login failed (${res.status})`);
    }

    const data = await res.json();
    setToken(data.token);
    setAuthUser({ userId: data.userId, role: data.role });
    setIsAuthenticated(true);
  };

  const logout = async () => {
    try {
      await authFetch('/api/Users/logout', { method: 'POST' });
    } catch {
      // ignore — token is cleared regardless
    }
    removeToken();
    setIsAuthenticated(false);
  };

  const register = async (_email: string, _password: string) => {
    throw new Error('Registration is not supported. Contact your administrator.');
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
