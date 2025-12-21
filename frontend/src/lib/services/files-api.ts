import { FileDto } from "../dtos/shared.dtos";
import { UploadType } from "../enums/upload-type";
import { BaseApiService } from "./core/base-api-service";

export class FilesApiService extends BaseApiService{
    public upload(file: File, uploadType?: UploadType){
        const form = new FormData();
        form.append("file", file);
        return this.client.post<FileDto>("/files/upload", form, { params: { uploadType } })
    }
}