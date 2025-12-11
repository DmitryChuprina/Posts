import { apiFetch } from "./base";
import { AuthTokensDto, AuthUserDto, SignInRequestDto, SignInResponseDto, SignUpRequestDto, SignUpResponseDto } from "./dtos/auth.dtos";

export const AuthApi = {
    signIn: (dto: SignInRequestDto): Promise<SignInResponseDto> =>
        apiFetch<SignInResponseDto>("/auth/sign-in", {
            method: "POST",
            body: JSON.stringify(dto),
        }),

    signUp: (dto: SignUpRequestDto): Promise<SignUpResponseDto> =>
        apiFetch("/auth/sign-up", {
            method: "POST",
            body: JSON.stringify(dto),
        }),

    refresh: (dto: AuthTokensDto): Promise<AuthTokensDto> => 
        apiFetch("/auth/refresh-token", {
            method: "POST",
            body: JSON.stringify(dto)
        }),

    me: (): Promise<AuthUserDto> =>
        apiFetch("/auth/me"),
}