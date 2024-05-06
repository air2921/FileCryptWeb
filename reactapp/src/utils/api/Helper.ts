import cookie from 'react-cookies'

export const BASE_URL = 'https://localhost:8081/';
const STORAGE_PREFIX = 'Storage#'

export function errorHandler(error: any) {
    console.error(error);
    let statusCode = 500;
    let errorMessage = 'An error occurred during the request';
    if (error.response) {
        errorMessage = error.response.data && error.response.data.message ? error.response.data.message : 'Unknown error';
        statusCode = error.response.status;

        if (error.response.headers && error.response.headers['X-AUTH_REQUIRED']) {
            cookie.remove('auth_success');
        }
    }

    return {
        success: false,
        statusCode: statusCode,
        data: null,
        message: errorMessage
    }
}

export function getStorageCode(storageId: number) {
    const code = sessionStorage.getItem(`${STORAGE_PREFIX}${storageId}`);

    if (!code) {
        return null;
    } else {
        return parseInt(code);
    }
}

export function setStorageCode(storageId: number, code: number) {
    sessionStorage.setItem(`${STORAGE_PREFIX}${storageId}`, code.toString());
}