import React, { ChangeEvent, useEffect, useState } from 'react';
import Message from '../../../utils/helpers/message/Message';
import { CypherProps, FileProps, cypherFile, deleteFile, getFiles } from '../../../utils/api/Files';
import Loader from '../../../components/widgets/loader/Loader';

interface FileButtonProps {
    id: string,
    font: string,
    onChange: (event: ChangeEvent<HTMLInputElement>, operationType: string) => void,
    operationType: string
}


const Files = () => {
    //const step = 10;
    //const [skip, setSkip] = useState(0);
    //const [orderBy, setOrderBy] = useState('true');
    //const [mime, setMime] = useState('');
    //const [mimeCategory, setCategory] = useState('');

    //const [files, setFiles] = useState<FileProps[] | null>();
    //const [lastTimeModified, setLastTimeModified] = useState(Date.now());
    //const [message, setMessage] = useState('');
    //const [icon, setIcon] = useState('');

    //const fetchData = async () => {
    //    const result = await getFiles({
    //        skip: skip,
    //        orderByDesc: orderBy,
    //        mime: mime,
    //        category: mimeCategory,
    //        count: step
    //    });

    //    if (result.success) {
    //        setFiles(result.data);
    //    } else {
    //        setMessage(result.message);
    //    }

    //    resetMessageAfterDelay();
    //}

    //const deleteFileSubmit = async (fileId: number) => {
    //    const result = await deleteFile(fileId);
    //    if (result.success) {
    //        setLastTimeModified(Date.now());
    //    } else {
    //        setMessage(result.message!);
    //        setIcon('error');
    //    }

    //    resetMessageAfterDelay();
    //}

    //const cypherFileSubmit = async (props: CypherProps) => {
    //    const result = await cypherFile(props);
    //    if (result.success) {
    //        setMessage('');
    //        setIcon('');
    //        setLastTimeModified(Date.now());
    //    } else {
    //        setMessage(result.message);
    //        setIcon('error');
    //    }

    //    resetMessageAfterDelay();
    //}

    //const handleFileChange = (event: ChangeEvent<HTMLInputElement>, encrypt: string) => {
    //    const file = event.target.files ? event.target.files[0] : null;
    //    if (file) {
    //        const formData = new FormData();
    //        formData.append('file', file);

    //        cypherFileSubmit({
    //            file: formData,
    //            filename: file.name,
    //            encrypt: encrypt
    //        });
    //    }
    //};

    //const resetMessageAfterDelay = () => {
    //    setTimeout(() => {
    //        setMessage('');
    //        setIcon('');
    //    }, 3000);
    //};

    //const handleLoadMore = () => {
    //    setSkip(prevSkip => prevSkip + step);
    //};

    //const handleLoadLess = () => {
    //    setSkip(prevSkip => Math.max(0, prevSkip - step));
    //};

    //useEffect(() => {
    //    fetchData();
    //}, [lastTimeModified, skip, orderBy, mimeCategory, mime]);

    //if (!files) {
    //    return message ? <div>{message}</div> : <Loader />;
    //}

    //function FileButton({ id, font, onChange, operationType }: FileButtonProps) {

    //    const clickElement = (elementId: string) => {
    //        document.getElementById(elementId)?.click();
    //    };

    //    return (
    //        <div>
    //            <input
    //                type="file"
    //                id={id}
    //                style={{ display: "none" }}
    //                required={true}
    //                onChange={(event) => onChange(event, operationType)}
    //            />
    //            <button onClick={() => clickElement(id)}>
    //                <Font font={font} />
    //            </button>
    //        </div>
    //    );
    //}

    //const SetFileAndEncrypt = () => {
    //    const [operation, setOperation] = useState('encrypt');

    //    return (
    //        <div>
    //            <p>Operation</p>
    //            <select
    //                className="operation-type"
    //                id="operation"
    //                required={true}
    //                value={operation}
    //                onChange={(e) => setOperation(e.target.value)}>

    //                <option value="true">Encrypt file</option>
    //                <option value="false">Decrypt file</option>
    //            </select>

    //            <FileButton id={`${operation}`}
    //                font={'add'}
    //                onChange={(e) => handleFileChange(e, operation)}
    //                operationType={operation}
    //            />
    //        </div>
    //    );
    //}

    //const SortFiles = () => {
    //    return (
    //        <div className="sort">
    //            <details>
    //                <summary>
    //                    <span>Category</span>
    //                </summary>
    //                <select
    //                    className="file-mime-category"
    //                    id="category"
    //                    required={true}
    //                    value={mimeCategory}
    //                    onChange={(e) => setCategory(e.target.value)}>
    //                    <option value="">All</option>
    //                    <option value="application">Application</option>
    //                    <option value="audio">Audio</option>
    //                    <option value="font">Font</option>
    //                    <option value="image">Image</option>
    //                    <option value="message">Message</option>
    //                    <option value="model">Model</option>
    //                    <option value="multipart">Multipart</option>
    //                    <option value="text">Text</option>
    //                    <option value="video">Video</option>
    //                </select>
    //            </details>
    //            <details>
    //                <summary>
    //                    <span>Order by</span>
    //                </summary>
    //                <select
    //                    className="order-by"
    //                    id="order"
    //                    required={true}
    //                    value={orderBy}
    //                    onChange={(e) => setOrderBy(e.target.value)}>
    //                    <option value="false">Order by ascending</option>
    //                    <option value="true">Order by descending</option>
    //                </select>
    //            </details>
    //        </div>
    //    );
    //}

    //return (
    //    <div className="container">
    //        <SetFileAndEncrypt />
    //        <div className="files">
    //            <SortFiles />
    //            <FileList files={files} isOwner={true} deleteFile={deleteFileSubmit} />
    //            <div className="scroll">
    //                {skip > 0 && <button onClick={handleLoadLess}>Previous</button>}
    //                {files.length > step - 1 && <button onClick={handleLoadMore}>Next</button>}
    //            </div>
    //        </div>
    //        <div className="message">
    //            {message && icon && < Message message={message} font={icon} />}
    //        </div>
    //    </div>
    //);
}

export default Files;