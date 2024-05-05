import axios from "axios";
import { BASE_URL, errorHandler } from './Helper';

export async function getRangeActivity(byDesc: boolean, start: Date, end: Date, type?: string) {
    try {
        const response = await axios.get(`api/core/activity?byDesc=${byDesc}&start=${start}&end=${end}`);

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
        const response = await axios.get(`api/core/activity/${id}`);

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