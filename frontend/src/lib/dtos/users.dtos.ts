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
    firstName: string;
    lastName: string;
    username: string;
    description: string;
    profileImage: FileDto;
    profileBanner: FileDto;
}

export interface UserSecurityDto{
    email: string;
    password: string;
    revokeSessions: boolean;
}