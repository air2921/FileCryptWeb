import React, { ChangeEvent, useEffect, useState } from 'react';
import Message from '../../../utils/helpers/message/Message';
import AxiosRequest from '../../../utils/api/AxiosRequest';
import AxiosRequestInterceptor from '../../../utils/api/AxiosRequestInterceptor';
import Font from '../../../utils/helpers/icon/Font';
import FileList from '../../../components/List/FileList/FileList';

interface FileButtonProps {
    id: string,
    font: string,
    onChange: (event: ChangeEvent<HTMLInputElement>, fileType: string, operationType: string) => void,
    fileType: string,
    operationType: string
}


const Files = () => {
    const [skip, setSkip] = useState(0);
    const step = 10;
    const [orderBy, setOrderBy] = useState('true');
    const [type, setType] = useState('');
    const [mime, setMime] = useState('');
    const [mimeCategory, setCategory] = useState('');

    const [errorMessage, setErrorMessage] = useState('');
    const [filesList, setFiles] = useState(null);
    const [lastTimeModified, setLastTimeModified] = useState(Date.now());
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');

    const fetchData = async () => {
        const response = await AxiosRequest({
            endpoint: `api/core/files/all?skip=${skip}&count=${step}&byDesc=${orderBy}&type=${type}&category=${mimeCategory}&mime=${mime}`,
            method: 'GET',
            withCookie: true,
            requestBody: null
        });

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
        const response = await AxiosRequest({ endpoint: `api/core/files/${fileId}`, method: 'DELETE', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setLastTimeModified(Date.now());
        }
        else {
            setMessage(response.data);
            setFont('error')
        }

        resetMessageAfterDelay();
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

            setMessage('');
            setFont('');
            setLastTimeModified(Date.now());
            downloadFile(response.data, filename);
        }
        catch (error: any) {
            const errorText = await error.response.data.text();
            try {
                const errorJson = JSON.parse(errorText);
                setMessage(errorJson.message)
                setFont('error');
            }
            catch (e) {
                setMessage('Unexpected error')
                setFont('error');
            }
        }

        resetMessageAfterDelay();
    }

    const handleFileChange = (event: ChangeEvent<HTMLInputElement>, fileType: string, operationType: string) => {
        const file = event.target.files ? event.target.files[0] : null;
        if (file) {
            const formData = new FormData();
            formData.append('file', file);

            encryptFile(formData, fileType, operationType, file.name);
        }
    };

    const resetMessageAfterDelay = () => {
        setTimeout(() => {
            setMessage('');
            setFont('');
        }, 3000);
    };

    useEffect(() => {
        fetchData();
    }, [lastTimeModified, skip, orderBy, type, mimeCategory, mime]);

    if (!filesList) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { files } = filesList as { files: any[] }

    function FileButton({ id, font, onChange, fileType, operationType }: FileButtonProps) {

        const clickElement = (elementId: string) => {
            document.getElementById(elementId)?.click();
        };

        return (
            <div>
                <input
                    type="file"
                    id={id}
                    style={{ display: "none" }}
                    required={true}
                    onChange={(event) => onChange(event, fileType, operationType)}
                />
                <button onClick={() => clickElement(id)}>
                    <Font font={font} />
                </button>
            </div>
        );
    }

    const SetFileAndEncrypt = () => {
        const [operation, setOperation] = useState('encrypt');
        const [keyType, setKeyType] = useState('private');

        return (
            <div>
                <p>Encryption key</p>
                <select
                    className="key-type"
                    id="key"
                    required={true}
                    value={keyType}
                    onChange={(e) => setKeyType(e.target.value)}>

                    <option value="private">Private</option>
                    <option value="internal">Internal</option>
                    <option value="received">Received</option>
                </select>
                <p>Operation</p>
                <select
                    className="operation-type"
                    id="operation"
                    required={true}
                    value={operation}
                    onChange={(e) => setOperation(e.target.value)}>

                    <option value="encrypt">Encrypt file</option>
                    <option value="decrypt">Decrypt file</option>
                </select>

                <FileButton id={`${keyType}-${operation}`} font={'add'} onChange={(e) => handleFileChange(e, keyType, operation)} fileType={keyType} operationType={operation} />
            </div>
        );
    }

    const SortFiles = () => {
        return (
            <div className="sort">
                <details>
                    <summary>
                        <span>Category</span>
                    </summary>
                    <select
                        className="file-mime-category"
                        id="category"
                        required={true}
                        value={mimeCategory}
                        onChange={(e) => setCategory(e.target.value)}>
                        <option value="">All</option>
                        <option value="application">Application</option>
                        <option value="audio">Audio</option>
                        <option value="font">Font</option>
                        <option value="image">Image</option>
                        <option value="message">Message</option>
                        <option value="model">Model</option>
                        <option value="multipart">Multipart</option>
                        <option value="text">Text</option>
                        <option value="video">Video</option>
                    </select>
                </details>
                <details>
                    <summary>
                        <span>Type</span>
                    </summary>
                    <select
                        className="file-type"
                        id="type"
                        required={true}
                        value={type}
                        onChange={(e) => setType(e.target.value)}>
                        <option value="">All</option>
                        <option value="private">Private</option>
                        <option value="internal">Internal</option>
                        <option value="received">Received</option>
                    </select>
                </details>
                <details>
                    <summary>
                        <span>Order by</span>
                    </summary>
                    <select
                        className="order-by"
                        id="order"
                        required={true}
                        value={orderBy}
                        onChange={(e) => setOrderBy(e.target.value)}>
                        <option value="false">Order by ascending</option>
                        <option value="true">Order by descending</option>
                    </select>
                </details>
            </div>
        );
    }

    return (
        <div className="container">
            <SetFileAndEncrypt />
            <div className="files">
                <SortFiles />
                <FileList files={files} isOwner={true} deleteFile={deleteFile} />
                <div className="scroll">
                    {skip > 0 && <button onClick={handleBack}>Previous</button>}
                    {files.length > step - 1 && <button onClick={handleLoadMore}>Next</button>}
                </div>
            </div>
            <div className="message">
                {message && font && < Message message={message} font={font} />}
            </div>
        </div>
    );
}

export default Files;