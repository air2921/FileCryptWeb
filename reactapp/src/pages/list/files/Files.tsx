import React, { ChangeEvent, useEffect, useState } from 'react';
import Message from '../../../utils/helpers/message/Message';
import Font from '../../../utils/helpers/icon/Font';
import FileList from '../../../components/lists/files/FileList';
import { FileProps, cypherFile, deleteFile, getFiles } from '../../../utils/api/Files';
import Loader from '../../static/loader/Loader';

interface FileButtonProps {
    id: string,
    font: string,
    onChange: (event: ChangeEvent<HTMLInputElement>, fileType: string, operationType: string) => void,
    fileType: string,
    operationType: string
}


const Files = () => {
    const step = 10;
    const [skip, setSkip] = useState(0);
    const [orderBy, setOrderBy] = useState('true');
    const [type, setType] = useState('');
    const [mime, setMime] = useState('');
    const [mimeCategory, setCategory] = useState('');

    const [files, setFiles] = useState<FileProps[] | null>();
    const [lastTimeModified, setLastTimeModified] = useState(Date.now());
    const [message, setMessage] = useState('');
    const [icon, setIcon] = useState('');

    const fetchData = async () => {
        const result = await getFiles({
            skip: skip,
            type: type,
            orderByDesc: orderBy,
            mime: mime,
            category: mimeCategory,
            count: step
        });

        if (result.success) {
            setFiles(result.data);
        } else {
            setMessage(result.message);
        }

        resetMessageAfterDelay();
    }

    const deleteFileSubmit = async (fileId: number) => {
        const result = await deleteFile(fileId);
        if (result.success) {
            setLastTimeModified(Date.now());
        } else {
            setMessage(result.message!);
            setIcon('error');
        }

        resetMessageAfterDelay();
    }

    const cypherFileSubmit = async (file: FormData, fileType: string, operationType: string, filename: string, signature: string) => {
        const result = await cypherFile(file, fileType, operationType, filename, signature);
        if (result.success) {
            setMessage('');
            setIcon('');
            setLastTimeModified(Date.now());
        } else {
            setMessage(result.message);
            setIcon('error');
        }

        resetMessageAfterDelay();
    }

    const handleFileChange = (event: ChangeEvent<HTMLInputElement>, fileType: string, operationType: string, signature: string) => {
        const file = event.target.files ? event.target.files[0] : null;
        if (file) {
            const formData = new FormData();
            formData.append('file', file);

            cypherFileSubmit(formData, fileType, operationType, file.name, signature);
        }
    };

    const resetMessageAfterDelay = () => {
        setTimeout(() => {
            setMessage('');
            setIcon('');
        }, 3000);
    };

    const handleLoadMore = () => {
        setSkip(prevSkip => prevSkip + step);
    };

    const handleLoadLess = () => {
        setSkip(prevSkip => Math.max(0, prevSkip - step));
    };

    useEffect(() => {
        fetchData();
    }, [lastTimeModified, skip, orderBy, type, mimeCategory, mime]);

    if (!files) {
        return message ? <div>{message}</div> : <Loader />;
    }

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
        const [signature, setSignature] = useState('false')

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
                <p>Signature</p>
                <select
                    className="signature-required"
                    id="signature"
                    required={true}
                    value={keyType}
                    onChange={(e) => setSignature(e.target.value)}>

                    <option value="true">Add signature</option>
                    <option value="false">No signature</option>
                </select>

                <FileButton id={`${keyType}-${operation}`} font={'add'} onChange={(e) => handleFileChange(e, keyType, operation, signature)} fileType={keyType} operationType={operation} />
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
                <FileList files={files} isOwner={true} deleteFile={deleteFileSubmit} />
                <div className="scroll">
                    {skip > 0 && <button onClick={handleLoadLess}>Previous</button>}
                    {files.length > step - 1 && <button onClick={handleLoadMore}>Next</button>}
                </div>
            </div>
            <div className="message">
                {message && icon && < Message message={message} font={icon} />}
            </div>
        </div>
    );
}

export default Files;