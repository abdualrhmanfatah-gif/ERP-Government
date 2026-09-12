import { createContext, useContext, useReducer, useCallback, useEffect, useRef, type ReactNode } from 'react';
import type { NotificationEntry } from './types';

type Action =
  | { type: 'ADD'; entry: NotificationEntry }
  | { type: 'MARK_READ'; id: string }
  | { type: 'REMOVE'; id: string }
  | { type: 'CLEAR_LOCAL' }
  | { type: 'SET_SERVER'; entries: NotificationEntry[] };

interface State {
  local: NotificationEntry[];
  server: NotificationEntry[];
}

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case 'ADD':
      return { ...state, local: [action.entry, ...state.local] };
    case 'MARK_READ':
      return {
        local: state.local.map((e) => (e.id === action.id ? { ...e, isRead: true } : e)),
        server: state.server.map((e) => (e.id === action.id ? { ...e, isRead: true } : e)),
      };
    case 'REMOVE':
      return {
        local: state.local.filter((e) => e.id !== action.id),
        server: state.server.filter((e) => e.id !== action.id),
      };
    case 'CLEAR_LOCAL':
      return { ...state, local: [] };
    case 'SET_SERVER':
      return { ...state, server: action.entries };
    default:
      return state;
  }
}

const LOCAL_TTL_MS = 5 * 60 * 1000;

interface NotificationStore {
  entries: NotificationEntry[];
  unreadCount: number;
  addLocal: (entry: NotificationEntry) => void;
  setServer: (entries: NotificationEntry[]) => void;
  markRead: (id: string) => void;
  remove: (id: string) => void;
  clearLocal: () => void;
}

const Ctx = createContext<NotificationStore | null>(null);

export function NotificationProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(reducer, { local: [], server: [] });
  const timerRef = useRef<ReturnType<typeof setInterval>>(null);

  useEffect(() => {
    timerRef.current = setInterval(() => {
      dispatch({ type: 'CLEAR_LOCAL' });
    }, LOCAL_TTL_MS);
    return () => clearInterval(timerRef.current!);
  }, []);

  const entries = [...state.local, ...state.server].sort(
    (a, b) => new Date(b.created).getTime() - new Date(a.created).getTime(),
  );
  const unreadCount = entries.filter((e) => !e.isRead).length;

  const addLocal = useCallback((entry: NotificationEntry) => dispatch({ type: 'ADD', entry }), []);
  const setServer = useCallback((entries: NotificationEntry[]) => dispatch({ type: 'SET_SERVER', entries }), []);
  const markRead = useCallback((id: string) => dispatch({ type: 'MARK_READ', id }), []);
  const remove = useCallback((id: string) => dispatch({ type: 'REMOVE', id }), []);
  const clearLocal = useCallback(() => dispatch({ type: 'CLEAR_LOCAL' }), []);

  return (
    <Ctx.Provider value={{ entries, unreadCount, addLocal, setServer, markRead, remove, clearLocal }}>
      {children}
    </Ctx.Provider>
  );
}

export function useNotificationStore(): NotificationStore {
  const ctx = useContext(Ctx);
  if (!ctx) throw new Error('useNotificationStore must be inside NotificationProvider');
  return ctx;
}
