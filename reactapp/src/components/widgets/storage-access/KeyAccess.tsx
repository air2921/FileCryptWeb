import React, { useState } from "react";
import { getStorageCode } from "../../../utils/api/Helper";
import Icon from "../icon/Icon";
import Modal from "../../modal/Modal";
import Verify from "../verify/Verify";

interface DeleteKeyProps {
    deleteKey: (storageId: number, keyId: number, code: number) => void;
    storageId: number;
    keyId: number;
}

interface AddKeyProps {
    addStorage: (storageId: number, code: number, name: string, value: string) => void;
    storageId: number;
    name: string;
    value: string;
}

interface KeyFormProps {
    name: string;
    onNameChange: (name: string) => void;
    value: string;
    onValueChange: (value: string) => void;
    code?: string;
    onCodeChange: (code: string) => void;
}

export function DeleteKey(props: DeleteKeyProps) {
    const [code, setCode] = useState('');
    const [isActive, setActive] = useState(false);

    function setCodeState(state: string) {
        setCode(state);
    }

    return (
        <>
            {getStorageCode(props.storageId) ? (
                <button onClick={() => props.deleteKey(props.storageId, props.keyId, getStorageCode(props.storageId)!)}>
                    <Icon icon={'delete'} width={16} height={16} />
                </button>
            ) : (
                    <>
                        <button onClick={() => setActive(true)}>
                            <Icon icon={'delete'} width={16} height={16} />
                        </button>
                        <Modal isActive={isActive} setActive={setActive}>
                            <Verify length={6} onChange={setCodeState} />
                            <button onClick={() => props.deleteKey(props.storageId, props.keyId, parseInt(code))}>
                                <Icon icon={'delete'} width={16} height={16} />
                            </button>
                        </Modal>
                    </>
            )}
        </>
    );
}

export function AddKey(props: AddKeyProps) {
    const [code, setCode] = useState(getStorageCode(props.storageId)?.toString());
    const [name, setName] = useState('');
    const [value, setValue] = useState('');
    const [isActive, setActive] = useState(false);

    function setNameState(name: string) {
        setName(name);
    }

    function setValueState(key: string) {
        setValue(key);
    }

    function setCodeState(code: string) {
        const codeNumber = parseInt(code);
        if (!isNaN(codeNumber)) {
            setCode(code);
        }
    }


    return (
        <>
            <button onClick={() => setActive(true)}>
                <Icon icon={'add'} width={16} height={16} />
            </button>
            <Modal isActive={isActive} setActive={setActive}>
                <AddKeyForm
                    name={name}
                    value={value}
                    code={code}
                    onNameChange={setNameState}
                    onValueChange={setValueState}
                    onCodeChange={setCodeState}
                />
                <button onClick={() => props.addStorage(props.storageId, parseInt(code!), name, value)}>
                    <Icon icon={'add'} width={16} height={16} />
                </button>
            </Modal>
        </>
    );
}

export function AddKeyForm(props: KeyFormProps) {
    return (
        <div>
            <div>
                <div>Key Name</div>
                <div>
                    <input
                        type={'text'}
                        value={props.name}
                        onChange={(e) => props.onNameChange(e.target.value)}
                    />
                </div>
            </div>
            <div>
                <div>Key Value</div>
                <div>
                    <input
                        type={'text'}
                        value={props.value}
                        onChange={(e) => props.onValueChange(e.target.value)}
                    />
                </div>
            </div>
            {!props.code && (
                <div>
                    <div>Storage Code</div>
                    <div>
                        <input
                            type={'text'}
                            value={props.code}
                            onChange={(e) => props.onCodeChange(e.target.value)}
                        />
                    </div>
                </div>
            )}
        </div>
    );
}