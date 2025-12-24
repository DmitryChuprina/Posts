import { describe, it, expect, vi } from 'vitest';
import { AuthService } from './auth-service';
import { SessionStore } from '../../stores/session';
import { AuthTokensDto } from '../../dtos/auth.dtos';
import { ICookieStorage } from '../../stores/cookie-store';
import * as httpClientModule from '../../clients/http-client';

class InMemoryCookieStorage implements ICookieStorage {
    private map = new Map<string, string>();
    get(key: string) {
        return this.map.get(key) || undefined;
    }
    set(key: string, value: string) {
        this.map.set(key, value);
    }
    delete(key: string) {
        this.map.delete(key);
    }
}

const delay = (ms: number) => new Promise((r) => setTimeout(r, ms));

describe('AuthService refresh single-flight', () => {
    it('should perform only one refresh for concurrent requests of the same session', async () => {
        const storage = new InMemoryCookieStorage();
        const session = new SessionStore(storage);

        const expiredTokens: AuthTokensDto = {
            accessToken: 'old-access',
            refreshToken: 'refresh-1',
            expiresAt: new Date(Date.now() - 1000).toISOString(),
        };

        session.setTokens(expiredTokens);

        const postSpy = vi.spyOn(httpClientModule.httpClient, 'post').mockImplementation(async (url: string, body) => {
            await delay(50);
            const tokenBody = body as { refreshToken: string };
            const newTokens: AuthTokensDto = {
                accessToken: 'new-access',
                refreshToken: tokenBody['refreshToken'],
                expiresAt: new Date(Date.now() + 60_000).toISOString(),
            };
            return newTokens;
        });

        const service = new AuthService(session, true);

        const results = await Promise.all(Array.from({ length: 6 }).map(() => service.getValidAccessToken()));

        expect(results.every((r) => r === 'new-access')).toBeTruthy();
        expect(postSpy).toHaveBeenCalledTimes(1);

        postSpy.mockRestore();
    });
});