import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { UserProps, getFullyUser } from '../../utils/api/Users';
import ErrorPage from '../static/error-status/ErrorPage';
import Loader from '../static/loader/Loader';
import { FileProps } from '../../utils/api/Files';
import { StorageProps } from '../../utils/api/Storages';
import { OfferProps } from '../../utils/api/Offers';
import { DayActivityProps, getRangeActivity } from '../../utils/api/Activity';
import Board from '../../components/activityBoard/Board';

const User = () => {
    let { year } = useParams();
    const { userId } = useParams();
    const [currentYear, setYear] = useState(new Date().getFullYear());
    const [data, setData] = useState(null);
    const [activity, setActivity] = useState<null | DayActivityProps[]>();
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
        let start
        let end

        if (year) {
            start = `${year}-01-01`
            end = `${year}-12-31`;
            setYear(parseInt(year))
        } else {
            start = `${currentYear}-01-01`;
            end = `${currentYear}-12-31`;
        }

        const startDate = new Date(start);
        const endDate = new Date(end);

        const response = await getRangeActivity(true, startDate, endDate, null);

        if (response.success) {
            setActivity(response.data);
        } else {
            setStatus(response.statusCode);
            setErrorMessage(response.message!);
        }
    }

    useEffect(() => {
        fetchData();
        fetchActivity();
    }, [userId]);

    if (!data || !activity) {
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
                <Board days={activity} year={currentYear} />
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