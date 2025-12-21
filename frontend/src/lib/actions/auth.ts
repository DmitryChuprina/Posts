"use server";

import { redirect } from "next/navigation";
import { api, getSession } from "../server-api";
import { AuthApiService } from "../services/auth-api";
import { SignInRequestDto, SignUpRequestDto } from "../dtos/auth.dtos";
import { safeAction } from "../action-utils";

async function signIn(dto: SignInRequestDto, authApi: AuthApiService) {
    const result = await authApi.signIn(dto);
    const session = await getSession();

    session.setUser(result.user);
    session.setTokens({ ...result.tokens });
    redirect('/');
}

export async function signInAction(dto: SignInRequestDto) {
    return safeAction(async () => {
        const [authApi] = await api(AuthApiService);
        await signIn(dto, authApi)
    })
}

export async function signUpAction(dto: SignUpRequestDto) {
    return safeAction(async () => {
        const [authApi] = await api(AuthApiService);
        await authApi.signUp(dto);

        try{
            await signIn(
                { 
                    emailOrUsername: dto.email, 
                    password: dto.password, 
                    rememberMe: true 
                },
                authApi
            )
        }catch(err){
            console.error(`Error when login after sign-up`, err);
            redirect('/sign-in')
        }
    });
}