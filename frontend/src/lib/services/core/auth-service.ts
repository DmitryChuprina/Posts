import { ISesionTokens, SessionStore } from "../../stores/session";
import { ApiError, httpClient } from "../../clients/http-client";
import { redirect } from "next/navigation";
import { AuthTokensDto } from "../../dtos/auth.dtos";

const globalLocks = new Map<string, Promise<unknown>>();

export class AuthService {
    constructor(
        private readonly session: SessionStore,
        private readonly preventRedirect?: boolean
    ) { }

    async getValidAccessToken(): Promise<string | null> {
        const tokens = this.session.getTokens();
        if (!tokens) {
            return null;
        }

        const lock = globalLocks.get(tokens.refreshToken);
        if (lock) {
            await lock;
            return this.getValidAccessToken();
        }

        const nowWithBuffer = new Date(Date.now() + 10000);
        if (new Date(tokens.expiresAt) > nowWithBuffer) {
            return tokens.accessToken;
        }

        return this.performRefresh(tokens);
    }

    private async performRefresh(oldTokens: ISesionTokens): Promise<string | null> {
        const lockKey = oldTokens.refreshToken;
        const lock = this.makeRefreshRequest(oldTokens)
        globalLocks.set(lockKey, lock)

        try {
            await lock.finally(() => globalLocks.delete(lockKey))
            return this.session.getTokens()?.accessToken || null;
        } catch {
            return null;
        }
    }

    private async makeRefreshRequest(oldTokens: ISesionTokens): Promise<string | null> {
        try {
            const newTokens = await httpClient
                .post<AuthTokensDto>('/auth/refresh-token', oldTokens);
            this.session.setTokens(newTokens);
            return newTokens.accessToken;
        } catch (err) {
            console.error(`Error when refreshing tokens: `, err);

            if (
                err instanceof ApiError &&
                ![0, 500].includes(err.status)
            ) {
                this.session.clean();
                if (!this.preventRedirect) {
                    redirect('/sign-in');
                }
                return null;
            }

            return oldTokens.accessToken;
        }
    }
}