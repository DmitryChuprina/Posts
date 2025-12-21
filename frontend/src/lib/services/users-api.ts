import { BaseApiService } from "./core/base-api-service";
import { IsTakenDto } from "../dtos/shared.dtos";
import { EmailIsTakenDto, UsernameIsTakenDto, UserProfileDto, UserSecurityDto } from "../dtos/users.dtos";

export class UsersApiService extends BaseApiService {
    emailIsTaken(dto: EmailIsTakenDto): Promise<IsTakenDto> {
        return this.client.get<IsTakenDto>('/users/is-taken/email', { params: dto })
    }

    usernameIsTaken(dto: UsernameIsTakenDto): Promise<IsTakenDto> {
        return this.client.get<IsTakenDto>('/users/is-taken/username', { params: dto })
    }

    getCurrentUserProfile() {
        return this.client.get<UserProfileDto>('/users/profile')
    }

    updateCurrentUserProfile(dto: UserProfileDto) {
        return this.client.put<UserProfileDto>('/users/profile', dto)
    }

    getCurrentUserSecurity() {
        return this.client.get<UserSecurityDto>('/users/security')
    }

    getUpdateUserSecurity(dto: UserSecurityDto) {
        return this.client.put<UserSecurityDto>('/users/security', dto)
    }
}