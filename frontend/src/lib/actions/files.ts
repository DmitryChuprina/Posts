"use server"

import { safeAction } from "../action-utils";
import { UploadType } from "../enums/upload-type";
import { api } from "../server-api";
import { FilesApiService } from "../services/files-api";

export async function uploadFile(file: File, uploadType?: UploadType) {
    const [service] = await api(FilesApiService);
    return safeAction(async () => service.upload(file, uploadType));
}