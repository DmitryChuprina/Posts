"use client"

import { AuthInput } from "@/components/auth/forms/AuthInput";
import { AuthCheckbox } from "@/components/auth/forms/AuthCheckbox";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation"
import { useState } from "react";
import { FormProvider, useForm } from "react-hook-form";
import { z } from "zod";
import { AuthApi } from "@/shared/api/auth";
import { ApiError } from "next/dist/server/api-utils";
import Link from "next/link";

export const signInSchema = z.object({
    emailOrUsername: z
        .string()
        .min(3, 'Enter at least 3 characters'),
    password: z
        .string()
        .min(6, 'Password must be at least 6 characters'),
    rememberMe: z.boolean(),
});

type SignInForm = z.infer<typeof signInSchema>;

export default function SignInPage() {
    const router = useRouter();
    const form = useForm<SignInForm>({
        resolver: zodResolver(signInSchema),
        mode: "all"
    });
    const { handleSubmit, formState: { isValid } } = form;

    const [loading, setLoading] = useState(false);
    const [apiError, setApiError] = useState<string | null>(null);

    const disableSubmit = loading || !isValid;

    const submit = (data: SignInForm) => {
        AuthApi.signIn(data)
            .then(async (res) => {
                return res;
            })
            .then((res) => {
                router.push("/");
            })
            .catch((error: ApiError) => {
                setApiError(error.message);
            })
            .finally(() => {
                setLoading(false);
            });
    };

    return (
        <FormProvider {...form}>
            <form className="flex flex-col" onSubmit={handleSubmit(submit)}>
                <h2 className="text-heading mb-2">Log in to your account</h2>
                <fieldset disabled={loading}>
                    <div className="flex flex-col gap-2">
                        <AuthInput
                            type="text"
                            placeholder="Enter email or username"
                            label="Email or username"
                            name="emailOrUsername"
                            required={true}>
                        </AuthInput>
                        <AuthInput
                            type="password"
                            placeholder="Enter password"
                            label="Password"
                            name="password"
                            required={true}>
                        </AuthInput>
                    </div>
                    <div className="my-2.5 ps-0.5 pe-1 w-full flex flex-row">
                        <AuthCheckbox name="rememberMe" label="Remember me"></AuthCheckbox>
                        <Link className="text-muted text-caption ms-auto" href="/">Forgot password?</Link>
                    </div>
                </fieldset>
                <div className="flex flex-col gap-2">
                    <button className="btn btn-primary" type="submit" disabled={disableSubmit}>
                        {loading ? "Loading..." : "Sign in"}
                    </button>
                    <p className="text-text text-caption text-center">
                        Don&apos;t have an account?&nbsp;
                        <Link className="underline underline-offset-2" href="/">Register</Link>
                    </p>
                    <Link className="text-muted text-caption text-center" href="/">Back to home</Link>
                </div>
            </form>
        </FormProvider>
    )
}