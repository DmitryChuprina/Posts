import { AuthHttpClient } from "../../clients/auth-http-client";

export abstract class BaseApiService {
    constructor(
        protected readonly client: AuthHttpClient
    ) {}
}