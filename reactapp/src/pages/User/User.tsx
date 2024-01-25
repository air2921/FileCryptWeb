import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import UserData from '../../components/User/UserData';
import UserKeys from '../../components/User/UserKeys';
import OfferList from '../../components/OfferList/OfferList';
import FileList from '../../components/FileList/FileList';
import AxiosRequest from '../../api/AxiosRequest';

const User = () => {
    const { userId } = useParams();
    const { username } = useParams();
    const [userData, setUserData] = useState(null);
    const [successStatusCode, setStatusCode] = useState(false)
    const [errorMessage, setErrorMessage] = useState('');
    const navigate = useNavigate();

    const fetchData = async () => {

        const response = await AxiosRequest({ endpoint: `api/core/users/${userId}/${username}`, method: 'GET', withCookie: true, requestBody: null })

        if (response.isSuccess) {
            setUserData(response.data);
            setStatusCode(true);
        }
        else {
            if (response.statusCode === 404) {
                navigate("*");
            }

            setErrorMessage(response.data);
        }
    };

    useEffect(() => {
        fetchData();
    }, [userId, username]);

    if (!successStatusCode || !userData) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { user, isOwner, keys, files, offers } = userData as {
        user: any, isOwner: boolean, keys: any, files: any[], offers: any[]
    };

    return (
        <div className="profile">
            <div className="user-container">
                <UserData user={user} isOwner={isOwner} />
                <UserKeys keys={keys} />
            </div>
            <div className="file-offer-container">
                <div className="files">
                    <FileList files={files} isOwner={isOwner} />
                </div>
                <div className="offers">
                    <OfferList offers={offers} isOwner={isOwner} />
                </div>
            </div>
        </div>
    );
};

export default User;