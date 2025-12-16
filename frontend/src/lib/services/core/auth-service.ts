import { ISesionTokens, SessionStore } from "../../stores/session";
import { httpClient } from "../../clients/http-client";
import { redirect } from "next/navigation";

export interface IRequestLock {
    promise: Promise<void> | null;
}

export class AuthService {
    constructor(
        private readonly session: SessionStore,
        private readonly lock?: IRequestLock 
    ) { }

    async getValidAccessToken(): Promise<string | null> {
        if (this.lock?.promise) {
            await this.lock.promise;
        }

        const tokens = this.session.getTokens();
        if (!tokens) {
            return null;
        }

        const nowWithBuffer = new Date(Date.now() + 10000);
        if (new Date(tokens.expiredAt) > nowWithBuffer) {
            return tokens.accessToken;
        }

        return this.performRefresh(tokens);
    }

    private async performRefresh(oldTokens: ISesionTokens): Promise<string | null> {
        if (!this.lock) {
            return this.makeRefreshRequest(oldTokens);
        }

        if (!this.lock.promise) {
            this.lock.promise = this.makeRefreshRequest(oldTokens)
                .then(() => {})
                .finally(() => { 
                    if(this.lock){
                        this.lock.promise = null
                    }
                });
        }

        try {
            await this.lock.promise;
            return this.session.getTokens()?.accessToken || null;
        } catch {
             return null; 
        }
    }

    private async makeRefreshRequest(oldTokens: ISesionTokens): Promise<string> {
        try {
            const newTokens = await httpClient.post<ISesionTokens>('/auth/refresh-token', oldTokens);
            
            if(!newTokens) {
                throw new Error("Empty tokens");
            }

            this.session.setTokens(newTokens);
            return newTokens.accessToken;
        } catch(err) {
            console.error(`Error when refreshing tokens: `, err);

            this.session.clean();
            redirect('/sign-in');
        }
    }
}