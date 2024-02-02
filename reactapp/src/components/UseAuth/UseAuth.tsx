import { useState, useEffect } from 'react';
import cookie from 'react-cookies'
import AxiosRequest from '../../api/AxiosRequest';
import { useNavigate } from 'react-router-dom';

const useAuth = () => {
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const navigate = useNavigate();

    const getAuthCookie = () => {
        const cookieValue: string | undefined = cookie.load('auth_success');
        console.log(cookieValue);
        return cookieValue;
    }

    const getAuthStatus = async () => {
        const response = await AxiosRequest({ endpoint: 'api/auth/check', method: 'GET', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setIsAuthenticated(true);
        }
        else {
            setIsAuthenticated(false);
        }
    }

    useEffect(() => {
        const authStatus = getAuthCookie();

        if (authStatus === undefined) {
            getAuthStatus();
        }
        else {
            if (authStatus === 'True') {
                setIsAuthenticated(true);
            }
            else {
                setIsAuthenticated(false);
            }
        }

    }, [navigate]);

    return isAuthenticated;
};

export default useAuth;