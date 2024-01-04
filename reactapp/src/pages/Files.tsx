import React, { useEffect, useState } from 'react';
import FileList from '../components/FileList/FileList';
import AxiosRequest from '../api/AxiosRequest';

const Files = () => {
    const [byAsc, setBy] = useState(true);
    const [errorMessage, setErrorMessage] = useState('');
    const [filesList, setFiles] = useState(null);

    const [lastTimeModified, setLastTimeModified] = useState(Date.now())
    const [deletingError, setDeletingError] = useState('');

    const fetchData = async () => {
        const response = await AxiosRequest({ endpoint: `api/core/files/all?byAscending=${byAsc}`, method: 'GET', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setFiles(response.data);
        }
        else {
            setErrorMessage(response.data);
        }
    }

    const deleteFile = async (fileId: number) => {
        const response = await AxiosRequest({ endpoint: `api/core/files?fileId=${fileId}&filename=&byId=true`, method: 'DELETE', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setDeletingError('');
            setLastTimeModified(Date.now());
        }
        else {
            setDeletingError(response.data);
        }
    }

    useEffect(() => {
        fetchData();
    }, [byAsc, lastTimeModified]);

    if (!filesList) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { files } = filesList as { files: any[] }

    return (
        <div className="container">
            <div className="cryptography">
                <div className="encrypt">

                </div>
                <div className="decrypt">

                </div>
            </div>
            <div className="files">
                <FileList files={files} isOwner={true} deleteFile={deleteFile} error={deletingError} />
            </div>
        </div>
    );
}

export default Files;