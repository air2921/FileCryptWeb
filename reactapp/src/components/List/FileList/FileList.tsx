import React, { useState } from 'react';
import DateComponent from '../../../utils/helpers/date/Date';
import Modal from '../../Modal/Modal';
import Message from '../../../utils/helpers/message/Message';
import Font from '../../../utils/helpers/icon/Font';

function FileList({ files, isOwner, deleteFile }: FileListProps) {
    const [fileData, setFile] = useState<FileProps | null>(null);
    const [active, setActive] = useState(false);

    if (!files || files.every(file => file === null)) {
        return <div><Message message={'No encrypted files here'} font='storage' /></div>;
    }

    const openModal = (file: FileProps) => {
        setFile(file);
        setActive(true);
    }

    const ModalContent = () => {

        if (!fileData)
            return;

        return (
            <div>
                <div className="file_header">
                    <div className="file-Name">File name: {fileData.file_name}</div>
                    <div className="mime">
                        <div className="file-mime">File MIME: {fileData.file_mime}</div>
                        <div className="file-mime-category">MIME Category: {fileData.file_mime_category}</div>
                    </div>
                    <div className="file-type">File type: {fileData.type}</div>
                </div>
                <div className="date">
                    <div className="time">Operation Date: <DateComponent date={fileData.operation_date} /></div>
                </div>
                <div className="id-info">
                    <div className="file-id">File ID: #{fileData.file_id}</div>
                    <div className="file-id">Owner ID: #{fileData.user_id}</div>
                </div>
                {isOwner && deleteFile && (
                    <button onClick={() => deleteFile(fileData.file_id)}>
                        <Font font={'delete'} />
                    </button>
                )}
            </div>
        );
    }

    return (
        <>
            <ul>
                <Message message={'Your Files'} font='storage' />
                {files
                    .filter(file => file !== null)
                    .map(file => (
                        <li key={file.file_id} className="file">
                            <div className="file_header">
                                <div className="file-Name">File: {file.file_name}</div>
                                <div className="file-mime-category">MIME Category: {file.file_mime_category}</div>
                                <div className="time">Operation Date: <DateComponent date={file.operation_date} /></div>
                            </div>
                            <button onClick={() => openModal(file)}>More</button>
                        </li>
                    ))}
            </ul>
            <Modal isActive={active} setActive={setActive}>
                <ModalContent />
            </Modal>
        </>
    );
};

export default FileList;