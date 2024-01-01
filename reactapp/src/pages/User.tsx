import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';

import UserData from '../components/User/UserData';
import UserKeys from '../components/User/UserKeys';
import FileList from '../components/FileList/FileList';
import OfferList from '../components/OfferList/OfferList';
import AxiosRequest from '../api/AxiosRequest';

const User = () => {
    const { userId } = useParams();
    const [userData, setUserData] = useState(null);
    const [successStatusCode, setStatusCode] = useState(false)
    const [errorMessage, setErrorMessage] = useState('');

    const fetchData = async () => {

        const response = await AxiosRequest({ endpoint: `api/core/users/${userId}`, method: 'GET', withCookie: true, requestBody: null })

        if (response.isSuccess) {
            setUserData(response.data);
            setStatusCode(true);
        }
        else {
            setErrorMessage(response.data);
        }
    };

    useEffect(() => {
        fetchData();
    }, [userId]);

    if (!successStatusCode || !userData) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { user, isOwner, keys, files, offers } = userData as {
        user: any, isOwner: boolean, keys: any, files: any[], offers: any[]
    };

    return (
        <div className="profile">
            <div className="user-container">
                <UserData user={user} />
                <UserKeys keys={keys} />
            </div>
            <div className="container">
                <FileList files={files} isOwner={isOwner} />
                <OfferList offers={offers} isOwner={isOwner} />
            </div>
        </div>
    );
};

export default User;