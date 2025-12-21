import { AuthTokensDto, AuthUserDto } from "../dtos/auth.dtos";
import { ICookieStorage } from "./cookie-store";

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface ISesionTokens extends AuthTokensDto {}
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface ISessionUser extends AuthUserDto{}

const COOKIE_TOKENS = "tokens";
const COOKIE_USER_DATA = "user_data";

const COOKIE_OPTIONS = {
    httpOnly: true,
    secure: process.env.NODE_ENV === "production",
    sameSite: "lax",
    path: "/",
} as const;

export class SessionStore {
    constructor(
        private readonly store: ICookieStorage
    ) { }

    getTokens(): ISesionTokens | null{
        return this.get(COOKIE_TOKENS)
    }

    setTokens(tokens: ISesionTokens): void {
        this.set(COOKIE_TOKENS, tokens);
    }

    getUser(): ISessionUser | null {
        return this.get(COOKIE_USER_DATA)
    }

    setUser(user: ISessionUser): void {
        this.set(COOKIE_USER_DATA, user);
    }

    clean(): void {
        this.store.delete(COOKIE_TOKENS);
        this.store.delete(COOKIE_USER_DATA);
    }

    private set<TVal>(key: string, val: TVal) {
        if(val == null){
            throw new Error(`Cannot be set null or undefined for ${key}`)
        }

        const serialized = JSON.stringify(val);

        if (Buffer.byteLength(serialized) > 4000) {
            console.warn(`${key} object is too large for cookie!`);
        }

        this.store.set(key, serialized, COOKIE_OPTIONS);
    }

    private get<TVal>(key: string) {
        const serialized = this.store.get(key);

        if (!serialized) {
            return null;
        }

        try {
            return JSON.parse(serialized) as TVal;
        } catch (error) {
            console.error(`Failed to parse ${key} cookie`, error);
            return null;
        }
    }
}