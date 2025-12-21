"use client"

import { MainLayoutHeader } from "@/(main)/_components/MainLayoutHeader";
import AppImage from "@/_components/AppImage";
import { FormApiErrorAlert } from "@/_components/forms/FormApiErrorAlert";
import { FormInput } from "@/_components/forms/FormInput";
import ProfileIcon from "@/_components/ProfileIcon";
import UploadContainer from "@/_components/UploadContainer";
import { getCurrentUserProfile, updateCurrentUserProfile } from "@/lib/actions/users";
import { handleActionCall } from "@/lib/client-api";
import { FileDto } from "@/lib/dtos/shared.dtos";
import { UpdateUserProfileDto, UserProfileDto } from "@/lib/dtos/users.dtos";
import { partOfNameSchema, usernameWithAviabilitySchema } from "@/lib/schemas";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import { FormProvider, useForm } from "react-hook-form";
import z from "zod";

export const profileSchema = z.object({
    firstName: partOfNameSchema.optional().or(z.literal('')),
    lastName: partOfNameSchema.optional().or(z.literal('')),
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
    const [isLoadingProfileImage, setIsLoadingProfileImage] = useState<boolean>(false);
    const [profileBanner, setProfileBanner] = useState<FileDto | null>(null);
    const [isLoadingProfileBanner, setIsLoadingProfileBanner] = useState<boolean>(false);
    const [profileBannerError, setProfileBannerError] = useState<string | null>(null);
    const [profileBannerPreview, setProfileBannerPreview] = useState<string | null>(null);

    const [isLoading, setIsLoading] = useState(true);
    const [loadingError, setLoadingError] = useState<string | null>(null);
    const [saveError, setSaveError] = useState<string | null>(null);

    const formDisabled = !!loadingError || isLoading;

    const setProfile = (dto: UserProfileDto | undefined) => {
        if (!dto) {
            setLoadingError("Error loading profile");
            return;
        }

        reset({
            firstName: dto.firstName || '',
            lastName: dto.lastName || '',
            username: dto.username,
            description: dto.description || ''
        });
        setProfileImage(dto.profileImage);
        setProfileBanner(dto.profileBanner);
    }

    const submit = async (data: ProfileForm) => {
        if (!isValid) {
            return;
        }

        const dto: UpdateUserProfileDto = {
            firstName: data.firstName || null,
            lastName: data.lastName || null,
            description: data.description || null,
            username: data.username,
            profileImage,
            profileBanner
        }

        handleActionCall(
            updateCurrentUserProfile(dto),
            {
                errorDefaultMessage: "Error when updating profile",
                onData: setProfile,
                onError: setSaveError,
                onIsLoading: setIsLoading
            }
        )
    }

    useEffect(() => {
        handleActionCall(
            getCurrentUserProfile(),
            {
                onData: setProfile,
                onError: setLoadingError,
                onIsLoading: setIsLoading
            }
        )
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const disabledSubmit = isLoadingProfileImage || isLoadingProfileBanner || isLoading;
    const bannerSrc = profileBannerPreview || profileBanner?.url;

    return (
        <FormProvider {...form}>
            <form onSubmit={handleSubmit(submit)}>
                <MainLayoutHeader
                    className="flex flex-row justify-end items-center h-full px-4">
                    <button
                        disabled={disabledSubmit}
                        className="btn btn-sm btn-outline"
                        type="submit">Save
                    </button>
                </MainLayoutHeader>
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
                        onIsLoadingChange={setIsLoadingProfileBanner}
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
                        onIsLoadingChange={setIsLoadingProfileImage}
                        allowUpload={true}
                        className="size-[60px] md:size-[75px] absolute! bottom-0 left-[25px] md:left-[35px] translate-y-1/2 z-100">
                    </ProfileIcon>
                </div>
                <div className="p-4 pt-2">
                    <FormApiErrorAlert message={profileImageError} setMessage={setProfileImageError} form={form} className="mb-2"></FormApiErrorAlert>
                    <FormApiErrorAlert message={profileBannerError} setMessage={setProfileBannerError} form={form} className="mb-2"></FormApiErrorAlert>
                    <FormApiErrorAlert message={saveError} setMessage={setSaveError} form={form} className="mb-2"></FormApiErrorAlert>
                    <FormApiErrorAlert message={loadingError} className="mb-2"></FormApiErrorAlert>
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
            </form>
        </FormProvider>
    )
}