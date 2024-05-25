import axios from "axios";
import { BASE_URL, errorHandler } from './Helper';

export interface OfferSortProps {
    skip: number,
    count: number,
    orderByDesc: string,
    sent?: string,
    closed?: string,
    type?: number
}

export interface OfferProps {
    offer_id: number;
    offer_header?: string | null;
    offer_body?: string | null;
    offer_type: number;
    is_accepted: boolean;
    created_at: string;
    sender_id: number;
    receiver_id: number;
}

export async function OpenOffer(userId: number, storageId: number, keyId: number, code: number) {
    try {
        const response = await axios.post(
            BASE_URL + `api/core/offer/open/${userId}?storageId=${storageId}&keyId=${keyId}&code=${code}`,
            null, { withCredentials: true }
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

export async function CloseOffer(offerId: number, keyname: string, storageId: number, code: number) {
    try {
        const response = await axios.post(
            BASE_URL + `api/core/offer/close/${offerId}?name=${keyname}&storageId=${storageId}&code=${code}`,
            null, { withCredentials: true }
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

export async function getOffer(offerId: number) {
    try {
        const response = await axios.get(BASE_URL + `api/core/offer/${offerId}`, { withCredentials: true });

        return {
            success: true,
            statusCode: response.status,
            data: response.data
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function getOffers(p: OfferSortProps) {
    try {
        const response = await axios.get(
            BASE_URL + `api/core/offer/range?skip=${p.skip}&count=${p.count}&byDesc=${p.orderByDesc}&sent=${p}closed=${p.closed}&type=${p.type}`,
            { withCredentials: true }
        )

        return {
            success: true,
            statusCode: response.status,
            data: response.data
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function deleteOffer(offerId: number) {
    try {
        const response = await axios.delete(BASE_URL + `api/core/offer/${offerId}`, { withCredentials: true });

        return {
            success: true,
            statusCode: response.status,
            message: undefined
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}