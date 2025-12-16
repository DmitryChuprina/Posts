import { AuthService } from "../services/core/auth-service";
import { HttpClient, ROpts } from "./http-client";

export class AuthHttpClient extends HttpClient {
    constructor(private authService: AuthService) {
        super();
    }

    async request<T>(endpoint: string, config: RequestInit & ROpts): Promise<T> {
        const accessToken = await this.authService.getValidAccessToken();

        const headers = {
            ...config.headers,
        } as Record<string, string>;

        if (accessToken) {
            headers['Authorization'] = `Bearer ${accessToken}`;
        }

        return super.request<T>(endpoint, { ...config, headers });
    }
}