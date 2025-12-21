import qs from "qs";

export class ApiError extends Error {
    readonly status: number;
    readonly type: string;
    readonly details?: object;

    constructor(status: number, message: string, type: string, details?: object) {
        super(message);
        this.status = status;
        this.details = details;
        this.type = type;
    }
}

export type RBody = FormData | object | null | undefined;
export type ROpts = {
    headers?: Record<string, string>
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    params?: Record<string, any>
}
export type RConf = Omit<RequestInit, 'body'> & ROpts;

const BASE_URL = process.env.NEXT_PUBLIC_API_URL;

export class HttpClient {
    async request<T>(endpoint: string, config: RequestInit & ROpts): Promise<T> {
        const headers = {
            ...config.headers,
            "Content-Type": "application/json",
        } as Record<string, string>;

        if(config.body instanceof FormData){
            delete headers["Content-Type"];
        }

        const url = `${BASE_URL}${endpoint}`;
        const params = config.params ? `?${qs.stringify(config.params)}` : '';
        const input = url + params;
        const init = {
            ...config,
            headers,
        }

        const response = await fetch(input, init);

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new ApiError(
                response.status ?? 0,
                errorData.message || `API Error: ${response.statusText}`,
                errorData.type,
                errorData.details
            );
        }

        if (response.status === 204) {
            return null as T;
        }

        return response.json() as T;
    }

    async get<T>(url: string, config?: RConf) {
        return this.request<T>(url, { ...config, method: "GET" });
    }

    async post<T>(url: string, body: RBody, config?: RConf) {
        return this.request<T>(url, { ...config, method: "POST", body: this.transformBody(body) });
    }

    async put<T>(url: string, body: RBody, config?: RConf) {
        return this.request<T>(url, { ...config, method: "PUT", body: this.transformBody(body) });
    }

    async delete<T>(url: string, config?: RConf) {
        return this.request<T>(url, { ...config, method: "DELETE" });
    }

    protected transformBody(body: RBody) {
        if (body == null) {
            return body;
        }
        if (body instanceof FormData) {
            return body;
        }
        return JSON.stringify(body);
    }
}

export const httpClient = new HttpClient();