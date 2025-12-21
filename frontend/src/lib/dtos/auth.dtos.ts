import { FileDto } from "./shared.dtos";

export interface AuthUserDto {
  id: string;
  firstName: string;
  lastName: string;
  username: string;
  email: string;
  role: string;

  profileImage: FileDto;
}

export interface AuthTokensDto {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface SignInRequestDto {
  emailOrUsername: string;
  password: string;
  rememberMe: boolean;
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