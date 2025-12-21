import type { cookies } from "next/headers";
import { NextRequest, NextResponse } from "next/server";

export interface ICookieStorage {
    get(key: string): string | undefined;
    set(key: string, value: string, opts?: object): void;
    delete(key: string): void;
}

export class ServerCookieStorage implements ICookieStorage {
    constructor(private cookieStore: Awaited<ReturnType<typeof cookies>>) { }

    get(key: string) { return this.cookieStore.get(key)?.value; }
    set(key: string, val: string, opts?: object) { this.cookieStore.set(key, val, opts); }
    delete(key: string) { this.cookieStore.delete(key); }
}

export type MiddlewareCookieStoreOpts = {
    req: NextRequest,
    res: NextResponse
}

export class MiddlewareCookieStorage implements ICookieStorage {
    private readonly req: NextRequest;
    private readonly res: NextResponse;

    constructor(
        opts: MiddlewareCookieStoreOpts
    ) { 
        this.req = opts.req;
        this.res = opts.res;
    }

    get(key: string) {
        const setCookie = this.res.cookies.get(key);
        if (setCookie) {
            return setCookie.value;
        }
        return this.req.cookies.get(key)?.value;
    }

    set(key: string, val: string, opts?: object) {
        this.res.cookies.set(key, val, opts);
    }

    delete(key: string) {
        this.res.cookies.delete(key);
    }
}

export class ClientCookieStorage implements ICookieStorage {
    get(key: string): string | undefined {
        if (typeof document === 'undefined') {
            return undefined;
        }

        const match = document.cookie.match(new RegExp('(^| )' + key + '=([^;]+)'));
        return match ? match[2] : undefined;
    }

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    set(key: string, value: string, opts?: any): void {
        if (typeof document === 'undefined') {
            return;
        }

        let cookieString = `${key}=${value}; path=/`;

        if (opts?.maxAge) {
            cookieString += `; max-age=${opts.maxAge}`;
        }

        document.cookie = cookieString;
    }

    delete(key: string): void {
        if (typeof document === 'undefined') { 
            return;
        }
        document.cookie = `${key}=; path=/; max-age=0`;
    }
}