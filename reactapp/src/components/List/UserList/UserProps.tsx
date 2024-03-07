interface UserProps {
    id: number,
    username: string,
    role: string,
    email?: string,
    password?: string,
    is_2fa_enabled?: string,
    is_blocked: string
}