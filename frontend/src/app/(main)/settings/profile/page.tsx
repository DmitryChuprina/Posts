"use client"

import AppImage from "@/_components/AppImage";
import { FormApiErrorAlert } from "@/_components/forms/FormApiErrorAlert";
import { FormInput } from "@/_components/forms/FormInput";
import ProfileIcon from "@/_components/ProfileIcon";
import UploadContainer from "@/_components/UploadContainer";
import { getCurrentUserProfile } from "@/lib/actions/users";
import { FileDto } from "@/lib/dtos/shared.dtos";
import { partOfNameSchema, usernameWithAviabilitySchema } from "@/lib/schemas";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import { FormProvider, useForm } from "react-hook-form";
import z from "zod";

export const profileSchema = z.object({
    firstName: partOfNameSchema.optional(),
    lastName: partOfNameSchema.optional(),
    username: usernameWithAviabilitySchema(),
    description: z.string()
});

type ProfileForm = z.infer<typeof profileSchema>;

export default function SettingsProfile() {
    const form = useForm<ProfileForm>({
        resolver: zodResolver(profileSchema),
        mode: "all"
    });
    const { handleSubmit, formState: { isValid }, reset } = form;

    const [profileImage, setProfileImage] = useState<FileDto | null>(null);
    const [profileImageError, setProfileImageError] = useState<string | null>(null);
    const [profileBanner, setProfileBanner] = useState<FileDto | null>(null);
    const [profileBannerError, setProfileBannerError] = useState<string | null>(null);
    const [profileBannerPreview, setProfileBannerPreview] = useState<string | null>(null);

    const [isLoading, setIsLoading] = useState(true);
    const [loadingError, setLoadingError] = useState<string | null>(null);
    const [saveError, setSaveError] = useState<string | null>(null);

    const formDisabled = !!loadingError || isLoading;

    useEffect(() => {
        let isMounted = true;

        const fetchData = async () => {
            setIsLoading(true);
            setLoadingError(null);

            try {
                const { data, error } = await getCurrentUserProfile();

                if (!isMounted) {
                    return;
                }

                if (error) {
                    setLoadingError(error);
                    return;
                }

                if (data) {
                    reset(data);

                    setProfileImage(data.profileImage);
                    setProfileBanner(data.profileBanner);
                }
            } catch {
                if (isMounted) {
                    setLoadingError("Network error occurred");
                }
            } finally {
                if (isMounted) {
                    setIsLoading(false);
                }
            }
        };
        fetchData();
        return () => { isMounted = false };
    }, [reset]);

    const bannerSrc = profileBannerPreview || profileBanner?.url;

    return (
        <div>
            <div className="w-full relative mb-8">
                <UploadContainer 
                    allowUpload={true}
                    onUploaded={(file) => {
                        setProfileBanner(file)
                        setProfileBannerPreview(file.url)
                    }}
                    onStartUpload={setProfileBannerPreview}
                    onUploadError={(err) => {
                        setProfileBannerPreview(null);
                        setProfileBannerError(err)
                    }}
                    className="w-full h-[135px] md:h-[180px] bg-gray-300">
                    {
                        bannerSrc && 
                            <AppImage 
                                className="size-full"
                                alt="Banner"
                                fill
                                src={bannerSrc}>
                            </AppImage>
                    }
                </UploadContainer>
                <ProfileIcon 
                    file={profileImage}
                    onUploaded={setProfileImage}
                    onUploadError={setProfileImageError}
                    allowUpload={true} 
                    className="size-[60px] md:size-[75px] absolute! bottom-0 left-[25px] md:left-[35px] translate-y-1/2 z-100">
                </ProfileIcon>
            </div>
            <FormProvider {...form}>
                <div className="p-4 pt-2">
                    <FormApiErrorAlert message={profileImageError} className="mb-2"></FormApiErrorAlert>
                    <FormApiErrorAlert message={profileBannerError} className="mb-2"></FormApiErrorAlert>
                    <FormApiErrorAlert message={loadingError} className="mb-2"></FormApiErrorAlert>
                    <FormApiErrorAlert message={saveError} className="mb-2"></FormApiErrorAlert>
                    <fieldset className="flex flex-col gap-2 pt-2" disabled={formDisabled}>
                        <FormInput
                            type="text"
                            placeholder="Enter username"
                            label="Username"
                            name="username"
                            required={true}>
                        </FormInput>
                        <div className="flex flex-col md:flex-row gap-2">
                            <FormInput
                                type="text"
                                placeholder="Enter first name"
                                label="First name"
                                name="firstName">
                            </FormInput>
                            <FormInput
                                type="text"
                                placeholder="Enter last name"
                                label="Last name"
                                name="lastName">
                            </FormInput>
                        </div>
                        <FormInput
                            type="text"
                            placeholder="Enter description"
                            label="Description"
                            name="description"
                            className="min-h-[115px] resize-none"
                            textarea={true}
                        ></FormInput>
                    </fieldset>
                </div>
            </FormProvider>
        </div>
    )
}