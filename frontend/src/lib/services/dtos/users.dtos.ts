interface UserIsTakenRequestDto{
    forUserId?: string
}

export interface EmailIsTakenDto extends UserIsTakenRequestDto{
    email: string;
}

export interface UsernameIsTakenDto extends UserIsTakenRequestDto{
    username: string;
}