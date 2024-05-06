import React, { useState } from 'react';
import { StorageProps, deleteStorage } from '../../../utils/api/Storages';
import DateComponent from '../../../utils/helpers/date/Date';
import Modal from '../../modal/Modal';
import { getStorageCode } from '../../../utils/api/Helper';
import NoState from '../../noState/NoState';

export interface StorageListProps {
    storages: StorageProps[] | null,
    isOwner: boolean,
    deleteStorage?: (storageId: number, code: number) => void,
}

function StorageList(props: StorageListProps) {
    const [isActive, setActive] = useState(false);
    const [code, setCode] = useState<number>();
    const [id, setId] = useState(0);

    const checkCode = (id: number) => {
        const code = getStorageCode(id);

        if (!code) {
            setActive(true);
            setId(id);
        } else {
            deleteStorage(id, code);
        }
    }

    const formatDescription = (description: string) => {
        const splitter = '|NEW_LINE|'
        const lines = description.split(splitter);
        return lines.join('\n');
    }

    if (!props.storages || props.storages.every(storage => storage === null)) {
        return <NoState />
    }

    return (
        <>
            <ul>
                {props.storages
                    .filter(storage => storage !== null)
                    .map(storage => (
                        <li key={storage.storage_id} className="storage">
                            <div className="storage-header">
                                <div>{storage.storage_name}#{storage.storage_id}</div>
                                <div>{storage.description && formatDescription(storage.description)}</div>
                                <div>Owner #{storage.user_id}</div>
                            </div>
                            <div className="storage-body">Last Modified At <DateComponent date={storage.last_time_modified} /></div>
                            {props.isOwner && props.deleteStorage && (
                                <div className="storage-try-delete-btn-container">
                                    <button className="storage-delete-btn" onClick={() => checkCode(storage.storage_id)}>
                                        Delete
                                    </button>
                                </div>
                            )}
                        </li>
                    ))}
            </ul>
            <Modal isActive={isActive} setActive={setActive}>
                <div className="confirm-info-message">
                    After deleting a storage, all keys associated with it will be irretrievably lost.
                </div>
                <label htmlFor="code">
                    Enter access code
                    <input
                        type="text"
                        id="code"
                        required={true}
                        value={code}
                        onChange={(e) => {
                            const value = e.target.value;
                            if (value === '') {
                                setCode(undefined);
                            } else {
                                const parsedValue = parseInt(value, 10);
                                if (!isNaN(parsedValue)) {
                                    setCode(parsedValue);
                                }
                            }
                        }}
                        inputMode="numeric"
                        placeholder="Code"
                    />
                </label>
                <div className="storage-delete-btn-container">
                    <button className="storage-delete-btn" disabled={code === undefined} onClick={() => deleteStorage(id, code!)}>
                        Delete
                    </button>
                </div>
            </Modal>
        </>
    );
}

export default StorageList;