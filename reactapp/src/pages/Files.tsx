﻿import React, { ChangeEvent, useEffect, useState } from 'react';
import FileList from '../components/FileList/FileList';
import AxiosRequest from '../api/AxiosRequest';
import Input from '../components/Helpers/Input';
import Message from '../components/Message/Message';
import AxiosRequestIntercaptor from '../api/AxiosRequestInterceptor';

const Files = () => {
    const [byAsc, setBy] = useState(true);
    const [errorMessage, setErrorMessage] = useState('');
    const [filesList, setFiles] = useState(null);

    const [lastTimeModified, setLastTimeModified] = useState(Date.now());
    const [deletingError, setDeletingError] = useState('');
    const [cryptographyError, setCryptographyError] = useState('');

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

    const downloadFile = (file: Blob, filename: string) => {
        const blob = new Blob([file], { type: file.type });

        const fileURL = URL.createObjectURL(blob);

        const downloadLink = document.createElement('a');
        downloadLink.href = fileURL;
        downloadLink.setAttribute('download', filename);

        document.body.appendChild(downloadLink);
        downloadLink.click();

        document.body.removeChild(downloadLink);
        URL.revokeObjectURL(fileURL);
    }

    const encryptFile = async (file: FormData, fileType: string, operationType: string, filename: string) => {
        try {
            const response = await AxiosRequestIntercaptor.post(
                `https://localhost:7067/api/core/cryptography/${fileType}/${operationType}`,
                file,
                {
                    withCredentials: true,
                    responseType: 'blob'
                }
            );

            setCryptographyError('');
            setLastTimeModified(Date.now());
            downloadFile(response.data, filename);
        }
        catch (error: any) {
            const errorText = await error.response.data.text();
            try {
                const errorJson = JSON.parse(errorText);
                setCryptographyError(errorJson.message)
            }
            catch (e) {
                setCryptographyError('Unexpected error')
            }
        }
    }

    const handleFileChange = (event: ChangeEvent<HTMLInputElement>, fileType: string, operationType: string) => {
        const file = event.target.files ? event.target.files[0] : null;
        if (file) {
            const formData = new FormData();
            formData.append('file', file);

            encryptFile(formData, fileType, operationType, file.name);
        }
    };

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
                    <Input text='Select file to encrypt' type="file" id='private-encrypt' require={true} onChange={(e) => handleFileChange(e, 'private', 'encrypt')} />
                </div>
                <div className="decrypt">
                    <Input text='Select file to decrypt' type="file" id='private-decrypt' require={true} onChange={(e) => handleFileChange(e, 'private', 'decrypt')} />
                </div>
                {cryptographyError && < Message message={cryptographyError} font={'error'} />}
            </div>
            <div className="files">
                <FileList files={files} isOwner={true} deleteFile={deleteFile} error={deletingError} />
            </div>
        </div>
    );
}

export default Files;