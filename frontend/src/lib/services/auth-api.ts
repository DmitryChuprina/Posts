import { BaseApiService } from "./core/base-api-service";
import { AuthUserDto, SignInRequestDto, SignInResponseDto, SignUpRequestDto, SignUpResponseDto } from "./dtos/auth.dtos";

export class AuthApiService extends BaseApiService{
    public signIn(dto: SignInRequestDto): Promise<SignInResponseDto>{
        return this.client.post(`/auth/sign-in`, dto);
    }

    public signUp(dto: SignUpRequestDto): Promise<SignUpResponseDto>{
        return this.client.post('/auth/sign-up', dto);
    }

    public me(): Promise<AuthUserDto>{
        return this.client.get('/me');
    }
}