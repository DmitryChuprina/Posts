"use server"

import { ActionState, safeAction } from "../action-utils"
import { api, getSession } from "../server-api";
import { IsTakenDto } from "../dtos/shared.dtos"
import { UsersApiService } from "../services/users-api";
import { UpdateUserProfileDto, UpdateUserSecurityDto, UserSecurityDto } from "../dtos/users.dtos";
import { AuthApiService } from "../services/auth-api";
import { ISessionUser } from "../stores/session";

async function checkAvailability(cb: (api: UsersApiService) => Promise<IsTakenDto>) {
    const [service] = await api(UsersApiService);
    return safeAction(async () => {
        const resp = await cb(service);
        return !resp.isTaken;
    })
}

async function updateSessionData() {
    const [authService] = await api(AuthApiService);
    const updatedUser = await safeAction(async () => authService.me());
    if (updatedUser.success) {
        const session = await getSession();
        session.setUser(updatedUser.data as ISessionUser)
    }
}

export async function checkEmailAvailability(email: string, forUserId?: string): Promise<ActionState<boolean>> {
    return checkAvailability((s) => s.emailIsTaken({ email, forUserId }))
}

export async function checkUsernameAvailability(username: string, forUserId?: string): Promise<ActionState<boolean>> {
    return checkAvailability((s) => s.usernameIsTaken({ username, forUserId }))
}

export async function getCurrentUserProfile() {
    const [service] = await api(UsersApiService);
    return safeAction(async () => service.getCurrentUserProfile());
}

export async function updateCurrentUserProfile(dto: UpdateUserProfileDto) {
    const [service] = await api(UsersApiService);
    const result = await safeAction(async () => service.updateCurrentUserProfile(dto));

    if (result.success) {
        await updateSessionData();
    }

    return result;
}

export async function getCurrentUserSecurity() {
    const [service] = await api(UsersApiService);
    return safeAction(async () => service.getCurrentUserSecurity());
}

export async function updateCurrentUserSecurity(dto: UpdateUserSecurityDto) {
    const [service] = await api(UsersApiService);
    const result = await safeAction(async () => service.updateUserSecurity(dto));

    if (result.success) {
        await updateSessionData();
    }

    return result;
}