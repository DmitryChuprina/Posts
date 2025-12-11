import { cookies } from "next/headers";
import { useSessionStore } from "./store";
import { AuthApi } from "@/shared/api/auth";

const ACCESS_TOKEN_KEY = 'auth:access-token';
const REFRESH_TOKEN_KEY = 'auth:refresh-token'

class SessionService {
    async init() {
        const cookieStore = await cookies();

        const accessToken = cookieStore.get("access")?.value;
        if (!accessToken) {
            useSessionStore.setState({ user: null });
            return;
        }

        const user = await AuthApi.me();
        useSessionStore.setState({ user });
    }

    async getAccessToken() {
        const cookieStore = await cookies();
        return cookieStore.get(ACCESS_TOKEN_KEY)?.value;
    }

    async getRefreshToken() {
        const cookieStore = await cookies();
        return cookieStore.get(REFRESH_TOKEN_KEY)?.value;
    }

    async logout() {
        const cookieStore = await cookies();
        cookieStore.delete(ACCESS_TOKEN_KEY);
        cookieStore.delete(REFRESH_TOKEN_KEY);
        useSessionStore.setState({ user: null });
        window.location.href = "/sign-in";
    }
}

export const sessionService = new SessionService()