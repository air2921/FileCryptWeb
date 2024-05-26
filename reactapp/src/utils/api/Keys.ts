import axios from "axios";
import { BASE_URL, errorHandler, setStorageCode } from './Helper';

export interface KeySortProps {
    skip: number,
    count: 10,
    orderByDesc: string
}

export interface KeyProps {
    key_id: number;
    key_name: string;
    key_value: string;
    created_at: string;
    storage_id: number;
}

export async function getKey(storageId: number, keyId: number, code: number) {
    try {
        const response = await axios.get(
            BASE_URL + `api/core/storage/${storageId}/${keyId}?code=${code}`,
            { withCredentials: true }
        );
        setStorageCode(storageId, code);

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

export async function getKeys(storageId: number, code: number, props: KeySortProps) {
    try {
        const response = await axios.get(
            BASE_URL + `api/core/storage/range/${storageId}/?skip=${props.skip}&count=${props.count}&byDesc=${props.orderByDesc}&code=${code}`,
            { withCredentials: true }
        )
        setStorageCode(storageId, code);

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

export async function deleteKey(storageId: number, keyId: number, code: number) {
    try {
        const response = await axios.delete(
            BASE_URL + `api/core/storage/${storageId}/${keyId}?code=${code}`,
            { withCredentials: true }
        );
        setStorageCode(storageId, code);

        return {
            success: true,
            statusCode: response.status,
            message: undefined
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function addKey(storageId: number, code: number, name: string, value: string) {
    try {
        const response = await axios.post(
            BASE_URL + `api/core/storage/${storageId}?name=${name}&value=${value}&code=${code}`,
            null, { withCredentials: true }
        );
        setStorageCode(storageId, code);

        return {
            success: true,
            statusCode: response.status,
            message: response.data.message
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}