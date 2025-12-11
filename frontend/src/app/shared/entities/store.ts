import { create } from "zustand";
import { AuthUserDto } from "../api/dtos/auth.dtos";

interface SessionState {
    user: AuthUserDto | null;
    setUser: (val: AuthUserDto | null) => void;
}

export const useSessionStore = create<SessionState>(set => ({
    user: null,
    setUser: (val) => set({ user: val })
}));