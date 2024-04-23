import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import OfferList from '../../components/lists/offers/OfferList';
import FileList from '../../components/lists/files/FileList';
import { getFiles } from '../../utils/api/Files';

const User = () => {
    //const { userId } = useParams();
    //const [userData, setUserData] = useState(null);
    //const [successStatusCode, setStatusCode] = useState(false)
    //const [errorMessage, setErrorMessage] = useState('');
    //const navigate = useNavigate();

    //const fetchData = async () => {
    //    const user = await getUser(parseInt(userId!), false);
    //};

    //useEffect(() => {
    //    fetchData();
    //}, [userId]);

    //if (!successStatusCode || !userData) {
    //    return <div className="error">{errorMessage || 'Loading...'}</div>;
    //}

    //const { user, isOwner, keys, files, offers } = userData as {
    //    user: any, isOwner: boolean, keys: any, files: any[], offers: any[]
    //};

    //return (
    //    <div className="profile">
    //        <div className="user-container">

    //        </div>
    //        <div className="file-offer-container">
    //            <div className="files">
    //                <FileList files={files} isOwner={isOwner} />
    //            </div>
    //            <div className="offers">
    //                <OfferList offers={offers} isOwner={isOwner} />
    //            </div>
    //        </div>
    //    </div>
    //);
};

export default User;