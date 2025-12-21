"use client"

import { FormApiErrorAlert } from "@/_components/forms/FormApiErrorAlert";
import { FormInput } from "@/_components/forms/FormInput";
import { signUpAction } from "@/lib/actions/auth";
import { emailWithAviabilitySchema, passwordSchema, usernameWithAviabilitySchema } from "@/lib/schemas";
import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useState } from "react";
import { FormProvider, useForm } from "react-hook-form";
import { z } from "zod";

const signUpSchema = z
    .object({
        email: emailWithAviabilitySchema(),
        username: usernameWithAviabilitySchema(),
        password: passwordSchema,
        repeatPassword: z.string()
    })
    .refine(val => 
        !val.password || !val.repeatPassword || val.password === val.repeatPassword, 
        { message: "Passwords is not same", path: ["repeatPassword"] }
    )

type SignUpForm = z.infer<typeof signUpSchema>;

export default function SignUp() {
    const form = useForm<SignUpForm>({
        resolver: zodResolver(signUpSchema),
        mode: "all"
    });
    const { handleSubmit, formState: { isValid } } = form;

    const [loading, setLoading] = useState(false);
    const [apiError, setApiError] = useState<string | null>(null);

    const submit = async (data: SignUpForm) => {
        setLoading(true);
        const res = await signUpAction(data)
        if (res.error) {
            setApiError(res.error);
        }
        setLoading(false);
    }

    const disableSubmit = loading || !isValid;
    
    return (
        <FormProvider {...form}>
            <form className="flex flex-col" onSubmit={handleSubmit(submit)}>
                <h2 className="text-heading mb-2">Create new account</h2>
                <fieldset disabled={loading} className="mb-4">
                    <div className="flex flex-col gap-2">
                        <FormInput
                            type="email"
                            placeholder="Enter email"
                            label="Email"
                            name="email"
                            required={true}>
                        </FormInput>
                        <FormInput
                            type="text"
                            placeholder="Enter username"
                            label="Username"
                            name="username"
                            required={true}>
                        </FormInput>
                        <FormInput
                            type="password"
                            placeholder="Enter password"
                            label="Password"
                            name="password"
                            required={true}>
                        </FormInput>
                        <FormInput
                            type="password"
                            placeholder="Repeat password"
                            label="Repeat password"
                            name="repeatPassword"
                            required={true}>
                        </FormInput>
                    </div>
                </fieldset>
                <FormApiErrorAlert message={apiError} setMessage={setApiError} form={form} className="mb-2"></FormApiErrorAlert>
                <div className="flex flex-col gap-2">
                    <button className="btn btn-primary" type="submit" disabled={disableSubmit}>
                        {loading ? "Loading..." : "Sign up"}
                    </button>
                    <p className="text-text text-caption text-center">
                        Alredy has account?&nbsp;
                        <Link className="underline underline-offset-2" href="/sign-in">Sign in</Link>
                    </p>
                    <Link className="text-muted text-caption text-center" href="/">Back to home</Link>
                </div>
            </form>
        </FormProvider>
    )
}