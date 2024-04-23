import axios from "axios";

export const BASE_URL = 'https://localhost:8081/';
export const JWT_ITEM = 'preference_xtr';

export function errorHandler(error: any) {
    console.error(error);
    let statusCode = 500;
    let errorMessage = 'An error occurred during the request';
    if (error.response) {
        errorMessage = error.response.data && error.response.data.message ? error.response.data.message : 'Unknown error';
        statusCode = error.response.status;
    }

    return {
        success: false,
        statusCode: statusCode,
        data: null,
        message: errorMessage
    }
}

export async function refreshJwt() {
    try {
        const response = await axios.post(BASE_URL + 'api/auth/refresh', null, { withCredentials: true })
        localStorage.removeItem(JWT_ITEM);
        localStorage.setItem(JWT_ITEM, JSON.stringify(response.data.access))

        return {
            success: true,
            statusCode: response.status,
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export function interceptor() {
    const instance = axios.create({ baseURL: BASE_URL });

    instance.interceptors.request.use(
        async (config) => {
            const tokenObject = localStorage.getItem(JWT_ITEM);
            if (!tokenObject) {
                await refreshJwt();
                return config;
            }

            const tokenJson = JSON.parse(tokenObject);
            if (!tokenJson) {
                await refreshJwt();
                return config;
            }

            const token = tokenJson.jwt;
            const expires = +tokenJson.expires;
            const secondsSinceEpoch = Math.floor(new Date().getTime() / 1000);
            if (expires > secondsSinceEpoch || token) {
                config.headers.Authorization = `Bearer ${token}`;
            } else {
                await refreshJwt();
            }
            return config;
        },
        (error) => {
            return Promise.reject(error);
        }
    );

    axios.interceptors.response.use(
        async function (response) {
            if ('X-AUTH-REQUIRED' in response.headers) {
                await refreshJwt();
                return response;
            }
            return response;
        },
        function (error) {
            return Promise.reject(error);
        }
    );
}