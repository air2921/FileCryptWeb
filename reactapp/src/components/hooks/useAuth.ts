import { useState, useEffect } from 'react';
import cookie from 'react-cookies'
import { useNavigate } from 'react-router-dom';
import { getAuth } from '../../utils/api/Auth';

const useAuth = () => {
    const [isAuthenticated, setIsAuthenticated] = useState<null | boolean>(null);
    const navigate = useNavigate();

    const getAuthCookie = () => {
        const cookieValue: string | undefined = cookie.load('auth_success');
        console.log(cookieValue);
        return cookieValue;
    }

    const getAuthStatus = async () => {
        const response = await getAuth();

        if (response.success) {
            setIsAuthenticated(true);
        } else {
            setIsAuthenticated(false);
        }
    }

    useEffect(() => {
        const authStatus = getAuthCookie();

        if (authStatus === undefined) {
            getAuthStatus();
        } else {
            if (authStatus === 'True') {
                setIsAuthenticated(true);
            } else {
                setIsAuthenticated(false);
            }
        }

    }, [navigate]);

    return isAuthenticated;
};

export default useAuth;