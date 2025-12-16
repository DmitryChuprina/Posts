"use server"

import { ActionState, safeAction } from "../action-utils"
import { api } from "../server-api";
import { IsTakenDto } from "../services/dtos/shared.dtos"
import { UsersApiService } from "../services/users-api";

async function checkAvailability(cb: (api: UsersApiService) => Promise<IsTakenDto>) {
    return safeAction(async () => {
        const [service] = await api(UsersApiService);
        const resp = await cb(service);
        return !resp.isTaken;
    })
}

export async function checkEmailAvailability(email: string, forUserId?: string): Promise<ActionState<boolean>> {
    return checkAvailability((s) => s.emailIsTaken({ email, forUserId }))
}

export async function checkUsernameAvailability(username: string, forUserId?: string): Promise<ActionState<boolean>> {
    return checkAvailability((s) => s.usernameIsTaken({ username, forUserId }))
}