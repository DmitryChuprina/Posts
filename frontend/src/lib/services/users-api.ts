import { BaseApiService } from "./core/base-api-service";
import { IsTakenDto } from "./dtos/shared.dtos";
import { EmailIsTakenDto, UsernameIsTakenDto } from "./dtos/users.dtos";

export class UsersApiService extends BaseApiService{
    emailIsTaken(dto: EmailIsTakenDto): Promise<IsTakenDto>{
        return this.client.get<IsTakenDto>('/users/is-taken/email', { params: dto })
    }

    usernameIsTaken(dto: UsernameIsTakenDto): Promise<IsTakenDto>{
        return this.client.get<IsTakenDto>('/users/is-taken/username', { params: dto })
    }
}