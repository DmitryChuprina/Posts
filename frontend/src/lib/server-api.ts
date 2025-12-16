import { cookies } from "next/headers";
import { cache } from "react";
import { ServerCookieStorage } from "./stores/cookie-store";
import { SessionStore } from "./stores/session";
import { AuthService, IRequestLock } from "./services/core/auth-service";
import { AuthHttpClient } from "./clients/auth-http-client";
import { BaseApiService } from "./services/core/base-api-service";

type ApiServiceConstructor<T extends BaseApiService = BaseApiService> = new (client: AuthHttpClient) => T;
type ApiServicesMap = Map<ApiServiceConstructor, BaseApiService>;
type ServicesResult<T extends ApiServiceConstructor[]> = { [K in keyof T]: T[K] extends ApiServiceConstructor<infer I> ? I : never; };

const getGlobalLock = cache((): IRequestLock => ({ promise: null }));

export const getSession = cache(async () => {
    const cookieStore = await cookies();
    const storage = new ServerCookieStorage(cookieStore);
    const session = new SessionStore(storage);
    return session;
})

const getContainer = cache(async () => {
    const session = await getSession();

    const authService = new AuthService(session, getGlobalLock());
    const httpClient = new AuthHttpClient(authService);
    const servicesMap: ApiServicesMap = new Map();

    return {
        httpClient,
        servicesMap
    }
});

const getServiceInstance = <T extends BaseApiService>(
    container: Awaited<ReturnType<typeof getContainer>>,
    serviceType: ApiServiceConstructor<T>
) => {
    const { servicesMap: map, httpClient: client } = container;

    if (map.has(serviceType)) {
        return map.get(serviceType) as T
    }

    const instance: T = new serviceType(client);
    map.set(serviceType, instance);
    return instance;
}

export async function api<T extends ApiServiceConstructor[]>(...services: T): Promise<ServicesResult<T>> {
    const container = await getContainer();
    return services
        .map(ServiceClass => getServiceInstance(container, ServiceClass)) as ServicesResult<T>;
}