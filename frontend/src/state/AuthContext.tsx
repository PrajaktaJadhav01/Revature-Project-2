import { createContext, useContext, useMemo, useState } from "react";

export type Role = "SalesRep" | "SalesManager" | "Admin" | null;

type AuthState = {
  token: string | null;
  role: Role;
  userId: number | null;
  assignedRepId: number | null;
};

type AuthContextValue = AuthState & {
  login: (next: Omit<AuthState, never>) => void;
  logout: () => void;
};

const STORAGE_KEY = "cms_auth";

const initial = (): AuthState => {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return { token: null, role: null, userId: null, assignedRepId: null };
  try {
    return JSON.parse(raw) as AuthState;
  } catch {
    return { token: null, role: null, userId: null, assignedRepId: null };
  }
};

const AuthContext = createContext<AuthContextValue | null>(null);

export const getToken = (): string | null => {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw) as AuthState;
    return parsed.token;
  } catch {
    return null;
  }
};

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>(initial);

  const value = useMemo<AuthContextValue>(
    () => ({
      ...state,
      login: (next) => {
        setState(next);
        localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
      },
      logout: () => {
        const cleared = { token: null, role: null, userId: null, assignedRepId: null };
        setState(cleared);
        localStorage.setItem(STORAGE_KEY, JSON.stringify(cleared));
      }
    }),
    [state]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("AuthContext not found");
  return ctx;
}
