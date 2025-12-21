
import { ActionState } from "./action-utils";
import { ClientCookieStorage } from "./stores/cookie-store";

export const clientStorage = new ClientCookieStorage();

export function mapActionError(err: unknown, defaultMessage?: string): string {
    if (typeof err === 'string') {
        return err;
    }
    if (typeof err === 'object' && !!err) {
        const errObj = err as Record<string, unknown>;
        const errMessage = errObj['message'];
        if (errMessage) {
            return String(errMessage);
        }
    }
    return defaultMessage || 'Unknown error';
}

type HandleActionCallOpts<TData> = {
    errorDefaultMessage?: string

    onIsLoading?: (val: boolean) => void
    onData?: (val?: TData) => void
    onError?: (val: string) => void
    onFinally?: () => void
}

export async function handleActionCall<TData>(
    action: Promise<ActionState<TData>>,
    opts?: HandleActionCallOpts<TData>
): Promise<ActionState<TData> | null> {
    opts?.onIsLoading?.(true);
    try {
        const response = await action;
        if (!response.error) {
            opts?.onData?.(response.data)
        }
        if (response.error) {
            opts?.onError?.(mapActionError(response.error, opts?.errorDefaultMessage));
        }
        return response;
    } catch (err) {
        const errorMessage = mapActionError(err);
        opts?.onError?.(errorMessage);
        return { error: errorMessage, data: undefined, success: false };
    } finally {
        opts?.onIsLoading?.(false);
        opts?.onFinally?.();
    }
}