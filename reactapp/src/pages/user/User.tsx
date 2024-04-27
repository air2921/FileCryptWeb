import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import OfferList from '../../components/lists/offers/OfferList';
import FileList from '../../components/lists/files/FileList';
import { UserProps, getFullyUser } from '../../utils/api/Users';
import ErrorPage from '../static/error-status/ErrorPage';
import Loader from '../static/loader/Loader';
import { FileProps } from '../../utils/api/Files';
import { StorageProps } from '../../utils/api/Storages';
import { OfferProps } from '../../utils/api/Offers';

const User = () => {
    const { userId } = useParams();
    const [data, setData] = useState(null);
    const [status, setStatus] = useState(500)
    const [errorMessage, setErrorMessage] = useState('');

    const fetchData = async () => {
        const response = await getFullyUser(parseInt(userId!));

        if (response.success) {
            setData(response.data);
        } else {
            setStatus(response.statusCode);
            setErrorMessage(response.message!);
        }
    };

    useEffect(() => {
        fetchData();
    }, [userId]);

    if (!data) {
        return errorMessage ? <ErrorPage statusCode={status} message={errorMessage} /> : <Loader />;
    }

    const { user, isOwner, storages, files, offers } = data as {
        user: UserProps, isOwner: boolean, storages: StorageProps[], files: FileProps[], offers: OfferProps[]
    };

    return (
        <div className="profile">
            <div className="user-container">

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