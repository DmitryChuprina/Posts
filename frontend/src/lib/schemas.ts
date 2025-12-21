import z from "zod";
import { ActionState } from "./action-utils";
import { checkEmailAvailability, checkUsernameAvailability } from "./actions/users";

const EMAIL_REGEX = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
const USERNAME_REGEX = /^[A-Za-z0-9_]{3,24}$/;

const MAX_LENGTH = 255;

const DEFAULT_DEBOUCE_DELAY = 500;

const createDebouncedCheck = <TVal>(
    checkFn: (val: TVal) => Promise<ActionState<boolean>>,
    delay: number = DEFAULT_DEBOUCE_DELAY,
    syncSchema?: { safeParse: (val: TVal) => { success: boolean } }
) => {
    let timer: NodeJS.Timeout;
    let prevResolve: ((val: boolean) => void) | null = null;

    let lastVal: TVal | undefined;
    let lastResult: boolean | undefined;

    return async (val: TVal): Promise<boolean> => {
        if (!!syncSchema && !syncSchema.safeParse(val).success) {
            return true;
        }

        if (val === lastVal && lastResult !== undefined) {
            return lastResult;
        }

        if (prevResolve) {
            clearTimeout(timer);
            prevResolve(true);
        }

        return new Promise<boolean>((resolve) => {
            prevResolve = resolve;

            timer = setTimeout(async () => {
                try {
                    const resp = await checkFn(val);

                    lastVal = val;
                    lastResult = resp.data;

                    resolve(lastResult as boolean);
                } catch (error) {
                    console.error("Availability check failed", error);
                    resolve(false);
                } finally {
                    prevResolve = null;
                }
            }, delay);
        });
    };
};

export const emailSchema = z.string()
    .max(MAX_LENGTH, `Enter a maximum of ${MAX_LENGTH} characters`)
    .min(3, "Enter at least 3 characters")
    .regex(EMAIL_REGEX, "Invalid email")

export const emailWithAviabilitySchema = (delay: number = DEFAULT_DEBOUCE_DELAY) => {
    const getAvaialability = createDebouncedCheck(checkEmailAvailability, delay, emailSchema)
    return emailSchema.refine(getAvaialability, "This email alredy used");
}

export const usernameSchema = z.string()
    .max(MAX_LENGTH, `Enter a maximum of ${MAX_LENGTH} characters`)
    .min(3, "Enter at least 3 characters")
    .regex(USERNAME_REGEX, "Invalid username");

export const usernameWithAviabilitySchema = (delay: number = DEFAULT_DEBOUCE_DELAY) => {
    const getAvaialability = createDebouncedCheck(checkUsernameAvailability, delay, usernameSchema)
    return usernameSchema.refine(getAvaialability, "This username alredy used");
}

export const emailOrUsernameSchema = z.string()
    .max(MAX_LENGTH, `Enter a maximum of ${MAX_LENGTH} characters`)
    .min(3, "Enter at least 3 characters")
    .refine(
        (val) => EMAIL_REGEX.test(val) || USERNAME_REGEX.test(val),
        "Invalid email or username"
    );

export const partOfNameSchema = z.string()
    .max(MAX_LENGTH, `Enter a maximum of ${MAX_LENGTH} characters`)
    .min(3, "Enter at least 3 characters")
    .regex(/^\p{L}+$/u, "Must contain only letters");

export const passwordSchema = z.string()
    .min(8, "Password must be at least 8 characters")
    .regex(/[A-Z]/, "Must contain at least one uppercase letter")
    .regex(/[a-z]/, "Must contain at least one lowercase letter")
    .regex(/[0-9]/, "Must contain at least one number")
    .regex(/[\W_]/, "Must contain at least one special character");