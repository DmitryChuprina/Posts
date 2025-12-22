"use client";

import { FormApiErrorAlert } from "@/_components/forms/FormApiErrorAlert";
import { FormCheckbox } from "@/_components/forms/FormCheckbox";
import { FormInput } from "@/_components/forms/FormInput";
import { getCurrentUserSecurity, updateCurrentUserSecurity } from "@/lib/actions/users";
import { handleActionCall } from "@/lib/client-api";
import { UpdateUserSecurityDto, UserSecurityDto } from "@/lib/dtos/users.dtos";
import { emailWithAviabilitySchema, passwordSchema } from "@/lib/schemas";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import { FormProvider, useForm } from "react-hook-form";
import z, { set } from "zod";

export const securitySchema = z.object({
    email: emailWithAviabilitySchema(),
    password: passwordSchema.optional().or(z.literal('')),
    repeatPassword: z.string().optional().or(z.literal('')),
    revokeSessions: z.boolean()
}).refine(val => {
        if(!val.password?.trim?.()){
            return true;
        }
        return val.password === val.repeatPassword;
    },
    { message: "Passwords is not same", path: ["repeatPassword"] }
)

type SecurityForm = z.infer<typeof securitySchema>;

export default function SettingsSecurity() {
    const form = useForm<SecurityForm>({
        resolver: zodResolver(securitySchema),
        mode: "all"
    });
    const { handleSubmit, formState: { isValid }, reset } = form;

    const [isLoading, setIsLoading] = useState(true);
    const [loadingError, setLoadingError] = useState<string | null>(null);
    const [saveError, setSaveError] = useState<string | null>(null);

    const setSecurity = (dto: UserSecurityDto | undefined) => {
        if (!dto) {
            setLoadingError("Error loading security settings");
            return;
        }

        reset({
            email: dto.email,
            revokeSessions: false,
            password: '',
            repeatPassword: ''
        });
    }

    const submit = (data: SecurityForm) => {
        if (!isValid) {
            return;
        }

        const dto: UpdateUserSecurityDto = {
            email: data.email,
            password: data.password || null,
            revokeSessions: data.revokeSessions
        }

        handleActionCall(
            updateCurrentUserSecurity(dto),
            {
                errorDefaultMessage: "Error when updating security settings",
                onData: data => {
                    setSecurity(data);
                    setSaveError(null);
                },
                onError: setSaveError,
                onIsLoading: setIsLoading
            }
        )
    }

    useEffect(() => {
        handleActionCall(
            getCurrentUserSecurity(),
            {
                onData: setSecurity,
                onError: setLoadingError,
                onIsLoading: setIsLoading
            }
        )
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const submitDisabled = isLoading || !!loadingError;
    const formDisabled = isLoading || !!loadingError;

    return (
        <div className="flex flex-col">
            <div className="p-4">
                <h2 className="text-subtitle">Main security settings</h2>
                <FormApiErrorAlert message={saveError} setMessage={setSaveError} form={form} className="mb-2"></FormApiErrorAlert>
                <FormApiErrorAlert message={loadingError} className="mb-2"></FormApiErrorAlert>
                <FormProvider {...form}>
                    <form onSubmit={handleSubmit(submit)}>
                        <fieldset className="flex flex-col gap-2" disabled={formDisabled}>
                            <FormInput
                                type="text"
                                placeholder="Enter email"
                                label="Email"
                                name="email"
                                required={true}>
                            </FormInput>
                            <div className="flex flex-row gap-2">
                                <FormInput
                                    type="password"
                                    placeholder="Enter password"
                                    label="Password"
                                    name="password">
                                </FormInput>
                                <FormInput
                                    type="password"
                                    placeholder="Repeat password"
                                    label="Repeat password"
                                    name="repeatPassword">
                                </FormInput>
                            </div>
                            <div className="mt-2 flex flex-row gap-2">
                                <FormCheckbox
                                    name="revokeSessions"
                                    label="Revoke existed sessions">
                                </FormCheckbox>
                                <button
                                    disabled={submitDisabled}
                                    className="btn btn-sm btn-outline ml-auto"
                                    type="submit">Save
                                </button>
                            </div>
                        </fieldset>
                    </form>
                </FormProvider>
            </div>
            <div className="border-t border-border w-full"></div>

        </div>
    )
}