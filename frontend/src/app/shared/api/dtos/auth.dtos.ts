export interface AuthUserDto {
  id: string;
  username: string;
  email: string;
  role: string;
}

export interface AuthTokensDto {
  accessToken: string;
  refreshToken: string;
}

export interface SignInRequestDto {
  emailOrUsername: string;
  password: string;
}

export interface SignInResponseDto{
    user: AuthUserDto;
    tokens: AuthTokensDto;
}

export interface SignUpRequestDto{
    email: string;
    username: string;
    password: string;
}

export interface SignUpResponseDto{
    user: AuthUserDto;
}