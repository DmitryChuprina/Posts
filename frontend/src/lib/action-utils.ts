

import { isRedirectError } from "next/dist/client/components/redirect-error";
import { ApiError } from "./clients/http-client";

export type ActionState<T = null> = {
    success: boolean;
    data?: T;
    error?: string | null;
    errorDetails?: object;
    errorType?: string;
};

export async function safeAction<T>(
    actionFn: () => Promise<T>
): Promise<ActionState<T>> {
    try {
        const data = await actionFn();
        
        return {
            success: true,
            data: data,
            error: null
        };
    } catch (error) {
        if (isRedirectError(error)) {
            throw error;
        }

        if (error instanceof ApiError) {
            return {
                success: false,
                error: error.message,
                errorDetails: error.details,
                errorType: error.type
            };
        }

        console.error("Server Action Error:", error);
        return {
            success: false,
            error: "An unexpected server error occurred."
        };
    }
}