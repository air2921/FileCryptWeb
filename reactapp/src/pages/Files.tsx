import React, { ChangeEvent, useEffect, useState } from 'react';
import FileList from '../components/FileList/FileList';
import AxiosRequest from '../api/AxiosRequest';
import Message from '../components/Message/Message';
import AxiosRequestInterceptor from '../api/AxiosRequestInterceptor';
import Button from '../components/Helpers/Button';
import FileButton from '../components/Helpers/FileButton';
import Font from '../components/Font/Font';

const Files = () => {
    const [byAsc, setBy] = useState(true);
    const [skip, setSkip] = useState(0);
    const step = 10;
    const [errorMessage, setErrorMessage] = useState('');
    const [filesList, setFiles] = useState(null);

    const [lastTimeModified, setLastTimeModified] = useState(Date.now());
    const [deletingError, setDeletingError] = useState('');
    const [cryptographyError, setCryptographyError] = useState('');

    const fetchData = async () => {
        const response = await AxiosRequest({ endpoint: `api/core/files/all?byAscending=${byAsc}&skip=${skip}&count=${step}`, method: 'GET', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setFiles(response.data);
        }
        else {
            setErrorMessage(response.data);
        }
    }

    const handleLoadMore = () => {
        setSkip(prevSkip => prevSkip + step);
    };

    const handleBack = () => {
        setSkip(prevSkip => Math.max(0, prevSkip - step));
    };

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
            const response = await AxiosRequestInterceptor.post(
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
    }, [byAsc, lastTimeModified, skip]);

    if (!filesList) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { files } = filesList as { files: any[] }

    return (
        <div className="container">
            <div className="cryptography">
                <p>Select file to encrypt</p>
                <div className="encrypt">
                    <FileButton id={'private-encrypt'} font={'upload_file'} onChange={(e) => handleFileChange(e, 'private', 'encrypt')} fileType={'private'} operationType={'encrypt'} />
                    <FileButton id={'internal-encrypt'} font={'upload_file'} onChange={(e) => handleFileChange(e, 'internal', 'encrypt')} fileType={'internal'} operationType={'encrypt'} />
                    <FileButton id={'received-encrypt'} font={'upload_file'} onChange={(e) => handleFileChange(e, 'received', 'encrypt')} fileType={'received'} operationType={'encrypt'} />
                </div>
                <p>Select file to decrypt</p>
                <div className="decrypt">
                    <FileButton id={'private-decrypt'} font={'upload_file'} onChange={(e) => handleFileChange(e, 'private', 'decrypt')} fileType={'private'} operationType={'decrypt'} />
                    <FileButton id={'internal-decrypt'} font={'upload_file'} onChange={(e) => handleFileChange(e, 'internal', 'decrypt')} fileType={'internal'} operationType={'decrypt'} />
                    <FileButton id={'received-decrypt'} font={'upload_file'} onChange={(e) => handleFileChange(e, 'received', 'decrypt')} fileType={'received'} operationType={'decrypt'} />
                </div>
                {cryptographyError && < Message message={cryptographyError} font={'error'} />}
                <p>You can manage your encryption keys<a href="/settings/keys"> here </a></p>
            </div>
            <div className="files">
                <FileList files={files} isOwner={true} deleteFile={deleteFile} error={deletingError} />
                {skip > 0 && <Button onClick={handleBack}><Font font={'arrow_back'} /></Button>}
                {files.length > step - 1 && <Button onClick={handleLoadMore}><Font font={'arrow_forward'} /></Button>}
            </div>
        </div>
    );
}

export default Files;