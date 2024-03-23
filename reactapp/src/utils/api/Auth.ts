import axios from 'axios';
const BASE_URL = 'https://localhost:8081/';

const EMAIL_IN_STORAGE = 'login_email';

export async function login(email: string, password: string) {
    try {
        const response = await axios.post(BASE_URL + 'api/auth/login',
            {
                email: email,
                password: password
            }, { withCredentials: true }
        );
        localStorage.setItem(EMAIL_IN_STORAGE, email);

        return {
            statusCode: response.status,
            verificationRequired: response.data.confirm,
            message: response.data.message
        }

    } catch (error: any) {
        console.error(error);
        let statusCode = 500;
        let errorMessage = 'An error occurred during the request';
        if (error.response) {
            errorMessage = error.response.data && error.response.data.message ? error.response.data.message : 'Unknown error';
            statusCode = error.response.status;
        }

        return {
            statusCode: statusCode,
            verificationRequired: false,
            message: errorMessage
        };
    }
}

export async function verifyLogin(code: number) {
    try {
        const email = localStorage.getItem(EMAIL_IN_STORAGE);
        if (!email || email === undefined) {
            return {
                statusCode: 400,
                message: 'Email not found in localStorage'
            };
        }

        const response = await axios.post(BASE_URL + `api/auth/verify/2fa?code=${code}&email=${email}`, null, {
            withCredentials: true
        });
        localStorage.removeItem(EMAIL_IN_STORAGE);

        return {
            statusCode: response.status,
            message: response.data.message
        }

    } catch (error: any) {
        console.error(error);
        let statusCode = 500;
        let errorMessage = 'An error occurred during the request';
        if (error.response) {
            errorMessage = error.response.data && error.response.data.message ? error.response.data.message : 'Unknown error';
            statusCode = error.response.status;
        }

        return {
            statusCode: statusCode,
            message: errorMessage
        };
    }
}

export async function createRecovery(email: string) {
    try {
        const response = await axios.post(BASE_URL + `api/auth/recovery/unique/token?email=${email}`, null, {
            withCredentials: true
        });
        return {
            statusCode: response.status,
            message: response.data.message
        };

    } catch (error: any) {
        console.error(error);
        let statusCode = 500;
        let errorMessage = 'An error occurred during the request';
        if (error.response) {
            errorMessage = error.response.data && error.response.data.message ? error.response.data.message : 'Unknown error';
            statusCode = error.response.status;
        }

        return {
            statusCode: statusCode,
            message: errorMessage
        };
    }
}

export async function recoveryAccount(password: string, token: string) {
    try {
        const response = await axios.post(BASE_URL + `api/auth/recovery/account`, {
            password: password,
            token: token
        }, { withCredentials: true })
        return {
            statusCode: response.status,
            message: response.data.message
        };

    } catch (error: any) {
        console.error(error);
        let statusCode = 500;
        let errorMessage = 'An error occurred during the request';
        if (error.response) {
            errorMessage = error.response.data && error.response.data.message ? error.response.data.message : 'Unknown error';
            statusCode = error.response.status;
        }

        return {
            statusCode: statusCode,
            message: errorMessage
        };
    }
}

export async function registration(email: string, password: string, username: string, is2fa: boolean) {
    try {
        const response = await axios.post(BASE_URL + 'api/auth/register', {
            email: email,
            password: password,
            username: username,
            is_2fa_enabled: is2fa
        }, { withCredentials: true });

        localStorage.setItem('registration_email', email);
        return {
            statusCode: response.status,
            message: response.data.message
        };

    } catch (error: any) {
        console.error(error);
        let statusCode = 500;
        let errorMessage = 'An error occurred during the request';
        if (error.response) {
            errorMessage = error.response.data && error.response.data.message ? error.response.data.message : 'Unknown error';
            statusCode = error.response.status;
        }

        return {
            statusCode: statusCode,
            message: errorMessage
        };
    }
}

export async function verifyRegistration(code: number) {
    try {
        const email = localStorage.getItem('registration_email');
        if (email === null || email === undefined) {
            return {
                statusCode: 400,
                message: 'Try again later'
            }
        }

        const response = await axios.post(BASE_URL + `api/auth/verify?code=${code}&email=${email}`, null, { withCredentials: true });
        localStorage.removeItem('registration_email')

        return {
            statusCode: response.status,
            message: undefined
        }
    } catch (error: any) {
        console.error(error);
        let statusCode = 500;
        let errorMessage = 'An error occurred during the request';
        if (error.response) {
            errorMessage = error.response.data && error.response.data.message ? error.response.data.message : 'Unknown error';
            statusCode = error.response.status;
        }

        return {
            statusCode: statusCode,
            message: errorMessage
        };
    }
}