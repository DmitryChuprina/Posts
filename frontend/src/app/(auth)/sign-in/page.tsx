"use client"

import { FormInput } from "@/_components/forms/FormInput";
import { FormCheckbox } from "@/_components/forms/FormCheckbox";
import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { FormProvider, useForm } from "react-hook-form";
import { z } from "zod";
import Link from "next/link";
import { signInAction } from "@/lib/actions/auth";
import { FormApiErrorAlert } from "@/_components/forms/FormApiErrorAlert";
import { emailOrUsernameSchema } from "@/lib/schemas";

const signInSchema = z.object({
    emailOrUsername: emailOrUsernameSchema,
    password: z.string(),
    rememberMe: z.boolean(),
});

type SignInForm = z.infer<typeof signInSchema>;

export default function SignInPage() {
    const form = useForm<SignInForm>({
        resolver: zodResolver(signInSchema),
        mode: "all"
    });
    const { handleSubmit, formState: { isValid } } = form;

    const [loading, setLoading] = useState(false);
    const [apiError, setApiError] = useState<string | null>(null);

    const disableSubmit = loading || !isValid;

    const submit = async (data: SignInForm) => {
        setLoading(true);
        const res = await signInAction(data)
        if (res.error) {
            setApiError(res.error);
        }
        setLoading(false);
    }

    return (
        <FormProvider {...form}>
            <form className="flex flex-col" onSubmit={handleSubmit(submit)}>
                <h2 className="text-heading mb-2">Log in to your account</h2>
                <fieldset disabled={loading}>
                    <div className="flex flex-col gap-2">
                        <FormInput
                            type="text"
                            placeholder="Enter email or username"
                            label="Email or username"
                            name="emailOrUsername"
                            required={true}>
                        </FormInput>
                        <FormInput
                            type="password"
                            placeholder="Enter password"
                            label="Password"
                            name="password"
                            required={true}>
                        </FormInput>
                    </div>
                    <div className="my-2.5 ps-0.5 pe-1 w-full flex flex-row">
                        <FormCheckbox name="rememberMe" label="Remember me"></FormCheckbox>
                        <Link className="text-muted text-caption ms-auto" href="/">Forgot password?</Link>
                    </div>
                </fieldset>
                <FormApiErrorAlert message={apiError} setMessage={setApiError} form={form} className="mb-2"></FormApiErrorAlert>
                <div className="flex flex-col gap-2">
                    <button className="btn btn-primary" type="submit" disabled={disableSubmit}>
                        {loading ? "Loading..." : "Sign in"}
                    </button>
                    <p className="text-text text-caption text-center">
                        Don&apos;t have an account?&nbsp;
                        <Link className="underline underline-offset-2" href="/sign-up">Register</Link>
                    </p>
                    <Link className="text-muted text-caption text-center" href="/">Back to home</Link>
                </div>
            </form>
        </FormProvider>
    )
}