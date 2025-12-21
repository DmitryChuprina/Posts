import { FileDto } from "./shared.dtos";

interface UserIsTakenRequestDto{
    forUserId?: string
}

export interface EmailIsTakenDto extends UserIsTakenRequestDto{
    email: string;
}

export interface UsernameIsTakenDto extends UserIsTakenRequestDto{
    username: string;
}

export interface UserProfileDto{
    id: string;
    firstName: string | null;
    lastName: string | null;
    username: string;
    description: string | null;
    profileImage: FileDto | null;
    profileBanner: FileDto | null;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UpdateUserProfileDto extends Omit<UserProfileDto, 'id'>{}

export interface UserSecurityDto{
    email: string;
    password: string;
    revokeSessions: boolean;
}