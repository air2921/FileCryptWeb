import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useParams } from 'react-router-dom';
import Error from '../components/Error/Error';

const User = () => {
    const { userId } = useParams();
    const [userData, setUserData] = useState(null);
    const [successStatusCode, setStatusCode] = useState(false)
    const [errorMessage, setErrorMessage] = useState('');

    const fetchData = async () => {
        try {
            const response = await axios.get(`https://localhost:7067/api/core/users/${userId}`, { withCredentials: true });
            setUserData(response.data);
            setStatusCode(true);

        } catch (error: any) {
            console.error(error);
            setStatusCode(false);
            if (error.response) {
                const errorMessage = error.response.data && error.response.data.message ? error.response.data.message : 'Unknown error';
                setErrorMessage(errorMessage);
            } else {
                setErrorMessage('Unknown error');
            }
        }
    };

    useEffect(() => {
        fetchData();
    }, [userId]);

    if (!successStatusCode || !userData) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { user, isOwner, keys, files } = userData as {
        user: any, isOwner: boolean, keys: any, files: any[]
    };

    return (
        <div className="main-container">
            <div className="user-container">
                <div>
                    <span className="username">
                        {`${user.username}#${user.id}`}
                        {isOwner && <button>Edit</button>}
                    </span>
                    <span className="role">
                        {user.role}
                    </span>
                </div>
                <div>
                    <span className="email">
                        {user.email}
                        {isOwner && <button>Edit</button>}
                    </span>
                </div>
                <div className="keys-container">
                    <div className="private-key">
                        <span className="private-key-name">
                            Private Key
                        </span>
                        <span className="has-key">
                            {keys.privateKey ? <i className="material-icons-sharp">check</i> : <i className="material-icons-sharp">close</i>}
                            {isOwner && <button>Change</button>}
                        </span>
                    </div>
                    <div className="internal-key">
                        <span className="internal-key-name">
                            Internal Key
                        </span>
                        <span className="has-key">
                            {keys.internalKey ? <i className="material-icons-sharp">check</i> : <i className="material-icons-sharp">close</i>}
                            {isOwner && <button>Change</button>}
                        </span>
                    </div>
                    <div className="received-key">
                        <span className="received-key-name">
                            Received Key
                        </span>
                        <span className="has-key">
                            {keys.receivedKey ? <i className="material-icons-sharp">check</i> : <i className="material-icons-sharp">close</i>}
                            {isOwner && <button>Change</button>}
                        </span>
                    </div>
                </div>
            </div>
            <div className="files-container">
                <ul>
                    {files && files.some(file => file !== null) ? (
                        files
                            .filter(file => file !== null)
                            .map((file) => (
                                <li key={file.file_id} className="file">
                                    <div className="file_header">
                                        <div className="file-name-type">
                                            <span className="file-Name">
                                                {file.file_name}
                                            </span>
                                        </div>
                                    </div>
                                    <div className="file-details">
                                        <div className="time">
                                            {file.operation_date}
                                        </div>
                                        <div className="brief-File-Info">
                                            <div className="file-Type">
                                                {file.type}
                                            </div>
                                            <div className="file-id">
                                                #{file.file_id}
                                            </div>
                                        </div>
                                    </div>
                                    {isOwner && <button>Delete</button>}
                                </li>
                            ))
                    ) : (
                            <div>
                                {<Error errorMessage={'No encrypted files here'} errorFont={'home_storage'} />}
                            </div>
                    )}
                </ul>
            </div>
        </div>
    );
};

export default User;