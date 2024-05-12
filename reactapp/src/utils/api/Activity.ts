import axios from "axios";
import { BASE_URL, errorHandler } from './Helper';

export interface DayActivityProps {
    Date: string,
    ActivityCount: number
    Activities: ActivityProps[]
}

export interface ActivityProps {
    action_id: number,
    type_id: number,
    action_date: string,
    user_id: number
}

export async function getRangeActivity(byDesc: boolean, start: Date, end: Date, type?: number | null) {
    try {
        let typeParam = '';
        if (type) {
            typeParam = `&type=${type}`
        }

        const response = await axios.get(BASE_URL + `api/core/activity?byDesc=${byDesc}&start=${start}&end=${end}${typeParam}`);

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

export async function getActivity(id: number) {
    try {
        const response = await axios.get(BASE_URL + `api/core/activity/${id}`);

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