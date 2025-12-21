"use client";

import { FileDto } from "@/lib/dtos/shared.dtos";
import clsx from "clsx";

import { ChangeEvent, MouseEvent, useEffect, useRef, useState } from "react";
import { UploadType } from "@/lib/enums/upload-type";
import { uploadFile } from "@/lib/actions/files";

import Camera from "@/public/camera.svg";
import Spinner from "./Spinner";

export interface UploadContainerProps {
    children: React.ReactNode,
    disabled?: boolean;
    uploadAccept?: string;
    allowUpload?: boolean;
    uploadType?: UploadType;
    onStartUpload?: (file: string) => void;
    onUploaded?: (file: FileDto) => void;
    onUploadError?: (error: string) => void;
    onIsLoadingChange?: (isLoading: boolean) => void;
    className?: string
}

export default function UploadContainer(
    {
        disabled,
        allowUpload,
        uploadAccept,
        uploadType,
        onUploaded,
        onStartUpload,
        onUploadError,
        onIsLoadingChange,
        className,
        children
    }: UploadContainerProps
) {
    const [isLoading, setIsLoading] = useState(false);
    const inputRef = useRef<HTMLInputElement>(null);
    const objectUrlRef = useRef<string | null>(null);

    const handleContainerClick = (ev: MouseEvent) => {
        if (!disabled && !isLoading && allowUpload && inputRef.current) {
            ev.stopPropagation();
            inputRef.current.click();
        }
    };

    useEffect(() => {
        return () => {
            if (objectUrlRef.current) {
                URL.revokeObjectURL(objectUrlRef.current);
            }
        };
    }, []);

    const handleFileChange = async (e: ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) {
            return;
        }

        if (objectUrlRef.current) {
            URL.revokeObjectURL(objectUrlRef.current);
        }

        const previewUrl = URL.createObjectURL(file);
        objectUrlRef.current = previewUrl;
        onStartUpload?.(previewUrl);
        setIsLoading(true);
        onIsLoadingChange?.(true);

        try {
            const response = await uploadFile(file, uploadType)

            if (response.error) {
                throw new Error(response.error);
            }

            if (!response.data) {
                throw new Error();
            }

            console.log("Uploaded", response.data)
            onUploaded?.(response.data);
        } catch (error) {
            console.error("Upload failed", error);
            let errorMessage = "Failed to upload image";
            if (error instanceof Error && error.message) {
                errorMessage = error.message;
            }
            onUploadError?.(errorMessage);
        } finally {
            setIsLoading(false);
            onIsLoadingChange?.(false);
            if (inputRef.current) {
                inputRef.current.value = '';
            }
        }
    };

    return (
        <div
            onClick={handleContainerClick}
            className={
                clsx(
                    "relative overflow-hidden",
                    allowUpload && !disabled && "cursor-pointer group",
                    className
                )
            }>
            <input
                type="file"
                ref={inputRef}
                className="hidden"
                accept={uploadAccept || "image/png, image/jpeg, image/webp"}
                onChange={handleFileChange}
            />

            {
                allowUpload && (
                    <div className="absolute inset-0 z-10 flex items-center justify-center size-full">
                        <div className="flex items-center justify-center max-w-20 max-h-20 size-full">
                            <div className={
                                clsx(
                                    "size-[60%] rounded-full bg-primary/50 flex items-center justify-center p-[15%] backdrop-blur-[1px]",
                                    !isLoading && "group-hover:backdrop-blur-[3px] group-hover:bg-primary/70",
                                    isLoading && "backdrop-blur-[3px] bg-primary/70"
                                )
                            }>
                                {
                                    isLoading ?
                                        (<Spinner className="size-full"></Spinner>) :
                                        (<Camera className="size-full text-white/90 group-hover:text-white" />)
                                }
                            </div>
                        </div>
                    </div>
                )
            }
            {children}
        </div>
    )
}