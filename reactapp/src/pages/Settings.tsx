import React, { useEffect, useState } from 'react';
import AxiosRequest from '../api/AxiosRequest';
import UserData from '../components/User/UserData';
import Username from '../components/Settings/Username';

const Settings = async () => {

    useEffect(() => {
        fetchData();
    }, []);


    const [userData, setUserData] = useState(null); 

    const [errorMessage, setErrorMessage] = useState('');
    const [successStatusCode, setStatus] = useState(false);

    const fetchData = async () => {
        const response = await AxiosRequest({ endpoint: `api/core/users/only`, method: 'GET', withCookie: true, requestBody: null })

        if (response.isSuccess) {
            setUserData(response.data)
            setStatus(true)
        }
        else {
            setErrorMessage(response.data);
        }
    }

    if (!successStatusCode || !userData) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { user } = userData as { user: any };

    return (
        <div className="container">
            <UserData user={user} isOwner={true} />
            <Username />
        </div>
    );
}

export default Settings;