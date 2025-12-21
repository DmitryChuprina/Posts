import { FileDto } from "@/lib/dtos/shared.dtos";
import ProfileIconSvg from "@/public/profile.svg";
import clsx from "clsx";
import UploadContainer, { UploadContainerProps } from "./UploadContainer";
import { useEffect, useState } from "react";
import AppImage from "./AppImage";

export interface ProfileIconProps extends Omit<UploadContainerProps, 'children'> {
    file: FileDto | null;
    alt?: string;
    className?: string;
}

export default function ProfileIcon({ file, alt, className, ...props }: ProfileIconProps) {
    const [previewImage, setPreviewImage] = useState<string | null>(null);

    useEffect(() => {
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setPreviewImage(null);
    }, [file]);

    const onStartUpload = (preview: string) => {
        setPreviewImage(preview)
        props.onStartUpload?.(preview)
    }

    const onUploaded = (file: FileDto) => {
        setPreviewImage(file.url);
        props.onUploaded?.(file);
    }

    const onUploadError = (error: string) => {
        setPreviewImage(null);
        props.onUploadError?.(error);
    }

    const imgSrc = previewImage || file?.url;

    return (
        <UploadContainer 
            {...props}
            onStartUpload={onStartUpload}
            onUploaded={onUploaded}
            onUploadError={onUploadError}
            className={
                clsx(
                    "overflow-hidden bg-gray-200",
                    "border-(--border) border",
                    "rounded-full",
                    className
                )
            }>
            {
                imgSrc ?
                    (
                        <AppImage
                            src={imgSrc}
                            alt={alt || "Profile"}
                            className="img size-full"
                            width={50}
                            height={50}>
                        </AppImage>
                    ) :
                    <ProfileIconSvg className="img svg p-1 size-full"></ProfileIconSvg>
            }
        </UploadContainer>
    )
}