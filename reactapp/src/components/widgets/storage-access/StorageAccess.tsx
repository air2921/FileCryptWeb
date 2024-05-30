import React, { useState } from "react";
import { getStorageCode } from "../../../utils/api/Helper";
import Modal from "../../modal/Modal";
import Verify from "../verify/Verify";
import Icon from "../icon/Icon";

interface DeleteStorageProps {
    deleteStorage: (storageId: number, code: number) => void;
    storageId: number;
}

export function DeleteStorage(props: DeleteStorageProps) {
    const [code, setCode] = useState('');
    const [isActive, setActive] = useState(false);

    function setCodeState(state: string) {
        setCode(state);
    }

    return (
        <>
            {getStorageCode(props.storageId) ? (
                <button onClick={() => props.deleteStorage(props.storageId, getStorageCode(props.storageId)!)}>
                    <Icon icon={'delete'} width={16} height={16} />
                </button>
            ) : (
                    <>
                        <button onClick={() => setActive(true)}>
                            <Icon icon={'delete'} width={16} height={16} />
                        </button>
                        <Modal isActive={isActive} setActive={setActive}>
                            <Verify length={6} onChange={setCodeState} />
                            <button onClick={() => props.deleteStorage(props.storageId, parseInt(code))}>
                                <Icon icon={'delete'} width={16} height={16} />
                            </button>
                        </Modal>
                    </>
            )}
        </>
    );
}