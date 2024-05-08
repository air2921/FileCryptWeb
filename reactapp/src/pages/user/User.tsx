import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { UserProps, getFullyUser } from '../../utils/api/Users';
import ErrorPage from '../static/error-status/ErrorPage';
import Loader from '../static/loader/Loader';
import { FileProps } from '../../utils/api/Files';
import { StorageProps } from '../../utils/api/Storages';
import { OfferProps } from '../../utils/api/Offers';
import { getRangeActivity } from '../../utils/api/Activity';

const User = () => {
    const { userId } = useParams();
    const { year } = useParams();
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

    const fetchActivity = async () => {
        let start = `2024-01-01`;
        let end = `2024-12-31`;

        if (year) {
            start = `${year}-01-01`
            end = `${year}-12-31`;
        }

        const startDate = new Date(start);
        const endDate = new Date(end);

        const response = await getRangeActivity(true, startDate, endDate, null);

        if (response.success) {

        } else {

        }
    }

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
            <div className="user-activity">

            </div>
            <div className="file-offer-container">
                <div className="files">

                </div>
                <div className="offers">

                </div>
            </div>
        </div>
    );
};

export default User;