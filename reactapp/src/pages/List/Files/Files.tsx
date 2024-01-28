import React, { ChangeEvent, useEffect, useState } from 'react';
import AxiosRequest from '../../../api/AxiosRequest';
import AxiosRequestInterceptor from '../../../api/AxiosRequestInterceptor';
import Button from '../../../components/Helpers/Button';
import Font from '../../../components/Font/Font';
import Modal from '../../../components/Modal/Modal';
import FileButton from '../../../components/Helpers/FileButton';
import FileList from '../../../components/FileList/FileList';
import Message from '../../../components/Message/Message';

const Files = () => {
    const [skip, setSkip] = useState(0);
    const step = 10;
    const [errorMessage, setErrorMessage] = useState('');
    const [filesList, setFiles] = useState(null);
    const [encryptModalActive, setEncryptModalActive] = useState(false);
    const [decryptModalActive, setDecryptModalActive] = useState(false); 
    const [lastTimeModified, setLastTimeModified] = useState(Date.now());
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');

    const fetchData = async () => {
        const response = await AxiosRequest({ endpoint: `api/core/files/all?skip=${skip}&count=${step}`, method: 'GET', withCookie: true, requestBody: null });

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

        setTimeout(() => {
            setMessage('');
            setFont('');
        }, 5000)
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

        setTimeout(() => {
            setMessage('');
            setFont('');
        }, 5000)
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
    }, [lastTimeModified, skip]);

    if (!filesList) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { files } = filesList as { files: any[] }

    return (
        <div className="container">
            <div className="cryptography">
                <div className="encrypt">
                    <p>Encrypt File</p>
                    <Button onClick={() => setEncryptModalActive(true)}>
                        <Font font={'lock'} />
                    </Button>
                    <Modal isActive={encryptModalActive} setActive={setEncryptModalActive}>
                        <p>Encrypt via Private Key</p>
                        <FileButton id={'private-encrypt'} font={'add'} onChange={(e) => handleFileChange(e, 'private', 'encrypt')} fileType={'private'} operationType={'encrypt'} />
                        <p>Encrypt via Internal Key</p>
                        <FileButton id={'internal-encrypt'} font={'add'} onChange={(e) => handleFileChange(e, 'internal', 'encrypt')} fileType={'internal'} operationType={'encrypt'} />
                        <p>Encrypt via Received Key</p>
                        <FileButton id={'received-encrypt'} font={'add'} onChange={(e) => handleFileChange(e, 'received', 'encrypt')} fileType={'received'} operationType={'encrypt'} />
                    </Modal>
                </div>
                <div className="decrypt">
                    <p>Decrypt File</p>
                    <Button onClick={() => setDecryptModalActive(true)}>
                        <Font font={'lock_open'} />
                    </Button>
                    <Modal isActive={decryptModalActive} setActive={setDecryptModalActive}>
                        <p>Decrypt via Private Key</p>
                        <FileButton id={'private-decrypt'} font={'add'} onChange={(e) => handleFileChange(e, 'private', 'decrypt')} fileType={'private'} operationType={'decrypt'} />
                        <p>Decrypt via Internal Key</p>
                        <FileButton id={'internal-decrypt'} font={'add'} onChange={(e) => handleFileChange(e, 'internal', 'decrypt')} fileType={'internal'} operationType={'decrypt'} />
                        <p>Decrypt via Received Key</p>
                        <FileButton id={'received-decrypt'} font={'add'} onChange={(e) => handleFileChange(e, 'received', 'decrypt')} fileType={'received'} operationType={'decrypt'} />
                    </Modal>
                </div>
            </div>
            {message && font && < Message message={message} font={font} />}
            <div className="files">
                <FileList files={files} isOwner={true} deleteFile={deleteFile} />
                {skip > 0 && <Button onClick={handleBack}><Font font={'arrow_back'} /></Button>}
                {files.length > step - 1 && <Button onClick={handleLoadMore}><Font font={'arrow_forward'} /></Button>}
            </div>
        </div>
    );
}

export default Files;