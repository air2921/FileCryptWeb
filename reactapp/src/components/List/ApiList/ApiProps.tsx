interface ApiProps {
    api_id: number,
    api_key: string,
    type: string,
    expiry_date?: string | null,
    is_blocked: boolean,
    last_time_activity: string,
    max_request_of_day: number,
    user_id: number,
    apiCallLeft?: number
}