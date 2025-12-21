import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';
import { MiddlewareCookieStorage } from '@/lib/stores/cookie-store';
import { SessionStore } from '@/lib/stores/session';
import { AuthService } from '@/lib/services/core/auth-service';

const ONLY_PUBLIC_PATHS: string[] = ['/sign-in', '/sign-up'];
const ONLY_PRIVATE_PATHS: string[] = []

export async function middleware(req: NextRequest) {
    const { pathname } = req.nextUrl;

    if (
        pathname.startsWith('/_next') ||
        pathname.startsWith('/api') ||
        pathname.startsWith('/static') ||
        pathname.includes('.') 
    ) {
        return NextResponse.next();
    }

    const res = NextResponse.next();
    const storage = new MiddlewareCookieStorage({ req, res });
    const session = new SessionStore(storage);
    const authService = new AuthService(session, undefined, true);

    const accessToken = await authService.getValidAccessToken();

    const isOnlyPublicPath = ONLY_PUBLIC_PATHS.some(path => pathname.startsWith(path));
    const isOnlyPrivatePath = ONLY_PRIVATE_PATHS.some(path => pathname.startsWith(path));

    if (!accessToken && isOnlyPrivatePath) {
        const url = req.nextUrl.clone();
        url.pathname = '/sign-in';
        url.searchParams.set('from', pathname);
        return NextResponse.redirect(url);
    }

    if (accessToken && isOnlyPublicPath) {
        return NextResponse.redirect(new URL('/', req.url));
    }

    return res;
}

export const config = {
    matcher: [
        '/((?!api|_next/static|_next/image|favicon.ico).*)',
    ],
};