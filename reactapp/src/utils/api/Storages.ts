import axios from "axios";
import { BASE_URL, errorHandler } from "./Helper";

export interface StoragesSortProps {
    skip: number,
    count: 10,
    orderByDesc: string
}

export interface StorageProps {
    storage_id: number,
    storage_name: string,
    description?: string,
    last_time_modified: string,
    user_id: number
}

export async function getStorage(storageId: number) {
    try {
        const response = await axios.get(BASE_URL + `api/core/storage/${storageId}`, { withCredentials: true });

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

export async function getStorages(props: StoragesSortProps) {
    try {
        const response = await axios.get(
            BASE_URL + `api/core/storage/range?skip=${props.skip}&count=${props.count}&byDesc=${props.orderByDesc}`,
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

export async function deleteStorage(storageId: number, code: number) {
    try {
        const response = await axios.delete(
            BASE_URL + `api/core/storage/${storageId}?code=${code}`,
            { withCredentials: true }
        );

        return {
            success: true,
            statusCode: response.status,
            message: undefined
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function addStorage(name: string, description: string, code: number) {
    try {
        const response = await axios.post(
            BASE_URL + `api/core/storage`,
            {
                Name: name,
                Description: description,
                Code: code
            }, { withCredentials: true }
        )

        return {
            success: true,
            statusCode: response.status,
            message: response.data.message
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}