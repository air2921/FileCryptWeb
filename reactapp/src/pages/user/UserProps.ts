export interface UserProps {
    id: number,
    username: string,
    role: string,
    email?: string,
    is_2fa_enabled: boolean,
    is_blocked: boolean
}