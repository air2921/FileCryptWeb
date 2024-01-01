import React from 'react';
import DateComponent from '../Date/Date';
import Message from '../Message/Message';

function FileList({ files, isOwner }: FileListProps) {

    if (!files || files.every(file => file === null)) {
        return <div><Message message={'No encrypted files here'} font='home_storage' /></div>;
    }

    return (
        <ul>
            {files
                .filter(file => file !== null)
                .map(file => (
                    <li key={file.file_id} className="file">
                        <div className="file_header">
                            <div className="file-name-type">
                                <div className="file-Name">{file.file_name}</div>
                                <div className="file-Type">{file.type}</div>
                            </div>
                        </div>
                        <div className="file-details">
                            <div className="time"><DateComponent date={file.operation_date} /></div>
                            <div className="brief-file-info">
                                <div className="file-id">FID#{file.file_id}</div>
                                <div className="file-id">UID#{file.user_id}</div>
                            </div>
                        </div>
                        {isOwner && <button>Delete</button>}
                    </li>
                ))}
        </ul>
    );
};

export default FileList;