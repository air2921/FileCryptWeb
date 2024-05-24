import axios from "axios";
import { BASE_URL, errorHandler } from "./Helper"

export interface UserProps {
    id: number,
    username: string,
    role: string,
    email?: string,
    last_time_password_modified: string,
    is_2fa_enabled: boolean,
    is_blocked: boolean
}

export async function getAvatar(userId: number) {
    try {
        const response = await axios.get(`api/account/avatar/${userId}`, { withCredentials: true });
        const blob = new Blob([response.data], { type: response.headers['content-type'] });
        const url = URL.createObjectURL(blob);

        return {
            success: true,
            statusCode: response.status,
            data: url,
            message: undefined
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function getUser(userId: number, own: boolean) {
    try {
        const response = await axios.get(BASE_URL + `api/core/user/${userId}?own=${own}`, { withCredentials: true });

        return {
            success: true,
            statusCode: response.status,
            data: response.data,
            message: undefined
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function getFullyUser(userId: number) {
    try {
        const response = await axios.get(BASE_URL + `api/core/user/fully/${userId}`, { withCredentials: true });

        return {
            success: true,
            statusCode: response.status,
            data: response.data,
            message: undefined
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function getRangeUsers(username: string, skip: number = 0, count: number = 25) {
    try {
        const response = await axios.get(
            BASE_URL + `api/core/user/range/${username}?skip=${skip}&count=${count}`,
            { withCredentials: true }
        );

        return {
            success: true,
            statusCode: response.status,
            data: response.data,
            message: undefined
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function deleteAccount() {
    try {
        const response = await axios.delete(BASE_URL + 'api/core/user', { withCredentials: true });

        return {
            success: true,
            statusCode: response.status,
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function updateUsername(username: string) {
    try {
        const response = await axios.post(
            BASE_URL + `api/account/username?username=${username}`,
            null,
            { withCredentials: true }
        );

        return {
            success: true,
            statusCode: response.status,
            data: null,
            message: response.data.message
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function updatePassword(oldP: string, newP: string) {
    try {
        const response = await axios.post(
            BASE_URL + 'api/account/password',
            {
                OldPassword: oldP,
                NewPassword: newP
            },
            { withCredentials: true }
        );

        return {
            success: true,
            statusCode: response.status,
            data: null,
            message: response.data.message
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function twoFaPasswordConfirm(password: string) {
    try {
        const response = await axios.post(
            BASE_URL + `api/account/2fa/send/mail?password=${password}`,
            null,
            { withCredentials: true }
        );

        return {
            success: true,
            statusCode: response.status,
            data: null,
            message: response.data.message
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function twoFaEmailVerify(code: number, enable: boolean) {
    try {
        const response = await axios.post(
            BASE_URL + `api/account/2fa/verify?code=${code}&enable=${enable}`,
            null,
            { withCredentials: true }
        );

        return {
            success: true,
            statusCode: response.status,
            data: null,
            message: response.data.message
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function veridyPasswordAndSendCode(password: string) {
    try {
        const response = await axios.post(
            BASE_URL + `api/account/email/send/current?password=${password}`,
            null,
            { withCredentials: true }
        );

        return {
            success: true,
            statusCode: response.status,
            data: null,
            message: response.data.message
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function verifyCodeAndSendCode(code: number, email: string) {
    try {
        const response = await axios.post(
            BASE_URL + `api/account/email/verify/current?email=${email}&code=${code}`,
            null,
            { withCredentials: true }
        );

        return {
            success: true,
            statusCode: response.status,
            data: null,
            message: response.data.message
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function verifyCodeAndUpdate(code: number) {
    try {
        const response = await axios.post(
            BASE_URL + `api/account/email/verify/new?code=${code}`,
            null,
            { withCredentials: true }
        );

        return {
            success: true,
            statusCode: response.status,
            data: null,
            message: response.data.message
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}